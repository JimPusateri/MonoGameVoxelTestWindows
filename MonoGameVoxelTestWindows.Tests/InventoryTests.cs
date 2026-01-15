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
    
    [Fact]
    public void HasEnoughBlocks_WithSufficientQuantity_ReturnsTrue()
    {
        var inventory = new Inventory();
        inventory.AddBlock(BlockType.CrystalBlue);
        inventory.AddBlock(BlockType.CrystalBlue);
        
        bool result = inventory.HasEnoughBlocks(BlockType.CrystalBlue, 2);
        
        Assert.True(result);
    }
    
    [Fact]
    public void HasEnoughBlocks_WithInsufficientQuantity_ReturnsFalse()
    {
        var inventory = new Inventory();
        inventory.AddBlock(BlockType.CrystalBlue);
        
        bool result = inventory.HasEnoughBlocks(BlockType.CrystalBlue, 2);
        
        Assert.False(result);
    }
    
    [Fact]
    public void HasEnoughBlocks_WithZeroQuantity_ReturnsTrue()
    {
        var inventory = new Inventory();
        
        bool result = inventory.HasEnoughBlocks(BlockType.CrystalBlue, 0);
        
        Assert.True(result);
    }
    
    [Fact]
    public void HasEnoughBlocks_WithNegativeQuantity_ThrowsException()
    {
        var inventory = new Inventory();
        
        Assert.Throws<System.ArgumentException>(() => 
            inventory.HasEnoughBlocks(BlockType.CrystalBlue, -1));
    }
    
    [Fact]
    public void TryConsumeBlocks_WithSufficientQuantity_ReturnsTrue()
    {
        var inventory = new Inventory();
        inventory.AddBlock(BlockType.CrystalBlue);
        inventory.AddBlock(BlockType.CrystalBlue);
        
        bool result = inventory.TryConsumeBlocks(BlockType.CrystalBlue, 2);
        
        Assert.True(result);
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalBlue));
    }
    
    [Fact]
    public void TryConsumeBlocks_WithInsufficientQuantity_ReturnsFalse()
    {
        var inventory = new Inventory();
        inventory.AddBlock(BlockType.CrystalBlue);
        
        bool result = inventory.TryConsumeBlocks(BlockType.CrystalBlue, 2);
        
        Assert.False(result);
        Assert.Equal(1, inventory.GetCount(BlockType.CrystalBlue)); // Not consumed
    }
    
    [Fact]
    public void TryConsumeBlocks_WithExactQuantity_Success()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalBlue);
        
        bool result = inventory.TryConsumeBlocks(BlockType.CrystalBlue, 100);
        
        Assert.True(result);
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalBlue));
    }
    
    [Fact]
    public void TryConsumeBlocks_WithNegativeQuantity_ThrowsException()
    {
        var inventory = new Inventory();
        
        Assert.Throws<System.ArgumentException>(() => 
            inventory.TryConsumeBlocks(BlockType.CrystalBlue, -5));
    }
    
    [Fact]
    public void ConvertBlocks_BlueToRed_WithExact100Blocks_Success()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalBlue);
        
        int batches = inventory.ConvertBlocks(BlockType.CrystalBlue, BlockType.CrystalRed);
        
        Assert.Equal(1, batches);
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalBlue));
        Assert.Equal(1, inventory.GetCount(BlockType.CrystalRed));
    }
    
    [Fact]
    public void ConvertBlocks_BlueToRed_With250Blocks_Converts2Batches()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 250; i++)
            inventory.AddBlock(BlockType.CrystalBlue);
        
        int batches = inventory.ConvertBlocks(BlockType.CrystalBlue, BlockType.CrystalRed);
        
        Assert.Equal(2, batches);
        Assert.Equal(50, inventory.GetCount(BlockType.CrystalBlue)); // 50 remaining
        Assert.Equal(2, inventory.GetCount(BlockType.CrystalRed));
    }
    
    [Fact]
    public void ConvertBlocks_RedToGreen_WithExact100Blocks_Success()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalRed);
        
        int batches = inventory.ConvertBlocks(BlockType.CrystalRed, BlockType.CrystalGreen);
        
        Assert.Equal(1, batches);
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalRed));
        Assert.Equal(1, inventory.GetCount(BlockType.CrystalGreen));
    }
    
    [Fact]
    public void ConvertBlocks_WithZeroBlocks_ReturnsZero()
    {
        var inventory = new Inventory();
        
        int batches = inventory.ConvertBlocks(BlockType.CrystalBlue, BlockType.CrystalRed);
        
        Assert.Equal(0, batches);
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalBlue));
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalRed));
    }
    
    [Fact]
    public void ConvertBlocks_WithLessThan100Blocks_ReturnsZero()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 99; i++)
            inventory.AddBlock(BlockType.CrystalBlue);
        
        int batches = inventory.ConvertBlocks(BlockType.CrystalBlue, BlockType.CrystalRed);
        
        Assert.Equal(0, batches);
        Assert.Equal(99, inventory.GetCount(BlockType.CrystalBlue)); // Unchanged
        Assert.Equal(0, inventory.GetCount(BlockType.CrystalRed));
    }
    
    [Fact]
    public void ConvertBlocks_InvalidDirection_BlueToGreen_ThrowsException()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalBlue);
        
        Assert.Throws<System.ArgumentException>(() => 
            inventory.ConvertBlocks(BlockType.CrystalBlue, BlockType.CrystalGreen));
    }
    
    [Fact]
    public void ConvertBlocks_InvalidDirection_RedToBlue_ThrowsException()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalRed);
        
        Assert.Throws<System.ArgumentException>(() => 
            inventory.ConvertBlocks(BlockType.CrystalRed, BlockType.CrystalBlue));
    }
    
    [Fact]
    public void ConvertBlocks_InvalidDirection_GreenToRed_ThrowsException()
    {
        var inventory = new Inventory();
        for (int i = 0; i < 100; i++)
            inventory.AddBlock(BlockType.CrystalGreen);
        
        Assert.Throws<System.ArgumentException>(() => 
            inventory.ConvertBlocks(BlockType.CrystalGreen, BlockType.CrystalRed));
    }
}
