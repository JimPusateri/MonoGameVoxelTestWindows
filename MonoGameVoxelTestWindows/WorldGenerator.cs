public sealed class WorldGenerator : IBlockAccessor
{
    public BlockType GetBlock(int wx, int wy, int wz)
    {
        // Example: a stone "tower" at x=10..12, z=10..12 for all y 0..40
        if (wx >= 10 && wx <= 12 && wz >= 10 && wz <= 12 && wy >= 0 && wy <= 40)
            return BlockType.Stone;

        // Example: a dirt slab region
        if (wx >= 0 && wx < 64 && wz >= 0 && wz < 64 && wy >= 0 && wy < 6)
            return BlockType.Dirt;

        // Example: a floating grass platform at y=20
        if (wy == 20 && wx >= 5 && wx <= 30 && wz >= 5 && wz <= 30)
            return BlockType.Grass;

        return BlockType.Stone;
    }
}
