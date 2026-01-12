using Xunit;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class ArrayWorldTests
{
    [Fact]
    public void GetBlock_ReturnsAirForOutOfBounds()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        
        Assert.Equal(BlockType.Air, world.GetBlock(100, 100, 100));
        Assert.Equal(BlockType.Air, world.GetBlock(-1, -1, -1));
    }

    [Fact]
    public void GetBlock_ReturnsCorrectBlock()
    {
        var blocks = new BlockType[10, 10, 10];
        blocks[5, 3, 7] = BlockType.Stone;
        var world = new ArrayWorld(blocks);
        
        Assert.Equal(BlockType.Stone, world.GetBlock(5, 3, 7));
    }

    [Fact]
    public void GetBlock_WorksAtBoundaries()
    {
        var blocks = new BlockType[10, 10, 10];
        blocks[0, 0, 0] = BlockType.Dirt;
        blocks[9, 9, 9] = BlockType.Grass;
        var world = new ArrayWorld(blocks);
        
        Assert.Equal(BlockType.Dirt, world.GetBlock(0, 0, 0));
        Assert.Equal(BlockType.Grass, world.GetBlock(9, 9, 9));
    }

    [Fact]
    public void GetBlock_DefaultsToAir()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        
        Assert.Equal(BlockType.Air, world.GetBlock(5, 5, 5));
    }
}
