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
}
