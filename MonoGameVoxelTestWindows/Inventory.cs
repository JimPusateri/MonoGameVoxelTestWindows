using System;
using System.Collections.Generic;

namespace MonoGameVoxelTestWindows;

public class Inventory
{
    private readonly Dictionary<BlockType, int> _blockCounts = new();

    public Inventory()
    {
        // Initialize all mineable block types to 0
        _blockCounts[BlockType.CrystalBlue] = 0;
        _blockCounts[BlockType.CrystalRed] = 0;
        _blockCounts[BlockType.CrystalGreen] = 0;
    }

    public void AddBlock(BlockType blockType)
    {
        if (_blockCounts.ContainsKey(blockType))
        {
            _blockCounts[blockType]++;
        }
    }

    public int GetCount(BlockType blockType)
    {
        return _blockCounts.TryGetValue(blockType, out int count) ? count : 0;
    }

    public IEnumerable<KeyValuePair<BlockType, int>> GetAllCounts()
    {
        return _blockCounts;
    }
    
    /// <summary>
    /// Checks if the inventory contains at least the specified quantity of a block type.
    /// </summary>
    /// <param name="blockType">The type of block to check.</param>
    /// <param name="quantity">The required quantity.</param>
    /// <returns>True if the inventory has enough blocks; otherwise false.</returns>
    public bool HasEnoughBlocks(BlockType blockType, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        
        return GetCount(blockType) >= quantity;
    }
    
    /// <summary>
    /// Attempts to consume the specified quantity of a block type.
    /// </summary>
    /// <param name="blockType">The type of block to consume.</param>
    /// <param name="quantity">The number of blocks to consume.</param>
    /// <returns>True if the blocks were consumed; false if insufficient quantity available.</returns>
    public bool TryConsumeBlocks(BlockType blockType, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        
        if (!HasEnoughBlocks(blockType, quantity))
            return false;
        
        _blockCounts[blockType] -= quantity;
        return true;
    }
    
    /// <summary>
    /// Converts blocks from one type to another at a 100:1 ratio.
    /// Converts all available complete batches of 100 blocks.
    /// </summary>
    /// <param name="fromType">The source block type (must be CrystalBlue or CrystalRed).</param>
    /// <param name="toType">The destination block type (must be CrystalRed or CrystalGreen).</param>
    /// <returns>The number of batches converted (e.g., 2 if 200+ blocks were available).</returns>
    /// <exception cref="ArgumentException">Thrown when conversion direction is invalid.</exception>
    public int ConvertBlocks(BlockType fromType, BlockType toType)
    {
        // Validate conversion directions
        bool validConversion = (fromType == BlockType.CrystalBlue && toType == BlockType.CrystalRed) ||
                               (fromType == BlockType.CrystalRed && toType == BlockType.CrystalGreen);
        
        if (!validConversion)
        {
            throw new ArgumentException($"Invalid conversion from {fromType} to {toType}. " +
                                      "Only Blue→Red and Red→Green conversions are allowed.");
        }
        
        int available = GetCount(fromType);
        int batches = available / 100;
        
        if (batches == 0)
            return 0;
        
        int toConvert = batches * 100;
        _blockCounts[fromType] -= toConvert;
        _blockCounts[toType] += batches;
        
        return batches;
    }
}
