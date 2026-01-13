using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class TextureAtlas
{
    public Texture2D Texture { get; }
    public int TileSizePx { get; }

    private readonly float _invW;
    private readonly float _invH;
    private readonly float _insetPx;

    public TextureAtlas(Texture2D texture, int tileSizePx, float insetPx = 0f)
    {
        Texture = texture;
        TileSizePx = tileSizePx;
        _invW = 1f / texture.Width;
        _invH = 1f / texture.Height;
        _insetPx = insetPx;
    }

    // Returns UVs matching the vertex order: v0,v1,v2,v3
    public void GetFaceUVs(int tileX, int tileY, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2, out Vector2 uv3)
    {
        float x0 = tileX * TileSizePx + _insetPx;
        float y0 = tileY * TileSizePx + _insetPx;
        float x1 = (tileX + 1) * TileSizePx - _insetPx;
        float y1 = (tileY + 1) * TileSizePx - _insetPx;

        float u0 = x0 * _invW;
        float v0 = y0 * _invH;
        float u1 = x1 * _invW;
        float v1 = y1 * _invH;

        // These are arranged so the texture isn't upside-down given the quad ordering below.
        uv0 = new Vector2(u0, v1); // bottom-left
        uv1 = new Vector2(u1, v1); // bottom-right
        uv2 = new Vector2(u1, v0); // top-right
        uv3 = new Vector2(u0, v0); // top-left
    }
    public void GetTileRectUV(int tileX, int tileY, out float uMin, out float vMin, out float uMax, out float vMax)
{
    float x0 = tileX * TileSizePx + _insetPx;
    float y0 = tileY * TileSizePx + _insetPx;
    float x1 = (tileX + 1) * TileSizePx - _insetPx;
    float y1 = (tileY + 1) * TileSizePx - _insetPx;

    uMin = x0 * _invW;
    vMin = y0 * _invH;
    uMax = x1 * _invW;
    vMax = y1 * _invH;
}

    // Create a texture atlas programmatically with solid colors
    public static TextureAtlas CreateColorAtlas(GraphicsDevice device, int tileSizePx, int tilesX, int tilesY, Func<int, int, Color> colorSelector)
    {
        int width = tileSizePx * tilesX;
        int height = tileSizePx * tilesY;
        var texture = new Texture2D(device, width, height);
        
        Color[] pixels = new Color[width * height];
        
        for (int ty = 0; ty < tilesY; ty++)
        {
            for (int tx = 0; tx < tilesX; tx++)
            {
                Color tileColor = colorSelector(tx, ty);
                
                // Fill the tile with solid color
                for (int py = 0; py < tileSizePx; py++)
                {
                    for (int px = 0; px < tileSizePx; px++)
                    {
                        int x = tx * tileSizePx + px;
                        int y = ty * tileSizePx + py;
                        pixels[y * width + x] = tileColor;
                    }
                }
            }
        }
        
        texture.SetData(pixels);
        return new TextureAtlas(texture, tileSizePx, insetPx: 0f);
    }

}
