namespace MonoGameVoxelTestWindows;

public sealed class ArrayWorld : IBlockAccessor
{
    private readonly BlockType[,,] _blocks;

    public int SizeX => _blocks.GetLength(0);
    public int SizeY => _blocks.GetLength(1);
    public int SizeZ => _blocks.GetLength(2);

    public ArrayWorld(BlockType[,,] blocks)
    {
        _blocks = blocks;
    }

    public BlockType GetBlock(int wx, int wy, int wz)
    {
        if ((uint)wx >= (uint)SizeX || (uint)wy >= (uint)SizeY || (uint)wz >= (uint)SizeZ)
            return BlockType.Air;

        return _blocks[wx, wy, wz];
    }

    public void SetBlock(int wx, int wy, int wz, BlockType type)
    {
        if ((uint)wx >= (uint)SizeX || (uint)wy >= (uint)SizeY || (uint)wz >= (uint)SizeZ)
            return;

        _blocks[wx, wy, wz] = type;
    }
}
