using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameVoxelTestWindows;

public class MonoGameCameraInput : ICameraInput
{
    private readonly Game _game;
    private Point _lastMousePos;

    public MonoGameCameraInput(Game game)
    {
        _game = game;
        var ms = Mouse.GetState();
        _lastMousePos = new Point(ms.X, ms.Y);
    }

    public bool IsKeyDown(Keys key)
    {
        return Keyboard.GetState().IsKeyDown(key);
    }

    public Point GetMouseDelta()
    {
        var ms = Mouse.GetState();
        var currentPos = new Point(ms.X, ms.Y);
        var delta = currentPos - _lastMousePos;
        _lastMousePos = currentPos;
        return delta;
    }

    public bool IsWindowFocused()
    {
        return _game.IsActive;
    }
}
