using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Builds optimized mesh for a chunk with face culling
/// Only renders faces exposed to air/water, reducing vertex count significantly
/// </summary>
public class ChunkMeshBuilder
{
    // Face directions (order: Top, Bottom, Right, Left, Front, Back)
    private static readonly Vector3Int[] FaceDirections = new Vector3Int[]
    {
        Vector3Int.up,      // Top (Y+)
        Vector3Int.down,    // Bottom (Y-)
        Vector3Int.right,   // Right (X+)
        Vector3Int.left,    // Left (X-)
        new Vector3Int(0, 0, 1),  // Front (Z+)
        new Vector3Int(0, 0, -1)  // Back (Z-)
    };

    // Vertex offsets for each face (4 vertices per face, counter-clockwise)
    private static readonly Vector3[][] FaceVertices = new Vector3[][]
    {
        // Top (Y+)
        new Vector3[] { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) },
        // Bottom (Y-)
        new Vector3[] { new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1) },
        // Right (X+)
        new Vector3[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
        // Left (X-)
        new Vector3[] { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
        // Front (Z+)
        new Vector3[] { new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1) },
        // Back (Z-)
        new Vector3[] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) }
    };

    // Normal vectors for each face
    private static readonly Vector3[] FaceNormals = new Vector3[]
    {
        Vector3.up,
        Vector3.down,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back
    };

    // Triangle indices for a quad (two triangles)
    private static readonly int[] QuadTriangles = new int[] { 0, 1, 2, 0, 2, 3 };

    private ChunkData chunkData;
    private Vector2Int chunkPosition;

    // Mesh data lists
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Color> colors = new List<Color>();

    // Separate lists for water (transparent) mesh
    private List<Vector3> waterVertices = new List<Vector3>();
    private List<int> waterTriangles = new List<int>();
    private List<Vector3> waterNormals = new List<Vector3>();
    private List<Color> waterColors = new List<Color>();

    public ChunkMeshBuilder(ChunkData data, Vector2Int chunkPos)
    {
        chunkData = data;
        chunkPosition = chunkPos;
    }

    /// <summary>
    /// Build the chunk mesh with face culling optimization
    /// </summary>
    public void BuildMesh(out Mesh solidMesh, out Mesh waterMesh)
    {
        // Clear previous data
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colors.Clear();
        waterVertices.Clear();
        waterTriangles.Clear();
        waterNormals.Clear();
        waterColors.Clear();

        // Iterate through all blocks in chunk
        for (int x = 0; x < ChunkData.SIZE; x++)
        {
            for (int y = 0; y < ChunkData.HEIGHT; y++)
            {
                for (int z = 0; z < ChunkData.SIZE; z++)
                {
                    BlockType blockType = chunkData.GetBlock(x, y, z);
                    if (blockType == BlockType.Air)
                        continue;

                    bool isWater = (blockType == BlockType.Water);
                    Vector3 blockPos = new Vector3(x, y, z);
                    Color blockColor = BlockTypeConfig.GetColor(blockType);

                    // Check each face
                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighborPos = new Vector3Int(x, y, z) + FaceDirections[face];

                        // Check if face should be rendered
                        if (ShouldRenderFace(neighborPos, isWater))
                        {
                            if (isWater)
                            {
                                AddFace(waterVertices, waterTriangles, waterNormals, waterColors,
                                       blockPos, face, blockColor);
                            }
                            else
                            {
                                AddFace(vertices, triangles, normals, colors,
                                       blockPos, face, blockColor);
                            }
                        }
                    }
                }
            }
        }

        // Create solid mesh
        solidMesh = new Mesh();
        solidMesh.name = $"Chunk_{chunkPosition.x}_{chunkPosition.y}_Solid";
        if (vertices.Count > 0)
        {
            solidMesh.SetVertices(vertices);
            solidMesh.SetTriangles(triangles, 0);
            solidMesh.SetNormals(normals);
            solidMesh.SetColors(colors);
            solidMesh.RecalculateBounds();
        }

        // Create water mesh
        waterMesh = new Mesh();
        waterMesh.name = $"Chunk_{chunkPosition.x}_{chunkPosition.y}_Water";
        if (waterVertices.Count > 0)
        {
            waterMesh.SetVertices(waterVertices);
            waterMesh.SetTriangles(waterTriangles, 0);
            waterMesh.SetNormals(waterNormals);
            waterMesh.SetColors(waterColors);
            waterMesh.RecalculateBounds();
        }
    }

    /// <summary>
    /// Check if a face should be rendered (neighbor is air or transparent)
    /// </summary>
    private bool ShouldRenderFace(Vector3Int localPos, bool isWaterBlock)
    {
        // Check bounds within chunk
        if (localPos.x >= 0 && localPos.x < ChunkData.SIZE &&
            localPos.y >= 0 && localPos.y < ChunkData.HEIGHT &&
            localPos.z >= 0 && localPos.z < ChunkData.SIZE)
        {
            BlockType neighbor = chunkData.GetBlock(localPos.x, localPos.y, localPos.z);

            // Water blocks only render faces next to air, not next to other water
            if (isWaterBlock)
            {
                return neighbor == BlockType.Air;
            }

            // Solid blocks render faces next to air or water
            return BlockTypeConfig.IsTransparent(neighbor);
        }

        // At chunk boundary - check neighboring chunk
        // For simplicity, render faces at chunk boundaries
        // A more complete implementation would check the neighboring chunk
        if (localPos.y < 0)
            return false; // Don't render bottom of world
        if (localPos.y >= ChunkData.HEIGHT)
            return true; // Always render top

        // For horizontal boundaries, render the face
        // This could be optimized by checking neighboring chunks
        return true;
    }

    /// <summary>
    /// Add a face to the mesh data
    /// </summary>
    private void AddFace(List<Vector3> verts, List<int> tris, List<Vector3> norms, List<Color> cols,
                        Vector3 blockPos, int faceIndex, Color color)
    {
        int vertexStart = verts.Count;

        // Add 4 vertices for this face
        for (int i = 0; i < 4; i++)
        {
            verts.Add(blockPos + FaceVertices[faceIndex][i]);
            norms.Add(FaceNormals[faceIndex]);
            cols.Add(color);
        }

        // Add 6 triangle indices (2 triangles = 1 quad)
        foreach (int triIndex in QuadTriangles)
        {
            tris.Add(vertexStart + triIndex);
        }
    }

    /// <summary>
    /// Get statistics about the built mesh
    /// </summary>
    public (int solidVerts, int solidTris, int waterVerts, int waterTris) GetStats()
    {
        return (vertices.Count, triangles.Count / 3, waterVertices.Count, waterTriangles.Count / 3);
    }
}
