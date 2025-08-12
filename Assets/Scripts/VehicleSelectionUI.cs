using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Use TextMeshPro for better text rendering

public class VehicleSelectionUI : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform carDisplayAnchor; // The empty object where car models will spawn
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Button startButton;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private TextMeshProUGUI unlockCostText;

    private int currentCarIndex;
    private GameObject currentCarInstance;
    private CarData currentCarData;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Make sure it's in your startup scene.");
            this.enabled = false;
            return;
        }

        currentCarIndex = GameManager.Instance.selectedCarIndex;

        leftButton.onClick.AddListener(PreviousCar);
        rightButton.onClick.AddListener(NextCar);
        unlockButton.onClick.AddListener(UnlockCar);
        startButton.onClick.AddListener(StartGame);

        DisplayCar();
    }

    void DisplayCar()
    {
        Debug.Log("DisplayCar is running for index: " + currentCarIndex); // <-- ADD THIS

        // Get the data for the current car
        currentCarIndex = Mathf.Clamp(currentCarIndex, 0, GameManager.Instance.allCars.Count - 1);
        currentCarData = GameManager.Instance.allCars[currentCarIndex];

        Debug.Log("Displaying model for: " + currentCarData.carName); // <-- ADD THIS

        // Display the 3D model
        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }

        currentCarInstance = Instantiate(currentCarData.carPrefab, carDisplayAnchor);
        currentCarInstance.transform.localPosition = currentCarData.displayPositionOffset;
        currentCarInstance.transform.localRotation = Quaternion.Euler(currentCarData.displayRotation);
        currentCarInstance.transform.localScale = Vector3.one * currentCarData.displayScale;

        // --- NEW, IMPROVED VERSION ---

        // Disable all physics-related components on the display model
        // to prevent errors and unwanted behavior.
        CarController controller = currentCarInstance.GetComponent<CarController>();
        if (controller != null)
        {
            // Disable all wheel colliders
            foreach (var wheelInfo in controller.wheels)
            {
                if (wheelInfo.collider != null)
                {
                    wheelInfo.collider.enabled = false;
                }
            }
            // Then disable the controller script itself
            controller.enabled = false;
        }

        Rigidbody rb = currentCarInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Make the Rigidbody non-physical. It will still exist but won't move,
            // fall, or interact with physics. This satisfies the WheelColliders.
            rb.isKinematic = true;
            rb.useGravity = false;
        }


        // Update UI Text
        carNameText.text = currentCarData.carName;

        // Update Buttons based on Unlock Status
        bool isUnlocked = GameManager.Instance.IsCarUnlocked(currentCarData.carID);

        if (isUnlocked)
        {
            unlockButton.gameObject.SetActive(false);
            startButton.interactable = true;
            GameManager.Instance.selectedCarIndex = currentCarIndex; // Set this as the selected car
        }
        else
        {
            unlockButton.gameObject.SetActive(true);
            unlockCostText.text = currentCarData.unlockCost.ToString();
            startButton.interactable = false; // Can't start with a locked car
        }
    }

    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= GameManager.Instance.allCars.Count)
        {
            currentCarIndex = 0; // Wrap around
        }
        Debug.Log("NextCar clicked! New index: " + currentCarIndex); // <-- ADD THIS

        DisplayCar();
    }

    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = GameManager.Instance.allCars.Count - 1; // Wrap around
        }
        Debug.Log("NextCar clicked! New index: " + currentCarIndex); // <-- ADD THIS

        DisplayCar();
    }

    void UnlockCar()
    {
        // Get the cost of the currently displayed car
        int cost = currentCarData.unlockCost;

        // 1. Check if the player can afford it
        if (GameManager.Instance.CanAfford(cost))
        {
            Debug.Log("Player can afford the car. Unlocking...");

            // 2. Spend the coins
            GameManager.Instance.SpendCoins(cost);

            // 3. Unlock the car
            GameManager.Instance.UnlockCar(currentCarData.carID);

            // 4. Refresh the UI to show the car is now unlocked
            DisplayCar();
        }
        else
        {
            Debug.Log("Player cannot afford this car! Needs " + cost + " coins, but only has " + GameManager.Instance.playerCoins);
            // Optional: Add a UI pop-up or sound effect here to tell the player
            // they don't have enough money.
        }
    }

    void StartGame()
    {
        // Save the player's final selection before loading the game
        GameManager.Instance.SaveGameData();
        // The scene with the TerrainGenerator should be named "GameScene"
        SceneManager.LoadScene("endless");
    }
}