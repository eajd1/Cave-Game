using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator
{
    public static float[,,] GenerateNoise(int size, float randNum, Vector3 pos, int octaves)
    {
        float[,,] noise = new float[size, size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    float total = 0;
                    for (int o = 1; o <= octaves; o++)
                    {
                        float frequency = octaves * 2 / o;
                        float xy = Mathf.PerlinNoise(((x + pos.x) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, ((y + pos.y) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
                        float yz = Mathf.PerlinNoise(((y + pos.y) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, ((z + pos.z) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
                        float zx = Mathf.PerlinNoise(((z + pos.z) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, ((x + pos.x) * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
                        total += ((xy + yz + zx) / 3f) / o;
                    }
                    noise[x, y, z] += total / Mathf.Sqrt(octaves);
                }
            }
        }
        return noise;
    }

    public static Color32 GetResource(float randNum, Vector3 pos, int octaves)
    {
        float total = 0;
        for (int o = 1; o <= octaves; o++)
        {
           float frequency = octaves * 2 / o;
           float xy = Mathf.PerlinNoise((pos.x * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, (pos.y * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
           float yz = Mathf.PerlinNoise((pos.y * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, (pos.z * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
           float zx = Mathf.PerlinNoise((pos.z * 0.3f + WorldTerrain.WorldSize * randNum) / frequency, (pos.x * 0.3f + WorldTerrain.WorldSize * randNum) / frequency);
           total += ((xy + yz + zx) / 3f) / o;
        }
        total /= Mathf.Sqrt(octaves);
        if (total < 0.3) { return ResourceColours.Iron; }
        if (total >= 0.7) { return ResourceColours.Coal; }
        return ResourceColours.Stone;
    }

    public static (ArrayList, ArrayList) GenerateMeshData(float[,,] noise, int size, float surface, bool interpolate)
    {
        ArrayList vertices = new ArrayList();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    vertices = MarchCube(x, y, z, vertices, noise, surface, interpolate);
                }
            }
        }
        return (vertices, MakeTrianglesArray(vertices));
    }

    static ArrayList MarchCube(int x, int y, int z, ArrayList vertices, float[,,] noise, float surface, bool interpolate)
    {
        Vector3[] edges = new Vector3[12];
        if (interpolate)
        {
            edges[0] = InterpolatePoints(new Vector3(x, y, z), new Vector3(x, y, z + 1), surface, noise);
            edges[1] = InterpolatePoints(new Vector3(x, y, z + 1), new Vector3(x + 1, y, z + 1), surface, noise);
            edges[2] = InterpolatePoints(new Vector3(x + 1, y, z), new Vector3(x + 1, y, z + 1), surface, noise);
            edges[3] = InterpolatePoints(new Vector3(x, y, z), new Vector3(x + 1, y, z), surface, noise);
            edges[4] = InterpolatePoints(new Vector3(x, y + 1, z), new Vector3(x, y + 1, z + 1), surface, noise);
            edges[5] = InterpolatePoints(new Vector3(x, y + 1, z + 1), new Vector3(x + 1, y + 1, z + 1), surface, noise);
            edges[6] = InterpolatePoints(new Vector3(x + 1, y + 1, z), new Vector3(x + 1, y + 1, z + 1), surface, noise);
            edges[7] = InterpolatePoints(new Vector3(x, y + 1, z), new Vector3(x + 1, y + 1, z), surface, noise);
            edges[8] = InterpolatePoints(new Vector3(x, y, z), new Vector3(x, y + 1, z), surface, noise);
            edges[9] = InterpolatePoints(new Vector3(x, y, z + 1), new Vector3(x, y + 1, z + 1), surface, noise);
            edges[10] = InterpolatePoints(new Vector3(x + 1, y, z + 1), new Vector3(x + 1, y + 1, z + 1), surface, noise);
            edges[11] = InterpolatePoints(new Vector3(x + 1, y, z), new Vector3(x + 1, y + 1, z), surface, noise);
        } else
        {
            edges[0] = new Vector3(x, y, z + 0.5f);
            edges[1] = new Vector3(x + 0.5f, y, z + 1);
            edges[2] = new Vector3(x + 1, y, z + 0.5f);
            edges[3] = new Vector3(x + 0.5f, y, z);
            edges[4] = new Vector3(x, y + 1, z + 0.5f);
            edges[5] = new Vector3(x + 0.5f, y + 1, z + 1);
            edges[6] = new Vector3(x + 1, y + 1, z + 0.5f);
            edges[7] = new Vector3(x + 0.5f, y + 1, z);
            edges[8] = new Vector3(x, y + 0.5f, z);
            edges[9] = new Vector3(x, y + 0.5f, z + 1);
            edges[10] = new Vector3(x + 1, y + 0.5f, z + 1);
            edges[11] = new Vector3(x + 1, y + 0.5f, z);
        }

        float[] cubeVertices = new float[8];
        cubeVertices[0] = noise[x, y, z];
        cubeVertices[1] = noise[x, y, z + 1];
        cubeVertices[2] = noise[x + 1, y, z + 1];
        cubeVertices[3] = noise[x + 1, y, z];
        cubeVertices[4] = noise[x, y + 1, z];
        cubeVertices[5] = noise[x, y + 1, z + 1];
        cubeVertices[6] = noise[x + 1, y + 1, z + 1];
        cubeVertices[7] = noise[x + 1, y + 1, z];

        int cubeCode = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cubeVertices[i] < surface)
            {
                cubeCode |= 1 << i;
            }
        }

        for (int i = 0; i < TriangleTable.table.GetLength(1); i++)
        {
            if (TriangleTable.table[cubeCode, i] != -1)
            {
                vertices.Add(edges[TriangleTable.table[cubeCode, i]]);
            }
        }
        return vertices;
    }

    static Vector3 InterpolatePoints(Vector3 p1, Vector3 p2, float surface, float[,,] noise)
    {
        float t = (surface - noise[(int)p1.x, (int)p1.y, (int)p1.z]) / (noise[(int)p2.x, (int)p2.y, (int)p2.z] - noise[(int)p1.x, (int)p1.y, (int)p1.z]);
        return p1 + t * (p2 - p1);
    }

    static ArrayList MakeTrianglesArray(ArrayList vertices)
    {
        ArrayList triangles = new ArrayList();
        for (int i = 0; i < vertices.Count; i++)
        {
            triangles.Add(i);
        }
        return triangles;
    }

    public static Mesh MakeMesh(ArrayList v, ArrayList t)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = v.ToArray(typeof(Vector3)) as Vector3[];
        mesh.triangles = t.ToArray(typeof(int)) as int[];
        mesh.RecalculateNormals();
        return mesh;
    }
    public static int[] MakeTrianglesArray(Vector3[] vertices)
    {
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            triangles[i] = i;
        }
        return triangles;
    }

    public static Mesh MakeMesh(Vector3[] v, int[] t)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateNormals();
        return mesh;
    }
}
