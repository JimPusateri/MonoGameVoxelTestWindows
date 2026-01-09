using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public struct BlockInstance
{
    public Vector3 Position;
    public BlockType Type;

    public BlockInstance(Vector3 position, BlockType type)
    {
        Position = position;
        Type = type;
    }
}

public sealed class Chunk
{
    public readonly Vector3 Origin;
    public readonly int ChunkX;
    public readonly int ChunkY;
    public readonly int ChunkZ;


    public readonly BlockType[,,] Blocks =
        new BlockType[VoxelConstants.ChunkSize, VoxelConstants.ChunkSize, VoxelConstants.ChunkSize];

    public List<BlockInstance> BlockInstances = new();
    public bool Dirty = true;

   public Chunk(GraphicsDevice gd, TextureAtlas atlas, int cx, int cy, int cz)
{
    ChunkX = cx;
    ChunkY = cy;
    ChunkZ = cz;
    Origin = new Vector3(cx * VoxelConstants.ChunkSize, cy * VoxelConstants.ChunkSize, cz * VoxelConstants.ChunkSize);
}


    public void GenerateFlat()
    {
        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        {
            if (y == 4) Blocks[x,y,z] = BlockType.Grass;
            else if (y < 4) Blocks[x,y,z] = BlockType.Dirt;
            else Blocks[x,y,z] = BlockType.Air;
        }
        Dirty = true;
    }
    public void GenerateFlat2()
    {
        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        {
            int worldY = (int)Origin.Y + y; // Origin.Y already equals cy*ChunkSize

            if (worldY == 4) Blocks[x,y,z] = BlockType.Grass;
            else if (worldY < 4) Blocks[x,y,z] = BlockType.Dirt;
            else Blocks[x,y,z] = BlockType.Air;
        }

        Dirty = true;
    }
public void GenerateFromWorld(IBlockAccessor world)
{
    for (int x = 0; x < VoxelConstants.ChunkSize; x++)
    for (int y = 0; y < VoxelConstants.ChunkSize; y++)
    for (int z = 0; z < VoxelConstants.ChunkSize; z++)
    {
        int wx = (int)Origin.X + x;
        int wy = (int)Origin.Y + y;
        int wz = (int)Origin.Z + z;

        Blocks[x,y,z] = world.GetBlock(wx, wy, wz);
    }

    Dirty = true;
}


    private BlockType GetLocal(int x, int y, int z)
    {
        if (x < 0 || x >= VoxelConstants.ChunkSize ||
            y < 0 || y >= VoxelConstants.ChunkSize ||
            z < 0 || z >= VoxelConstants.ChunkSize)
            return BlockType.Air;

        return Blocks[x,y,z];
    }

    public void RebuildMesh(IBlockAccessor world)
    {
        BlockInstances.Clear();

        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        {
            var t = Blocks[x,y,z];
            if (t == BlockType.Air) continue;

            Vector3 position = Origin + new Vector3(x, y, z);
            BlockInstances.Add(new BlockInstance(position, t));
        }

        Dirty = false;
    }
}
