// --- CREATE NEW FILE: CoinDisplayUpdater.cs ---

using UnityEngine;
using TMPro;

public class CoinDisplayUpdater : MonoBehaviour
{
    // This can be your coin/star count, or your highscore, etc.
    [SerializeField] private TextMeshProUGUI valueText;

    // A variable to store the last displayed value to prevent unnecessary updates
    private int lastDisplayedValue = -1;

    void Update()
    {
        if (GameManager.Instance == null) return;

        int currentValue = GameManager.Instance.coins;

        // Only update the text if the value has actually changed
        if (currentValue != lastDisplayedValue)
        {
            UpdateText(currentValue);
        }
    }

    public void UpdateText(int newValue)
    {
        if (valueText != null)
        {
            // Instead of "STAR", we now use the actual number
            valueText.text = newValue.ToString();
            lastDisplayedValue = newValue;
        }
    }
}