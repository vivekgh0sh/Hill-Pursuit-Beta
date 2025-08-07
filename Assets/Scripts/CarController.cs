using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class WheelInfo
{
    public WheelCollider collider;
    public Transform visual;
    public bool canSteer = false;
    public bool hasMotor = true;
}

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 12000f;
    public float activeBrakeForce = 3000f;
    public float airControlTorque = 1000f;
    public float stoppedSpeedThreshold = 0.1f;

    [Header("References")]
    public Transform centerOfMass;
    public WheelInfo[] wheels;

    private float verticalInput;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null)
        {
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }
    }

    void Update()
    {
        HandleInput();
        UpdateWheelVisuals();
    }

    void FixedUpdate()
    {
        ApplyDrivingForces();
        ApplyAirControl();
    }

    void HandleInput()
    {
        verticalInput = 0;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return;

        if (pointer.position.ReadValue().x > Screen.width / 2) { verticalInput = 1; }
        else { verticalInput = -1; }
    }

    // --- REWRITTEN WITH RIGIDBODY.SLEEP() ---
    void ApplyDrivingForces()
    {
        // If the player gives any input, make sure the Rigidbody is awake
        if (verticalInput != 0)
        {
            rb.WakeUp();
        }

        // Determine motor and brake for the entire car
        float targetMotorTorque = motorForce * verticalInput;
        float targetBrakeTorque = 0f;

        // Active braking
        if (verticalInput < 0)
        {
            targetBrakeTorque = activeBrakeForce;
        }

        // Apply to all wheels
        foreach (var wheel in wheels)
        {
            if (wheel.hasMotor)
            {
                wheel.collider.motorTorque = targetMotorTorque;
            }
            wheel.collider.brakeTorque = targetBrakeTorque;
        }

        // If we are idle AND the car is very slow, put it to sleep
        if (verticalInput == 0 && rb.linearVelocity.magnitude < stoppedSpeedThreshold)
        {
            rb.Sleep();
        }
    }

    void ApplyAirControl()
    {
        bool isGrounded = false;
        foreach (var wheel in wheels) { if (wheel.collider.isGrounded) { isGrounded = true; break; } }
        if (!isGrounded) { rb.AddTorque(Vector3.forward * -verticalInput * airControlTorque); }
    }

    void UpdateWheelVisuals()
    {
        foreach (var wheel in wheels)
        {
            Vector3 pos;
            Quaternion rot;
            wheel.collider.GetWorldPose(out pos, out rot);
            wheel.visual.position = pos;
            wheel.visual.rotation = rot;
        }
    }
}