using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject terrainChunkPrefab;

    [Header("Visuals")]
    [Tooltip("The top-layer block prefab (e.g., ground_cube_grass). Collider MUST be disabled.")]
    public GameObject grassBlockPrefab; // Renamed for clarity
    [Tooltip("The block prefab to stack underneath the top layer. Collider MUST be disabled.")]
    public GameObject dirtBlockPrefab; // --- NEW ---
    [Tooltip("How many blocks to place per unit of distance. Higher = more dense.")]
    public float blockDensity = 1.0f;
    [Tooltip("How far down the dirt blocks should be generated.")]
    public float groundBedrockLevel = -15f; // --- NEW ---

    [Tooltip("The final fine-tuning knob. Use a small positive value (like 0.05) to push the ground UP and close the last pixel gap.")]
    public float verticalOffset = 0f;

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    public float terrainHeight = 10f;
    public float noiseScale = 0.07f;

    [Header("Generator Settings")]
    public int chunksVisibleAhead = 3;
    private float spawnX = 0.0f;
    private float seed;

    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        seed = Random.Range(0f, 100f);

        if (grassBlockPrefab == null || dirtBlockPrefab == null)
        {
            Debug.LogError("The 'Grass Block Prefab' or 'Dirt Block Prefab' is not assigned! Disabling generator.", this);
            this.enabled = false;
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
        GenerateChunkContent(newChunk);
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

    // --- FINAL VERSION OF THIS FUNCTION ---
    void GenerateChunkContent(GameObject chunk)
    {
        // --- PART 1: SHARED SETUP ---
        float blockSize = grassBlockPrefab.transform.localScale.x;
        int blocksToSpawn = Mathf.CeilToInt(chunkLength / blockSize * blockDensity);
        float placementStep = chunkLength / blocksToSpawn;
        float startX = chunk.transform.position.x;

        Mesh collisionMesh = new Mesh();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        chunk.GetComponent<MeshRenderer>().enabled = false;
        List<Vector3> collisionVertices = new List<Vector3>();
        List<int> collisionTriangles = new List<int>();

        // --- PART 2: SINGLE LOOP TO BUILD EVERYTHING ---
        for (int i = 0; i < blocksToSpawn; i++)
        {
            // A. Calculate position and rotation
            float xPos = i * placementStep;
            float yPos = Mathf.PerlinNoise((startX + xPos) * noiseScale, seed) * terrainHeight;
            float nextXPos = xPos + 0.1f;
            float nextYPos = Mathf.PerlinNoise((startX + nextXPos) * noiseScale, seed) * terrainHeight;
            float angle = Mathf.Atan2(nextYPos - yPos, nextXPos - xPos) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // B. Build the INVISIBLE COLLIDER segment for this block (This is perfectly aligned with the car's wheels)
            Vector3 colliderCenter = new Vector3(xPos, yPos, 0);
            Vector3 halfWidth = rotation * Vector3.right * (blockSize / 2f);
            Vector3 halfDepth = Vector3.forward * 10;
            Vector3 topLeft = colliderCenter - halfWidth - halfDepth;
            Vector3 topRight = colliderCenter + halfWidth - halfDepth;
            Vector3 btmLeft = colliderCenter - halfWidth + halfDepth;
            Vector3 btmRight = colliderCenter + halfWidth + halfDepth;
            int vertIndex = collisionVertices.Count;
            collisionVertices.AddRange(new[] { topLeft, topRight, btmLeft, btmRight });
            collisionTriangles.AddRange(new[] { vertIndex, vertIndex + 3, vertIndex + 1, vertIndex, vertIndex + 2, vertIndex + 3 });

            // C. Place the VISIBLE BLOCKS for this segment
            // This is the automatic offset to align the block's center with its top edge.
            float automaticOffset = blockSize / 2f;

            // --- THE FINAL FIX IS HERE ---
            // We combine the automatic offset with your manual fine-tuning knob.
            // A positive verticalOffset REDUCES the amount we pull the block down, effectively pushing it UP.
            float totalOffset = automaticOffset - this.verticalOffset;

            Vector3 finalOffsetVector = rotation * Vector3.up * totalOffset;
            Vector3 grassBlockLocalPosition = colliderCenter - finalOffsetVector;

            // Instantiate grass block
            GameObject grassBlock = Instantiate(grassBlockPrefab);
            grassBlock.transform.SetParent(chunk.transform, false);
            grassBlock.transform.localPosition = grassBlockLocalPosition;
            grassBlock.transform.localRotation = rotation;

            // Instantiate dirt blocks
            Vector3 currentDirtPosition = grassBlockLocalPosition + (Vector3.down * blockSize);
            while (currentDirtPosition.y > groundBedrockLevel)
            {
                GameObject dirtBlock = Instantiate(dirtBlockPrefab);
                dirtBlock.transform.SetParent(chunk.transform, false);
                dirtBlock.transform.localPosition = currentDirtPosition;
                dirtBlock.transform.localRotation = Quaternion.identity;
                currentDirtPosition += Vector3.down * blockSize;
            }
        }

        // --- PART 3: FINALIZE THE COLLISION MESH ---
        collisionMesh.vertices = collisionVertices.ToArray();
        collisionMesh.triangles = collisionTriangles.ToArray();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }
}