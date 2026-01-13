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
    private Dictionary<BlockType, Model> _blockModels;
    private Dictionary<BlockType, float> _blockModelScales;
    private Dictionary<BlockType, Vector3> _blockModelOffsets;
    private Inventory _inventory;
    private List<NpcEntity> _npcs;

    private MouseState _previousMouseState;
    private double? _respawnTimer;
    private const float RaycastDistance = 100f;
    private const double RespawnDelay = 5.0;
    private static readonly Vector3Int PileOrigin = new Vector3Int(8, 4, 8);

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
        _inventory = new Inventory();

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

        // Load block models - Add your 3D models here
        // To use this system:
        // 1. Import your .fbx/.obj box models into the Content project (Content/Models/ folder)
        // 2. Build them through the Content Pipeline
        // 3. Load them here for each BlockType you want to render
        // The models should be 1x1x1 unit boxes with beveled edges and appropriate textures
        _blockModels = new Dictionary<BlockType, Model>();
        _blockModels[BlockType.Grass] = Content.Load<Model>("models/shape-cube-rounded");
        _blockModels[BlockType.Dirt] = Content.Load<Model>("models/shape-cube-rounded");
        _blockModels[BlockType.Stone] = Content.Load<Model>("models/shape-cube-rounded");
        _blockModels[BlockType.CrystalBlue] = Content.Load<Model>("models/shape-cube-rounded");
        _blockModels[BlockType.CrystalRed] = Content.Load<Model>("models/shape-cube-rounded");
        _blockModels[BlockType.CrystalGreen] = Content.Load<Model>("models/shape-cube-rounded");

        // Calculate scale factors and offsets to fit each model into a 1x1x1 unit cube
        _blockModelScales = new Dictionary<BlockType, float>();
        _blockModelOffsets = new Dictionary<BlockType, Vector3>();
        foreach (var kvp in _blockModels)
        {
            CalculateModelTransform(kvp.Value, out float scale, out Vector3 offset);
            _blockModelScales[kvp.Key] = scale;
            _blockModelOffsets[kvp.Key] = offset;
            Console.WriteLine($"{kvp.Key}: scale={scale:F4}, offset={offset}");
        }

        int worldX = 32, worldY = 16, worldZ = 32;
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
        
        // Load and initialize NPCs
        _npcs = new List<NpcEntity>();
        Model keeperModel = Content.Load<Model>("models/character-keeper");
        CalculateModelTransform(keeperModel, out float keeperScale, out Vector3 keeperOffset);
        
        Vector3 keeperPosition = new Vector3(16f, 4.5f, 1f);
        Vector3 pilePosition = new Vector3(PileOrigin.X, PileOrigin.Y, PileOrigin.Z);
        Vector3 directionToPile = pilePosition - keeperPosition;
        float keeperRotation = (float)Math.Atan2(directionToPile.X, directionToPile.Z);
        
        var keeper = new NpcEntity(keeperModel, keeperPosition, keeperRotation, keeperScale, keeperOffset);
        _npcs.Add(keeper);
        
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

        // Update NPCs
        UpdateNpcs(gameTime);

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
        
        // Test ray against actual model bounding boxes instead of grid cells
        Ray ray = new Ray(nearPoint, rayDirection);
        float? closestDistance = null;
        Vector3Int? closestBlock = null;
        
        foreach (var chunk in _chunks)
        {
            foreach (var instance in chunk.BlockInstances)
            {
                // Only test destructible blocks
                var blockType = _destructibleLayer.GetBlock((int)instance.Position.X, (int)instance.Position.Y, (int)instance.Position.Z);
                if (blockType == BlockType.Air) continue;
                
                // Create bounding box for this model instance
                float scale = _blockModelScales[instance.Type];
                Vector3 offset = _blockModelOffsets[instance.Type];
                Vector3 cellCenter = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 worldPos = offset * scale + cellCenter + instance.Position;
                
                // Create a 1x1x1 box centered at worldPos
                BoundingBox box = new BoundingBox(worldPos - new Vector3(0.5f), worldPos + new Vector3(0.5f));
                
                float? distance = ray.Intersects(box);
                if (distance.HasValue && distance.Value <= RaycastDistance)
                {
                    if (!closestDistance.HasValue || distance.Value < closestDistance.Value)
                    {
                        closestDistance = distance;
                        closestBlock = new Vector3Int((int)instance.Position.X, (int)instance.Position.Y, (int)instance.Position.Z);
                    }
                }
            }
        }
        
        if (closestBlock.HasValue)
        {
            var hitBlock = closestBlock.Value;
            Console.WriteLine($"Hit destructible block at: {hitBlock.X}, {hitBlock.Y}, {hitBlock.Z}");
            var blockType = _destructibleLayer.GetBlock(hitBlock.X, hitBlock.Y, hitBlock.Z);
            Console.WriteLine($"Block type: {blockType}");
            
            // Add to inventory
            _inventory.AddBlock(blockType);
            
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

    private void UpdateNpcs(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var npc in _npcs)
        {
            // Check if NPC should return to start (collected 5 blocks)
            if (!npc.ReturningToStart && npc.CollectedBlocks.Count >= NpcEntity.MaxCollectedBlocks)
            {
                npc.ReturningToStart = true;
                npc.TargetBlock = null;
            }

            if (npc.ReturningToStart)
            {
                // Move back to starting position (X,Z plane only)
                Vector3 targetPos = new Vector3(npc.StartingPosition.X, npc.Position.Y, npc.StartingPosition.Z);
                Vector3 direction = targetPos - npc.Position;
                direction.Y = 0; // Constrain to X,Z plane
                float distanceXZ = new Vector2(direction.X, direction.Z).Length();

                if (distanceXZ <= 0.5f)
                {
                    // Arrived at start - transfer collected blocks to inventory
                    foreach (var blockType in npc.CollectedBlocks)
                    {
                        _inventory.AddBlock(blockType);
                    }
                    npc.CollectedBlocks.Clear();
                    npc.ReturningToStart = false;
                }
                else
                {
                    // Move towards start
                    direction.Normalize();
                    npc.Position += direction * npc.MoveSpeed * deltaTime;
                    npc.Rotation = (float)Math.Atan2(direction.X, direction.Z);
                }
            }
            else
            {
                // Find nearest destructible block if we don't have a target
                if (!npc.TargetBlock.HasValue || _destructibleLayer.GetBlock(npc.TargetBlock.Value.X, npc.TargetBlock.Value.Y, npc.TargetBlock.Value.Z) == BlockType.Air)
                {
                    npc.TargetBlock = FindNearestDestructibleBlock(npc.Position);
                }

                // If we have a target, move towards it
                if (npc.TargetBlock.HasValue)
                {
                    Vector3 targetCenter = new Vector3(
                        npc.TargetBlock.Value.X + 0.5f,
                        npc.TargetBlock.Value.Y + 0.5f,
                        npc.TargetBlock.Value.Z + 0.5f
                    );

                    // Calculate distance in X,Z plane only
                    Vector3 direction = targetCenter - npc.Position;
                    direction.Y = 0; // Ignore height difference
                    float distanceXZ = direction.Length();

                    // Check if close enough to destroy (X,Z distance only)
                    if (distanceXZ <= npc.DestroyRange)
                    {
                        // Destroy the block and add to NPC's collected blocks
                        var blockType = _destructibleLayer.GetBlock(npc.TargetBlock.Value.X, npc.TargetBlock.Value.Y, npc.TargetBlock.Value.Z);
                        npc.CollectedBlocks.Add(blockType);
                        _destructibleLayer.RemoveBlock(npc.TargetBlock.Value.X, npc.TargetBlock.Value.Y, npc.TargetBlock.Value.Z);
                        MarkChunkDirtyAt(npc.TargetBlock.Value.X, npc.TargetBlock.Value.Y, npc.TargetBlock.Value.Z);

                        // Clear target to find next block
                        npc.TargetBlock = null;

                        // Check if all blocks destroyed
                        if (_destructibleLayer.Count == 0 && !_respawnTimer.HasValue)
                        {
                            _respawnTimer = gameTime.TotalGameTime.TotalSeconds + RespawnDelay;
                        }
                    }
                    else
                    {
                        // Move towards target (X,Z plane only)
                        direction.Normalize();
                        npc.Position += direction * npc.MoveSpeed * deltaTime;

                        // Update rotation to face target (XZ plane only)
                        npc.Rotation = (float)Math.Atan2(direction.X, direction.Z);
                    }
                }
            }
        }
    }

    private Vector3Int? FindNearestDestructibleBlock(Vector3 fromPosition)
    {
        Vector3Int? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var kvp in _destructibleLayer.GetAllBlocks())
        {
            Vector3 blockCenter = new Vector3(kvp.Key.X + 0.5f, kvp.Key.Y + 0.5f, kvp.Key.Z + 0.5f);
            // Only consider X,Z distance (ignore height)
            float distanceXZ = new Vector2(blockCenter.X - fromPosition.X, blockCenter.Z - fromPosition.Z).Length();

            if (distanceXZ < nearestDistance)
            {
                nearestDistance = distanceXZ;
                nearest = kvp.Key;
            }
        }

        return nearest;
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

        // Draw models for each block instance
        foreach (var chunk in _chunks)
        {
            foreach (var instance in chunk.BlockInstances)
            {
                if (!_blockModels.TryGetValue(instance.Type, out var model)) continue;

                foreach (var mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        // Scale, apply offset to center model at origin, then position at cell center
                        float scale = _blockModelScales[instance.Type];
                        Vector3 offset = _blockModelOffsets[instance.Type];
                        
                        // Cell center relative to grid corner
                        Vector3 cellCenter = new Vector3(0.5f, 0.5f, 0.5f);
                        
                        // Apply transforms: scale first, then translate (offset centers at origin, cellCenter+position moves to world)
                        effect.World = Matrix.CreateScale(scale) * 
                                      Matrix.CreateTranslation(offset) * 
                                      Matrix.CreateTranslation(cellCenter + instance.Position);
                        effect.View = _camera.View;
                        effect.Projection = _camera.Projection;
                        effect.TextureEnabled = true;
                        effect.LightingEnabled = true;
                        effect.EnableDefaultLighting();
                    }
                    mesh.Draw();
                }
            }
        }
        
        // Draw NPCs
        foreach (var npc in _npcs)
        {
            foreach (var mesh in npc.Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Apply transforms: Scale → Offset → Rotation → Position
                    effect.World = Matrix.CreateScale(npc.Scale) * 
                                  Matrix.CreateTranslation(npc.ModelOffset) *
                                  Matrix.CreateRotationY(npc.Rotation) *
                                  Matrix.CreateTranslation(npc.Position);
                    effect.View = _camera.View;
                    effect.Projection = _camera.Projection;
                    effect.TextureEnabled = true;
                    effect.LightingEnabled = true;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
        
        DrawDebugHud();
        DrawInventory();

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

    private void DrawInventory()
    {
        // Display inventory at top-right corner
        int blueCount = _inventory.GetCount(BlockType.CrystalBlue);
        int redCount = _inventory.GetCount(BlockType.CrystalRed);
        int greenCount = _inventory.GetCount(BlockType.CrystalGreen);

        string inventoryText =
            $"Inventory:\n" +
            $"Blue:  {blueCount}\n" +
            $"Red:   {redCount}\n" +
            $"Green: {greenCount}";

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.DrawString(_debugFont, inventoryText, new Vector2(1400, 12), Color.White);
        _spriteBatch.End();
    }

    private void CalculateModelTransform(Model model, out float scale, out Vector3 offset)
    {
        // Calculate the actual bounding box by examining all vertices
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);

        foreach (var mesh in model.Meshes)
        {
            // Get vertex data from the mesh
            foreach (var meshPart in mesh.MeshParts)
            {
                var vertexBuffer = meshPart.VertexBuffer;
                var vertexData = new byte[vertexBuffer.VertexCount * vertexBuffer.VertexDeclaration.VertexStride];
                vertexBuffer.GetData(vertexData);

                // Parse positions (assuming first 3 floats are position)
                for (int i = 0; i < vertexBuffer.VertexCount; i++)
                {
                    int dataOffset = i * vertexBuffer.VertexDeclaration.VertexStride;
                    float x = BitConverter.ToSingle(vertexData, dataOffset);
                    float y = BitConverter.ToSingle(vertexData, dataOffset + 4);
                    float z = BitConverter.ToSingle(vertexData, dataOffset + 8);

                    Vector3 pos = new Vector3(x, y, z);
                    min = Vector3.Min(min, pos);
                    max = Vector3.Max(max, pos);
                }
            }
        }

        // Calculate center and size of the bounding box
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
        
        // Find the largest dimension
        float maxDimension = Math.Max(Math.Max(size.X, size.Y), size.Z);
        
        // Scale to fit in a 1x1x1 unit cube
        scale = maxDimension > 0 ? 1.0f / maxDimension : 1.0f;
        
        // Offset to center the model at origin after scaling
        offset = -center * scale;
    }

}
