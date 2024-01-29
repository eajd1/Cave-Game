using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTerrain : MonoBehaviour
{
    public const int viewDst = 100;
    public const int viewDstSqrd = 10000;
    public const int chunkSize = 20;

    public Transform player;
    public Material material;
    public int layer;
    public bool interpolate;
    public bool gpuAccelerate;
    public PhysicMaterial physicMaterial;
    [Range(0.3f, 0.45f)]
    public float emptiness;
    public int seed;
    public ComputeShader meshGenerator;

    public static Vector3 playerPos;
    int numVisibleChunks;
    ArrayList chunks;

    Dictionary<Vector3, TerrainChunk> chunkDictionary = new Dictionary<Vector3, TerrainChunk>();
    List<TerrainChunk> chunksVisibleLastUpdate = new List<TerrainChunk>();
    public const int WorldSize = 10000;

    // Start is called before the first frame update
    void Start()
    {
        numVisibleChunks = Mathf.RoundToInt(viewDst / chunkSize);
        playerPos = new Vector3(player.position.x, player.position.y, player.position.z);
        chunks = new ArrayList();
    }

    // Update is called once per frame
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
                        chunkDictionary[viewedChunkCoord].Update(playerPos, viewDstSqrd, interpolate);
                        if (chunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            chunksVisibleLastUpdate.Add(chunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(chunkSize, viewedChunkCoord, material, transform, layer, physicMaterial, emptiness, seed, meshGenerator, gpuAccelerate));
                    }
                }
            }
        }
    }
    public void EditTerrain(Vector3 loc, float radius, float percentage, Texture3D brush)
    {
        chunks = new ArrayList();
        for (int x = (int)(loc.x - brush.width); x <= (int)(loc.x + brush.width); x ++)
        {
            for (int y = (int)(loc.y - brush.height); y <= (int)(loc.y + brush.height); y ++)
            {
                for (int z = (int)(loc.z - brush.depth); z <= (int)(loc.z + brush.depth); z ++)
                {
                    float dist = Vector3.Distance(loc, new Vector3(x, y, z));
                    if (dist <= radius)
                    {
                        float pixelX = Mathf.InverseLerp(loc.x - radius, loc.x + radius, x);
                        float pixelY = Mathf.InverseLerp(loc.y - radius, loc.y + radius, y);
                        float pixelZ = Mathf.InverseLerp(loc.z - radius, loc.z + radius, z);
                        Color pixel = brush.GetPixel((int)(pixelX * brush.width), (int)(pixelY * brush.height), (int)(pixelZ * brush.depth));
                        MakeChange(x, y, z, pixel.a * percentage);
                    }
                }
            }
        }
        foreach (TerrainChunk c in chunks)
        {
            c.NeedUpdate();
            c.Update(playerPos, viewDstSqrd, interpolate);
        }
    }

    void MakeChange(int x, int y, int z, float amount)
    {
        Vector3 loc = new Vector3(x, y, z);
        Vector3 chunkCoord = GetChunkCoord(loc);
        TerrainChunk chunk = chunkDictionary[chunkCoord];
        Vector3 chunkLoc = loc - (chunkCoord * chunkSize);
        float num = Mathf.Clamp(chunk.vertexValues[(int)chunkLoc.x, (int)chunkLoc.y, (int)chunkLoc.z] - chunk.vertexValues[(int)chunkLoc.x, (int)chunkLoc.y, (int)chunkLoc.z] * amount, 0f, 1f);
        chunk.vertexValues[(int)chunkLoc.x, (int)chunkLoc.y, (int)chunkLoc.z] = num;
        if (!chunks.Contains(chunk))
        {
            chunks.Add(chunk);
        }
        foreach (TerrainChunk c in GetChunks(loc))
        {
            Vector3 possibleChunkCoord = GetSpecificChunkCoord(loc, c);
            if (possibleChunkCoord.x <= chunkSize && possibleChunkCoord.x >= 0 && possibleChunkCoord.y <= chunkSize && possibleChunkCoord.y >= 0 && possibleChunkCoord.z <= chunkSize && possibleChunkCoord.z >= 0)
            {
                c.vertexValues[(int)possibleChunkCoord.x, (int)possibleChunkCoord.y, (int)possibleChunkCoord.z] = num;
            }
        }
    }

    Vector3 GetChunkCoord(Vector3 loc)
    {
        return new Vector3(SpecialRound(loc.x) / chunkSize, SpecialRound(loc.y) / chunkSize, SpecialRound(loc.z) / chunkSize);
    }

    int SpecialRound(float num)
    {
        if (num >= 0)
        {
            return Mathf.FloorToInt(num / chunkSize) * chunkSize;
        } else
        {
            return -Mathf.CeilToInt(-num / chunkSize) * chunkSize;
        }
    }

    ArrayList GetChunks(Vector3 loc)
    {
        ArrayList chunks = new ArrayList();
        TerrainChunk chunk = chunkDictionary[GetChunkCoord(loc)];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.forward];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right - Vector3.forward];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right - Vector3.forward];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward - Vector3.right];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward + Vector3.right];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }

        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.forward - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right - Vector3.forward - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right - Vector3.forward - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward - Vector3.right - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward + Vector3.right - Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }

        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.forward + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) - Vector3.right - Vector3.forward + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.right - Vector3.forward + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward - Vector3.right + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }
        chunk = chunkDictionary[GetChunkCoord(loc) + Vector3.forward + Vector3.right + Vector3.up];
        if (!chunks.Contains(chunk)) { chunks.Add(chunk); }

        return chunks;
    }

    Vector3 GetSpecificChunkCoord(Vector3 loc, TerrainChunk chunk)
    {
        return loc - chunk.GetPos();
    }

    public void PaintTerrain(RaycastHit hit, Color32 colour)
    {
        TerrainChunk chunk = chunkDictionary[GetChunkCoord(hit.point)];
        Mesh mesh = chunk.GetObject().GetComponent<MeshFilter>().mesh;
        Color32[] colours = mesh.colors32;
        colours[hit.triangleIndex * 3 + 0] = colour;
        colours[hit.triangleIndex * 3 + 1] = colour;
        colours[hit.triangleIndex * 3 + 2] = colour;
        mesh.colors32 = colours;
    }
}
