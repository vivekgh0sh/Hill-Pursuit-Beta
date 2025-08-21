// --- START OF FILE GameManager.cs (REVISED FOR LEVEL COMPLETE) ---

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState;
    public Transform playerTransform;

    [Header("Player Stats")]
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
    public enum GameState { MainMenu, Playing, GameOver }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else { Destroy(gameObject); }
    }

    private void InitializeManager()
    {
        carDataLookUp = new Dictionary<string, CarData>();
        foreach (var car in allCars) { if (!string.IsNullOrEmpty(car.carID) && !carDataLookUp.ContainsKey(car.carID)) { carDataLookUp.Add(car.carID, car); } }
        LoadGameData();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "endless" || (levelSceneNames != null && levelSceneNames.Contains(scene.name)))
        {
            currentState = GameState.Playing;
            currentDistance = 0;
            coinsThisRun = 0;
            // Store the current level index if we're in a level scene
            currentLevelIndex = levelSceneNames.IndexOf(scene.name);
        }
        else { currentState = GameState.MainMenu; }
    }

    void Update()
    {
        if (currentState == GameState.Playing && playerTransform != null)
        {
            int newDistance = Mathf.FloorToInt(playerTransform.position.x - startPositionX);
            currentDistance = Mathf.Max(currentDistance, newDistance);
        }
    }

    public void RegisterPlayerStart(Transform player)
    {
        playerTransform = player;
        startPositionX = player.position.x;
    }

    // Called when fuel runs out or car is stuck
    public void EndGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (currentDistance > highscore) { highscore = currentDistance; }

        GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();
        if (uiController != null)
        {
            uiController.ShowGameOverScreen(currentDistance, highscore, coins, coinsThisRun);
        }
        CommitRunStats();
    }

    // Called by the FinishLine script
    public void LevelCompleted()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver; // Use same state to pause game
        Time.timeScale = 0f;

        // Unlock the next level if applicable
        if (currentLevelIndex + 1 < levelSceneNames.Count && currentLevelIndex >= highestLevelUnlocked)
        {
            highestLevelUnlocked = currentLevelIndex + 1;
        }

        // Check if this was the very last level
        bool isLastLevel = (currentLevelIndex >= levelSceneNames.Count - 1);

        GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();
        if (uiController != null)
        {
            uiController.ShowLevelCompleteScreen(currentDistance, coins, coinsThisRun, isLastLevel);
        }
        CommitRunStats();
    }

    // Helper method to reduce code duplication
    private void CommitRunStats()
    {
        coins += coinsThisRun;
        SaveGameData();
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel < levelSceneNames.Count)
        {
            LoadLevel(nextLevel);
        }
        else
        {
            // If there is no next level, just go to the menu
            GoToMenu();
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < levelSceneNames.Count)
        {
            currentLevelIndex = levelIndex;
            SceneManager.LoadScene(levelSceneNames[levelIndex]);
        }
    }

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