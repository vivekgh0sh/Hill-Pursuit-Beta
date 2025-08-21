// --- CREATE NEW FILE: LevelSelectUI.cs ---

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer; // This is the 'Content' object of the ScrollView
    [SerializeField] private Button backButton;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Cannot populate level select screen.");
            // Optionally load the boot scene to fix this
            // SceneManager.LoadScene("Boot"); 
            return;
        }

        PopulateLevels();
        backButton.onClick.AddListener(GoBack);
    }

    void PopulateLevels()
    {
        // Clear any old buttons that might be there (for editor testing)
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        int highestUnlocked = GameManager.Instance.highestLevelUnlocked;
        var allLevels = GameManager.Instance.levelSceneNames;

        for (int i = 0; i < allLevels.Count; i++)
        {
            // Create a new button from the prefab
            GameObject buttonGO = Instantiate(levelButtonPrefab, buttonContainer);

            // Get the LevelButton script component from it
            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();

            // Determine if this level is unlocked
            bool isUnlocked = (i <= highestUnlocked);

            // Set it up
            levelButton.Setup(i, isUnlocked);
        }
    }

    void GoBack()
    {
        // Use the GameManager's existing method to go to the menu
        GameManager.Instance.GoToMenu();
    }
}