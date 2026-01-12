using Xunit;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class ChunkManagerTests
{
    [Fact]
    public void AddChunk_MarksChunkAsDirty()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        var manager = new ChunkManager(world);
        var chunk = new Chunk(0, 0, 0);
        
        manager.AddChunk(chunk);
        
        Assert.Contains(chunk, manager.GetDirtyChunks());
    }

    [Fact]
    public void GetChunkAt_ReturnsCorrectChunk()
    {
        var blocks = new BlockType[20, 20, 20];
        var world = new ArrayWorld(blocks);
        var manager = new ChunkManager(world);
        var chunk = new Chunk(1, 2, 3);
        manager.AddChunk(chunk);
        
        // Chunk (1,2,3) contains world coordinates (4-7, 8-11, 12-15) with ChunkSize=4
        var result = manager.GetChunkAt(5, 9, 13);
        
        Assert.Same(chunk, result);
    }

    [Fact]
    public void MarkNeighboringChunksDirty_MarksChunkAtBoundary()
    {
        var blocks = new BlockType[20, 20, 20];
        var world = new ArrayWorld(blocks);
        var manager = new ChunkManager(world);
        var chunk1 = new Chunk(0, 0, 0);
        var chunk2 = new Chunk(1, 0, 0);
        manager.AddChunk(chunk1);
        manager.AddChunk(chunk2);
        manager.ClearDirtyFlag(chunk1);
        manager.ClearDirtyFlag(chunk2);
        
        // Block at x=3 is at edge of chunk1 (ChunkSize-1), should mark chunk2 dirty
        manager.MarkNeighboringChunksDirty(3, 0, 0);
        
        Assert.Contains(chunk2, manager.GetDirtyChunks());
    }

    [Fact]
    public void ClearDirtyFlag_RemovesChunkFromDirtyList()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        var manager = new ChunkManager(world);
        var chunk = new Chunk(0, 0, 0);
        manager.AddChunk(chunk);
        
        Assert.Contains(chunk, manager.GetDirtyChunks());
        
        manager.ClearDirtyFlag(chunk);
        
        Assert.DoesNotContain(chunk, manager.GetDirtyChunks());
    }

    [Fact]
    public void MarkChunkDirty_AddsChunkToDirtyList()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        var manager = new ChunkManager(world);
        var chunk = new Chunk(0, 0, 0);
        manager.AddChunk(chunk);
        manager.ClearDirtyFlag(chunk);
        
        Assert.DoesNotContain(chunk, manager.GetDirtyChunks());
        
        manager.MarkChunkDirty(chunk);
        
        Assert.Contains(chunk, manager.GetDirtyChunks());
    }
}
