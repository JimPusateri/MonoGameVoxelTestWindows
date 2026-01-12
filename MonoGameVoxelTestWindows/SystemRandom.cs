using System;

namespace MonoGameVoxelTestWindows;

public class SystemRandom : IRandom
{
    private readonly Random _random;

    public SystemRandom(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }
}
