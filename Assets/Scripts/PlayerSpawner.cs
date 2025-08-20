// --- START OF FILE PlayerSpawner.cs (REVISED) ---

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

        if (GameManager.Instance == null) { return; }
        CarData selectedCar = GameManager.Instance.GetSelectedCar();
        if (selectedCar == null) { return; }

        GameObject playerCar = Instantiate(selectedCar.carPrefab, spawnPosition, spawnRotation);
        playerCar.name = "Player - " + selectedCar.carName;

        // --- ADD THIS LINE ---
        GameManager.Instance.RegisterPlayerStart(playerCar.transform);
        // --- END OF ADDED LINE ---

        if (gameplayUIController != null)
        {
            CarController car = playerCar.GetComponent<CarController>();
            gameplayUIController.CarController = car;
        }

        if (virtualCamera != null) { virtualCamera.Follow = playerCar.transform; virtualCamera.LookAt = playerCar.transform; }
        if (terrainGenerator != null) { terrainGenerator.player = playerCar.transform; }
    }
}