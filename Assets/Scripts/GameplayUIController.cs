// --- START OF FILE GameplayUIController.cs (REVISED) ---

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

    [Header("Game Over / Level Complete UI")]
    [SerializeField] private GameObject resultPanel; // Renamed for clarity
    [SerializeField] private TextMeshProUGUI resultTitleText; // To show "GAME OVER" or "LEVEL COMPLETE"
    [SerializeField] private TextMeshProUGUI finalDistanceText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI runCoinsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button nextLevelButton; // ADD THIS NEW BUTTON

    private CarController _carController;
    public CarController CarController
    {
        get { return _carController; }
        set
        {
            _carController = value;
            if (boostButtonHandler != null)
            {
                boostButtonHandler.carController = _carController;
            }
        }
    }

    void Start()
    {
        resultPanel.SetActive(false);

        flipButton.onClick.AddListener(() => { if (CarController != null) CarController.PerformFlip(); });
        restartButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.RestartGame(); });
        menuButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.GoToMenu(); });
        nextLevelButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.LoadNextLevel(); }); // Add listener for the new button
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

    // This is for when you run out of fuel or get stuck
    public void ShowGameOverScreen(int finalDistance, int highscore, int previousTotalCoins, int collectedInRun)
    {
        resultPanel.SetActive(true);
        resultTitleText.text = "GAME OVER";
        finalDistanceText.text = "Distance: " + finalDistance.ToString() + "m";
        highscoreText.text = "Highscore: " + highscore.ToString() + "m";

        restartButton.gameObject.SetActive(true);
        nextLevelButton.gameObject.SetActive(false); // Hide "Next Level" button
        highscoreText.gameObject.SetActive(true); // Show highscore on game over

        StartCoroutine(AnimateCoins(previousTotalCoins, collectedInRun));
    }

    // --- ADD THIS NEW METHOD ---
    // This is for when you reach the finish line
    public void ShowLevelCompleteScreen(int finalDistance, int previousTotalCoins, int collectedInRun, bool isLastLevel)
    {
        resultPanel.SetActive(true);
        resultTitleText.text = "LEVEL COMPLETE!";
        finalDistanceText.text = "Distance: " + finalDistance.ToString() + "m";

        restartButton.gameObject.SetActive(false); // Hide "Restart" button
        nextLevelButton.gameObject.SetActive(true); // Show "Next Level" button
        highscoreText.gameObject.SetActive(false); // No need to show highscore on level complete

        // If this was the last level, disable the "Next Level" button
        if (isLastLevel)
        {
            nextLevelButton.interactable = false;
        }

        StartCoroutine(AnimateCoins(previousTotalCoins, collectedInRun));
    }

    private IEnumerator AnimateCoins(int startTotal, int collected)
    {
        restartButton.interactable = false;
        menuButton.interactable = false;
        nextLevelButton.interactable = false;

        float duration = 2.5f;
        float elapsedTime = 0f;
        int finalTotal = startTotal + collected;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            int currentRunDisplay = (int)Mathf.Lerp(collected, 0, progress);
            int currentTotalDisplay = (int)Mathf.Lerp(startTotal, finalTotal, progress);
            runCoinsText.text = "Gained: + " + currentRunDisplay.ToString();
            totalCoinsText.text = "Stars: " + currentTotalDisplay.ToString();
            yield return null;
        }

        runCoinsText.text = "Gained: + 0";
        totalCoinsText.text = "Stars: " + finalTotal.ToString();

        restartButton.interactable = true;
        menuButton.interactable = true;
        nextLevelButton.interactable = true;
    }
}