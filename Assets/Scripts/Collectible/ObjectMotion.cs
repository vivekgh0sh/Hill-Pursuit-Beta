// --- CREATE NEW FILE: ObjectMotion.cs ---

using UnityEngine;

public class ObjectMotion : MonoBehaviour
{
    [Header("Spin Settings")]
    [Tooltip("Check to enable spinning.")]
    public bool enableSpin = true;
    [Tooltip("The speed of rotation in degrees per second around each axis.")]
    public Vector3 spinSpeed = new Vector3(0f, 60f, 0f);

    [Header("Bobbing Settings")]
    [Tooltip("Check to enable the up-and-down bobbing motion.")]
    public bool enableBobbing = true;
    [Tooltip("How high and low the object will bob from its starting point.")]
    public float bobHeight = 0.15f;
    [Tooltip("How fast the object bobs up and down.")]
    public float bobSpeed = 2.5f;

    // Internal state
    private Vector3 startPosition;

    void Awake()
    {
        // Store the initial position of the object when the game starts.
        // All bobbing calculations will be relative to this point.
        startPosition = transform.position;
    }

    void Update()
    {
        // --- Handle Spinning ---
        if (enableSpin)
        {
            // Rotate the object every frame based on the spin speed.
            // Time.deltaTime makes the rotation smooth and independent of the frame rate.
            transform.Rotate(spinSpeed * Time.deltaTime);
        }

        // --- Handle Bobbing ---
        if (enableBobbing)
        {
            // Calculate the new vertical position using a sine wave.
            // Mathf.Sin creates a smooth wave that oscillates between -1 and 1.
            // We multiply by Time.time * bobSpeed to make the wave move over time.
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

            // Apply the offset to the object's starting position.
            transform.position = startPosition + new Vector3(0f, yOffset, 0f);
        }
    }
}