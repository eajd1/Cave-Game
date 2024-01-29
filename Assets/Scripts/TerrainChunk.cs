using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainChunk
{
    GameObject gameObj;
    int size;
    int octaves = 8;
    float surface;
    (float, float) randNums;
    ComputeShader meshGenerator;

    public float[,,] vertexValues;
    Vector3 pos;
    bool visible;
    bool generated;
    bool waiting;
    bool updateRequired = true;
    bool start = true;
    bool gpu;
    (ArrayList, ArrayList) meshData;
    Bounds bounds;

    public TerrainChunk(int size, Vector3 chunkCoord, Material material, Transform parent, int layer, PhysicMaterial physicMaterial, float surface, int seed, ComputeShader meshGenerator, bool gpu)
    {
        this.size = size;
        this.surface = surface;
        this.meshGenerator = meshGenerator;
        this.gpu = gpu;
        Random.InitState(seed);
        randNums.Item1 = Random.value + 1;
        randNums.Item2 = Random.value + 1;
        pos = chunkCoord * size;
        bounds = new Bounds(pos, Vector3.one * size);
        gameObj = new GameObject("Chunk");
        gameObj.transform.position = pos;
        gameObj.transform.parent = parent;
        gameObj.layer = layer;
        gameObj.AddComponent<MeshCollider>().material = physicMaterial;
        gameObj.AddComponent<MeshFilter>();
        gameObj.AddComponent<MeshRenderer>().material = material;
        SetVisible(true);
    }

    public void Update(Vector3 playerPos, int viewDstSqrd, bool interpolate)
    {
        float viewerDstFromNearestEdgeSqrd = bounds.SqrDistance(playerPos);
        visible = viewerDstFromNearestEdgeSqrd <= viewDstSqrd;
        if (gpu && updateRequired)
        {
            GenerateValues();
            GenerateMeshDataGPU();
        }
        else
        {
            if (visible && !generated && !waiting && updateRequired)
            {
                ThreadStart threadstart = delegate
                {
                    if (start)
                    {
                        GenerateValues();
                        start = false;
                    }
                    GenerateMeshData(interpolate);
                };
                new Thread(threadstart).Start();
                waiting = true;
            }
            if (visible && generated && !waiting && updateRequired)
            {
                Mesh mesh = TerrainGenerator.MakeMesh(meshData.Item1, meshData.Item2);
                gameObj.GetComponent<MeshCollider>().sharedMesh = mesh;
                gameObj.GetComponent<MeshFilter>().mesh = mesh;
                updateRequired = false;
            }
        }
        SetVisible(visible);
    }

    void GenerateValues()
    {
        vertexValues = TerrainGenerator.GenerateNoise(size + 1, randNums.Item1, pos, octaves);
    }

    void GenerateMeshData(bool interpolate)
    {
        meshData = TerrainGenerator.GenerateMeshData(vertexValues, size, surface, interpolate);
        generated = true;
        waiting = false;
    }

    void GenerateMeshDataGPU()
    {
        Texture3D noise = ArrayToTexture(this.vertexValues);
        meshGenerator.SetTexture(0, "noiseTexture", noise);

        meshGenerator.SetInt("size", size);
        meshGenerator.SetFloat("surface", surface);

        ComputeBuffer vertexBuffer = new ComputeBuffer(16 * size * size * size, sizeof(float) * 3);
        meshGenerator.SetBuffer(0, Shader.PropertyToID("vertexBuffer"), vertexBuffer);

        meshGenerator.Dispatch(0, 1, 1, size);

        Vector3[] vertices = new Vector3[vertexBuffer.count];
        vertexBuffer.GetData(vertices);
        vertexBuffer.Release();

        Mesh mesh = TerrainGenerator.MakeMesh(vertices, TerrainGenerator.MakeTrianglesArray(vertices));
        gameObj.GetComponent<MeshCollider>().sharedMesh = mesh;
        gameObj.GetComponent<MeshFilter>().mesh = mesh;
    }

    Texture3D ArrayToTexture(float[,,] array)
    {
        Texture3D texture = new Texture3D(size + 1, size + 1, size + 1, TextureFormat.RGBAFloat, false);
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int z = 0; z < array.GetLength(2); z++)
                {
                    texture.SetPixel(x, y, z, new Color(0, 0, 0, array[x, y, z]));
                }
            }
        }
        return texture;
    }

    public void SetVisible(bool visible)
    {
        gameObj.SetActive(visible);
    }

    public bool IsVisible()
    {
        return gameObj.activeSelf;
    }

    public void NeedUpdate()
    {
        generated = false;
        updateRequired = true;
    }

    public Vector3 GetPos()
    {
        return pos;
    }

    public GameObject GetObject()
    {
        return gameObj;
    }
}
