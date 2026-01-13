using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameVoxelTestWindows;

public interface ICameraInput
{
    bool IsKeyDown(Keys key);
    Point GetMouseDelta();
    bool IsWindowFocused();
}
