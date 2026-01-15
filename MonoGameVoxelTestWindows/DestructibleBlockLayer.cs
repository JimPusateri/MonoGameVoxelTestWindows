using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameVoxelTestWindows;

public sealed class DestructibleBlockLayer : IBlockAccessor
{
    private readonly Dictionary<Vector3Int, BlockType> _blocks = new();
    private readonly Dictionary<Vector3, float> _blockCurrentHitPoints = new();
    private readonly IRandom _random;
    
    /// <summary>
    /// Gets or sets the difficulty multiplier applied to block hit points.
    /// Increases with each respawn cycle.
    /// </summary>
    public float DifficultyMultiplier { get; private set; } = 1.0f;
    
    // Probability distribution for block types: 80% Blue, 15% Red, 5% Green
    private static readonly (BlockType type, float probability)[] _blockDistribution = 
    {
        (BlockType.CrystalBlue, 0.80f),
        (BlockType.CrystalRed, 0.15f),
        (BlockType.CrystalGreen, 0.05f)
    };

    public int Count => _blocks.Count;

    public DestructibleBlockLayer(IRandom random)
    {
        _random = random;
    }

    public BlockType GetBlock(int wx, int wy, int wz)
    {
        var key = new Vector3Int(wx, wy, wz);
        return _blocks.TryGetValue(key, out var type) ? type : BlockType.Air;
    }

    public IEnumerable<KeyValuePair<Vector3Int, BlockType>> GetAllBlocks()
    {
        return _blocks;
    }

    /// <summary>
    /// Applies damage to a block at the specified position.
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <returns>True if the block was destroyed (HP reached 0); false if still damaged but not destroyed.</returns>
    public bool DestroyBlock(Vector3 position, float damage)
    {
        var key = new Vector3Int((int)position.X, (int)position.Y, (int)position.Z);
        
        if (!_blocks.TryGetValue(key, out var blockType))
            return false; // No block at this position
        
        // Initialize HP if not yet tracked
        if (!_blockCurrentHitPoints.ContainsKey(position))
        {
            _blockCurrentHitPoints[position] = GetMaxHitPoints(blockType);
        }
        
        // Apply damage
        _blockCurrentHitPoints[position] -= damage;
        
        // Check if destroyed
        if (_blockCurrentHitPoints[position] <= 0)
        {
            _blocks.Remove(key);
            _blockCurrentHitPoints.Remove(position);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Removes a block at the specified position without damage tracking (legacy method).
    /// </summary>
    /// <param name="wx">World X coordinate.</param>
    /// <param name="wy">World Y coordinate.</param>
    /// <param name="wz">World Z coordinate.</param>
    public void RemoveBlock(int wx, int wy, int wz)
    {
        var key = new Vector3Int(wx, wy, wz);
        _blocks.Remove(key);
        _blockCurrentHitPoints.Remove(new Vector3(wx, wy, wz));
    }

    public void ClearAll()
    {
        _blocks.Clear();
        _blockCurrentHitPoints.Clear();
    }
    
    /// <summary>
    /// Gets the maximum hit points for a block type based on current difficulty.
    /// </summary>
    /// <param name="blockType">The type of block.</param>
    /// <returns>The maximum hit points scaled by difficulty multiplier.</returns>
    public float GetMaxHitPoints(BlockType blockType)
    {
        int baseHitPoints = BlockProperties.GetBaseHitPoints(blockType);
        return (float)Math.Ceiling(baseHitPoints * DifficultyMultiplier);
    }
    
    /// <summary>
    /// Gets the current hit points remaining for a block at the specified position.
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <returns>The current hit points, or 0 if the block doesn't exist.</returns>
    public float GetCurrentHitPoints(Vector3 position)
    {
        if (_blockCurrentHitPoints.TryGetValue(position, out var hp))
            return hp;
        
        var key = new Vector3Int((int)position.X, (int)position.Y, (int)position.Z);
        if (_blocks.TryGetValue(key, out var blockType))
            return GetMaxHitPoints(blockType);
        
        return 0;
    }
    
    /// <summary>
    /// Gets the damage percentage for a block (0.0 = undamaged, 1.0 = destroyed).
    /// </summary>
    /// <param name="position">The world position of the block.</param>
    /// <returns>A value from 0.0 to 1.0 representing damage level.</returns>
    public float GetDamagePercent(Vector3 position)
    {
        var key = new Vector3Int((int)position.X, (int)position.Y, (int)position.Z);
        if (!_blocks.TryGetValue(key, out var blockType))
            return 0;
        
        float maxHp = GetMaxHitPoints(blockType);
        float currentHp = GetCurrentHitPoints(position);
        
        return 1.0f - (currentHp / maxHp);
    }

    /// <summary>
    /// Spawns a pile of mineable blocks at the specified origin.
    /// Increases difficulty multiplier with each spawn.
    /// </summary>
    /// <param name="origin">The origin position for the pile.</param>
    public void AddPile(Vector3Int origin)
    {
        // Increase difficulty with each respawn
        DifficultyMultiplier += 0.25f;
        
        // Clear all damage tracking for new pile
        _blockCurrentHitPoints.Clear();
        
        // Pile dimensions: 6 width (X) × 4 height (Y) × 10 depth (Z)
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    var pos = new Vector3Int(origin.X + x, origin.Y + y, origin.Z + z);
                    var blockType = GetRandomBlockType();
                    _blocks[pos] = blockType;
                    
                    // Initialize hit points for new blocks
                    var worldPos = new Vector3(pos.X, pos.Y, pos.Z);
                    _blockCurrentHitPoints[worldPos] = GetMaxHitPoints(blockType);
                }
            }
        }
    }

    private BlockType GetRandomBlockType()
    {
        int roll = _random.Next(100); // 0-99

        if (roll < 80) return BlockType.CrystalBlue;     // 80% chance
        if (roll < 95) return BlockType.CrystalRed;      // 15% chance
        return BlockType.CrystalGreen;                   // 5% chance
    }
}

public struct Vector3Int
{
    public int X;
    public int Y;
    public int Z;

    public Vector3Int(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector3Int other)
            return X == other.X && Y == other.Y && Z == other.Z;
        return false;
    }
}
