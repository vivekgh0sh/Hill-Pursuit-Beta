// --- START OF FILE CarController.cs (REVISED FOR PROGRESSIVE ACCELERATION) ---

using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class WheelInfo { public WheelCollider collider; public Transform visual; public bool canSteer = false; public bool hasMotor = true; }

public class CarController : MonoBehaviour
{
    [Header("Control Type")]
    public bool isPlayerControlled = true;

    [Header("Driving Physics")]
    [Tooltip("The MAXIMUM torque the engine can output. This is now set by CarData upgrades.")]
    public float maxMotorTorque = 20000f; // Formerly motorForce
    [Tooltip("How quickly the engine reaches max torque. THIS IS THE KEY VALUE FOR 'GAME FEEL'.")]
    public float accelerationRate = 8000f;
    [Tooltip("How quickly the car slows down when not accelerating. Simulates engine braking and drag.")]
    public float coastingDrag = 4000f;

    [Header("Car Settings")]
    public float activeBrakeForce = 3000f;
    public float airControlTorque = 1000f;
    // ... (rest of variables are the same as before)
    public float stoppedSpeedThreshold = 0.1f;
    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float fuelDepletionRate = 1f;
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
    public bool isGrounded { get; private set; }
    [Header("Game Over Conditions")]
    public float stuckTimeThreshold = 3.0f;
    private float timeSinceStuck = 0f;
    [Header("References")]
    public Transform centerOfMass;
    public WheelInfo[] wheels;

    private Rigidbody rb;
    private float motorInput;
    private float brakeInput;
    private float airControlInput;

    // --- NEW VARIABLE TO TRACK CURRENT ENGINE POWER ---
    private float currentAppliedTorque = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null) { rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position); }
    }

    public void Initialize(CarData data)
    {
        if (GameManager.Instance == null) return;
        foreach (var upgrade in data.upgrades)
        {
            int level = GameManager.Instance.GetUpgradeLevel(data.carID, upgrade.upgradeID);
            float finalValue = upgrade.baseValue + (upgrade.valuePerLevel * level);
            switch (upgrade.upgradeID)
            {
                // IMPORTANT: The upgrade now controls MAX torque
                case "engine_power": this.maxMotorTorque = finalValue; break;
                case "fuel_tank": this.maxFuel = finalValue; break;
                case "boost_power": this.boostForce = finalValue; break;
                case "tire_grip": this.activeBrakeForce = finalValue; break;
            }
        }
        currentFuel = maxFuel;
        currentBoost = maxBoost;
    }

    void Update()
    {
        if (isPlayerControlled) { HandlePlayerInput(); }
        UpdateWheelVisuals();
        HandleBoostRegen();
    }

    void FixedUpdate()
    {
        CheckGroundedStatus();
        ApplyDrivingForces();
        ApplyAirControl();
        if (isPlayerControlled) { CheckGameOverConditions(); }
    }

    public void SetAI_MotorInput(float input) { motorInput = Mathf.Clamp(input, -1f, 1f); }
    public void SetAI_BrakeInput(float input) { brakeInput = Mathf.Clamp01(input); }
    public void SetAI_AirControlInput(float input) { airControlInput = Mathf.Clamp(input, -1f, 1f); }

    private void HandlePlayerInput()
    {
        float verticalInput = 0;
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed)
        {
            verticalInput = pointer.position.ReadValue().x > Screen.width / 2 ? 1 : -1;
        }
        motorInput = verticalInput;
        brakeInput = 0;
        airControlInput = verticalInput;
    }

    private void ApplyDrivingForces()
    {
        // --- NEW PROGRESSIVE ACCELERATION LOGIC ---

        // 1. Ramp up or down the current torque based on input
        if (motorInput > 0) // Accelerating
        {
            currentAppliedTorque += accelerationRate * Time.fixedDeltaTime;
        }
        else if (motorInput < 0) // Reversing
        {
            currentAppliedTorque -= accelerationRate * Time.fixedDeltaTime;
        }
        else // Coasting
        {
            currentAppliedTorque = Mathf.MoveTowards(currentAppliedTorque, 0f, coastingDrag * Time.fixedDeltaTime);
        }

        // 2. Clamp the torque to the car's maximum capability
        currentAppliedTorque = Mathf.Clamp(currentAppliedTorque, -maxMotorTorque, maxMotorTorque);

        // --- END OF NEW LOGIC ---

        float finalTorque = currentAppliedTorque;
        if (isBoosting && currentBoost > 0)
        {
            // Boost adds a direct force on top of the current engine torque
            finalTorque += boostForce * motorInput;
            currentBoost -= boostDepletionRate * Time.fixedDeltaTime;
        }

        if (isPlayerControlled && motorInput > 0 && currentFuel > 0)
        {
            currentFuel -= fuelDepletionRate * Time.fixedDeltaTime;
        }

        // Use brake input for braking, overriding motor
        float targetBrakeTorque = activeBrakeForce * brakeInput;

        foreach (var wheel in wheels)
        {
            if (wheel.hasMotor)
            {
                wheel.collider.motorTorque = finalTorque;
            }
            // If we are braking, apply brakes.
            wheel.collider.brakeTorque = targetBrakeTorque;
        }
    }

    // (The rest of the methods like PerformFlip, CheckGameOverConditions, etc., are unchanged)
    public void PerformFlip() { if (!isGrounded && Time.time > lastFlipTime + flipCooldown) { lastFlipTime = Time.time; rb.AddTorque(Vector3.forward * -flipTorque, ForceMode.Impulse); currentBoost = Mathf.Min(maxBoost, currentBoost + boostRewardForFlip); } }
    void CheckGameOverConditions() { if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing) return; if (currentFuel <= 0) { GameManager.Instance.EndGame("OUT OF FUEL"); return; } bool isFlipped = Vector3.Dot(transform.up, Vector3.down) > 0; bool isStopped = rb.linearVelocity.magnitude < stoppedSpeedThreshold; if (isFlipped && isStopped) { timeSinceStuck += Time.fixedDeltaTime; if (timeSinceStuck >= stuckTimeThreshold) { GameManager.Instance.EndGame("STUCK"); } } else { timeSinceStuck = 0; } }
    void CheckGroundedStatus() { isGrounded = false; foreach (var wheel in wheels) { if (wheel.collider.isGrounded) { isGrounded = true; return; } } }
    void HandleBoostRegen() { if (!isBoosting && currentBoost < maxBoost) { currentBoost += boostRegenRate * Time.deltaTime; currentBoost = Mathf.Min(currentBoost, maxBoost); } }
    void ApplyAirControl() { if (!isGrounded) { rb.AddTorque(Vector3.forward * airControlInput * airControlTorque); } }
    void UpdateWheelVisuals() { foreach (var wheel in wheels) { Vector3 pos; Quaternion rot; wheel.collider.GetWorldPose(out pos, out rot); wheel.visual.position = pos; wheel.visual.rotation = rot; } }
    public void AddFuel(float amount) { currentFuel += amount; currentFuel = Mathf.Min(currentFuel, maxFuel); }
}