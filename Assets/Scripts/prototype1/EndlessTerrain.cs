using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float viewDst = 40;
    public const float viewDstSqrd = 1600;
    public const int size = 5;
    public const float scale = 2f;
    public float seed = 1f;
    public Transform player;
    public Material material;
    public Material highlightMaterial;

    public static Vector3 playerPos;

    int chunkSize;
    int numVisibleChunks;
    MeshGenerator meshGenerator;

    Dictionary<Vector3, TerrainChunk> chunkDictionary = new Dictionary<Vector3, TerrainChunk>();
    List<TerrainChunk> chunksVisibleLastUpdate = new List<TerrainChunk>();

    // Start is called before the first frame update
    void Start()
    {
        meshGenerator = new MeshGenerator();
        chunkSize = Mathf.RoundToInt(size * scale);
        numVisibleChunks = Mathf.RoundToInt(viewDst / chunkSize);
        playerPos = new Vector3(player.position.x, player.position.y, player.position.z);
    }

    void Update()
    {
        playerPos = new Vector3(player.position.x, player.position.y, player.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        int currentChunkX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(playerPos.y / chunkSize);
        int currentChunkZ = Mathf.RoundToInt(playerPos.z / chunkSize);

        for (int zOffset = -numVisibleChunks; zOffset <= numVisibleChunks; zOffset++)
        {
            for (int xOffset = -numVisibleChunks; xOffset <= numVisibleChunks; xOffset++)
            {
                for (int yOffset = -numVisibleChunks; yOffset <= numVisibleChunks; yOffset++)
                {
                    Vector3 viewedChunkCoord = new Vector3(currentChunkX + xOffset, currentChunkY + yOffset, currentChunkZ + zOffset);
                    if (chunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        chunkDictionary[viewedChunkCoord].UpdateChunk();
                        if (chunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            chunksVisibleLastUpdate.Add(chunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, material, seed, meshGenerator));
                    }
                }
            }
        }
    }

    public void EditTerrain(Vector3 pos, float radius, float amount)
    {
        int chunkX = Mathf.RoundToInt(pos.x / chunkSize);
        int chunkY = Mathf.RoundToInt(pos.y / chunkSize);
        int chunkZ = Mathf.RoundToInt(pos.z / chunkSize);
        Vector3 chunkCoord = new Vector3(chunkX, chunkY, chunkZ);
        Debug.Log(chunkCoord);
        TerrainChunk chunk = chunkDictionary[chunkCoord];
        chunk.meshObject.GetComponent<MeshRenderer>().material = highlightMaterial;
    }

    public class TerrainChunk
    {
        public float[,,] noise;

        Vector3 pos;
        float seed;
        public GameObject meshObject;
        MeshGenerator generator;
        Bounds bounds;
        public TerrainChunk(Vector3 coord, int chunkSize, Transform parent, Material material, float seed, MeshGenerator meshGenerator)
        {
            pos = coord * chunkSize;
            this.seed = seed;
            generator = meshGenerator;
            noise = ChunkGenerator.GenerateChunk(size + 1, size + 1, size + 1, seed, pos, scale);
            bounds = new Bounds(pos, Vector3.one * chunkSize);
            meshObject = new GameObject("Chunk");
            meshObject.transform.position = pos;
            meshObject.transform.parent = parent;
            meshObject.layer = 6;
            SetVisible(false);
            meshObject.AddComponent<MeshCollider>();
            meshObject.AddComponent<MeshFilter>();
            meshObject.AddComponent<MeshRenderer>().material = material;
        }

        public void UpdateChunk()
        {
            float viewerDstFromNearestEdgeSqrd = (bounds.SqrDistance(playerPos));
            bool visible = viewerDstFromNearestEdgeSqrd <= viewDstSqrd;
            if (visible && (meshObject.GetComponent<MeshFilter>().mesh == null || meshObject.GetComponent<MeshCollider>().sharedMesh == null))
            {
                Mesh mesh = generator.GenerateMeshData(noise, size, scale);
                meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
                meshObject.GetComponent<MeshFilter>().mesh = mesh;
            }
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
