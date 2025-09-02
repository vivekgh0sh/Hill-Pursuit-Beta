// --- CREATE NEW FILE: ShopUIController.cs ---

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button buyStarsButton;
    [SerializeField] private TextMeshProUGUI buyStarsPriceText;

    [SerializeField] private Button unlockLevelsButton;
    [SerializeField] private TextMeshProUGUI unlockLevelsPriceText;

    [SerializeField] private Button backButton;

    void Start()
    {
        // Add listeners to the buttons
        buyStarsButton.onClick.AddListener(Buy10000Stars);
        unlockLevelsButton.onClick.AddListener(BuyUnlockAllLevels);
        backButton.onClick.AddListener(GoBack);

        // Start a coroutine to wait for IAP to initialize before updating prices
        StartCoroutine(UpdatePricesWhenReady());
    }

    private System.Collections.IEnumerator UpdatePricesWhenReady()
    {
        // Wait until the IAP Manager is initialized
        while (IAPManager.Instance == null || IAPManager.Instance.GetProductPrice(IAPManager.PRODUCT_10000_STARS) == "Loading...")
        {
            yield return null; // Wait for the next frame
        }

        // Now that it's ready, update the price text
        buyStarsPriceText.text = IAPManager.Instance.GetProductPrice(IAPManager.PRODUCT_10000_STARS);
        unlockLevelsPriceText.text = IAPManager.Instance.GetProductPrice(IAPManager.PRODUCT_UNLOCK_ALL_LEVELS);
    }

    private void Buy10000Stars()
    {
        IAPManager.Instance.BuyProductID(IAPManager.PRODUCT_10000_STARS);
    }

    private void BuyUnlockAllLevels()
    {
        IAPManager.Instance.BuyProductID(IAPManager.PRODUCT_UNLOCK_ALL_LEVELS);
    }

    private void GoBack()
    {
        SceneManager.LoadScene("VehicleSelectionUI");
    }
}