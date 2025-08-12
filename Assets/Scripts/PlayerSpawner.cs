using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    // We will now get a reference to the camera instead of a spawn point
    [SerializeField] private CinemachineCamera virtualCamera;

    // --- We no longer need the 'spawnPoint' Transform ---
    // [SerializeField] private Transform spawnPoint;

    void Start()
    {
        // --- Hardcoded Spawn Position and Rotation ---
        Vector3 spawnPosition = new Vector3(50f, 20f, 0f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 90f, 0f);

        if (GameManager.Instance == null)
        {
            Debug.LogError("PlayerSpawner cannot find GameManager. Make sure the VehicleSelectionScene is run first.");
            return;
        }

        CarData selectedCar = GameManager.Instance.GetSelectedCar();

        if (selectedCar == null)
        {
            Debug.LogError("No car was selected in the GameManager.");
            return;
        }

        // Spawn the selected car using our new hardcoded values
        GameObject playerCar = Instantiate(selectedCar.carPrefab, spawnPosition, spawnRotation);
        playerCar.name = "Player - " + selectedCar.carName;

        // --- NEW: Assign the camera's target ---
        if (virtualCamera != null)
        {
            // Tell the Cinemachine camera to Follow and Look At the car's transform
            virtualCamera.Follow = playerCar.transform;
            virtualCamera.LookAt = playerCar.transform;
        }
        else
        {
            Debug.LogError("The Virtual Camera has not been assigned in the PlayerSpawner's Inspector!", this);
        }

        // This part remains the same
        if (terrainGenerator != null)
        {
            terrainGenerator.player = playerCar.transform;
        }
        else
        {
            Debug.LogError("PlayerSpawner is missing a reference to the TerrainGenerator!", this);
        }
    }
}