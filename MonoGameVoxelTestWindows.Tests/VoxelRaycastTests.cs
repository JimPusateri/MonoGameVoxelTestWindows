using Xunit;
using Microsoft.Xna.Framework;
using MonoGameVoxelTestWindows;

namespace MonoGameVoxelTestWindows.Tests;

public class VoxelRaycastTests
{
    [Fact]
    public void Raycast_HitsSolidBlock()
    {
        var blocks = new BlockType[20, 20, 20];
        blocks[10, 10, 10] = BlockType.Stone;
        var world = new ArrayWorld(blocks);
        
        var origin = new Vector3(5, 10, 10);
        var direction = Vector3.Normalize(new Vector3(1, 0, 0));
        
        var hit = VoxelRaycast.Raycast(world, origin, direction, 100f, out var hitPos);
        
        Assert.True(hit);
        Assert.Equal(10, hitPos.X);
        Assert.Equal(10, hitPos.Y);
        Assert.Equal(10, hitPos.Z);
    }

    [Fact]
    public void Raycast_ReturnsNoHitWhenNoBlocks()
    {
        var blocks = new BlockType[20, 20, 20];
        var world = new ArrayWorld(blocks);
        
        var origin = new Vector3(0, 0, 0);
        var direction = Vector3.Normalize(new Vector3(1, 1, 1));
        
        var hit = VoxelRaycast.Raycast(world, origin, direction, 100f, out var hitPos);
        
        Assert.False(hit);
    }

    [Fact]
    public void Raycast_RespectsMaxDistance()
    {
        var blocks = new BlockType[200, 200, 200];
        blocks[150, 0, 0] = BlockType.Stone;
        var world = new ArrayWorld(blocks);
        
        var origin = new Vector3(0, 0, 0);
        var direction = new Vector3(1, 0, 0);
        
        var hit = VoxelRaycast.Raycast(world, origin, direction, 50f, out var hitPos);
        
        Assert.False(hit);
    }

    [Fact]
    public void Raycast_IgnoresAirBlocks()
    {
        var blocks = new BlockType[20, 20, 20];
        blocks[5, 5, 5] = BlockType.Air;
        blocks[10, 5, 5] = BlockType.Stone;
        var world = new ArrayWorld(blocks);
        
        var origin = new Vector3(0, 5, 5);
        var direction = new Vector3(1, 0, 0);
        
        var hit = VoxelRaycast.Raycast(world, origin, direction, 100f, out var hitPos);
        
        Assert.True(hit);
        Assert.Equal(10, hitPos.X);
    }
}
