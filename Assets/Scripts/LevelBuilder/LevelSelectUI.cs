// --- START OF FILE LevelSelectUI.cs (REVISED FOR PHASE DATA) ---

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Required for List

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button backButton;
    [SerializeField] private Image background; // --- ADD THIS ---

    [Header("Pagination")]
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private TextMeshProUGUI pageText; // This will now be our phase title
    [SerializeField] private int levelsPerPage = 25;

    // --- ADD THIS NEW LIST ---
    [Header("Phase Data")]
    [Tooltip("Assign your PhaseData assets here in order (Phase 1, Phase 2, etc.).")]
    [SerializeField] private List<PhaseData> phases;
    // --- END OF ADDED LIST ---

    private int currentPage = 0;
    private int totalPages = 0;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        int totalLevels = GameManager.Instance.levelSceneNames.Count;
        totalPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);

        nextPageButton.onClick.AddListener(NextPage);
        prevPageButton.onClick.AddListener(PreviousPage);
        backButton.onClick.AddListener(GoBack);

        DisplayCurrentPage();
    }

    private void DisplayCurrentPage()
    {
        // 1. Clear any old buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Calculate which levels to show
        int startLevelIndex = currentPage * levelsPerPage;
        int endLevelIndex = Mathf.Min(startLevelIndex + levelsPerPage - 1, GameManager.Instance.levelSceneNames.Count - 1);
        int highestUnlocked = GameManager.Instance.highestLevelUnlocked;

        // 3. Create the buttons for the current page
        for (int i = startLevelIndex; i <= endLevelIndex; i++)
        {
            GameObject buttonGO = Instantiate(levelButtonPrefab, buttonContainer);
            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();
            bool isUnlocked = (i <= highestUnlocked);
            levelButton.Setup(i, isUnlocked);
        }

        // 4. Update navigation and visuals
        UpdatePageVisuals();
    }

    // --- RENAMED and MODIFIED this method ---
    private void UpdatePageVisuals()
    {
        // Check if the phases list is set up correctly for the current page
        if (phases != null && currentPage < phases.Count)
        {
            // Get the specific PhaseData asset for the current page
            PhaseData currentPhaseData = phases[currentPage];

            // --- Update Phase Title ---
            if (pageText != null)
            {
                pageText.text = currentPhaseData.phaseName;
            }

            // --- Update Background Image (WITH DEBUGGING) ---
            if (background != null) // First, check if the UI Image reference is assigned in the Inspector
            {
                // Second, check if the PhaseData asset actually has a background sprite assigned
                if (currentPhaseData.backgroundImage != null)
                {
                    // If everything is valid, print a success message to the console and set the sprite
                    Debug.Log($"SUCCESS: Setting background for '{currentPhaseData.phaseName}' using sprite '{currentPhaseData.backgroundImage.name}'.");
                    background.sprite = currentPhaseData.backgroundImage;
                }
                else
                {
                    // If the sprite is missing on the PhaseData asset, show a warning
                    Debug.LogWarning($"WARNING: The PhaseData asset for '{currentPhaseData.phaseName}' is missing its background image sprite!");
                }
            }
            else
            {
                // If the UI Image reference is not assigned at all, show an error
                Debug.LogError("ERROR: The 'background' Image reference is NULL. Please drag the Background Image from the Hierarchy into the script's slot in the Inspector.");
            }
        }
        else
        {
            // This is a fallback in case the phases list isn't set up correctly.
            // It will just display the default "Phase X / Y" text.
            if (pageText != null)
            {
                pageText.text = $"Phase {currentPage + 1} / {totalPages}";
            }
            Debug.LogWarning($"Could not find PhaseData for page {currentPage}. Displaying default text.");
        }

        // This part remains the same: update the navigation buttons
        prevPageButton.gameObject.SetActive(currentPage > 0);
        nextPageButton.gameObject.SetActive(currentPage < totalPages - 1);
    }
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            DisplayCurrentPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
        }
    }

    void GoBack()
    {
        GameManager.Instance.GoToMenu();
    }
}