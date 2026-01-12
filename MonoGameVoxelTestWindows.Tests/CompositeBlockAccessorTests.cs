using Xunit;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class CompositeBlockAccessorTests
{
    [Fact]
    public void GetBlock_ReturnsDestructibleLayerWhenNotAir()
    {
        var blocks = new BlockType[20, 20, 20];
        blocks[10, 10, 10] = BlockType.Grass;
        var world = new ArrayWorld(blocks);
        
        var mockRandom = new MockRandom(50);
        var destructible = new DestructibleBlockLayer(mockRandom);
        destructible.AddPile(new Vector3Int(3, 3, 3));
        
        var composite = new CompositeBlockAccessor(destructible, world);
        
        // Destructible layer should take precedence - check a block in the pile
        var blockAt3 = composite.GetBlock(3, 3, 3);
        Assert.NotEqual(BlockType.Air, blockAt3);
        Assert.Equal(BlockType.CrystalBlue, blockAt3); // MockRandom(50) -> 50 % 100 = 50, which is < 80, so CrystalBlue
        
        // World layer should be returned when destructible is air
        var blockAt10 = composite.GetBlock(10, 10, 10);
        Assert.Equal(BlockType.Grass, blockAt10);
    }

    [Fact]
    public void GetBlock_FallsBackToWorldWhenDestructibleIsAir()
    {
        var blocks = new BlockType[20, 20, 20];
        blocks[10, 10, 10] = BlockType.Stone;
        var world = new ArrayWorld(blocks);
        
        var destructible = new DestructibleBlockLayer(new SystemRandom());
        var composite = new CompositeBlockAccessor(destructible, world);
        
        Assert.Equal(BlockType.Stone, composite.GetBlock(10, 10, 10));
    }

    [Fact]
    public void GetBlock_ReturnsAirWhenBothLayersAreAir()
    {
        var blocks = new BlockType[10, 10, 10];
        var world = new ArrayWorld(blocks);
        
        var destructible = new DestructibleBlockLayer(new SystemRandom());
        var composite = new CompositeBlockAccessor(destructible, world);
        
        Assert.Equal(BlockType.Air, composite.GetBlock(5, 5, 5));
    }
}
