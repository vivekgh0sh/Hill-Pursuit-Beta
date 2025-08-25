// --- START OF FILE GameManager.cs (REVISED FOR PAUSE) ---

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState;
    public Transform playerTransform;

    // ... (Player Stats, Car Management, and Level Management variables are the same)
    public int coins;
    public int coinsThisRun { get; private set; }
    public int currentDistance;
    public int highscore;
    private float startPositionX;
    public List<CarData> allCars;
    public int selectedCarIndex = 0;
    public List<string> levelSceneNames;
    public int highestLevelUnlocked { get; private set; }
    private int currentLevelIndex = -1;
    private Dictionary<string, CarData> carDataLookUp;


    // --- ADD "Paused" TO THE ENUM ---
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused, // New state
        GameOver
    }

    // ... (Awake, InitializeManager, OnEnable, OnDisable methods are the same)
    void Awake() { if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); InitializeManager(); } else { Destroy(gameObject); } }
    private void InitializeManager() { carDataLookUp = new Dictionary<string, CarData>(); foreach (var car in allCars) { if (!string.IsNullOrEmpty(car.carID) && !carDataLookUp.ContainsKey(car.carID)) { carDataLookUp.Add(car.carID, car); } } LoadGameData(); }
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "endless" || (levelSceneNames != null && levelSceneNames.Contains(scene.name)))
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f; // Ensure time is running when a level starts
            currentDistance = 0;
            coinsThisRun = 0;
            currentLevelIndex = levelSceneNames.IndexOf(scene.name);
        }
        else { currentState = GameState.MainMenu; }
    }

    void Update()
    {
        // Only track distance when actively playing
        if (currentState == GameState.Playing && playerTransform != null)
        {
            int newDistance = Mathf.FloorToInt(playerTransform.position.x - startPositionX);
            currentDistance = Mathf.Max(currentDistance, newDistance);
        }
    }

    // --- ADD PAUSE/RESUME METHODS ---
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f; // Freeze time
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f; // Unfreeze time
        }
    }
    // --- END OF NEW METHODS ---

    // ... (The rest of the script, including EndGame, LevelCompleted, etc., is the same)
    public void RegisterPlayerStart(Transform player) { playerTransform = player; startPositionX = player.position.x; }
    public void EndGame() { if (currentState != GameState.Playing) return; currentState = GameState.GameOver; Time.timeScale = 0f; if (currentDistance > highscore) { highscore = currentDistance; } GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>(); if (uiController != null) { uiController.ShowGameOverScreen(currentDistance, highscore, coins, coinsThisRun); } CommitRunStats(); }
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
}