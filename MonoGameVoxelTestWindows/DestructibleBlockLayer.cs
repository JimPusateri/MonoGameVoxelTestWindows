using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameVoxelTestWindows;

public sealed class DestructibleBlockLayer : IBlockAccessor
{
    private readonly Dictionary<Vector3Int, BlockType> _blocks = new();
    private readonly IRandom _random;
    
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

    public void RemoveBlock(int wx, int wy, int wz)
    {
        var key = new Vector3Int(wx, wy, wz);
        _blocks.Remove(key);
    }

    public void ClearAll()
    {
        _blocks.Clear();
    }

    public void AddPile(Vector3Int origin)
    {
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
