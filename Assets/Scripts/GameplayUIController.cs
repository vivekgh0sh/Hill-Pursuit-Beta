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

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalDistanceText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI runCoinsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

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
        gameOverPanel.SetActive(false);

        flipButton.onClick.AddListener(() => { if (CarController != null) CarController.PerformFlip(); });
        restartButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.RestartGame(); });
        menuButton.onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.GoToMenu(); });
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

    public void ShowGameOverScreen(int finalDistance, int highscore, int previousTotalCoins, int collectedInRun)
    {
        gameOverPanel.SetActive(true);
        finalDistanceText.text = "Distance: " + finalDistance.ToString() + "m";
        highscoreText.text = "Highscore: " + highscore.ToString() + "m";

        StartCoroutine(AnimateCoins(previousTotalCoins, collectedInRun));
    }

    private IEnumerator AnimateCoins(int startTotal, int collected)
    {
        restartButton.interactable = false;
        menuButton.interactable = false;

        // --- CHANGE #1: Slower Animation ---
        // Increased duration from 1.5f to 2.5f. You can adjust this value for the perfect speed.
        float duration = 2.5f;
        float elapsedTime = 0f;

        int finalTotal = startTotal + collected;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;

            int currentRunDisplay = (int)Mathf.Lerp(collected, 0, progress);
            int currentTotalDisplay = (int)Mathf.Lerp(startTotal, finalTotal, progress);

            // --- CHANGE #2: New Text Format ---
            runCoinsText.text = "Gained: + " + currentRunDisplay.ToString();
            totalCoinsText.text = "Stars: " + currentTotalDisplay.ToString();

            yield return null;
        }

        // --- CHANGE #3: Ensure Final Text Format is Correct ---
        runCoinsText.text = "Gained: + 0";
        totalCoinsText.text = "Stars: " + finalTotal.ToString();

        restartButton.interactable = true;
        menuButton.interactable = true;
    }
}