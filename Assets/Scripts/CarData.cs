// --- START OF FILE CarData.cs (REVISED FOR UPGRADES) ---

using UnityEngine;
using System.Collections.Generic;

// We define the upgrade structure here so it can be configured in the Inspector.
[System.Serializable]
public class UpgradeData
{
    [Tooltip("The display name for the upgrade, e.g., 'Engine' or 'Fuel Tank'.")]
    public string upgradeName = "New Upgrade";
    [Tooltip("A unique ID used for saving/loading, e.g., 'engine_power'.")]
    public string upgradeID;

    [Header("Upgrade Progression")]
    public int maxLevel = 10;
    [Tooltip("The starting value of the stat at level 0.")]
    public float baseValue;
    [Tooltip("How much the stat increases with each level purchased.")]
    public float valuePerLevel;

    [Header("Cost Progression")]
    [Tooltip("The cost to purchase Level 1.")]
    public int baseCost = 50;
    [Tooltip("How much the cost increases for each subsequent level.")]
    public int costIncreasePerLevel = 25;

    /// <summary>
    /// Calculates the cost for a specific target level (e.g., targetLevel 1 is the first upgrade).
    /// </summary>
    public int GetCostForLevel(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxLevel) return int.MaxValue;
        return baseCost + (costIncreasePerLevel * (targetLevel - 1));
    }
}


[CreateAssetMenu(fileName = "NewCarData", menuName = "Hill Pursuit/Car Data")]
public class CarData : ScriptableObject
{
    [Header("Info")]
    public string carName = "New Car";
    public string carID;

    [Header("Game Objects")]
    public GameObject carPrefab; // The actual car prefab to spawn in-game

    [Header("Store/UI")]
    public Sprite carIcon; // Optional: for UI buttons if needed
    public int unlockCost = 300;
    public bool isUnlockedByDefault = false;

    [Header("Display Settings")]
    [Tooltip("The position offset for this car when shown in the selection screen.")]
    public Vector3 displayPositionOffset = Vector3.zero;
    [Tooltip("The rotation for this car when shown in the selection screen.")]
    public Vector3 displayRotation = Vector3.zero;
    [Tooltip("The scale multiplier for this car when shown in the selection screen.")]
    public float displayScale = 1.0f;

    // --- ADDED FOR UPGRADES ---
    [Header("Upgrades")]
    [Tooltip("A list of all upgradable stats for this car.")]
    public List<UpgradeData> upgrades = new List<UpgradeData>();
    // --- END OF ADDED SECTION ---
}