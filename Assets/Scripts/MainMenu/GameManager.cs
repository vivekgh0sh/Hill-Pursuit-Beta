// --- START OF FILE GameManager.cs (REVISED FOR PURSUIT) ---

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections; // Required for Coroutines

public class GameManager : MonoBehaviour
{
    // ... (Existing variables are the same)
    public static GameManager Instance { get; private set; }
    [Header("Game State")]
    public GameState currentState;
    public Transform playerTransform;
    public int coins;
    public int coinsThisRun { get; private set; }
    public int currentDistance;
    public int highscore;
    private float startPositionX;
    [Header("Car Management")]
    public List<CarData> allCars;
    public int selectedCarIndex = 0;
    [Header("Level Management")]
    public List<string> levelSceneNames;
    public int highestLevelUnlocked { get; private set; }
    private int currentLevelIndex = -1;
    private Dictionary<string, CarData> carDataLookUp;

    // --- ADD THESE NEW VARIABLES ---
    [Header("Pursuit Settings")]
    [Tooltip("The police car prefab to spawn.")]
    public GameObject policeCarPrefab;
    [Tooltip("How long after the level starts (in seconds) before the cop appears.")]
    public float pursuitStartDelay = 8f;
    private PoliceAIController activePoliceCar;
    // --- END OF ADDED VARIABLES ---

    public enum GameState { MainMenu, Playing, Paused, GameOver }

