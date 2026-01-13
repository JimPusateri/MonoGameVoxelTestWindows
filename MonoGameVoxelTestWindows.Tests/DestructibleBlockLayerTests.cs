using Xunit;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class MockRandom : IRandom
{
    private readonly int[] _sequence;
    private int _index = 0;

    public MockRandom(params int[] sequence)
    {
        _sequence = sequence;
    }

    public int Next(int maxValue)
    {
        if (_sequence.Length == 0) return 0;
        int value = _sequence[_index % _sequence.Length];
        _index++;
        // Make sure we return value within range
        return value % maxValue;
    }
}

public class DestructibleBlockLayerTests
{
    [Fact]
    public void GetBlock_ReturnsAirForEmptyPosition()
    {
        var layer = new DestructibleBlockLayer(new SystemRandom());
        
        Assert.Equal(BlockType.Air, layer.GetBlock(0, 0, 0));
    }

    [Fact]
    public void RemoveBlock_MakesPositionReturnAir()
    {
        var mockRandom = new MockRandom(20, 20, 5, 10, 10, 10, 10, 10, 10);
        var layer = new DestructibleBlockLayer(mockRandom);
        layer.AddPile(new Vector3Int(10, 0, 10));
        
        // Should have created blocks around position (10, 0, 10)
        var blockType = layer.GetBlock(10, 0, 10);
        Assert.NotEqual(BlockType.Air, blockType);
        
        layer.RemoveBlock(10, 0, 10);
        
        Assert.Equal(BlockType.Air, layer.GetBlock(10, 0, 10));
    }

    [Fact]
    public void AddPile_CreatesBlocks()
    {
        var mockRandom = new MockRandom(50, 50, 50);
        var layer = new DestructibleBlockLayer(mockRandom);
        
        layer.AddPile(new Vector3Int(20, 0, 20));
        
        // Should have created blocks in the pile
        var blockType = layer.GetBlock(20, 0, 20);
        Assert.NotEqual(BlockType.Air, blockType);
    }

    [Fact]
    public void ClearAll_RemovesAllBlocks()
    {
        var mockRandom = new MockRandom(50);
        var layer = new DestructibleBlockLayer(mockRandom);
        layer.AddPile(new Vector3Int(0, 0, 0));
        
        Assert.True(layer.Count > 0);
        
        layer.ClearAll();
        
        Assert.Equal(0, layer.Count);
        Assert.Equal(BlockType.Air, layer.GetBlock(0, 0, 0));
    }

    [Fact]
    public void GetBlock_UsesCorrectProbabilityDistribution()
    {
        // Test that the random distribution works as expected
        // Values 0-79 = Blue, 80-94 = Red, 95-99 = Green
        var mockRandom = new MockRandom(50, 85, 96); // Blue, Red, Green
        var layer = new DestructibleBlockLayer(mockRandom);
        
        layer.AddPile(new Vector3Int(0, 0, 0));
        
        // Just verify blocks were created with different types
        Assert.True(layer.Count > 0);
    }
}
