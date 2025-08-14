using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    // We will now get a reference to the camera instead of a spawn point
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private GameplayUIController gameplayUIController;
    // --- We no longer need the 'spawnPoint' Transform ---
    // [SerializeField] private Transform spawnPoint;

    void Start()
    {
        Vector3 spawnPosition = new Vector3(50f, 20f, 0f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 90f, 0f);

        if (GameManager.Instance == null) { /* ... error handling ... */ return; }
        CarData selectedCar = GameManager.Instance.GetSelectedCar();
        if (selectedCar == null) { /* ... error handling ... */ return; }

        GameObject playerCar = Instantiate(selectedCar.carPrefab, spawnPosition, spawnRotation);
        playerCar.name = "Player - " + selectedCar.carName;

        // --- NEW: Connect the spawned car to the UI ---
        if (gameplayUIController != null)
        {
            // Get the CarController component from the new car
            CarController car = playerCar.GetComponent<CarController>();
            // Assign it to the UI controller's public variable
            gameplayUIController.carController = car;
        }
        else
        {
            Debug.LogError("GameplayUIController not assigned in the PlayerSpawner!");
        }

        // This part stays the same
        if (virtualCamera != null) { virtualCamera.Follow = playerCar.transform; virtualCamera.LookAt = playerCar.transform; }
        if (terrainGenerator != null) { terrainGenerator.player = playerCar.transform; }
    }
}