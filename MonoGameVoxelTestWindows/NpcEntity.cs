using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameVoxelTestWindows;

public class NpcEntity
{
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }
    public Model Model { get; set; }
    public float Scale { get; set; }
    public Vector3 ModelOffset { get; set; }
    public Vector3Int? TargetBlock { get; set; }
    public float MoveSpeed { get; set; } = 2.0f;
    public float DestroyRange { get; set; } = 1.5f;
    public Vector3 StartingPosition { get; set; }
    public List<BlockType> CollectedBlocks { get; set; } = new List<BlockType>();
    public bool ReturningToStart { get; set; } = false;
    public const int MaxCollectedBlocks = 5;

    public NpcEntity(Model model, Vector3 position, float rotation, float scale, Vector3 modelOffset)
    {
        Model = model;
        Position = position;
        Rotation = rotation;
        Scale = scale;
        ModelOffset = modelOffset;
        StartingPosition = position;
    }
}
