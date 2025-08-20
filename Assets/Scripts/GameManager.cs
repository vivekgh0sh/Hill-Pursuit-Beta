// --- START OF FILE GameManager.cs (REVISED) ---

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
    private float startPositionX; // ADD THIS LINE to store the starting X position

    [Header("Car Management")]
    public List<CarData> allCars;
    public int selectedCarIndex = 0;

    private Dictionary<string, CarData> carDataLookUp;

    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        carDataLookUp = new Dictionary<string, CarData>();
        foreach (var car in allCars)
        {
            if (!string.IsNullOrEmpty(car.carID) && !carDataLookUp.ContainsKey(car.carID))
            {
                carDataLookUp.Add(car.carID, car);
            }
        }
        LoadGameData();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "endless")
        {
            currentState = GameState.Playing;
            currentDistance = 0;
            coinsThisRun = 0;
        }
        else
        {
            currentState = GameState.MainMenu;
        }
    }

    void Update()
    {
        if (currentState == GameState.Playing && playerTransform != null)
        {
            // --- CHANGE THIS CALCULATION ---
            // Calculate distance relative to the starting point
            int newDistance = Mathf.FloorToInt(playerTransform.position.x - startPositionX);
            // --- END OF CHANGE ---

            currentDistance = Mathf.Max(currentDistance, newDistance);
        }
    }

    // --- ADD THIS NEW METHOD ---
    // The PlayerSpawner will call this to set up the player correctly
    public void RegisterPlayerStart(Transform player)
    {
        playerTransform = player;
        startPositionX = player.position.x;
    }
    // --- END OF NEW METHOD ---

    public void EndGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;

            if (currentDistance > highscore)
            {
                highscore = currentDistance;
            }

            GameplayUIController uiController = FindFirstObjectByType<GameplayUIController>();
            if (uiController != null)
            {
                uiController.ShowGameOverScreen(currentDistance, highscore, coins, coinsThisRun);
            }

            coins += coinsThisRun;
            SaveGameData();
        }
    }

    public void CollectCoin(int amount)
    {
        if (currentState == GameState.Playing)
        {
            coinsThisRun += amount;
        }
    }

    #region Existing Methods

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("VehicleSelectionScene");
    }

    public CarData GetSelectedCar()
    {
        if (selectedCarIndex >= 0 && selectedCarIndex < allCars.Count)
        {
            return allCars[selectedCarIndex];
        }
        return null;
    }

    public bool CanAfford(int cost) { return coins >= cost; }
    public void SpendCoins(int amount) { coins -= amount; }
    public void UnlockCar(string carID) { PlayerPrefs.SetInt("CarUnlocked_" + carID, 1); }

    public bool IsCarUnlocked(string carID)
    {
        CarData car = carDataLookUp.ContainsKey(carID) ? carDataLookUp[carID] : null;
        if (car != null && car.isUnlockedByDefault) return true;
        return PlayerPrefs.GetInt("CarUnlocked_" + carID, 0) == 1;
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Highscore", highscore);
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        PlayerPrefs.Save();
    }

    public void LoadGameData()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        highscore = PlayerPrefs.GetInt("Highscore", 0);
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
    }
    #endregion
}