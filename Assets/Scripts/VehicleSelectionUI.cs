using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VehicleSelectionUI : MonoBehaviour
{
    [Header("Showroom References")]
    [SerializeField] private Transform showroomCarAnchor;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Scene References")]
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
            this.enabled = false;
            return;
        }

        currentCarIndex = GameManager.Instance.selectedCarIndex;

        leftButton.onClick.AddListener(PreviousCar);
        unlockButton.onClick.AddListener(UnlockCar);
        startButton.onClick.AddListener(StartGame);

        DisplayCar();
    }

    void DisplayCar()
    {
        currentCarIndex = Mathf.Clamp(currentCarIndex, 0, GameManager.Instance.allCars.Count - 1);
        currentCarData = GameManager.Instance.allCars[currentCarIndex];

        if (currentCarInstance != null)
        {
            Destroy(currentCarInstance);
        }

        currentCarInstance = Instantiate(currentCarData.carPrefab, showroomCarAnchor);
        currentCarInstance.transform.localPosition = currentCarData.displayPositionOffset;
        currentCarInstance.transform.localRotation = Quaternion.Euler(currentCarData.displayRotation);
        currentCarInstance.transform.localScale = Vector3.one * currentCarData.displayScale;

        SetLayerRecursively(currentCarInstance, LayerMask.NameToLayer("ShowroomCar"));

        CarController controller = currentCarInstance.GetComponent<CarController>();
        if (controller != null)
        {
            foreach (var wheelInfo in controller.wheels)
            {
                if (wheelInfo.collider != null)
                {
                    wheelInfo.collider.enabled = false;
                }
            }
            controller.enabled = false;
        }

        Rigidbody rb = currentCarInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }


        carNameText.text = currentCarData.carName;

        bool isUnlocked = GameManager.Instance.IsCarUnlocked(currentCarData.carID);

        if (isUnlocked)
        {
            unlockButton.gameObject.SetActive(false);
            startButton.interactable = true;
            GameManager.Instance.selectedCarIndex = currentCarIndex;
        }
        else
        {
            unlockButton.gameObject.SetActive(true);
            unlockCostText.text = currentCarData.unlockCost.ToString();
            startButton.interactable = false;
        }
    }

    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= GameManager.Instance.allCars.Count)
        {
            currentCarIndex = 0;
        }

        DisplayCar();
    }

    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = GameManager.Instance.allCars.Count - 1;
        }
        DisplayCar();
    }

    void UnlockCar()
    {
        int cost = currentCarData.unlockCost;

        if (GameManager.Instance.CanAfford(cost))
        {
            GameManager.Instance.SpendCoins(cost);

            GameManager.Instance.UnlockCar(currentCarData.carID);

            DisplayCar();
        }

    }

    public void StartGame()
    {
        this.enabled = false;
        GameManager.Instance.SaveGameData();
        SceneManager.LoadScene("endless");
    }
    void Update()
    {
        if (showroomCarAnchor.childCount > 0)
        {
            showroomCarAnchor.GetChild(0).Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}