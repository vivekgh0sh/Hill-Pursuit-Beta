// --- START OF FILE CarController.cs (CORRECTED & RESTORED) ---

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
    [SerializeField]
    [Tooltip("The current amount of fuel. This is initialized to 'Max Fuel' when the game starts.")]
    private float currentFuel;
    public float FuelPercent => currentFuel / maxFuel;

    [Header("Boost Settings")]
    public float maxBoost = 100f;
    public float boostForce = 8000f;
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

    [Header("Game Over Conditions")]
    [Tooltip("How long the car can be flipped and stopped before game over.")]
    public float stuckTimeThreshold = 3.0f;
    private float timeSinceStuck = 0f;

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
        CheckGameOverConditions();
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

    // --- THIS METHOD HAS BEEN RESTORED TO ITS ORIGINAL, WORKING STATE ---
    void HandleInput()
    {
        verticalInput = 0;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return;

        // This check was removed in the original file to allow driving while pressing the boost button.
        // We ensure it stays removed, as intended.
        // if (EventSystem.current.IsPointerOverGameObject()) return;

        if (pointer.position.ReadValue().x > Screen.width / 2)
        {
            verticalInput = 1;
        }
        else
        {
            verticalInput = -1;
        }
    }

    // --- THIS METHOD HAS BEEN RESTORED TO ITS ORIGINAL, WORKING STATE ---
    void ApplyDrivingForces()
    {
        if (verticalInput != 0) { rb.WakeUp(); }

        float motorInput = verticalInput > 0 && currentFuel > 0 ? verticalInput : 0;
        float finalMotorForce = motorForce;

        if (isBoosting && currentBoost > 0)
        {
            finalMotorForce += boostForce;
            currentBoost -= boostDepletionRate * Time.fixedDeltaTime;
        }

        float targetMotorTorque = finalMotorForce * motorInput;

        if (verticalInput > 0 && currentFuel > 0)
        {
            currentFuel -= fuelDepletionRate * Time.fixedDeltaTime;
        }

        float targetBrakeTorque = verticalInput < 0 ? activeBrakeForce : 0f;

        foreach (var wheel in wheels)
        {
            if (wheel.hasMotor) { wheel.collider.motorTorque = targetMotorTorque; }
            wheel.collider.brakeTorque = targetBrakeTorque;
        }
    }

    // --- THIS IS THE CORRECTLY INTEGRATED GAME OVER LOGIC ---
    void CheckGameOverConditions()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        // 1. Out of Fuel Condition
        if (currentFuel <= 0)
        {
            GameManager.Instance.EndGame();
            return;
        }

        // 2. Flipped and Stuck Condition
        bool isFlipped = Vector3.Dot(transform.up, Vector3.down) > 0;
        bool isStopped = rb.linearVelocity.magnitude < stoppedSpeedThreshold;

        if (isFlipped && isStopped)
        {
            timeSinceStuck += Time.fixedDeltaTime;
            if (timeSinceStuck >= stuckTimeThreshold)
            {
                GameManager.Instance.EndGame();
            }
        }
        else
        {
            // Reset timer if the car is not stuck
            timeSinceStuck = 0;
        }
    }

    void CheckGroundedStatus()
    {
        isGrounded = false;
        foreach (var wheel in wheels) { if (wheel.collider.isGrounded) { isGrounded = true; return; } }
    }

    void HandleBoostRegen()
    {
        if (!isBoosting && currentBoost < maxBoost)
        {
            currentBoost += boostRegenRate * Time.deltaTime;
            currentBoost = Mathf.Min(currentBoost, maxBoost);
        }
    }

    void ApplyAirControl()
    {
        if (!isGrounded) { rb.AddTorque(Vector3.forward * -verticalInput * airControlTorque); }
    }

    // --- THIS METHOD IS CRITICAL FOR WHEEL ROTATION AND IS CORRECT ---
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
    public void AddFuel(float amount)
    {
        currentFuel += amount;
        // Clamp the fuel so it doesn't go above the maximum
        currentFuel = Mathf.Min(currentFuel, maxFuel);
    }
}