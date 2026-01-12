using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameVoxelTestWindows;

public class ChunkManager
{
    private readonly List<Chunk> _chunks = new();
    private readonly Dictionary<Chunk, bool> _dirtyFlags = new();
    private readonly IBlockAccessor _worldAccessor;

    public ChunkManager(IBlockAccessor worldAccessor)
    {
        _worldAccessor = worldAccessor;
    }

    public void AddChunk(Chunk chunk)
    {
        _chunks.Add(chunk);
        _dirtyFlags[chunk] = true;
    }

    public IEnumerable<Chunk> GetAllChunks() => _chunks;

    public IEnumerable<Chunk> GetDirtyChunks()
    {
        return _dirtyFlags.Where(kvp => kvp.Value).Select(kvp => kvp.Key);
    }

    public void MarkChunkDirty(Chunk chunk)
    {
        if (_dirtyFlags.ContainsKey(chunk))
        {
            _dirtyFlags[chunk] = true;
        }
    }

    public void ClearDirtyFlag(Chunk chunk)
    {
        if (_dirtyFlags.ContainsKey(chunk))
        {
            _dirtyFlags[chunk] = false;
        }
    }

    public Chunk GetChunkAt(int worldX, int worldY, int worldZ)
    {
        int chunkX = (int)Math.Floor((double)worldX / VoxelConstants.ChunkSize);
        int chunkY = (int)Math.Floor((double)worldY / VoxelConstants.ChunkSize);
        int chunkZ = (int)Math.Floor((double)worldZ / VoxelConstants.ChunkSize);

        return _chunks.FirstOrDefault(c => c.ChunkX == chunkX && c.ChunkY == chunkY && c.ChunkZ == chunkZ);
    }

    public void MarkNeighboringChunksDirty(int worldX, int worldY, int worldZ)
    {
        int lx = worldX % VoxelConstants.ChunkSize;
        int ly = worldY % VoxelConstants.ChunkSize;
        int lz = worldZ % VoxelConstants.ChunkSize;

        if (lx == 0)
        {
            var neighbor = GetChunkAt(worldX - 1, worldY, worldZ);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
        if (lx == VoxelConstants.ChunkSize - 1)
        {
            var neighbor = GetChunkAt(worldX + 1, worldY, worldZ);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
        if (ly == 0)
        {
            var neighbor = GetChunkAt(worldX, worldY - 1, worldZ);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
        if (ly == VoxelConstants.ChunkSize - 1)
        {
            var neighbor = GetChunkAt(worldX, worldY + 1, worldZ);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
        if (lz == 0)
        {
            var neighbor = GetChunkAt(worldX, worldY, worldZ - 1);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
        if (lz == VoxelConstants.ChunkSize - 1)
        {
            var neighbor = GetChunkAt(worldX, worldY, worldZ + 1);
            if (neighbor != null) MarkChunkDirty(neighbor);
        }
    }
}
