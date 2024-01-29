using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    float scale;
    const float surface = 0.4f;

    ArrayList vertices;
    ArrayList triangles;
    int triangleIndex;

    public bool generatedMeshData = false;

    float[,,] noise;

    public Mesh GenerateMeshData(float[,,] noise, int size, float scale)
    {
        this.noise = noise;
        this.scale = scale;
        vertices = new ArrayList();
        triangles = new ArrayList();
        triangleIndex = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    MarchCube(x,y,z);
                }
            }
        }
        return UpdateMesh();
    }

    void MarchCube(int x, int y, int z)
    {
        Vector3[] edges = new Vector3[12];
        edges[0] = InterpolatePoints(x, y, z, x, y, z + 1);
        edges[1] = InterpolatePoints(x, y, z + 1, x + 1, y, z + 1);
        edges[2] = InterpolatePoints(x + 1, y, z + 1, x + 1, y, z);
        edges[3] = InterpolatePoints(x + 1, y, z, x, y, z);
        edges[4] = InterpolatePoints(x, y + 1, z, x, y + 1, z + 1);
        edges[5] = InterpolatePoints(x, y + 1, z + 1, x + 1, y + 1, z + 1);
        edges[6] = InterpolatePoints(x + 1, y + 1, z + 1, x + 1, y + 1, z);
        edges[7] = InterpolatePoints(x + 1, y + 1, z, x, y + 1, z);
        edges[8] = InterpolatePoints(x, y + 1, z, x, y, z);
        edges[9] = InterpolatePoints(x, y + 1, z + 1, x, y, z + 1);
        edges[10] = InterpolatePoints(x + 1, y + 1, z + 1, x + 1, y, z + 1);
        edges[11] = InterpolatePoints(x + 1, y + 1, z, x + 1, y, z);

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
        for (int i = 0; i < cubeVertices.Length; i++)
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
                triangles.Add(triangleIndex);
                triangleIndex++;
            }
        }
    }

    Vector3 InterpolatePoints(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        if (noise[x1, y1, z1] > noise[x2, y2, z2])
        {
            if (noise[x1, y1, z1] > surface && noise[x2, y2, z2] < surface)
            {
                return Vector3.MoveTowards(new Vector3(x1 * scale, y1 * scale, z1 * scale), new Vector3(x2 * scale, y2 * scale, z2 * scale),
                    (surface - noise[x1, y1, z1]) * (scale / (noise[x2, y2, z2] - noise[x1, y1, z1])));
            }
        }
        if (noise[x1, y1, z1] < noise[x2, y2, z2])
        {
            if (noise[x1, y1, z1] < surface && noise[x2, y2, z2] > surface)
            {
                return Vector3.MoveTowards(new Vector3(x2 * scale, y2 * scale, z2 * scale), new Vector3(x1 * scale, y1 * scale, z1 * scale),
                    (surface - noise[x2, y2, z2]) * (scale / (noise[x1, y1, z1] - noise[x2, y2, z2])));
            }
        }
        return new Vector3((x1 + x2) / 2, (y1 + y2) / 2, (z1 + z2) / 2);
    }
    public Mesh UpdateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray(typeof(Vector3)) as Vector3[];
        mesh.triangles = triangles.ToArray(typeof(int)) as int[];
        mesh.RecalculateNormals();
        generatedMeshData = true;
        return mesh;
    }
}
