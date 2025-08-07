using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public BoxCollider playerCollider;
    public GameObject terrainChunkPrefab;

    [Header("Theme Materials")] // <-- MODIFIED SECTION
    public Material topMaterial; // Assign Grass material here
    public Material sideMaterial; // Assign Dirt/Stone material here

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    public int verticesPerChunk = 100;
    public float terrainHeight = 10f; // Lowered for better driveability
    public float noiseScale = 0.07f; // Made hills wider

    [Header("Generator Settings")]
    public int chunksVisibleAhead = 3;
    private float spawnX = 0.0f;
    private float seed;

    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        seed = UnityEngine.Random.Range(0f, 100f);
        if (playerCollider == null || topMaterial == null || sideMaterial == null)
        {
            Debug.LogError("Player Collider or Materials are not assigned in the TerrainGenerator!");
            return;
        }

        for (int i = 0; i < chunksVisibleAhead; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        if (player.position.x > spawnX - (chunksVisibleAhead * chunkLength))
        {
            SpawnChunk();
            DestroyOldestChunk();
        }
    }

    void SpawnChunk()
    {
        GameObject newChunk = Instantiate(terrainChunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
        GenerateTerrain(newChunk);
        activeChunks.Enqueue(newChunk);
        spawnX += chunkLength;
    }

    void DestroyOldestChunk()
    {
        if (activeChunks.Count > chunksVisibleAhead * 2)
        {
            GameObject oldestChunk = activeChunks.Dequeue();
            Destroy(oldestChunk);
        }
    }

    // --- THIS ENTIRE FUNCTION HAS BEEN REWRITTEN FOR SUB-MESHES ---
    void GenerateTerrain(GameObject chunk)
    {
        MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        float terrainDepth = playerCollider.size.z * 1.2f;
        float halfDepth = terrainDepth / 2f;
        float step = chunkLength / verticesPerChunk;
        float startX = chunk.transform.position.x;

        // We need lists for triangles because we don't know the exact size ahead of time
        List<Vector3> vertices = new List<Vector3>();
        List<int> topTriangles = new List<int>();
        List<int> sideTriangles = new List<int>();

        // Generate the vertices first
        for (int i = 0; i <= verticesPerChunk; i++)
        {
            float xPos = i * step;
            float perlinY = Mathf.PerlinNoise((startX + xPos) * noiseScale, seed) * terrainHeight;

            // Add the 4 corner vertices for this segment
            vertices.Add(new Vector3(xPos, perlinY, -halfDepth)); // Front-Top
            vertices.Add(new Vector3(xPos, perlinY - 20f, -halfDepth)); // Front-Bottom
            vertices.Add(new Vector3(xPos, perlinY, halfDepth)); // Back-Top
            vertices.Add(new Vector3(xPos, perlinY - 20f, halfDepth)); // Back-Bottom
        }

        // Generate the triangles for the sub-meshes
        for (int i = 0; i < verticesPerChunk; i++)
        {
            int vertIndex = i * 4;

            // Vertex indices for the current quad and the next one
            int currentFrontTop = vertIndex + 0;
            int currentFrontBottom = vertIndex + 1;
            int currentBackTop = vertIndex + 2;
            int currentBackBottom = vertIndex + 3;

            int nextFrontTop = vertIndex + 4;
            int nextFrontBottom = vertIndex + 5;
            int nextBackTop = vertIndex + 6;

            // 1. Top Face (Goes into topTriangles list)
            topTriangles.Add(currentFrontTop);
            topTriangles.Add(nextBackTop);
            topTriangles.Add(nextFrontTop);
            topTriangles.Add(currentFrontTop);
            topTriangles.Add(currentBackTop);
            topTriangles.Add(nextBackTop);

            // 2. Front Face (Goes into sideTriangles list)
            sideTriangles.Add(currentFrontBottom);
            sideTriangles.Add(currentFrontTop);
            sideTriangles.Add(nextFrontTop);
            sideTriangles.Add(currentFrontBottom);
            sideTriangles.Add(nextFrontTop);
            sideTriangles.Add(nextFrontBottom);
        }

        // --- APPLY TO MESH ---
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2; // Tell the mesh it has two parts

        // Set the triangles for each part
        mesh.SetTriangles(topTriangles.ToArray(), 0); // Sub-mesh 0
        mesh.SetTriangles(sideTriangles.ToArray(), 1); // Sub-mesh 1

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        // Assign the array of materials to the renderer
        meshRenderer.materials = new Material[] { topMaterial, sideMaterial };
    }
}