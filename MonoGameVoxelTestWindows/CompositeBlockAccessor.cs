public sealed class CompositeBlockAccessor : IBlockAccessor
{
    private readonly DestructibleBlockLayer _destructibleLayer;
    private readonly IBlockAccessor _worldLayer;

    public CompositeBlockAccessor(DestructibleBlockLayer destructibleLayer, IBlockAccessor worldLayer)
    {
        _destructibleLayer = destructibleLayer;
        _worldLayer = worldLayer;
    }

    public BlockType GetBlock(int wx, int wy, int wz)
    {
        // Check destructible layer first
        var destructibleBlock = _destructibleLayer.GetBlock(wx, wy, wz);
        if (destructibleBlock != BlockType.Air)
            return destructibleBlock;

        // Fall back to world layer
        return _worldLayer.GetBlock(wx, wy, wz);
    }
}
