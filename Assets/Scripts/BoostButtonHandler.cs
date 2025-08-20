// --- CREATE NEW FILE: BoostButtonHandler.cs (WITH DEBUGGING) ---

using UnityEngine;
using UnityEngine.EventSystems;

public class BoostButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public CarController carController;

    void Awake()
    {
        Debug.Log("BoostButtonHandler is awake on GameObject: " + gameObject.name);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown event DETECTED on boost button!"); // Did the click register at all?
        if (carController != null)
        {
            Debug.Log("CarController reference is VALID. Setting isBoosting to TRUE.");
            carController.isBoosting = true;
        }
        else
        {
            Debug.LogError("CarController reference is NULL on BoostButtonHandler! This is the problem!");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUp event DETECTED on boost button!");
        if (carController != null)
        {
            carController.isBoosting = false;
        }
    }
}