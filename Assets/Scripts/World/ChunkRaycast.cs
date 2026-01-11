using UnityEngine;

/// <summary>
/// Performs raycast against voxel world using DDA (Digital Differential Analyzer) algorithm
/// More efficient than physics raycast for voxel worlds
/// </summary>
public static class ChunkRaycast
{
    /// <summary>
    /// Cast a ray through the voxel world and find the first solid block hit
    /// </summary>
    /// <param name="origin">Ray origin in world coordinates</param>
    /// <param name="direction">Ray direction (will be normalized)</param>
    /// <param name="maxDistance">Maximum raycast distance</param>
    /// <param name="hitBlock">Output: world position of hit block</param>
    /// <param name="hitNormal">Output: normal of the face that was hit</param>
    /// <returns>True if a block was hit</returns>
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance,
                               out Vector3Int hitBlock, out Vector3 hitNormal)
    {
        hitBlock = Vector3Int.zero;
        hitNormal = Vector3.zero;

        if (WorldData.Instance == null)
            return false;

        direction = direction.normalized;

        // Handle zero direction components
        if (Mathf.Approximately(direction.x, 0)) direction.x = 0.0001f;
        if (Mathf.Approximately(direction.y, 0)) direction.y = 0.0001f;
        if (Mathf.Approximately(direction.z, 0)) direction.z = 0.0001f;

        // Current block position
        Vector3Int mapPos = new Vector3Int(
            Mathf.FloorToInt(origin.x),
            Mathf.FloorToInt(origin.y),
            Mathf.FloorToInt(origin.z)
        );

        // Step direction (+1 or -1)
        Vector3Int step = new Vector3Int(
            direction.x >= 0 ? 1 : -1,
            direction.y >= 0 ? 1 : -1,
            direction.z >= 0 ? 1 : -1
        );

        // Distance to travel along ray to cross one cell in each direction
        Vector3 tDelta = new Vector3(
            Mathf.Abs(1f / direction.x),
            Mathf.Abs(1f / direction.y),
            Mathf.Abs(1f / direction.z)
        );

        // Distance to next cell boundary
        Vector3 tMax = new Vector3(
            step.x > 0 ? (mapPos.x + 1 - origin.x) * tDelta.x : (origin.x - mapPos.x) * tDelta.x,
            step.y > 0 ? (mapPos.y + 1 - origin.y) * tDelta.y : (origin.y - mapPos.y) * tDelta.y,
            step.z > 0 ? (mapPos.z + 1 - origin.z) * tDelta.z : (origin.z - mapPos.z) * tDelta.z
        );

        float distance = 0f;
        int lastAxis = -1; // Track which axis we last stepped along

        // Maximum iterations to prevent infinite loop
        int maxIterations = Mathf.CeilToInt(maxDistance) * 3 + 10;
        int iterations = 0;

        while (distance < maxDistance && iterations < maxIterations)
        {
            iterations++;

            // Skip the first check if we're inside the starting block
            if (iterations > 1 || !IsInsideBlock(origin, mapPos))
            {
                // Check if current block is solid
                BlockType block = WorldData.Instance.GetBlock(mapPos);
                if (BlockTypeConfig.IsSolid(block))
                {
                    hitBlock = mapPos;

                    // Determine which face was hit based on last step
                    switch (lastAxis)
                    {
                        case 0: hitNormal = new Vector3(-step.x, 0, 0); break;
                        case 1: hitNormal = new Vector3(0, -step.y, 0); break;
                        case 2: hitNormal = new Vector3(0, 0, -step.z); break;
                        default: hitNormal = -direction; break;
                    }

                    return true;
                }
            }

            // Step to next block
            if (tMax.x < tMax.y && tMax.x < tMax.z)
            {
                distance = tMax.x;
                tMax.x += tDelta.x;
                mapPos.x += step.x;
                lastAxis = 0;
            }
            else if (tMax.y < tMax.z)
            {
                distance = tMax.y;
                tMax.y += tDelta.y;
                mapPos.y += step.y;
                lastAxis = 1;
            }
            else
            {
                distance = tMax.z;
                tMax.z += tDelta.z;
                mapPos.z += step.z;
                lastAxis = 2;
            }
        }

        return false;
    }

    /// <summary>
    /// Raycast using Unity Ray struct
    /// </summary>
    public static bool Raycast(Ray ray, float maxDistance, out Vector3Int hitBlock, out Vector3 hitNormal)
    {
        return Raycast(ray.origin, ray.direction, maxDistance, out hitBlock, out hitNormal);
    }

    /// <summary>
    /// Check if a point is inside a block's bounding box
    /// </summary>
    private static bool IsInsideBlock(Vector3 point, Vector3Int blockPos)
    {
        return point.x >= blockPos.x && point.x < blockPos.x + 1 &&
               point.y >= blockPos.y && point.y < blockPos.y + 1 &&
               point.z >= blockPos.z && point.z < blockPos.z + 1;
    }

    /// <summary>
    /// Get the exact hit point on the block surface
    /// </summary>
    public static Vector3 GetHitPoint(Vector3 origin, Vector3 direction, Vector3Int hitBlock, Vector3 hitNormal)
    {
        direction = direction.normalized;

        // Calculate plane of the hit face
        Vector3 planePoint = new Vector3(hitBlock.x, hitBlock.y, hitBlock.z);

        // Adjust plane point based on which face was hit
        if (hitNormal.x > 0) planePoint.x = hitBlock.x;
        else if (hitNormal.x < 0) planePoint.x = hitBlock.x + 1;
        else if (hitNormal.y > 0) planePoint.y = hitBlock.y;
        else if (hitNormal.y < 0) planePoint.y = hitBlock.y + 1;
        else if (hitNormal.z > 0) planePoint.z = hitBlock.z;
        else if (hitNormal.z < 0) planePoint.z = hitBlock.z + 1;

        // Ray-plane intersection
        float denom = Vector3.Dot(hitNormal, direction);
        if (Mathf.Abs(denom) < 0.0001f)
            return planePoint; // Ray parallel to plane

        float t = Vector3.Dot(planePoint - origin, hitNormal) / denom;
        return origin + direction * t;
    }

    /// <summary>
    /// Calculate the position where a block would be placed
    /// </summary>
    public static Vector3Int GetPlacePosition(Vector3Int hitBlock, Vector3 hitNormal)
    {
        return hitBlock + Vector3Int.RoundToInt(hitNormal);
    }
}
