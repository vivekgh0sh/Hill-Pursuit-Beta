using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class WheelInfo
{
    public WheelCollider collider;
    public Transform visual;
    public bool canSteer = false; // Optional for later
    public bool hasMotor = true;
}

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;

    [Header("Wheel Setup")]
    public WheelInfo[] wheels; // Assign your 4 wheels here in the inspector

    private float horizontalInput;
    private float verticalInput;

    void Update()
    {
        HandleInput();
        UpdateWheelVisuals();
    }

    void FixedUpdate()
    {
        // Physics calculations should be in FixedUpdate
        ApplyMotorAndBrake();
    }

    void HandleInput()
    {
        // Reset input
        verticalInput = 0;

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed)
        {
            return; // No touch/click, do nothing
        }

        // Check if press is on the right or left half of the screen
        if (pointer.position.ReadValue().x > Screen.width / 2)
        {
            // Right side: Accelerate
            verticalInput = 1;
        }
        else
        {
            // Left side: Brake/Reverse
            verticalInput = -1;
        }
    }

    void ApplyMotorAndBrake()
    {
        foreach (var wheel in wheels)
        {
            if (verticalInput > 0) // Accelerating
            {
                if (wheel.hasMotor)
                {
                    wheel.collider.motorTorque = verticalInput * motorForce;
                }
                wheel.collider.brakeTorque = 0;
            }
            else if (verticalInput < 0) // Braking
            {
                // Apply brake force to all wheels
                wheel.collider.brakeTorque = brakeForce;
                if (wheel.hasMotor)
                {
                    wheel.collider.motorTorque = 0;
                }
            }
            else // No input
            {
                wheel.collider.motorTorque = 0;
                wheel.collider.brakeTorque = 0;
            }
        }
    }

    void UpdateWheelVisuals()
    {
        foreach (var wheel in wheels)
        {
            Vector3 pos;
            Quaternion rot;
            // Get the world position and rotation from the physics wheel
            wheel.collider.GetWorldPose(out pos, out rot);

            // Apply it to the visual wheel
            wheel.visual.position = pos;
            wheel.visual.rotation = rot;
        }
    }
}