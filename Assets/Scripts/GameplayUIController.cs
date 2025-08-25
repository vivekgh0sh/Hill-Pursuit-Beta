// --- START OF FILE GameplayUIController.cs (REVISED FOR PAUSE) ---

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameplayUIController : MonoBehaviour
{
    [Header("In-Game UI")]
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Slider boostSlider;
    [SerializeField] private Button flipButton;
    [SerializeField] private BoostButtonHandler boostButtonHandler;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI distanceText;

    // --- ADD PAUSE MENU VARIABLES ---
    [Header("Pause Menu")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pause_MainMenuButton;
    // --- END OF ADDED VARIABLES ---

    [Header("Game Over / Level Complete UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private TextMeshProUGUI finalDistanceText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI runCoinsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button nextLevelButton;

    private CarController _carController;
    public CarController CarController { get { return _carController; } set { _carController = value; if (boostButtonHandler != null) { boostButtonHandler.carController = _carController; } } }

    void Start()
    {
        // Hide panels at the start
        resultPanel.SetActive(false);
        pausePanel.SetActive(false);

        // Add listeners for all buttons
        flipButton.onClick.AddListener(() => { if (CarController != null) CarController.PerformFlip(); });
        restartButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.RestartGame(); });
        menuButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.GoToMenu(); });
        nextLevelButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.LoadNextLevel(); });

        // --- ADD PAUSE LISTENERS ---
        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        pause_MainMenuButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.GoToMenu(); });
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (CarController != null)
        {
            fuelSlider.value = CarController.FuelPercent;
            boostSlider.value = CarController.BoostPercent;
        }
        coinText.text = GameManager.Instance.coinsThisRun.ToString();
        distanceText.text = GameManager.Instance.currentDistance.ToString() + "m";
    }

    // --- ADD PAUSE/RESUME UI METHODS ---
    private void Pause()
    {
        GameManager.Instance.PauseGame();
        pausePanel.SetActive(true);
    }

    private void Resume()
    {
        GameManager.Instance.ResumeGame();
        pausePanel.SetActive(false);
    }
    // --- END OF NEW METHODS ---

    public void ShowGameOverScreen(int finalDistance, int highscore, int previousTotalCoins, int collectedInRun)
    {
        pauseButton.gameObject.SetActive(false); // Hide pause button on results screen
        resultPanel.SetActive(true);
        resultTitleText.text = "GAME OVER";
        finalDistanceText.text = "Distance: " + finalDistance.ToString() + "m";
        highscoreText.text = "Highscore: " + highscore.ToString() + "m";

        restartButton.gameObject.SetActive(true);
        nextLevelButton.gameObject.SetActive(false);
        highscoreText.gameObject.SetActive(true);

        StartCoroutine(AnimateCoins(previousTotalCoins, collectedInRun));
    }

    public void ShowLevelCompleteScreen(int finalDistance, int previousTotalCoins, int collectedInRun, bool isLastLevel)
    {
        pauseButton.gameObject.SetActive(false); // Hide pause button on results screen
        resultPanel.SetActive(true);
        resultTitleText.text = "LEVEL COMPLETE!";
        finalDistanceText.text = "Distance: " + finalDistance.ToString() + "m";

        restartButton.gameObject.SetActive(false);
        nextLevelButton.gameObject.SetActive(true);
        highscoreText.gameObject.SetActive(false);

        if (isLastLevel) { nextLevelButton.interactable = false; }

        StartCoroutine(AnimateCoins(previousTotalCoins, collectedInRun));
    }

    // ... (AnimateCoins coroutine is the same)
    private IEnumerator AnimateCoins(int startTotal, int collected) { restartButton.interactable = false; menuButton.interactable = false; nextLevelButton.interactable = false; float duration = 2.5f; float elapsedTime = 0f; int finalTotal = startTotal + collected; while (elapsedTime < duration) { elapsedTime += Time.unscaledDeltaTime; float progress = elapsedTime / duration; int currentRunDisplay = (int)Mathf.Lerp(collected, 0, progress); int currentTotalDisplay = (int)Mathf.Lerp(startTotal, finalTotal, progress); runCoinsText.text = "Gained: + " + currentRunDisplay.ToString(); totalCoinsText.text = "Stars: " + currentTotalDisplay.ToString(); yield return null; } runCoinsText.text = "Gained: + 0"; totalCoinsText.text = "Stars: " + finalTotal.ToString(); restartButton.interactable = true; menuButton.interactable = true; nextLevelButton.interactable = true; }
}