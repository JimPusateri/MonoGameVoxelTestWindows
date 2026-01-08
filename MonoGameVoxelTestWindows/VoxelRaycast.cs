using System;
using Microsoft.Xna.Framework;

public static class VoxelRaycast
{
    /// <summary>
    /// Performs a DDA raycast through the voxel grid to find the first solid block hit.
    /// </summary>
    /// <param name="accessor">Block accessor to query blocks</param>
    /// <param name="origin">Ray origin in world space</param>
    /// <param name="direction">Ray direction (should be normalized)</param>
    /// <param name="maxDistance">Maximum raycast distance</param>
    /// <param name="hitBlock">The world coordinates of the hit block</param>
    /// <returns>True if a solid block was hit, false otherwise</returns>
    public static bool Raycast(IBlockAccessor accessor, Vector3 origin, Vector3 direction, float maxDistance, out Vector3Int hitBlock)
    {
        hitBlock = new Vector3Int();
        
        // Normalize direction
        direction.Normalize();
        
        // Current voxel coordinates
        int x = (int)Math.Floor(origin.X);
        int y = (int)Math.Floor(origin.Y);
        int z = (int)Math.Floor(origin.Z);
        
        // Direction signs
        int stepX = Math.Sign(direction.X);
        int stepY = Math.Sign(direction.Y);
        int stepZ = Math.Sign(direction.Z);
        
        // Distances to next voxel boundary
        float tMaxX = direction.X != 0 ? IntBound(origin.X, direction.X) / Math.Abs(direction.X) : float.MaxValue;
        float tMaxY = direction.Y != 0 ? IntBound(origin.Y, direction.Y) / Math.Abs(direction.Y) : float.MaxValue;
        float tMaxZ = direction.Z != 0 ? IntBound(origin.Z, direction.Z) / Math.Abs(direction.Z) : float.MaxValue;
        
        // Distance to traverse one voxel along each axis
        float tDeltaX = direction.X != 0 ? stepX / direction.X : float.MaxValue;
        float tDeltaY = direction.Y != 0 ? stepY / direction.Y : float.MaxValue;
        float tDeltaZ = direction.Z != 0 ? stepZ / direction.Z : float.MaxValue;
        
        float t = 0;
        
        while (t <= maxDistance)
        {
            // Check current block
            var blockType = accessor.GetBlock(x, y, z);
            if (blockType != BlockType.Air)
            {
                hitBlock = new Vector3Int(x, y, z);
                return true;
            }
            
            // Advance to next voxel
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    x += stepX;
                    t = tMaxX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    z += stepZ;
                    t = tMaxZ;
                    tMaxZ += tDeltaZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    y += stepY;
                    t = tMaxY;
                    tMaxY += tDeltaY;
                }
                else
                {
                    z += stepZ;
                    t = tMaxZ;
                    tMaxZ += tDeltaZ;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate distance to next integer boundary
    /// </summary>
    private static float IntBound(float s, float ds)
    {
        if (ds < 0)
        {
            return IntBound(-s, -ds);
        }
        else
        {
            s = Mod(s, 1);
            return (1 - s) / ds;
        }
    }
    
    private static float Mod(float value, float modulus)
    {
        return (value % modulus + modulus) % modulus;
    }
}
