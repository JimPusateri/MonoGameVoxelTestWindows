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
    private ArrayWorld _world;
    private DestructibleBlockLayer _destructibleLayer;
    private CompositeBlockAccessor _compositeAccessor;
    private SpriteBatch _spriteBatch;
    private SpriteFont _debugFont;

    private MouseState _previousMouseState;
    private double? _respawnTimer;
    private const float RaycastDistance = 100f;
    private const double RespawnDelay = 5.0;
    private static readonly Vector3Int PileOrigin = new Vector3Int(29, 4, 64);

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
        
        // Initialize destructible layer and composite accessor
        _destructibleLayer = new DestructibleBlockLayer();
        _compositeAccessor = new CompositeBlockAccessor(_destructibleLayer, _world);
        
        // Spawn initial pile
        _destructibleLayer.AddPile(PileOrigin);
        
        int chunksX = (worldX + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;
        int chunksY = (worldY + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;
        int chunksZ = (worldZ + VoxelConstants.ChunkSize - 1) / VoxelConstants.ChunkSize;

        for (int cz = 0; cz < chunksZ; cz++)
            for (int cy = 0; cy < chunksY; cy++)
                for (int cx = 0; cx < chunksX; cx++)
                {
                    var chunk = new Chunk(GraphicsDevice, atlas, cx, cy, cz);
                    chunk.GenerateFromWorld(_compositeAccessor);
                    chunk.RebuildMesh(_compositeAccessor);
                    _chunks.Add(chunk);
                }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        bool windowActive = IsActive;
        _camera.Update(gameTime, windowActive);

        // Handle block destruction on left mouse click
        var currentMouseState = Mouse.GetState();
        if (windowActive && currentMouseState.LeftButton == ButtonState.Pressed && 
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            Console.WriteLine($"Mouse clicked! Camera pos: {_camera.Position}, Forward: {_camera.Forward()}");
            TryDestroyBlock(gameTime);
        }
        _previousMouseState = currentMouseState;

        // Handle respawn timer
        if (_respawnTimer.HasValue)
        {
            if (gameTime.TotalGameTime.TotalSeconds >= _respawnTimer.Value)
            {
                // Respawn pile
                _destructibleLayer.AddPile(PileOrigin);
                _respawnTimer = null;
                
                // Mark affected chunks dirty
                MarkChunksDirtyInRegion(PileOrigin.X, PileOrigin.Y, PileOrigin.Z, 6, 4, 10);
            }
        }

        // Rebuild any dirty chunks
        for (int i = 0; i < _chunks.Count; i++)
        {
            if (_chunks[i].Dirty)
            {
                _chunks[i].GenerateFromWorld(_compositeAccessor);
                _chunks[i].RebuildMesh(_compositeAccessor);
            }
        }

        base.Update(gameTime);
    }

    private void TryDestroyBlock(GameTime gameTime)
    {
        // Get mouse position and convert to world ray
        var mouseState = Mouse.GetState();
        
        // Unproject near and far points to get the ray in world space
        Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(
            new Vector3(mouseState.X, mouseState.Y, 0),
            _camera.Projection,
            _camera.View,
            Matrix.Identity);
        Vector3 farPoint = GraphicsDevice.Viewport.Unproject(
            new Vector3(mouseState.X, mouseState.Y, 1),
            _camera.Projection,
            _camera.View,
            Matrix.Identity);
        
        Vector3 rayDirection = Vector3.Normalize(farPoint - nearPoint);
        
        Console.WriteLine($"TryDestroyBlock called - Mouse: ({mouseState.X}, {mouseState.Y})");
        Console.WriteLine($"Ray origin: {nearPoint}, direction: {rayDirection}");
        
        // Raycast specifically for destructible blocks only
        if (VoxelRaycast.Raycast(_destructibleLayer, nearPoint, rayDirection, 
            RaycastDistance, out var hitBlock))
        {
            Console.WriteLine($"Hit destructible block at: {hitBlock.X}, {hitBlock.Y}, {hitBlock.Z}");
            var blockType = _destructibleLayer.GetBlock(hitBlock.X, hitBlock.Y, hitBlock.Z);
            Console.WriteLine($"Block type: {blockType}");
            
            // Destroy block
            Console.WriteLine("Destroying block!");
            _destructibleLayer.RemoveBlock(hitBlock.X, hitBlock.Y, hitBlock.Z);
            
            // Mark affected chunks dirty
            MarkChunkDirtyAt(hitBlock.X, hitBlock.Y, hitBlock.Z);
            
            // If all blocks destroyed, start respawn timer
            if (_destructibleLayer.Count == 0 && !_respawnTimer.HasValue)
            {
                Console.WriteLine("All blocks destroyed, starting respawn timer");
                _respawnTimer = gameTime.TotalGameTime.TotalSeconds + RespawnDelay;
            }
        }
        else
        {
            Console.WriteLine("Raycast missed - no destructible block hit");
        }
    }

    private void MarkChunkDirtyAt(int wx, int wy, int wz)
    {
        int cx = wx / VoxelConstants.ChunkSize;
        int cy = wy / VoxelConstants.ChunkSize;
        int cz = wz / VoxelConstants.ChunkSize;
        
        foreach (var chunk in _chunks)
        {
            if (chunk.ChunkX == cx && chunk.ChunkY == cy && chunk.ChunkZ == cz)
            {
                chunk.Dirty = true;
                break;
            }
        }
        
        // Also mark neighboring chunks if on boundary
        if (wx % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx - 1, cy, cz);
        if ((wx + 1) % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx + 1, cy, cz);
        if (wy % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx, cy - 1, cz);
        if ((wy + 1) % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx, cy + 1, cz);
        if (wz % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx, cy, cz - 1);
        if ((wz + 1) % VoxelConstants.ChunkSize == 0) MarkChunkDirtyAtCoord(cx, cy, cz + 1);
    }

    private void MarkChunkDirtyAtCoord(int cx, int cy, int cz)
    {
        foreach (var chunk in _chunks)
        {
            if (chunk.ChunkX == cx && chunk.ChunkY == cy && chunk.ChunkZ == cz)
            {
                chunk.Dirty = true;
                break;
            }
        }
    }

    private void MarkChunksDirtyInRegion(int x, int y, int z, int width, int height, int depth)
    {
        int minCx = x / VoxelConstants.ChunkSize;
        int maxCx = (x + width - 1) / VoxelConstants.ChunkSize;
        int minCy = y / VoxelConstants.ChunkSize;
        int maxCy = (y + height - 1) / VoxelConstants.ChunkSize;
        int minCz = z / VoxelConstants.ChunkSize;
        int maxCz = (z + depth - 1) / VoxelConstants.ChunkSize;
        
        for (int cx = minCx; cx <= maxCx; cx++)
            for (int cy = minCy; cy <= maxCy; cy++)
                for (int cz = minCz; cz <= maxCz; cz++)
                    MarkChunkDirtyAtCoord(cx, cy, cz);
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
