// --- CREATE NEW FILE: DynamicCameraZoom.cs (REVISED FOR REVERSE) ---

using UnityEngine;
using Unity.Cinemachine;

public class DynamicCameraZoom : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Rigidbody of the car. This will be assigned at runtime by the PlayerSpawner.")]
    public Rigidbody carRigidbody;

    [Header("Zoom Settings")]
    [Tooltip("The camera's size when the car is stationary (zoomed IN).")]
    [SerializeField] private float minOrthographicSize = 8f;
    [Tooltip("The camera's size at max speed (zoomed OUT).")]
    [SerializeField] private float maxOrthographicSize = 12f;
    [Tooltip("The speed at which the camera will be fully zoomed out.")]
    [SerializeField] private float maxSpeedForZoom = 60f;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera adjusts its zoom. Lower values are faster/snappier.")]
    [SerializeField] private float smoothTime = 0.5f;

    private CinemachineCamera virtualCamera;
    private float zoomVelocity; // Used by SmoothDamp

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("DynamicCameraZoom script requires a CinemachineCamera component on the same GameObject!", this);
            this.enabled = false;
        }
    }

    void Update()
    {
        if (carRigidbody == null || virtualCamera == null)
        {
            return;
        }

        // --- REVISED LOGIC TO HANDLE REVERSING ---

        // 1. Get the car's velocity ONLY along the X-axis. This gives us direction.
        // A positive value means moving forward (right), negative means reversing (left).
        float forwardSpeed = carRigidbody.linearVelocity.x;

        float targetOrthoSize;

        // 2. Check if the car is reversing.
        if (forwardSpeed < 0)
        {
            // If we are reversing, force the camera to the default, zoomed-in state.
            targetOrthoSize = minOrthographicSize;
        }
        else
        {
            // If we are stationary or moving forward, calculate zoom as normal.
            float speedPercentage = Mathf.Clamp01(forwardSpeed / maxSpeedForZoom);
            targetOrthoSize = Mathf.Lerp(minOrthographicSize, maxOrthographicSize, speedPercentage);
        }

        // 3. Smoothly adjust the camera's current Orthographic Size towards the calculated target.
        virtualCamera.Lens.OrthographicSize = Mathf.SmoothDamp(
            virtualCamera.Lens.OrthographicSize,
            targetOrthoSize,
            ref zoomVelocity,
            smoothTime
        );

        // --- END OF REVISED LOGIC ---
    }
}