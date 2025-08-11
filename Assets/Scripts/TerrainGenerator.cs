using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ThemeInfo
{
    public string themeName; // For organizing in the Inspector (e.g., "Grasslands", "Ice Caves")

    [Tooltip("The block for the top-most, drivable surface.")]
    public GameObject topBlockPrefab;

    [Tooltip("The list of blocks to stack underneath the top block, in order from top to bottom.")]
    public List<GameObject> undergroundBlockPrefabs;

    [Tooltip("How many chunks this theme should last for before switching to the next one.")]
    public int themeLengthInChunks = 15;

}

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject terrainChunkPrefab;

    // --- NEW THEME MANAGEMENT SYSTEM ---
    [Header("Theme Management")]
    [Tooltip("The list of all possible themes/biomes for the game.")]
    public ThemeInfo[] themes;

    // Private variables to track our current theme
    private int currentThemeIndex = 0;
    private int chunksInCurrentTheme = 0;
    // --- END OF NEW SYSTEM ---

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    [Tooltip("How many blocks to place per unit of distance. Higher = more dense.")]
    public float blockDensity = 1.0f;
    [Tooltip("The y-position where dirt generation stops completely.")]
    public float groundBedrockLevel = -15f;

    [Tooltip("The final fine-tuning knob. Use a small positive value (like 0.05) to push all visual blocks UP and close the last pixel gap.")]
    public float verticalOffset = 0f;

    // Per-theme settings are now inside ThemeInfo
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

        if (themes == null || themes.Length == 0)
        {
            Debug.LogError("No themes are assigned in the TerrainGenerator! Please create at least one theme.", this);
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
        // --- THEME SWITCHING LOGIC ---
        // Get the current theme's data
        ThemeInfo currentTheme = themes[currentThemeIndex];

        // Check if it's time to switch to the next theme
        if (chunksInCurrentTheme >= currentTheme.themeLengthInChunks)
        {
            // Move to the next theme index, wrapping around if we reach the end
            currentThemeIndex = (currentThemeIndex + 1) % themes.Length;
            // Get the new theme's data
            currentTheme = themes[currentThemeIndex];
            // Reset the chunk counter for the new theme
            chunksInCurrentTheme = 0;
        }

        GameObject newChunk = Instantiate(terrainChunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
        // Pass the chosen theme's data to the generation function
        GenerateChunkContent(newChunk, currentTheme);

        activeChunks.Enqueue(newChunk);
        spawnX += chunkLength;
        chunksInCurrentTheme++; // Increment counter for the current theme
    }

    void DestroyOldestChunk()
    {
        if (activeChunks.Count > chunksVisibleAhead * 2)
        {
            GameObject oldestChunk = activeChunks.Dequeue();
            Destroy(oldestChunk);
        }
    }

    // --- FINAL, UPGRADED GENERATION FUNCTION ---
    void GenerateChunkContent(GameObject chunk, ThemeInfo theme)
    {
        // Part 1: Setup and Collider Generation
        float blockSize = theme.topBlockPrefab.transform.localScale.x;
        int blocksToSpawn = Mathf.CeilToInt(chunkLength / blockSize * blockDensity);
        float placementStep = chunkLength / blocksToSpawn;
        float startX = chunk.transform.position.x;

        Mesh collisionMesh = new Mesh();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        chunk.GetComponent<MeshRenderer>().enabled = false;
        List<Vector3> collisionVertices = new List<Vector3>();
        List<int> collisionTriangles = new List<int>();

        // Part 2: Loop to Build Everything
        for (int i = 0; i < blocksToSpawn; i++)
        {
            float xPos = i * placementStep;
            float yPos = Mathf.PerlinNoise((startX + xPos) * noiseScale, seed) * terrainHeight;
            float nextXPos = xPos + 0.1f;
            float nextYPos = Mathf.PerlinNoise((startX + nextXPos) * noiseScale, seed) * terrainHeight;
            float angle = Mathf.Atan2(nextYPos - yPos, nextXPos - xPos) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            Vector3 colliderCenter = new Vector3(xPos, yPos, 0);

            // Build the invisible "blocky" collider segment
            Vector3 halfWidth = rotation * Vector3.right * (blockSize / 2f);
            Vector3 halfDepth = Vector3.forward * 10;
            Vector3 topLeft = colliderCenter - halfWidth - halfDepth;
            Vector3 topRight = colliderCenter + halfWidth - halfDepth;
            Vector3 btmLeft = colliderCenter - halfWidth + halfDepth;
            Vector3 btmRight = colliderCenter + halfWidth + halfDepth;
            int vertIndex = collisionVertices.Count;
            collisionVertices.AddRange(new[] { topLeft, topRight, btmLeft, btmRight });
            collisionTriangles.AddRange(new[] { vertIndex, vertIndex + 3, vertIndex + 1, vertIndex, vertIndex + 2, vertIndex + 3 });

            // Place the VISIBLE top block using the verticalOffset
            float automaticOffset = blockSize / 2f;
            float totalOffset = automaticOffset - this.verticalOffset;
            Vector3 finalOffsetVector = rotation * Vector3.up * totalOffset;
            Vector3 topBlockLocalPosition = colliderCenter - finalOffsetVector;

            GameObject topBlock = Instantiate(theme.topBlockPrefab, chunk.transform);
            topBlock.transform.localPosition = topBlockLocalPosition;
            topBlock.transform.localRotation = rotation;

            // --- NEW MULTI-LAYER STACKING LOGIC ---
            Vector3 currentUndergroundPosition = topBlockLocalPosition + (Vector3.down * blockSize);

            // 1. Place the defined layers from the theme list first
            if (theme.undergroundBlockPrefabs != null)
            {
                foreach (GameObject layerPrefab in theme.undergroundBlockPrefabs)
                {
                    if (currentUndergroundPosition.y <= groundBedrockLevel) break;

                    GameObject dirtBlock = Instantiate(layerPrefab, chunk.transform);
                    dirtBlock.transform.localPosition = currentUndergroundPosition;
                    dirtBlock.transform.localRotation = Quaternion.identity;
                    currentUndergroundPosition += Vector3.down * blockSize;
                }
            }

            // 2. Fill remaining space with the LAST layer type, if any layers were defined
            if (theme.undergroundBlockPrefabs != null && theme.undergroundBlockPrefabs.Count > 0)
            {
                GameObject lastLayerPrefab = theme.undergroundBlockPrefabs[theme.undergroundBlockPrefabs.Count - 1];
                while (currentUndergroundPosition.y > groundBedrockLevel)
                {
                    GameObject dirtBlock = Instantiate(lastLayerPrefab, chunk.transform);
                    dirtBlock.transform.localPosition = currentUndergroundPosition;
                    dirtBlock.transform.localRotation = Quaternion.identity;
                    currentUndergroundPosition += Vector3.down * blockSize;
                }
            }
        }

        // Part 3: Finalize Collision Mesh
        collisionMesh.vertices = collisionVertices.ToArray();
        collisionMesh.triangles = collisionTriangles.ToArray();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }
}