// --- START OF FILE LevelBuilder.cs (DEFINITIVE MESH COLLIDER VERSION) ---

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
    [Header("Level Design")]
    public ThemeInfo theme;
    public float levelLength = 500f;
    public float terrainSeed = 1;

    [Header("Object Placement")]
    public GameObject starPrefab;
    [Range(0f, 1f)] public float starSpawnChance = 0.15f;
    public float starVerticalOffset = 1.5f;
    public GameObject fuelPrefab;
    public float fuelSpawnInterval = 400f;
    public float fuelVerticalOffset = 1.0f;

    [Header("TERRAIN SETTINGS (MATCH ENDLESS SCENE)")]
    public float terrainHeight = 10f;
    public float noiseScale = 0.07f;
    public float blockDensity = 1.0f;
    public float groundBedrockLevel = -15f;
    public float verticalOffset = 0f; // Fine-tuning knob from original generator

    [ContextMenu("Generate Level")]
    private void GenerateLevel()
    {
        ClearLevel();

        if (theme == null || theme.topBlockPrefab == null)
        {
            Debug.LogError("Theme or its Top Block Prefab is not assigned!");
            return;
        }

        Transform container = this.transform;

        // --- Generate Terrain using the EXACT logic from TerrainGenerator ---
        GenerateTerrainWithSmoothCollider(container);

        // --- Spawn Fuel Cans ---
        if (fuelPrefab != null)
        {
            for (float x = fuelSpawnInterval; x < levelLength; x += fuelSpawnInterval)
            {
                float y = Mathf.PerlinNoise(x * noiseScale, terrainSeed) * terrainHeight;
                Vector3 spawnPos = new Vector3(x, y + fuelVerticalOffset, 0);
                Instantiate(fuelPrefab, spawnPos, Quaternion.identity, container);
            }
        }

        Debug.Log("Level generation complete with smooth MeshCollider!");
    }

    [ContextMenu("Clear Level")]
    private void ClearLevel()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void GenerateTerrainWithSmoothCollider(Transform container)
    {
        float blockSize = theme.topBlockPrefab.transform.localScale.x;
        int blocksToSpawn = Mathf.CeilToInt(levelLength / blockSize * blockDensity);
        float placementStep = levelLength / blocksToSpawn;

        // Create a dedicated object for the physics mesh
        GameObject physicsChunk = new GameObject("TerrainPhysicsCollider");
        physicsChunk.transform.SetParent(container);
        MeshCollider meshCollider = physicsChunk.AddComponent<MeshCollider>();

        List<Vector3> collisionVertices = new List<Vector3>();
        List<int> collisionTriangles = new List<int>();

        for (int i = 0; i < blocksToSpawn; i++)
        {
            float xPos = i * placementStep;
            float yPos = Mathf.PerlinNoise(xPos * noiseScale, terrainSeed) * terrainHeight;
            float nextXPos = xPos + 0.1f; // Small step forward to calculate angle
            float nextYPos = Mathf.PerlinNoise(nextXPos * noiseScale, terrainSeed) * terrainHeight;
            float angle = Mathf.Atan2(nextYPos - yPos, nextXPos - xPos) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // --- 1. MESH COLLIDER LOGIC (Copied directly from TerrainGenerator) ---
            Vector3 colliderCenter = new Vector3(xPos, yPos, 0);
            Vector3 halfWidth = rotation * Vector3.right * (blockSize / 2f);
            Vector3 halfDepth = Vector3.forward * 10; // Match Z-depth for collision
            Vector3 topLeft = colliderCenter - halfWidth - halfDepth;
            Vector3 topRight = colliderCenter + halfWidth - halfDepth;
            Vector3 btmLeft = colliderCenter - halfWidth + halfDepth;
            Vector3 btmRight = colliderCenter + halfWidth + halfDepth;
            int vertIndex = collisionVertices.Count;
            collisionVertices.AddRange(new[] { topLeft, topRight, btmLeft, btmRight });
            collisionTriangles.AddRange(new[] { vertIndex, vertIndex + 3, vertIndex + 1, vertIndex, vertIndex + 2, vertIndex + 3 });

            // --- 2. VISUAL BLOCKS LOGIC (Copied directly from TerrainGenerator) ---
            float automaticOffset = blockSize / 2f;
            float totalOffset = automaticOffset - this.verticalOffset;
            Vector3 finalOffsetVector = rotation * Vector3.up * totalOffset;
            Vector3 topBlockLocalPosition = colliderCenter - finalOffsetVector;
            GameObject topBlock = Instantiate(theme.topBlockPrefab, container);
            topBlock.transform.position = topBlockLocalPosition; // Use world position
            topBlock.transform.rotation = rotation;

            // Spawn Stars
            if (starPrefab != null && Random.value < starSpawnChance)
            {
                Vector3 starPos = topBlock.transform.position + new Vector3(0, starVerticalOffset, 0);
                Instantiate(starPrefab, starPos, Quaternion.identity, container);
            }

            // Spawn underground blocks
            Vector3 currentUndergroundPosition = new Vector3(colliderCenter.x, topBlockLocalPosition.y - blockSize, colliderCenter.z);
            if (theme.undergroundBlockPrefabs != null)
            {
                // Fill down to bedrock with the deepest layer
                GameObject lastLayerPrefab = theme.undergroundBlockPrefabs[theme.undergroundBlockPrefabs.Count - 1];
                while (currentUndergroundPosition.y > groundBedrockLevel)
                {
                    GameObject fillerBlock = Instantiate(lastLayerPrefab, container);
                    fillerBlock.transform.position = currentUndergroundPosition;
                    fillerBlock.transform.rotation = Quaternion.identity;
                    currentUndergroundPosition.y -= blockSize;
                }
            }
        }

        // Finalize and apply the mesh to the collider
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = collisionVertices.ToArray();
        collisionMesh.triangles = collisionTriangles.ToArray();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }
}