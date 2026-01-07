using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public sealed class Chunk
{
    public readonly Vector3 Origin;
    private readonly TextureAtlas _atlas;


    public readonly BlockType[,,] Blocks =
        new BlockType[VoxelConstants.ChunkSize, VoxelConstants.ChunkSize, VoxelConstants.ChunkSize];

    public VertexBuffer VB;
    public IndexBuffer IB;
    public int IndexCount;
    public bool Dirty = true;

    private readonly GraphicsDevice _gd;

   public Chunk(GraphicsDevice gd, TextureAtlas atlas, int cx, int cy, int cz)
{
    _gd = gd;
    _atlas = atlas;
    Origin = new Vector3(cx * VoxelConstants.ChunkSize, cy * VoxelConstants.ChunkSize, cz * VoxelConstants.ChunkSize);
}


    public void GenerateFlat()
    {
        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        {
            if (y == 4) Blocks[x,y,z] = BlockType.Grass;
            else if (y < 4) Blocks[x,y,z] = BlockType.Dirt;
            else Blocks[x,y,z] = BlockType.Air;
        }
        Dirty = true;
    }
    public void GenerateFlat2()
    {
        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        {
            int worldY = (int)Origin.Y + y; // Origin.Y already equals cy*ChunkSize

            if (worldY == 4) Blocks[x,y,z] = BlockType.Grass;
            else if (worldY < 4) Blocks[x,y,z] = BlockType.Dirt;
            else Blocks[x,y,z] = BlockType.Air;
        }

        Dirty = true;
    }
public void GenerateFromWorld(IBlockAccessor world)
{
    for (int x = 0; x < VoxelConstants.ChunkSize; x++)
    for (int y = 0; y < VoxelConstants.ChunkSize; y++)
    for (int z = 0; z < VoxelConstants.ChunkSize; z++)
    {
        int wx = (int)Origin.X + x;
        int wy = (int)Origin.Y + y;
        int wz = (int)Origin.Z + z;

        Blocks[x,y,z] = world.GetBlock(wx, wy, wz);
    }

    Dirty = true;
}


    private BlockType GetLocal(int x, int y, int z)
    {
        if (x < 0 || x >= VoxelConstants.ChunkSize ||
            y < 0 || y >= VoxelConstants.ChunkSize ||
            z < 0 || z >= VoxelConstants.ChunkSize)
            return BlockType.Air;

        return Blocks[x,y,z];
    }

    public void RebuildMesh(IBlockAccessor world)
    {
        var verts = new List<VertexPositionColorTexture>(8192);
        var inds  = new List<ushort>(12288);

        for (int x = 0; x < VoxelConstants.ChunkSize; x++)
        for (int y = 0; y < VoxelConstants.ChunkSize; y++)
        for (int z = 0; z < VoxelConstants.ChunkSize; z++)
        {
            var t = Blocks[x,y,z];
if (t == BlockType.Air) continue;

Vector3 p = Origin + new Vector3(x, y, z);

EmitFaceIfVisible(verts, inds, p, t, x,y,z,  1,0,0, BlockFace.PosX);
EmitFaceIfVisible(verts, inds, p, t, x,y,z, -1,0,0, BlockFace.NegX);
EmitFaceIfVisible(verts, inds, p, t, x,y,z,  0,1,0, BlockFace.PosY);
EmitFaceIfVisible(verts, inds, p, t, x,y,z,  0,-1,0, BlockFace.NegY);
EmitFaceIfVisible(verts, inds, p, t, x,y,z,  0,0,1, BlockFace.PosZ);
EmitFaceIfVisible(verts, inds, p, t, x,y,z,  0,0,-1, BlockFace.NegZ);

        }

        if (verts.Count == 0)
        {
            VB?.Dispose(); IB?.Dispose();
            VB = null; IB = null; IndexCount = 0; Dirty = false;
            return;
        }

        VB?.Dispose();
        IB?.Dispose();

        VB = new VertexBuffer(_gd, VertexPositionColorTexture.VertexDeclaration, verts.Count, BufferUsage.WriteOnly);
        VB.SetData(verts.ToArray());

        IB = new IndexBuffer(_gd, IndexElementSize.SixteenBits, inds.Count, BufferUsage.WriteOnly);
        IB.SetData(inds.ToArray());

        IndexCount = inds.Count;
        Dirty = false;
    }

    private void EmitFaceIfVisible(List<VertexPositionColorTexture> verts, List<ushort> inds,
                               Vector3 p, BlockType type,
                               int x, int y, int z,
                               int nx, int ny, int nz,
                               BlockFace face)
{
    if (GetLocal(x + nx, y + ny, z + nz) != BlockType.Air) return;

    var (tx, ty) = BlockTiles.GetTile(type, face);

    _atlas.GetTileRectUV(tx, ty, out float uMin, out float vMin, out float uMax, out float vMax);

    // Bevel amount - insets faces to create rounded/chamfered edge appearance
    const float bevel = 0.08f;

    float inv = 1f / VoxelConstants.ChunkSize;

    // Normalized block extents within the chunk
    float sx0 = x * inv,     sx1 = (x + 1) * inv;
    float sy0 = y * inv,     sy1 = (y + 1) * inv;
    float sz0 = z * inv,     sz1 = (z + 1) * inv;

    // Helper: lerp inside tile rect
    Vector2 UV(float s, float t)
    {
        float u = MathHelper.Lerp(uMin, uMax, s);
        float v = MathHelper.Lerp(vMin, vMax, t);
        return new Vector2(u, v);
    }

    // Choose which axes map to (s,t) for this face
    Vector2 uv0, uv1, uv2, uv3;

    switch (face)
    {
        case BlockFace.PosY: // top: map (x,z)
        case BlockFace.NegY: // bottom: map (x,z)
            uv0 = UV(sx0, sz1);
            uv1 = UV(sx1, sz1);
            uv2 = UV(sx1, sz0);
            uv3 = UV(sx0, sz0);
            break;

        case BlockFace.PosX: // +X: map (z,y)
        case BlockFace.NegX: // -X: map (z,y)
            uv0 = UV(sz0, sy1);
            uv1 = UV(sz1, sy1);
            uv2 = UV(sz1, sy0);
            uv3 = UV(sz0, sy0);
            break;

        default: // PosZ / NegZ: map (x,y)
            uv0 = UV(sx0, sy1);
            uv1 = UV(sx1, sy1);
            uv2 = UV(sx1, sy0);
            uv3 = UV(sx0, sy0);
            break;
    }

    // Quad corners with bevel applied - insets each face to create rounded edge appearance
    Vector3 v0, v1p, v2p, v3p;
    Vector3 inset = new Vector3(nx, ny, nz) * bevel;

    if (nx == 1)      { v0=p+new Vector3(1,0,1)-inset; v1p=p+new Vector3(1,0,0)-inset; v2p=p+new Vector3(1,1,0)-inset; v3p=p+new Vector3(1,1,1)-inset; }
    else if (nx==-1)  { v0=p+new Vector3(0,0,0)-inset; v1p=p+new Vector3(0,0,1)-inset; v2p=p+new Vector3(0,1,1)-inset; v3p=p+new Vector3(0,1,0)-inset; }
    else if (ny == 1) { v0=p+new Vector3(0,1,1)-inset; v1p=p+new Vector3(1,1,1)-inset; v2p=p+new Vector3(1,1,0)-inset; v3p=p+new Vector3(0,1,0)-inset; }
    else if (ny==-1)  { v0=p+new Vector3(0,0,0)-inset; v1p=p+new Vector3(1,0,0)-inset; v2p=p+new Vector3(1,0,1)-inset; v3p=p+new Vector3(0,0,1)-inset; }
    else if (nz == 1) { v0=p+new Vector3(0,0,1)-inset; v1p=p+new Vector3(1,0,1)-inset; v2p=p+new Vector3(1,1,1)-inset; v3p=p+new Vector3(0,1,1)-inset; }
    else              { v0=p+new Vector3(1,0,0)-inset; v1p=p+new Vector3(0,0,0)-inset; v2p=p+new Vector3(0,1,0)-inset; v3p=p+new Vector3(1,1,0)-inset; }

    ushort baseIndex = (ushort)verts.Count;

    // Base face-based shading for depth
    float baseBrightness = face switch
    {
        BlockFace.PosY => 1.0f,   // Top - full brightness
        BlockFace.NegY => 0.31f,  // Bottom - very dark
        BlockFace.PosX or BlockFace.NegX => 0.71f, // Sides - medium
        _ => 0.63f                // Front/back - slightly darker
    };
    
    // Add edge darkening for curved appearance - corners darker than centers
    float EdgeDarken(Vector2 uv)
    {
        // Distance from center of face (0.5, 0.5)
        float dx = Math.Abs(uv.X - 0.5f) * 2; // 0 at center, 1 at edge
        float dy = Math.Abs(uv.Y - 0.5f) * 2;
        float edgeDist = Math.Max(dx, dy); // 0 at center, 1 at edge/corner
        
        // Darken towards edges with smooth falloff
        return 1.0f - (edgeDist * 0.3f); // 30% darker at edges
    }
    
    Color GetVertexColor(Vector2 uv)
    {
        float brightness = baseBrightness * EdgeDarken(uv);
        byte val = (byte)(brightness * 255);
        return new Color(val, val, val);
    }
    
    verts.Add(new VertexPositionColorTexture(v0,  GetVertexColor(uv0), uv0));
    verts.Add(new VertexPositionColorTexture(v1p, GetVertexColor(uv1), uv1));
    verts.Add(new VertexPositionColorTexture(v2p, GetVertexColor(uv2), uv2));
    verts.Add(new VertexPositionColorTexture(v3p, GetVertexColor(uv3), uv3));

    inds.Add((ushort)(baseIndex + 0));
    inds.Add((ushort)(baseIndex + 2));
    inds.Add((ushort)(baseIndex + 1));
    inds.Add((ushort)(baseIndex + 0));
    inds.Add((ushort)(baseIndex + 3));
    inds.Add((ushort)(baseIndex + 2));
}



    private void EmitFaceIfVisible(List<VertexPositionTexture> verts, List<ushort> inds,
                                   Vector3 p,
                                   int x, int y, int z,
                                   int nx, int ny, int nz)
    {
        if (GetLocal(x + nx, y + ny, z + nz) != BlockType.Air) return;

        // UVs for a single texture (full tile)
        Vector2 uv0 = new(0, 1);
        Vector2 uv1 = new(1, 1);
        Vector2 uv2 = new(1, 0);
        Vector2 uv3 = new(0, 0);

        // Quad corners (CCW as seen from outside)
        Vector3 v0, v1p, v2p, v3p;

        if (nx == 1)      { v0=p+new Vector3(1,0,1); v1p=p+new Vector3(1,0,0); v2p=p+new Vector3(1,1,0); v3p=p+new Vector3(1,1,1); }
        else if (nx==-1)  { v0=p+new Vector3(0,0,0); v1p=p+new Vector3(0,0,1); v2p=p+new Vector3(0,1,1); v3p=p+new Vector3(0,1,0); }
        else if (ny == 1) { v0=p+new Vector3(0,1,1); v1p=p+new Vector3(1,1,1); v2p=p+new Vector3(1,1,0); v3p=p+new Vector3(0,1,0); }
        else if (ny==-1)  { v0=p+new Vector3(0,0,0); v1p=p+new Vector3(1,0,0); v2p=p+new Vector3(1,0,1); v3p=p+new Vector3(0,0,1); }
        else if (nz == 1) { v0=p+new Vector3(0,0,1); v1p=p+new Vector3(1,0,1); v2p=p+new Vector3(1,1,1); v3p=p+new Vector3(0,1,1); }
        else              { v0=p+new Vector3(1,0,0); v1p=p+new Vector3(0,0,0); v2p=p+new Vector3(0,1,0); v3p=p+new Vector3(1,1,0); }

        ushort baseIndex = (ushort)verts.Count;

        verts.Add(new VertexPositionTexture(v0,  uv0));
        verts.Add(new VertexPositionTexture(v1p, uv1));
        verts.Add(new VertexPositionTexture(v2p, uv2));
        verts.Add(new VertexPositionTexture(v3p, uv3));

        inds.Add((ushort)(baseIndex + 0));
        inds.Add((ushort)(baseIndex + 1));
        inds.Add((ushort)(baseIndex + 2));
        inds.Add((ushort)(baseIndex + 0));
        inds.Add((ushort)(baseIndex + 2));
        inds.Add((ushort)(baseIndex + 3));
    }
}
