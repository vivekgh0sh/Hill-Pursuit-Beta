using UnityEngine;

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
}