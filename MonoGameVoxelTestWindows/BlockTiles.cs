using Microsoft.Xna.Framework;

namespace MonoGameVoxelTestWindows;

public enum BlockFace : byte { PosX, NegX, PosY, NegY, PosZ, NegZ }

public static class BlockTiles
{
    // Match these to where your tiles are in atlas.png (tile coords, not pixels).
    private static readonly (int x, int y) Dirt      = (2, 0);
    private static readonly (int x, int y) GrassTop  = (0, 0);
    private static readonly (int x, int y) GrassSide = (1, 0);
    private static readonly (int x, int y) Stone     = (2, 1);
    private static readonly (int x, int y) CrystalBlue  = (0, 1);
    private static readonly (int x, int y) CrystalRed   = (1, 1);
    private static readonly (int x, int y) CrystalGreen = (0, 2);

    public static (int tx, int ty) GetTile(BlockType type, BlockFace face) => type switch
    {
        BlockType.Dirt => Dirt,
        BlockType.Stone => Stone,
        BlockType.CrystalBlue => CrystalBlue,
        BlockType.CrystalRed => CrystalRed,
        BlockType.CrystalGreen => CrystalGreen,
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
        (0, 1) => new Color(100, 180, 255), // CrystalBlue - bright blue
        (1, 1) => new Color(255, 80, 80),   // CrystalRed - bright red
        (0, 2) => new Color(80, 255, 120),  // CrystalGreen - bright green
        _ => Color.White                     // Default
    };
}
