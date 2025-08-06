using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public BoxCollider playerCollider; // <-- ADDED: To get the car's width
    public GameObject terrainChunkPrefab;
    public Material terrainMaterial;

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    public int verticesPerChunk = 100;
    public float terrainHeight = 15f;
    public float noiseScale = 0.1f;

    [Header("Generator Settings")]
    public int chunksVisibleAhead = 3;
    private float spawnX = 0.0f;
    private float seed;

    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        seed = UnityEngine.Random.Range(0f, 100f);
        if (playerCollider == null)
        {
            Debug.LogError("Player Collider is not assigned in the TerrainGenerator!");
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

    void GenerateTerrain(GameObject chunk)
    {
        MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;
        Mesh mesh = new Mesh();

        // --- MODIFIED FOR AUTO-SIZING ---
        // Get terrain depth from the car's collider size, plus a little padding.
        float terrainDepth = playerCollider.size.z * 1.2f;
        float halfDepth = terrainDepth / 2f;
        // ---------------------------------

        Vector3[] vertices = new Vector3[(verticesPerChunk + 1) * 4];
        int[] triangles = new int[verticesPerChunk * 6 * 3];

        float step = chunkLength / verticesPerChunk;
        float startX = chunk.transform.position.x;

        for (int i = 0; i <= verticesPerChunk; i++)
        {
            float xPos = i * step;
            float perlinY = Mathf.PerlinNoise((startX + xPos) * noiseScale, seed) * terrainHeight;
            int vertIndex = i * 4;

            // --- VERTICES ARE NOW CENTERED AROUND Z=0 ---
            // Front-Top vertex (now at -halfDepth)
            vertices[vertIndex + 0] = new Vector3(xPos, perlinY, -halfDepth);
            // Front-Bottom vertex
            vertices[vertIndex + 1] = new Vector3(xPos, perlinY - 20f, -halfDepth);
            // Back-Top vertex (now at +halfDepth)
            vertices[vertIndex + 2] = new Vector3(xPos, perlinY, halfDepth);
            // Back-Bottom vertex
            vertices[vertIndex + 3] = new Vector3(xPos, perlinY - 20f, halfDepth);
            // ------------------------------------------

            if (i < verticesPerChunk)
            {
                // This triangle logic remains the same
                int triIndex = i * 18;
                int currentFrontTop = vertIndex + 0;
                int currentBackTop = vertIndex + 2;
                int nextFrontTop = vertIndex + 4;
                int nextBackTop = vertIndex + 6;
                triangles[triIndex + 0] = currentFrontTop;
                triangles[triIndex + 1] = nextBackTop;
                triangles[triIndex + 2] = nextFrontTop;
                triangles[triIndex + 3] = currentFrontTop;
                triangles[triIndex + 4] = currentBackTop;
                triangles[triIndex + 5] = nextBackTop;

                int currentFrontBottom = vertIndex + 1;
                int nextFrontBottom = vertIndex + 5;
                triangles[triIndex + 6] = currentFrontBottom;
                triangles[triIndex + 7] = nextFrontTop;
                triangles[triIndex + 8] = nextFrontBottom;
                triangles[triIndex + 9] = currentFrontBottom;
                triangles[triIndex + 10] = currentFrontTop;
                triangles[triIndex + 11] = nextFrontTop;

                int nextBackBottom = vertIndex + 7;
                int currentBackBottom = vertIndex + 3;
                triangles[triIndex + 12] = currentBackBottom;
                triangles[triIndex + 13] = nextBackBottom;
                triangles[triIndex + 14] = nextBackTop;
                triangles[triIndex + 15] = currentBackBottom;
                triangles[triIndex + 16] = nextBackTop;
                triangles[triIndex + 17] = currentBackTop;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}