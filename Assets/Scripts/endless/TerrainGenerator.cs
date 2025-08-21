// --- START OF FILE TerrainGenerator.cs (REVISED) ---

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ThemeInfo
{
    public string themeName;
    public GameObject topBlockPrefab;
    public List<GameObject> undergroundBlockPrefabs;
    [Tooltip("The Min (X) and Max (Y) number of chunks this theme should last for.")]
    public Vector2Int themeLengthInChunksRange = new Vector2Int(15, 25);
}

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject terrainChunkPrefab;

    [Header("Collectible Settings")]
    public GameObject collectiblePrefab;
    [Range(0f, 1f)]
    public float collectibleSpawnChance = 0.1f;
    public float collectibleVerticalOffset = 1.5f;

    // --- ADD THESE NEW VARIABLES ---
    [Header("Fuel Can Settings")]
    [Tooltip("The prefab for the fuel can collectible.")]
    public GameObject fuelCollectiblePrefab;
    [Tooltip("The Min (X) and Max (Y) distance in meters between fuel can spawns.")]
    public Vector2 fuelSpawnIntervalRange = new Vector2(300f, 500f);
    [Tooltip("How high above the terrain the fuel can should spawn.")]
    public float fuelVerticalOffset = 1.0f;
    private float nextFuelSpawnX; // Tracks the world X-coordinate for the next fuel spawn
    // --- END OF ADDED VARIABLES ---

    [Header("Theme Management")]
    public ThemeInfo[] themes;
    private int currentThemeIndex = -1;
    private int chunksSinceLastSwitch = 0;
    private int lengthOfCurrentThemeRun;

    [Header("Terrain Settings")]
    public float chunkLength = 50f;
    public float blockDensity = 1.0f;
    public float groundBedrockLevel = -15f;
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
            Debug.LogError("No themes are assigned in the TerrainGenerator!", this);
            this.enabled = false;
            return;
        }

        SwitchToRandomTheme();

        // --- ADD THIS LINE: Set the first fuel spawn location ---
        SetNextFuelSpawnPoint();

        for (int i = 0; i < chunksVisibleAhead; i++)
        {
            SpawnChunk();
        }
    }

    // --- ADD THIS NEW HELPER METHOD ---
    void SetNextFuelSpawnPoint()
    {
        nextFuelSpawnX += Random.Range(fuelSpawnIntervalRange.x, fuelSpawnIntervalRange.y);
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
        do
        {
            currentThemeIndex = Random.Range(0, themes.Length);
        } while (currentThemeIndex == oldThemeIndex && themes.Length > 1);
        ThemeInfo newTheme = themes[currentThemeIndex];
        lengthOfCurrentThemeRun = Random.Range(newTheme.themeLengthInChunksRange.x, newTheme.themeLengthInChunksRange.y + 1);
        chunksSinceLastSwitch = 0;
    }

    void SpawnChunk()
    {
        if (chunksSinceLastSwitch >= lengthOfCurrentThemeRun)
        {
            SwitchToRandomTheme();
        }

        ThemeInfo currentTheme = themes[currentThemeIndex];
        GameObject newChunk = Instantiate(terrainChunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);

        // --- ADD THIS BLOCK TO SPAWN FUEL ---
        // Check if our next fuel spawn point falls within this new chunk
        if (fuelCollectiblePrefab != null && nextFuelSpawnX >= spawnX && nextFuelSpawnX < spawnX + chunkLength)
        {
            // Calculate its position on the terrain
            float localX = nextFuelSpawnX - spawnX; // Position relative to the chunk's start
            float yPos = Mathf.PerlinNoise(nextFuelSpawnX * noiseScale, seed) * terrainHeight;
            Vector3 spawnPosition = new Vector3(localX, yPos + fuelVerticalOffset, 0);

            // Instantiate the fuel can and parent it to the chunk
            GameObject fuelCan = Instantiate(fuelCollectiblePrefab, newChunk.transform);
            fuelCan.transform.localPosition = spawnPosition;

            // Set the point for the *next* fuel can
            SetNextFuelSpawnPoint();
        }
        // --- END OF ADDED BLOCK ---

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

    void GenerateChunkContent(GameObject chunk, ThemeInfo theme)
    {
        // This entire method remains unchanged.
        // We moved the fuel spawning logic out of here for better control.
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
            if (collectiblePrefab != null && Random.value < collectibleSpawnChance)
            {
                Vector3 collectiblePosition = topBlock.transform.position + new Vector3(0, collectibleVerticalOffset, 0);
                Instantiate(collectiblePrefab, collectiblePosition, Quaternion.identity, chunk.transform);
            }
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