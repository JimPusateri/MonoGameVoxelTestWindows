using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameVoxelTestWindows;

public sealed class FpsCamera
{
    public Vector3 Position = new(-5, 24, -6);
    public float Yaw = -(float)Math.PI/6*11;
    public float Pitch = -(float)Math.PI/6;

    public Matrix View { get; private set; }
    public Matrix Projection { get; }

    private readonly GraphicsDevice _gd;

    public FpsCamera(GraphicsDevice gd)
    {
        _gd = gd;
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(70f),
            gd.Viewport.AspectRatio,
            0.1f,
            800f
        );
        RecalcView();
    }

    public void CenterMouse()
    {
        var vp = _gd.Viewport;
        Mouse.SetPosition(vp.X + vp.Width / 2, vp.Y + vp.Height / 2);
    }

    public void Update(GameTime gameTime, ICameraInput input)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float speed = 20f;

        Vector3 forward = Forward();
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.Up));

        if (input.IsKeyDown(Keys.W)) Position += forward * speed * dt;
        if (input.IsKeyDown(Keys.S)) Position -= forward * speed * dt;
        if (input.IsKeyDown(Keys.A)) Position -= right   * speed * dt;
        if (input.IsKeyDown(Keys.D)) Position += right   * speed * dt;
        if (input.IsKeyDown(Keys.Space)) Position += Vector3.Up * speed * dt;
        if (input.IsKeyDown(Keys.LeftControl)) Position -= Vector3.Up * speed * dt;

        RecalcView();
    }

    public Vector3 Forward()
    {
        // Forward vector from yaw/pitch
        return Vector3.Normalize(new Vector3(
            (float)(System.Math.Cos(Pitch) * System.Math.Sin(Yaw)),
            (float)(System.Math.Sin(Pitch)),
            (float)(System.Math.Cos(Pitch) * System.Math.Cos(Yaw))
        ));
    }

    private void RecalcView()
    {
        var f = Forward();
        View = Matrix.CreateLookAt(Position, Position + f, Vector3.Up);
    }
}
