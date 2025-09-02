// --- START OF FILE TerrainGenerator.cs (CORRECTED & COMPLETE) ---

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ThemeInfo
{
    public string themeName;
    public GameObject topBlockPrefab;
    public List<GameObject> undergroundBlockPrefabs;
    [Tooltip("The Min (X) and Max (Y) number of chunks this theme should last for.")]
    public Vector2Int themeLengthInChunksRange = new Vector2Int(15, 25);
    [Tooltip("The background color for the sky when this theme is active.")]
    public Color skyColor = Color.cyan;
}

public class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject terrainChunkPrefab;

    [Header("Sky Settings")]
    [Tooltip("How many seconds the sky color transition should take.")]
    public float skyTransitionDuration = 5.0f;

    [Header("Collectible Settings")]
    public GameObject collectiblePrefab;
    [Range(0f, 1f)] public float collectibleSpawnChance = 0.1f;
    public float collectibleVerticalOffset = 1.5f;

    [Header("Fuel Can Settings")]
    public GameObject fuelCollectiblePrefab;
    public Vector2 fuelSpawnIntervalRange = new Vector2(300f, 500f);
    public float fuelVerticalOffset = 1.0f;
    private float nextFuelSpawnX;

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

    private Color nextThemeSkyColor;
    private float themeTransitionX = float.MaxValue;
    private Coroutine activeSkyTransition;

    void Start()
    {
        seed = Random.Range(0f, 100f);
        if (themes == null || themes.Length == 0) { this.enabled = false; return; }
        SwitchToRandomTheme(true);
        SetNextFuelSpawnPoint();
        for (int i = 0; i < chunksVisibleAhead; i++) { SpawnChunk(); }
    }

    void Update()
    {
        if (player.position.x > spawnX - (chunksVisibleAhead * chunkLength))
        {
            SpawnChunk();
            DestroyOldestChunk();
        }

        if (player != null && player.position.x > themeTransitionX)
        {
            StartSkyTransition(nextThemeSkyColor);
            themeTransitionX = float.MaxValue;
        }
    }

    void SwitchToRandomTheme(bool isInitialSwitch = false)
    {
        int oldThemeIndex = currentThemeIndex;
        do { currentThemeIndex = Random.Range(0, themes.Length); }
        while (currentThemeIndex == oldThemeIndex && themes.Length > 1);

        ThemeInfo newTheme = themes[currentThemeIndex];
        lengthOfCurrentThemeRun = Random.Range(newTheme.themeLengthInChunksRange.x, newTheme.themeLengthInChunksRange.y + 1);
        chunksSinceLastSwitch = 0;

        if (isInitialSwitch)
        {
            if (Camera.main != null) Camera.main.backgroundColor = newTheme.skyColor;
        }
        else
        {
            themeTransitionX = spawnX;
            nextThemeSkyColor = newTheme.skyColor;
        }
    }

    private void StartSkyTransition(Color targetColor)
    {
        if (activeSkyTransition != null) StopCoroutine(activeSkyTransition);
        activeSkyTransition = StartCoroutine(TransitionSkyCoroutine(targetColor));
    }

    private IEnumerator TransitionSkyCoroutine(Color targetColor)
    {
        float elapsedTime = 0f;
        Color startColor = Camera.main.backgroundColor;
        while (elapsedTime < skyTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / skyTransitionDuration);
            Camera.main.backgroundColor = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        Camera.main.backgroundColor = targetColor;
    }

    void SetNextFuelSpawnPoint() { nextFuelSpawnX += Random.Range(fuelSpawnIntervalRange.x, fuelSpawnIntervalRange.y); }

    void SpawnChunk()
    {
        if (chunksSinceLastSwitch >= lengthOfCurrentThemeRun) { SwitchToRandomTheme(); }
        ThemeInfo currentTheme = themes[currentThemeIndex];
        GameObject newChunk = Instantiate(terrainChunkPrefab, new Vector3(spawnX, 0, 0), Quaternion.identity);
        if (fuelCollectiblePrefab != null && nextFuelSpawnX >= spawnX && nextFuelSpawnX < spawnX + chunkLength) { float localX = nextFuelSpawnX - spawnX; float yPos = Mathf.PerlinNoise(nextFuelSpawnX * noiseScale, seed) * terrainHeight; Vector3 spawnPosition = new Vector3(localX, yPos + fuelVerticalOffset, 0); GameObject fuelCan = Instantiate(fuelCollectiblePrefab, newChunk.transform); fuelCan.transform.localPosition = spawnPosition; SetNextFuelSpawnPoint(); }
        GenerateChunkContent(newChunk, currentTheme);
        activeChunks.Enqueue(newChunk);
        spawnX += chunkLength;
        chunksSinceLastSwitch++;
    }

    void DestroyOldestChunk()
    {
        if (activeChunks.Count > chunksVisibleAhead * 2) { GameObject oldestChunk = activeChunks.Dequeue(); Destroy(oldestChunk); }
    }

    // --- THIS IS THE FULLY RESTORED AND CORRECTED METHOD ---
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

            // Mesh Collider Logic (unchanged)
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

            // Top Block Placement (unchanged)
            float automaticOffset = blockSize / 2f;
            float totalOffset = automaticOffset - this.verticalOffset;
            Vector3 finalOffsetVector = rotation * Vector3.up * totalOffset;
            Vector3 topBlockLocalPosition = colliderCenter - finalOffsetVector;
            GameObject topBlock = Instantiate(theme.topBlockPrefab, chunk.transform);
            topBlock.transform.localPosition = topBlockLocalPosition;
            topBlock.transform.localRotation = rotation;

            // Collectible Spawning (unchanged)
            if (collectiblePrefab != null && Random.value < collectibleSpawnChance)
            {
                Vector3 collectiblePosition = topBlock.transform.position + new Vector3(0, collectibleVerticalOffset, 0);
                Instantiate(collectiblePrefab, collectiblePosition, Quaternion.identity, chunk.transform);
            }

            // --- UNDERGROUND GENERATION LOGIC (RESTORED) ---
            Vector3 currentUndergroundPosition = new Vector3(colliderCenter.x, topBlockLocalPosition.y - blockSize, colliderCenter.z);

            // 1. Place the specific layers defined in the theme
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

            // 2. Fill the remaining space down to bedrock with the last layer type
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
            // --- END OF RESTORED LOGIC ---
        }

        // Finalize Mesh Collider (unchanged)
        collisionMesh.vertices = collisionVertices.ToArray();
        collisionMesh.triangles = collisionTriangles.ToArray();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }
}