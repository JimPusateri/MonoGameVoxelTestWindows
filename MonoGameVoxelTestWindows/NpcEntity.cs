using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameVoxelTestWindows;

public class NpcEntity
{
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }
    public Model Model { get; set; }
    public float Scale { get; set; }
    public Vector3 ModelOffset { get; set; }
    public Vector3Int? TargetBlock { get; set; }
    public float MoveSpeed { get; set; } = 2.0f;
    public float DestroyRange { get; set; } = 1.5f;
    public Vector3 StartingPosition { get; set; }
    public List<BlockType> CollectedBlocks { get; set; } = new List<BlockType>();
    public bool ReturningToStart { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the storage capacity (maximum number of blocks the NPC can carry).
    /// </summary>
    public int StorageCapacity { get; set; } = 5;
    
    /// <summary>
    /// Gets or sets the current storage upgrade level.
    /// </summary>
    public int StorageLevel { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the mining strength (damage dealt per hit to blocks).
    /// </summary>
    public int Strength { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the current strength upgrade level.
    /// </summary>
    public int StrengthLevel { get; set; } = 1;

    public NpcEntity(Model model, Vector3 position, float rotation, float scale, Vector3 modelOffset)
    {
        Model = model;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        ModelOffset = modelOffset;
        StartingPosition = position;
    }
    
    /// <summary>
    /// Calculates the upgrade cost for a given level.
    /// Cost formula: Ceiling(10 * 1.25^level) CrystalBlue blocks.
    /// </summary>
    /// <param name="level">The current level (before upgrade).</param>
    /// <returns>The cost in CrystalBlue blocks.</returns>
    public static int GetUpgradeCost(int level)
    {
        return (int)System.Math.Ceiling(10 * System.Math.Pow(1.25, level));
    }
    
    /// <summary>
    /// Attempts to upgrade the miner's strength using materials from inventory.
    /// </summary>
    /// <param name="inventory">The inventory to consume materials from.</param>
    /// <returns>True if the upgrade succeeded; false if insufficient materials.</returns>
    public bool UpgradeStrength(Inventory inventory)
    {
        if (inventory == null)
            throw new System.ArgumentNullException(nameof(inventory));
        
        int cost = GetUpgradeCost(StrengthLevel);
        
        if (inventory.TryConsumeBlocks(BlockType.CrystalBlue, cost))
        {
            Strength++;
            StrengthLevel++;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Attempts to upgrade the miner's storage capacity using materials from inventory.
    /// </summary>
    /// <param name="inventory">The inventory to consume materials from.</param>
    /// <returns>True if the upgrade succeeded; false if insufficient materials.</returns>
    public bool UpgradeStorage(Inventory inventory)
    {
        if (inventory == null)
            throw new System.ArgumentNullException(nameof(inventory));
        
        int cost = GetUpgradeCost(StorageLevel);
        
        if (inventory.TryConsumeBlocks(BlockType.CrystalBlue, cost))
        {
            StorageCapacity += 5;
            StorageLevel++;
            return true;
        }
        
        return false;
    }
}
