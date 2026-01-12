using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MonoGameVoxelTestWindows;

public record DestructionResult(bool Hit, Vector3Int? Position, BlockType BlockType);

public class BlockDestructionSystem
{
    private readonly ChunkManager _chunkManager;
    private readonly DestructibleBlockLayer _destructibleLayer;
    private readonly Inventory _inventory;
    private readonly Dictionary<BlockType, float> _blockModelScales;
    private readonly Dictionary<BlockType, Vector3> _blockModelOffsets;

    public BlockDestructionSystem(
        ChunkManager chunkManager, 
        DestructibleBlockLayer destructibleLayer, 
        Inventory inventory,
        Dictionary<BlockType, float> blockModelScales,
        Dictionary<BlockType, Vector3> blockModelOffsets)
    {
        _chunkManager = chunkManager;
        _destructibleLayer = destructibleLayer;
        _inventory = inventory;
        _blockModelScales = blockModelScales;
        _blockModelOffsets = blockModelOffsets;
    }

    public DestructionResult TryDestroyBlock(Ray ray, float maxDistance)
    {
        float? closestDistance = null;
        Vector3Int? closestBlock = null;
        BlockType hitBlockType = BlockType.Air;

        foreach (var chunk in _chunkManager.GetAllChunks())
        {
            foreach (var instance in chunk.BlockInstances)
            {
                // Only test destructible blocks
                var blockType = _destructibleLayer.GetBlock((int)instance.Position.X, (int)instance.Position.Y, (int)instance.Position.Z);
                if (blockType == BlockType.Air) continue;

                // Create bounding box for this model instance
                float scale = _blockModelScales[instance.Type];
                Vector3 offset = _blockModelOffsets[instance.Type];
                Vector3 cellCenter = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 worldPos = offset * scale + cellCenter + instance.Position;

                // Create a 1x1x1 box centered at worldPos
                BoundingBox box = new BoundingBox(worldPos - new Vector3(0.5f), worldPos + new Vector3(0.5f));

                float? distance = ray.Intersects(box);
                if (distance.HasValue && distance.Value <= maxDistance)
                {
                    if (!closestDistance.HasValue || distance.Value < closestDistance.Value)
                    {
                        closestDistance = distance;
                        closestBlock = new Vector3Int((int)instance.Position.X, (int)instance.Position.Y, (int)instance.Position.Z);
                        hitBlockType = blockType;
                    }
                }
            }
        }

        if (closestBlock.HasValue)
        {
            var hitBlock = closestBlock.Value;
            
            // Add to inventory
            _inventory.AddBlock(hitBlockType);
            
            // Destroy block
            _destructibleLayer.RemoveBlock(hitBlock.X, hitBlock.Y, hitBlock.Z);
            
            // Mark affected chunks dirty
            var chunk = _chunkManager.GetChunkAt(hitBlock.X, hitBlock.Y, hitBlock.Z);
            if (chunk != null)
            {
                _chunkManager.MarkChunkDirty(chunk);
                _chunkManager.MarkNeighboringChunksDirty(hitBlock.X, hitBlock.Y, hitBlock.Z);
            }

            return new DestructionResult(true, hitBlock, hitBlockType);
        }

        return new DestructionResult(false, null, BlockType.Air);
    }
}
