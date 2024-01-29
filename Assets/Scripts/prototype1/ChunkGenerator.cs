using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator
{
    const int offset = 100000;
    public static float[,,] GenerateChunk(int breadth, int height, int length, float seed, Vector3 pos, float scale)
    {
        float[,,] noise = new float[breadth, height, length];
        for (int x = 0; x < noise.GetLength(0); x++)
        {
            for (int y = 0; y < noise.GetLength(1); y++)
            {
                for (int z = 0; z < noise.GetLength(2); z++)
                {
                    float xy = Mathf.PerlinNoise(((x + pos.x / scale) * 0.3f + offset) / seed, (y + pos.y / scale) * 0.3f + offset);
                    float yz = Mathf.PerlinNoise((y + pos.y / scale) * 0.3f + offset, ((z + pos.z / scale) * 0.3f + offset) / seed);
                    float zx = Mathf.PerlinNoise(((z + pos.z / scale) * 0.3f + offset) / seed, ((x + pos.x / scale) * 0.3f + offset) / seed);
                    noise[x, y, z] = (xy + yz + zx) / 3;
                }
            }
        }
        return noise;
    }
}
