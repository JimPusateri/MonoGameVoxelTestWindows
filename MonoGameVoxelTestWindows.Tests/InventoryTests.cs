using Xunit;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class InventoryTests
{
    [Fact]
    public void AddBlock_IncrementsCount()
    {
        var inventory = new Inventory();
        
        inventory.AddBlock(BlockType.CrystalBlue);
        
        Assert.Equal(1, inventory.GetCount(BlockType.CrystalBlue));
    }

    [Fact]
    public void GetBlockCount_ReturnsZeroForUninitialized()
    {
        var inventory = new Inventory();
        
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalRed));
    }

    [Fact]
    public void AddBlock_MultipleTimesAccumulates()
    {
        var inventory = new Inventory();
        
        inventory.AddBlock(BlockType.CrystalGreen);
        inventory.AddBlock(BlockType.CrystalGreen);
        inventory.AddBlock(BlockType.CrystalGreen);
        
        Assert.Equal(3, inventory.GetCount(BlockType.CrystalGreen));
    }

    [Fact]
    public void AddBlock_DifferentTypesTrackedSeparately()
    {
        var inventory = new Inventory();
        
        inventory.AddBlock(BlockType.CrystalBlue);
        inventory.AddBlock(BlockType.CrystalBlue);
        inventory.AddBlock(BlockType.CrystalRed);
        
        Assert.Equal(2, inventory.GetCount(BlockType.CrystalBlue));
        Assert.Equal(1, inventory.GetCount(BlockType.CrystalRed));
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalGreen));
    }
}
