using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Add these interfaces to handle press-and-hold events
public class GameplayUIController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Elements")]
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Slider boostSlider;
    [SerializeField] private Button flipButton;
    [SerializeField] private GameObject boostButtonObject; // The GameObject of the Boost Button

    // This will be assigned by the PlayerSpawner
    [HideInInspector] public CarController carController;

    void Start()
    {
        // We only need to set up the one-shot flip button listener here
        flipButton.onClick.AddListener(() =>
        {
            if (carController != null)
            {
                carController.PerformFlip();
            }
        });
    }

    void Update()
    {
        // If we don't have a car yet, do nothing.
        if (carController == null) return;

        // Update the sliders every frame based on the car's public properties
        fuelSlider.value = carController.FuelPercent;
        boostSlider.value = carController.BoostPercent;
    }

    // This is called when a pointer (mouse or finger) presses down on ANY UI element
    // that this script is attached to. We will attach it to the Canvas.
    public void OnPointerDown(PointerEventData eventData)
    {
        // LOG 1: See if this function is ever even called.
        Debug.Log("OnPointerDown was called!");

        // LOG 2: See what UI element the system thinks we clicked.
        Debug.Log("Pointer pressed on: " + eventData.pointerPress.name);

        // Check if the object we pressed down on is our boost button
        if (eventData.pointerPress == boostButtonObject)
        {
            // LOG 3: See if this specific check passes.
            Debug.Log("SUCCESS: The pressed object IS the Boost Button!");
            if (carController != null)
            {
                carController.isBoosting = true;
            }
        }
        else
        {
            // LOG 4: See if the check fails.
            Debug.Log("FAILURE: The pressed object was NOT the Boost Button. It was " + eventData.pointerPress.name);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // LOG 5: See if this function is called on release.
        Debug.Log("OnPointerUp was called!");

        if (carController != null && carController.isBoosting)
        {
            carController.isBoosting = false;
        }
    }
}