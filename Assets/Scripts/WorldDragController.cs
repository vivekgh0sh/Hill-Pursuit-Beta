using UnityEngine;
using UnityEngine.InputSystem; // <-- Add this line to use the new Input System

public class WorldDragController : MonoBehaviour
{
    [Header("References")]
    public Transform trackRoot;
    public Transform player;
    public Camera cam;

    [Header("Drag Settings")]
    public float dragToWorldScale = 0.02f;
    public float maxWorldSpeed = 10f;
    public float smoothing = 12f;
    public bool invert = false;

    [Header("Boundaries")]
    public float minY = -50f;
    public float maxY = 50f;

    [Header("Car Feedback")]
    public float tiltMaxDegrees = 8f;
    public float tiltReturnSpeed = 6f;

    private bool dragging;
    private Vector2 lastPointerPosition;
    private float targetDeltaY;
    private float currentVelocity;

    void Reset()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || trackRoot == null) return;

        HandleInput();
        ApplyWorldMovement();
        ApplyCarTilt();
    }

    void HandleInput()
    {
        var pointer = Pointer.current;
        if (pointer == null)
        {
            // No pointer detected, slow down
            targetDeltaY = Mathf.MoveTowards(targetDeltaY, 0f, maxWorldSpeed * 2f * Time.deltaTime);
            return;
        }

        // Check if the primary button (left mouse, first touch) is currently held down
        if (pointer.press.isPressed)
        {
            // Check if this is the VERY first frame of the press
            if (pointer.press.wasPressedThisFrame)
            {
                // This is the start of a new drag, so we record the initial position
                lastPointerPosition = pointer.position.ReadValue();
                // We don't calculate movement on the first frame, so we exit here
                return;
            }

            // If we are here, it means the button is being held down (not the first frame)
            Vector2 currentPointerPosition = pointer.position.ReadValue();
            Vector2 delta = currentPointerPosition - lastPointerPosition;

            // Update the last position for the next frame
            lastPointerPosition = currentPointerPosition;

            // Calculate the world movement
            float sign = invert ? -1f : 1f;
            float dy = delta.y * dragToWorldScale * sign;
            targetDeltaY = Mathf.Clamp(dy / Time.deltaTime, -maxWorldSpeed, maxWorldSpeed);
        }
        else
        {
            // The button is not pressed, so slow down to a stop
            targetDeltaY = Mathf.MoveTowards(targetDeltaY, 0f, maxWorldSpeed * 2f * Time.deltaTime);
        }
    }

    void ApplyWorldMovement()
    {
        // Smooth the current velocity towards the target velocity
        currentVelocity = Mathf.Lerp(currentVelocity, targetDeltaY, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

        // Only move if there is significant velocity
        if (Mathf.Abs(currentVelocity) > 0.01f)
        {
            Vector3 pos = trackRoot.position;
            pos.y += currentVelocity * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            trackRoot.position = pos;
        }
    }

    void ApplyCarTilt()
    {
        if (player == null) return;

        float tiltInfluence = Mathf.InverseLerp(-maxWorldSpeed, maxWorldSpeed, currentVelocity); // 0 to 1
        float signedTilt = (tiltInfluence - 0.5f) * 2f; // -1 to 1
        float targetTilt = -signedTilt * tiltMaxDegrees;

        Quaternion desiredRotation = Quaternion.Euler(targetTilt, player.eulerAngles.y, player.eulerAngles.z);
        player.rotation = Quaternion.Slerp(player.rotation, desiredRotation, 1f - Mathf.Exp(-tiltReturnSpeed * Time.deltaTime));
    }

    // Public method for other scripts to get the current speed
    public float GetCurrentVelocity()
    {
        return currentVelocity;
    }
}