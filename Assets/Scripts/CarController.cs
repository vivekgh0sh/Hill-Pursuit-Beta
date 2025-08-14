// --- START OF FILE CarController.cs (THE FINAL WORKING VERSION) ---

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float fuelDepletionRate = 1f;
    private float currentFuel;
    public float FuelPercent => currentFuel / maxFuel;

    [Header("Boost Settings")]
    public float maxBoost = 100f;
    public float boostForce = 8000f; // Use a value that is a significant addition to motorForce
    public float boostDepletionRate = 20f;
    public float boostRegenRate = 5f;
    private float currentBoost;
    public bool isBoosting = false;
    public float BoostPercent => currentBoost / maxBoost;

    [Header("Flip Settings")]
    public float flipTorque = 500f;
    public float boostRewardForFlip = 30f;
    public float flipCooldown = 2f;
    private float lastFlipTime = -99f;
    private bool isGrounded;

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
        currentFuel = maxFuel;
        currentBoost = maxBoost;
    }

    void Update()
    {
        HandleInput();
        UpdateWheelVisuals();
        HandleBoostRegen();
    }

    void FixedUpdate()
    {
        CheckGroundedStatus();
        ApplyDrivingForces();
        ApplyAirControl();
    }

    public void PerformFlip()
    {
        if (!isGrounded && Time.time > lastFlipTime + flipCooldown)
        {
            lastFlipTime = Time.time;
            rb.AddTorque(Vector3.forward * -flipTorque, ForceMode.Impulse);
            currentBoost = Mathf.Min(maxBoost, currentBoost + boostRewardForFlip);
        }
    }

    // --- THIS IS THE KEY CHANGE ---
    // The input handling is now simpler and doesn't block itself.
    void HandleInput()
    {
        verticalInput = 0;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return;

        // The GameplayUIController will handle setting 'isBoosting' when the boost button is pressed.
        // This HandleInput method will ALWAYS check for driving input, regardless of what UI is pressed.
        // This solves the conflict where pressing the boost button would stop the car from accelerating.
        if (pointer.position.ReadValue().x > Screen.width / 2)
        {
            verticalInput = 1;
        }
        else
        {
            verticalInput = -1;
        }
    }

    // --- THIS METHOD IS ALSO MODIFIED ---
    // It now correctly adds the boost force to the motor torque.
    void ApplyDrivingForces()
    {
        if (verticalInput != 0) { rb.WakeUp(); }

        float motorInput = verticalInput > 0 && currentFuel > 0 ? verticalInput : 0;
        float finalMotorForce = motorForce;

        // `isBoosting` is set by GameplayUIController.
        // `motorInput` is set by HandleInput. They work together now.
        if (isBoosting && currentBoost > 0)
        {
            finalMotorForce += boostForce;
        }

        float targetMotorTorque = finalMotorForce * motorInput;

        // Fuel depletion only happens on acceleration input
        if (verticalInput > 0 && currentFuel > 0)
        {
            currentFuel -= fuelDepletionRate * Time.fixedDeltaTime;
        }

        // Boost depletion happens when the boost is active, regardless of acceleration
        if (isBoosting && currentBoost > 0)
        {
            currentBoost -= boostDepletionRate * Time.fixedDeltaTime;
        }

        float targetBrakeTorque = verticalInput < 0 ? activeBrakeForce : 0f;

        foreach (var wheel in wheels)
        {
            if (wheel.hasMotor) { wheel.collider.motorTorque = targetMotorTorque; }
            wheel.collider.brakeTorque = targetBrakeTorque;
        }

        if (verticalInput == 0 && rb.linearVelocity.magnitude < stoppedSpeedThreshold)
        {
            rb.Sleep();
        }
    }

    void CheckGroundedStatus()
    {
        isGrounded = false;
        foreach (var wheel in wheels)
        {
            if (wheel.collider.isGrounded)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void HandleBoostRegen()
    {
        if (!isBoosting && currentBoost < maxBoost)
        {
            currentBoost += boostRegenRate * Time.deltaTime;
            currentBoost = Mathf.Min(currentBoost, maxBoost);
        }
    }

    // --- THIS METHOD IS REMOVED ---
    /*
    void ApplyBoostForce()
    {
        if (isBoosting && currentBoost > 0)
        {
            rb.AddForce(transform.right * boostForce, ForceMode.Force);
            currentBoost -= boostDepletionRate * Time.fixedDeltaTime;
        }
    }
    */

    void ApplyAirControl()
    {
        if (!isGrounded) { rb.AddTorque(Vector3.forward * -verticalInput * airControlTorque); }
    }

    void UpdateWheelVisuals()
    {
        foreach (var wheel in wheels)
        {
            Vector3 pos; Quaternion rot;
            wheel.collider.GetWorldPose(out pos, out rot);
            wheel.visual.position = pos;
            wheel.visual.rotation = rot;
        }
    }
}