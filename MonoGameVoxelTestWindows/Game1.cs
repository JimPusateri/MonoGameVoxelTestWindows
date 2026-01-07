using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameVoxelTestWindows;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private BasicEffect _effect;
    private FpsCamera _camera;
    private IBlockAccessor _world;
    private SpriteBatch _spriteBatch;
    private SpriteFont _debugFont;


    private readonly List<Chunk> _chunks = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugFont = Content.Load<SpriteFont>("DebugFont");

        _camera = new FpsCamera(GraphicsDevice);
        _camera.CenterMouse();

        // Create a color-based texture atlas procedurally (3x2 tiles, 16px each)
        var atlas = TextureAtlas.CreateColorAtlas(
            GraphicsDevice, 
            tileSizePx: 16, 
            tilesX: 3, 
            tilesY: 2, 
            BlockTiles.GetTileColor
        );

        _effect = new BasicEffect(GraphicsDevice)
        {
            World = Matrix.Identity,
            TextureEnabled = true,
            Texture = atlas.Texture,
            LightingEnabled = false,
            VertexColorEnabled = true
        };

        // important for crisp pixels
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        int worldX = 64, worldY = 32, worldZ = 128;
        var blocks = new BlockType[worldX, worldY, worldZ];

        // Example: fill a 6-high dirt base
        for (int x = 0; x < worldX; x++)
            for (int z = 0; z < worldZ; z++)
            //half a block of dirt
            for (int y = 0; y < VoxelConstants.ChunkSize/2; y++)
                    blocks[x, y, z] = BlockType.Dirt;
        for (int x = 0; x < worldX; x++)
            for (int z = 0; z < worldZ; z++)
            //half a block of grass
            for (int y = VoxelConstants.ChunkSize/2; y < VoxelConstants.ChunkSize; y++)
                    blocks[x, y, z] = BlockType.Grass;

        
        for (int x = worldX-2; x < worldX; x++)
        for (int z = 0; z < worldX; z++)
            for (int y = 0; y < worldY; y++)
                blocks[x, y, z] = BlockType.Stone;

        _world = new ArrayWorld(blocks);
        int chunksX = (worldX + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;
        int chunksY = (worldY + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;
        int chunksZ = (worldZ + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;

        for (int cz = 0; cz < chunksZ; cz++)
            for (int cy = 0; cy < chunksY; cy++)
                for (int cx = 0; cx < chunksX; cx++)
                {
                    var chunk = new Chunk(GraphicsDevice, atlas, cx, cy, cz);
                    chunk.GenerateFromWorld(_world);
                    chunk.RebuildMesh(_world);
                    _chunks.Add(chunk);
                }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        bool windowActive = IsActive;
        _camera.Update(gameTime, windowActive);

        // Rebuild any dirty chunks
        for (int i = 0; i < _chunks.Count; i++)
            if (_chunks[i].Dirty)
                _chunks[i].RebuildMesh(_world);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp; // pixelated


        _effect.View = _camera.View;
        _effect.Projection = _camera.Projection;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            foreach (var c in _chunks)
            {
                if (c.VB == null || c.IB == null || c.IndexCount == 0) continue;

                GraphicsDevice.SetVertexBuffer(c.VB);
                GraphicsDevice.Indices = c.IB;

                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    baseVertex: 0,
                    startIndex: 0,
                    primitiveCount: c.IndexCount / 3
                );
            }
        }
        DrawDebugHud();

        base.Draw(gameTime);
    }
    private void DrawDebugHud()
    {
        // If you use IsMouseVisible=false and recentering mouse, this still works.
        // Draw AFTER 3D so HUD is always on top.

        var pos = _camera.Position;

        // Convert to degrees for readability
        float yawDeg = MathHelper.ToDegrees(_camera.Yaw);
        float pitchDeg = MathHelper.ToDegrees(_camera.Pitch);

        var fwd = _camera.Forward();

        string text =
            $"Pos:  X={pos.X:0.00}  Y={pos.Y:0.00}  Z={pos.Z:0.00}\n" +
            $"Yaw:  {yawDeg:0.00}   Pitch: {pitchDeg:0.00}\n" +
            $"Fwd:  X={fwd.X:0.00}  Y={fwd.Y:0.00}  Z={fwd.Z:0.00}";

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.DrawString(_debugFont, text, new Vector2(12, 12), Color.White);
        _spriteBatch.End();
    }

}
