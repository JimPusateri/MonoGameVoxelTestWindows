using System;

namespace MonoGameVoxelTestWindows;

/// <summary>
/// Provides static properties and stats for different block types.
/// </summary>
public static class BlockProperties
{
    /// <summary>
    /// Gets the base hit points for a given block type before difficulty scaling.
    /// </summary>
    /// <param name="blockType">The type of block to query.</param>
    /// <returns>The base hit points for the block type.</returns>
    /// <exception cref="ArgumentException">Thrown when the block type is not a crystal type.</exception>
    public static int GetBaseHitPoints(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.CrystalBlue => 1,
            BlockType.CrystalRed => 2,
            BlockType.CrystalGreen => 4,
            _ => throw new ArgumentException($"Block type {blockType} does not have hit points defined.", nameof(blockType))
        };
    }
}
