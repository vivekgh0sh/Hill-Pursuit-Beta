// --- CREATE NEW FILE: IAPManager.cs ---

using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension; // Required for IDetailedStoreListener

// Note: Inherit from IDetailedStoreListener instead of IStoreListener to get more detailed purchase failure reasons.
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    // --- PRODUCT DEFINITIONS ---
    // Use reverse domain name notation for your product IDs
    public const string PRODUCT_10000_STARS = "com.yourcompany.hillpursuit.10000stars";
    public const string PRODUCT_UNLOCK_ALL_LEVELS = "com.yourcompany.hillpursuit.unlockalllevels";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize the IAP system when the game starts
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add products to the store
        builder.AddProduct(PRODUCT_10000_STARS, ProductType.Consumable);
        builder.AddProduct(PRODUCT_UNLOCK_ALL_LEVELS, ProductType.NonConsumable);

        // This line initializes Unity IAP with our products and listener
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    // --- PUBLIC METHOD FOR UI BUTTONS ---
    public void BuyProductID(string productId)
    {
        if (!IsInitialized())
        {
            Debug.LogError("IAP not initialized yet!");
            return;
        }

        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"Attempting to purchase product: {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogError($"Could not purchase product. It might not be available: {productId}");
        }
    }

    // --- IStoreListener CALLBACKS ---

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("Unity IAP Initialized Successfully!");
        this.storeController = controller;
        this.storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Unity IAP Initialization Failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"Unity IAP Initialization Failed: {error}. Message: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Debug.Log($"Successfully purchased product: {productId}");

        // --- AWARD THE CONTENT ---
        switch (productId)
        {
            case PRODUCT_10000_STARS:
                GameManager.Instance.AddStars(10000);
                break;
            case PRODUCT_UNLOCK_ALL_LEVELS:
                GameManager.Instance.UnlockAllLevels();
                break;
            default:
                Debug.LogWarning($"Unknown product ID: {productId}");
                break;
        }

        // Return Complete to confirm the transaction
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogError($"Purchase of product '{product.definition.id}' failed. Reason: {reason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        Debug.LogError($"Purchase of product '{product.definition.id}' failed. Reason: {description.reason}. Message: {description.message}");
    }

    // --- HELPER METHOD TO GET LOCALIZED PRICE ---
    public string GetProductPrice(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }
        }
        return "Loading...";
    }
}