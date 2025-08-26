// --- CREATE NEW FILE: UpgradePanelUI.cs ---

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Slider levelSlider;

    private CarData currentCar;
    private UpgradeData currentUpgrade;
    private Action onUpgradeClicked; // A callback to the main TuneUI controller

    public void Setup(CarData car, UpgradeData upgrade, Action upgradeCallback)
    {
        this.currentCar = car;
        this.currentUpgrade = upgrade;
        this.onUpgradeClicked = upgradeCallback;

        // Add a listener to our button that calls the main controller's method
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => onUpgradeClicked?.Invoke());

        UpdatePanel();
    }

    public void UpdatePanel()
    {
        if (currentCar == null || currentUpgrade == null) return;

        int currentLevel = GameManager.Instance.GetUpgradeLevel(currentCar.carID, currentUpgrade.upgradeID);

        // Update texts
        statNameText.text = currentUpgrade.upgradeName;
        levelText.text = $"LEVEL {currentLevel}";

        // Update slider
        levelSlider.maxValue = currentUpgrade.maxLevel;
        levelSlider.value = currentLevel;

        // Update button and cost
        if (currentLevel >= currentUpgrade.maxLevel)
        {
            // Max level reached
            upgradeButton.interactable = false;
            costText.text = "MAXED";
        }
        else
        {
            int cost = currentUpgrade.GetCostForLevel(currentLevel + 1);
            costText.text = cost.ToString();
            upgradeButton.interactable = GameManager.Instance.CanAfford(cost);
        }
    }
}