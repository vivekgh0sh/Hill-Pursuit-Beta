// --- START OF FILE PlayerSpawner.cs (REVISED FOR UPGRADES) ---

using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private GameplayUIController gameplayUIController;

    void Start()
    {
        Vector3 spawnPosition = new Vector3(50f, 20f, 0f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 90f, 0f);

        if (GameManager.Instance == null) { Debug.LogError("GameManager not found!"); return; }
        CarData selectedCarData = GameManager.Instance.GetSelectedCar();
        if (selectedCarData == null) { Debug.LogError("No car selected in GameManager!"); return; }
        if (selectedCarData.carPrefab == null) { Debug.LogError("Selected CarData has no prefab!"); return; }

        GameObject playerCar = Instantiate(selectedCarData.carPrefab, spawnPosition, spawnRotation);
        playerCar.name = "Player - " + selectedCarData.carName;

        GameManager.Instance.RegisterPlayerStart(playerCar.transform);

        CarController carController = playerCar.GetComponent<CarController>();
        if (carController != null)
        {
            // --- ADD THIS LINE ---
            // Initialize the car with its specific data to apply upgrades.
            carController.Initialize(selectedCarData);
            // --- END OF ADDED LINE ---

            if (gameplayUIController != null)
            {
                gameplayUIController.CarController = carController;
            }
        }
        else
        {
            Debug.LogError("Spawned car prefab is missing a CarController component!");
        }

        if (virtualCamera != null) { virtualCamera.Follow = playerCar.transform; virtualCamera.LookAt = playerCar.transform; }
        if (terrainGenerator != null) { terrainGenerator.player = playerCar.transform; }
    }
}