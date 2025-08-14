using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownTester : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // This is the only thing the script does.
        Debug.Log("SUCCESS! PointerDownTester on " + gameObject.name + " was triggered!");
    }
}