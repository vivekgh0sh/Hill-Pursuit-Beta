// --- CREATE NEW FILE: ResetDataEditor.cs (place in "Editor" folder) ---

using UnityEditor;
using UnityEngine;

public class ResetDataEditor
{
    // This creates a new menu item in the Unity Editor under "Tools/Hill Pursuit/Reset Data".
    [MenuItem("Tools/Hill Pursuit/Reset Car Unlocks (Keep Coins/Score)")]
    public static void ResetCarUnlocks()
    {
        // First, we need to find the GameManager to get the list of all car IDs.
        // This code finds the PREFAB of your GameManager in the project files.
        // Make sure you have a prefab of your configured GameManager object!
        // If not, simply drag your GameManager from the Hierarchy into the Project window to create one.

        GameManager gameManagerPrefab = null;
        string[] guids = AssetDatabase.FindAssets("t:Prefab GameManager"); // Search for a prefab named GameManager
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            gameManagerPrefab = AssetDatabase.LoadAssetAtPath<GameManager>(path);
        }

        if (gameManagerPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find the GameManager prefab in your project. Please create one.", "OK");
            return;
        }

        // Now, loop through every car in the GameManager's list
        foreach (CarData car in gameManagerPrefab.allCars)
        {
            if (!string.IsNullOrEmpty(car.carID))
            {
                // Construct the exact key that is used for saving
                string key = "CarUnlocked_" + car.carID;

                // Delete that specific key from PlayerPrefs
                PlayerPrefs.DeleteKey(key);
                Debug.Log("Deleted PlayerPrefs key: " + key);
            }
        }

        // Let the developer know it's done.
        EditorUtility.DisplayDialog("Success", "All car unlock data has been reset. Your coins and high score are safe.", "OK");
    }

    [MenuItem("Tools/Hill Pursuit/Reset ALL DATA (Deletes Everything!)")]
    public static void ResetAllData()
    {
        if (EditorUtility.DisplayDialog("Are you sure?",
            "This will delete ALL saved data, including coins, high score, and car unlocks. This cannot be undone.",
            "Yes, Delete Everything", "Cancel"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All PlayerPrefs data has been deleted.");
            EditorUtility.DisplayDialog("Success", "All saved data has been reset.", "OK");
        }
    }
}