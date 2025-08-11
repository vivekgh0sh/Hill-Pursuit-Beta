using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ThemeInfo
{
    public string themeName;
    public GameObject topBlockPrefab;
    public List<GameObject> undergroundBlockPrefabs;

    // --- REPLACE THIS LINE ---
    // public int themeLengthInChunks = 15;

    // --- WITH THIS LINE ---
    [Tooltip("The Min (X) and Max (Y) number of chunks this theme should last for.")]
    public Vector2Int themeLengthInChunksRange = new Vector2Int(15, 25);
}

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject terrainChunkPrefab;

    [Header("Theme Management")]
    [Tooltip("The list of all possible themes/biomes for the game.")]
    public ThemeInfo[] themes;

    // --- UPGRADED THEME TRACKING VARIABLES ---
    private int currentThemeIndex = -1; // Start at -1 to ensure first pick is always random
    private int chunksSinceLastSwitch = 0;
    private int lengthOfCurrentThemeRun; // The randomized length for the current biome instance

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    [Tooltip("How many blocks to place per unit of distance. Higher = more dense.")]
    public float blockDensity = 1.0f;
    [Tooltip("The y-position where dirt generation stops completely.")]
    public float groundBedrockLevel = -15f;
    [Tooltip("The final fine-tuning knob. Use a small positive value (like 0.05) to push all visual blocks UP and close the last pixel gap.")]
    public float verticalOffset = 0f;
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

        // Pick the very first theme and set its initial random length
        SwitchToRandomTheme();

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

    void SwitchToRandomTheme()
    {
        int oldThemeIndex = currentThemeIndex;

        // Make sure we don't pick the same theme twice in a row (if there's more than one choice)
        do
        {
            currentThemeIndex = Random.Range(0, themes.Length);
        } while (currentThemeIndex == oldThemeIndex && themes.Length > 1);

        // Get the new theme's data
        ThemeInfo newTheme = themes[currentThemeIndex];

        // Set the random duration for this new theme run
        lengthOfCurrentThemeRun = Random.Range(newTheme.themeLengthInChunksRange.x, newTheme.themeLengthInChunksRange.y + 1);

        // Reset the chunk counter for the new theme
        chunksSinceLastSwitch = 0;
    }

    void SpawnChunk()
    {
        // Check if it's time to switch to a new random theme
        if (chunksSinceLastSwitch >= lengthOfCurrentThemeRun)
        {
            SwitchToRandomTheme();
        }

        ThemeInfo currentTheme = themes[currentThemeIndex];

        GameObject newChunk = Instantiate(terrainChunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
        GenerateChunkContent(newChunk, currentTheme);

        activeChunks.Enqueue(newChunk);
        spawnX += chunkLength;
        chunksSinceLastSwitch++;
    }

    void DestroyOldestChunk()
    {
        if (activeChunks.Count > chunksVisibleAhead * 2)
        {
            GameObject oldestChunk = activeChunks.Dequeue();
            Destroy(oldestChunk);
        }
    }

    // --- This function remains the same, it's already perfect ---
    void GenerateChunkContent(GameObject chunk, ThemeInfo theme)
    {
        float blockSize = theme.topBlockPrefab.transform.localScale.x;
        int blocksToSpawn = Mathf.CeilToInt(chunkLength / blockSize * blockDensity);
        float placementStep = chunkLength / blocksToSpawn;
        float startX = chunk.transform.position.x;
        Mesh collisionMesh = new Mesh();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        chunk.GetComponent<MeshRenderer>().enabled = false;
        List<Vector3> collisionVertices = new List<Vector3>();
        List<int> collisionTriangles = new List<int>();
        for (int i = 0; i < blocksToSpawn; i++)
        {
            float xPos = i * placementStep;
            float yPos = Mathf.PerlinNoise((startX + xPos) * noiseScale, seed) * terrainHeight;
            float nextXPos = xPos + 0.1f;
            float nextYPos = Mathf.PerlinNoise((startX + nextXPos) * noiseScale, seed) * terrainHeight;
            float angle = Mathf.Atan2(nextYPos - yPos, nextXPos - xPos) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
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
            float automaticOffset = blockSize / 2f;
            float totalOffset = automaticOffset - this.verticalOffset;
            Vector3 finalOffsetVector = rotation * Vector3.up * totalOffset;
            Vector3 topBlockLocalPosition = colliderCenter - finalOffsetVector;
            GameObject topBlock = Instantiate(theme.topBlockPrefab, chunk.transform);
            topBlock.transform.localPosition = topBlockLocalPosition;
            topBlock.transform.localRotation = rotation;
            Vector3 currentUndergroundPosition = new Vector3(colliderCenter.x, topBlockLocalPosition.y - blockSize, colliderCenter.z);
            if (theme.undergroundBlockPrefabs != null)
            {
                foreach (GameObject layerPrefab in theme.undergroundBlockPrefabs)
                {
                    if (currentUndergroundPosition.y <= groundBedrockLevel) break;
                    GameObject undergroundBlock = Instantiate(layerPrefab, chunk.transform);
                    undergroundBlock.transform.localPosition = currentUndergroundPosition;
                    undergroundBlock.transform.localRotation = Quaternion.identity;
                    currentUndergroundPosition.y -= blockSize;
                }
            }
            if (theme.undergroundBlockPrefabs != null && theme.undergroundBlockPrefabs.Count > 0)
            {
                GameObject lastLayerPrefab = theme.undergroundBlockPrefabs[theme.undergroundBlockPrefabs.Count - 1];
                while (currentUndergroundPosition.y > groundBedrockLevel)
                {
                    GameObject fillerBlock = Instantiate(lastLayerPrefab, chunk.transform);
                    fillerBlock.transform.localPosition = currentUndergroundPosition;
                    fillerBlock.transform.localRotation = Quaternion.identity;
                    currentUndergroundPosition.y -= blockSize;
                }
            }
        }
        collisionMesh.vertices = collisionVertices.ToArray();
        collisionMesh.triangles = collisionTriangles.ToArray();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }
}