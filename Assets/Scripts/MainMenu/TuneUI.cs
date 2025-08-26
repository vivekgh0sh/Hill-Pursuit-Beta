// --- CREATE NEW FILE: TuneUI.cs ---

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TuneUI : MonoBehaviour
{
    [Header("Showroom References")]
    [SerializeField] private Transform showroomCarAnchor;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private TextMeshProUGUI carNameText;


    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private Button backButton;
    [SerializeField] private List<UpgradePanelUI> upgradePanels;

    private CarData currentCarData;
    private GameObject currentCarInstance;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Cannot display tune screen.");
            this.enabled = false;
            return;
        }

        currentCarData = GameManager.Instance.GetSelectedCar();
        if (currentCarData == null)
        {
            Debug.LogError("No car data found for tuning!");
            return;
        }

        backButton.onClick.AddListener(BackToVehicleSelect);

        DisplayCar();
        SetupUpgradePanels();
        UpdateCoinDisplay();
    }

    void DisplayCar()
    {
        carNameText.text = currentCarData.carName;

        if (currentCarInstance != null) Destroy(currentCarInstance);

        currentCarInstance = Instantiate(currentCarData.carPrefab, showroomCarAnchor);
        currentCarInstance.transform.localPosition = currentCarData.displayPositionOffset;
        currentCarInstance.transform.localRotation = Quaternion.Euler(currentCarData.displayRotation);
        currentCarInstance.transform.localScale = Vector3.one * currentCarData.displayScale;

        // Disable physics and controllers
        if (currentCarInstance.GetComponent<CarController>() != null)
        {
            currentCarInstance.GetComponent<CarController>().enabled = false;
        }
        if (currentCarInstance.GetComponent<Rigidbody>() != null)
        {
            currentCarInstance.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void SetupUpgradePanels()
    {
        for (int i = 0; i < upgradePanels.Count; i++)
        {
            if (i < currentCarData.upgrades.Count)
            {
                int upgradeIndex = i; // Important for lambda closure
                upgradePanels[i].gameObject.SetActive(true);
                upgradePanels[i].Setup(currentCarData, currentCarData.upgrades[i], () => HandleUpgradeClicked(upgradeIndex));
            }
            else
            {
                upgradePanels[i].gameObject.SetActive(false);
            }
        }
    }

    void HandleUpgradeClicked(int upgradeIndex)
    {
        if (GameManager.Instance != null && currentCarData != null)
        {
            UpgradeData upgradeToPurchase = currentCarData.upgrades[upgradeIndex];
            GameManager.Instance.PurchaseUpgrade(currentCarData, upgradeToPurchase);

            // Refresh UI after purchase attempt
            RefreshAllUI();
        }
    }

    void RefreshAllUI()
    {
        UpdateCoinDisplay();
        foreach (var panel in upgradePanels)
        {
            if (panel.gameObject.activeInHierarchy)
            {
                panel.UpdatePanel();
            }
        }
    }

    void UpdateCoinDisplay()
    {
        if (GameManager.Instance != null)
        {
            totalCoinsText.text = GameManager.Instance.coins.ToString();
        }
    }

    void BackToVehicleSelect()
    {
        SceneManager.LoadScene("VehicleSelectionUI");
    }

    void Update()
    {
        if (showroomCarAnchor != null && showroomCarAnchor.childCount > 0)
        {
            showroomCarAnchor.GetChild(0).Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}