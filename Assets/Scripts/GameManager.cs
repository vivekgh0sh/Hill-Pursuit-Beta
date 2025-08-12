using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Car Management")]
    public List<CarData> allCars;
    public int selectedCarIndex = 0;

    // --- NEW: CURRENCY MANAGEMENT ---
    [Header("Player Data")]
    public int playerCoins = 1000; // Start the player with some coins for testing

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadGameData();
    }

    public CarData GetSelectedCar()
    {
        // Make sure we have cars in our list and the index is valid
        if (allCars != null && allCars.Count > 0 && selectedCarIndex < allCars.Count)
        {
            return allCars[selectedCarIndex];
        }

        // If something is wrong, return null to prevent further errors
        Debug.LogError("Could not get selected car! Check if 'allCars' list is populated in the GameManager and if 'selectedCarIndex' is valid.");
        return null;
    }

    // --- NEW: Functions to manage coins ---
    public void AddCoins(int amount)
    {
        playerCoins += amount;
        // In a real game, you would update the UI here
        SaveGameData();
    }

    public bool CanAfford(int amount)
    {
        return playerCoins >= amount;
    }

    public void SpendCoins(int amount)
    {
        playerCoins -= amount;
        SaveGameData();
    }

    // This function stays the same
    public void UnlockCar(string carID)
    {
        PlayerPrefs.SetInt("Car_Unlocked_" + carID, 1);
        PlayerPrefs.Save();
    }

    // This function stays the same
    public bool IsCarUnlocked(string carID)
    {
        CarData car = allCars.Find(c => c.carID == carID);
        if (car != null && car.isUnlockedByDefault)
        {
            return true;
        }
        return PlayerPrefs.GetInt("Car_Unlocked_" + carID, 0) == 1;
    }

    // --- UPDATED: Save and Load player coins ---
    public void SaveGameData()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        PlayerPrefs.SetInt("PlayerCoins", playerCoins); // Save coins
        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 1000); // Load coins, with a default of 1000
    }
}