    void Awake() { if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitializeManager(); } else { Destroy(gameObject); } }
    private void InitializeManager() { carDataLookUp = new Dictionary<string, CarData>(); foreach (var car in allCars) { if (!string.IsNullOrEmpty(car.carID) && !carDataLookUp.ContainsKey(car.carID)) { carDataLookUp.Add(car.carID, car); } } LoadGameData(); }
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset the active police car reference on any scene load
        activePoliceCar = null;

        if (scene.name == "endless" || (levelSceneNames != null && levelSceneNames.Contains(scene.name)))
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            currentDistance = 0;
            coinsThisRun = 0;
            currentLevelIndex = levelSceneNames.IndexOf(scene.name);

            // --- ADD THIS LINE ---
            // Start the countdown to spawn the police car
            if (policeCarPrefab != null)
            {
                StartCoroutine(StartPursuitSequence());
            }
        }
        else { currentState = GameState.MainMenu; }
    }

    // --- ADD THIS NEW COROUTINE ---
    private IEnumerator StartPursuitSequence()
    {
        // Wait for the initial delay
        yield return new WaitForSeconds(pursuitStartDelay);

        // This loop will now run for the entire duration of the game
        while (currentState == GameState.Playing)
        {
            // If the cop doesn't exist (or was destroyed), spawn a new one.
            if (activePoliceCar == null && playerTransform != null)
            {
                // Spawn the police car off-screen behind the player
                Vector3 spawnPosition = new Vector3(playerTransform.position.x - 15f, 20f, 0);

                GameObject copGO = Instantiate(policeCarPrefab, spawnPosition, playerTransform.rotation);
                activePoliceCar = copGO.GetComponent<PoliceAIController>();

                if (activePoliceCar != null)
                {
                    activePoliceCar.playerTarget = playerTransform;
                }
                else
                {
                    Debug.LogError("Spawned Police Car Prefab is missing the PoliceAIController script!");
                    yield break; // Exit the coroutine if the prefab is broken
                }
            }
            else if (activePoliceCar != null && playerTransform != null)
            {
                // --- MONITORING LOGIC ---
                // Check if the cop has fallen too far behind or into the void.
                float distanceBehindPlayer = playerTransform.position.x - activePoliceCar.transform.position.x;
                bool isTooFarBehind = distanceBehindPlayer > 150f; // Respawn if 150m behind
                bool hasFallen = activePoliceCar.transform.position.y < -50f; // Respawn if fallen into void

                if (isTooFarBehind || hasFallen)
                {
                    Destroy(activePoliceCar.gameObject);
                    activePoliceCar = null; // Set to null so a new one spawns on the next loop iteration
                }
            }

            // Wait for 2 seconds before checking the cop's status again.
            yield return new WaitForSeconds(2.0f);
        }
    }

    // --- MODIFY THE EndGame METHOD ---
    public void EndGame(string reason = "GAME OVER")
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (currentDistance > highscore)
        {
            highscore = currentDistance;
        }

        GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();
        if (uiController != null)
        {
            // Pass the reason to the UI controller
            uiController.ShowGameOverScreen(currentDistance, highscore, coins, coinsThisRun, reason);
        }

        CommitRunStats();
    }
    // --- END OF MODIFICATION ---

    // ... (The rest of the script, Update, PauseGame, etc., is the same)
    void Update() { if (currentState == GameState.Playing && playerTransform != null) { int newDistance = Mathf.FloorToInt(playerTransform.position.x - startPositionX); currentDistance = Mathf.Max(currentDistance, newDistance); } }
    public void PauseGame() { if (currentState == GameState.Playing) { currentState = GameState.Paused; Time.timeScale = 0f; } }
    public void ResumeGame() { if (currentState == GameState.Paused) { currentState = GameState.Playing; Time.timeScale = 1f; } }
    public void RegisterPlayerStart(Transform player) { playerTransform = player; startPositionX = player.position.x; }
    public void LevelCompleted() { if (currentState != GameState.Playing) return; currentState = GameState.GameOver; Time.timeScale = 0f; if (currentLevelIndex + 1 < levelSceneNames.Count && currentLevelIndex >= highestLevelUnlocked) { highestLevelUnlocked = currentLevelIndex + 1; } bool isLastLevel = (currentLevelIndex >= levelSceneNames.Count - 1); GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>(); if (uiController != null) { uiController.ShowLevelCompleteScreen(currentDistance, coins, coinsThisRun, isLastLevel); } CommitRunStats(); }
    private void CommitRunStats() { coins += coinsThisRun; SaveGameData(); }
    public void LoadNextLevel() { Time.timeScale = 1f; int nextLevel = currentLevelIndex + 1; if (nextLevel < levelSceneNames.Count) { LoadLevel(nextLevel); } else { GoToMenu(); } }
    public void LoadLevel(int levelIndex) { if (levelIndex < levelSceneNames.Count) { currentLevelIndex = levelIndex; SceneManager.LoadScene(levelSceneNames[levelIndex]); } }
    public void CollectCoin(int amount) { if (currentState == GameState.Playing) { coinsThisRun += amount; } }
    public void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void GoToMenu() { Time.timeScale = 1f; SceneManager.LoadScene("VehicleSelectionUI"); }
    public CarData GetSelectedCar() { if (selectedCarIndex >= 0 && selectedCarIndex < allCars.Count) { return allCars[selectedCarIndex]; } return null; }
    public bool CanAfford(int cost) { return coins >= cost; }
    public void SpendCoins(int amount) { coins -= amount; }
    public void UnlockCar(string carID) { PlayerPrefs.SetInt("CarUnlocked_" + carID, 1); }
    public bool IsCarUnlocked(string carID) { CarData car = carDataLookUp.ContainsKey(carID) ? carDataLookUp[carID] : null; if (car != null && car.isUnlockedByDefault) return true; return PlayerPrefs.GetInt("CarUnlocked_" + carID, 0) == 1; }
    public void SaveGameData() { PlayerPrefs.SetInt("Coins", coins); PlayerPrefs.SetInt("Highscore", highscore); PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex); PlayerPrefs.SetInt("HighestLevelUnlocked", highestLevelUnlocked); PlayerPrefs.Save(); }
    public void LoadGameData() { coins = PlayerPrefs.GetInt("Coins", 0); highscore = PlayerPrefs.GetInt("Highscore", 0); selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0); highestLevelUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 0); }
    public int GetUpgradeLevel(string carID, string upgradeID) { return PlayerPrefs.GetInt($"Upgrade_{carID}_{upgradeID}", 0); }
    public void PurchaseUpgrade(CarData car, UpgradeData upgrade) { int currentLevel = GetUpgradeLevel(car.carID, upgrade.upgradeID); if (currentLevel >= upgrade.maxLevel) return; int cost = upgrade.GetCostForLevel(currentLevel + 1); if (CanAfford(cost)) { SpendCoins(cost); PlayerPrefs.SetInt($"Upgrade_{car.carID}_{upgrade.upgradeID}", currentLevel + 1); SaveGameData(); } }
}