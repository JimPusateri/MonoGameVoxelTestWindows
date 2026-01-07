using Microsoft.Xna.Framework;

public enum BlockFace : byte { PosX, NegX, PosY, NegY, PosZ, NegZ }

public static class BlockTiles
{
    // Match these to where your tiles are in atlas.png (tile coords, not pixels).
    private static readonly (int x, int y) Dirt      = (2, 0);
    private static readonly (int x, int y) GrassTop  = (0, 0);
    private static readonly (int x, int y) GrassSide = (1, 0);
    private static readonly (int x, int y) Stone     = (2, 1);

    public static (int tx, int ty) GetTile(BlockType type, BlockFace face) => type switch
    {
        BlockType.Dirt => Dirt,
        BlockType.Stone => Stone,
        BlockType.Grass => face switch
        {
            BlockFace.PosY => GrassTop,
            BlockFace.NegY => Dirt,
            _ => GrassSide
        },
        _ => Dirt
    };

    // Define colors for each tile position in the atlas
    public static Color GetTileColor(int tileX, int tileY) => (tileX, tileY) switch
    {
        (0, 0) => new Color(95, 168, 60),   // GrassTop - bright green
        (1, 0) => new Color(115, 140, 70),  // GrassSide - olive green
        (2, 0) => new Color(139, 111, 71),  // Dirt - brown
        (2, 1) => new Color(128, 128, 128), // Stone - gray
        _ => Color.White                     // Default
    };
}
