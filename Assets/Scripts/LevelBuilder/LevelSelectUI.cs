// --- START OF FILE LevelSelectUI.cs (PAGINATED VERSION) ---

using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure this is included

public class LevelSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private Transform buttonContainer; // The 'Content' object of the ScrollView
    [SerializeField] private Button backButton;

    [Header("Pagination")]
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private int levelsPerPage = 25; // 5x5 grid = 25

    private int currentPage = 0;
    private int totalPages = 0;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        // Calculate total pages based on the number of levels in GameManager
        int totalLevels = GameManager.Instance.levelSceneNames.Count;
        totalPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);

        // Add listeners to the navigation buttons
        nextPageButton.onClick.AddListener(NextPage);
        prevPageButton.onClick.AddListener(PreviousPage);
        backButton.onClick.AddListener(GoBack);

        // Display the first page
        DisplayCurrentPage();
    }

    private void DisplayCurrentPage()
    {
        // 1. Clear any old buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Calculate which levels to show on this page
        int startLevelIndex = currentPage * levelsPerPage;
        int endLevelIndex = Mathf.Min(startLevelIndex + levelsPerPage - 1, GameManager.Instance.levelSceneNames.Count - 1);

        int highestUnlocked = GameManager.Instance.highestLevelUnlocked;

        // 3. Create the buttons for the current page
        for (int i = startLevelIndex; i <= endLevelIndex; i++)
        {
            GameObject buttonGO = Instantiate(levelButtonPrefab, buttonContainer);
            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();

            bool isUnlocked = (i <= highestUnlocked);
            levelButton.Setup(i, isUnlocked); // The LevelButton script still uses the global index (0-99)
        }

        // 4. Update navigation UI
        UpdateNavigationUI();
    }

    private void UpdateNavigationUI()
    {
        // Update the page text
        if (pageText != null)
        {
            pageText.text = $"Phase {currentPage + 1} / {totalPages}";
        }

        // Enable/disable navigation buttons
        prevPageButton.interactable = (currentPage > 0);
        nextPageButton.interactable = (currentPage < totalPages - 1);
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