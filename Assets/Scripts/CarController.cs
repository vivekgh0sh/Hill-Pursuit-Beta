// --- START OF FILE CarController.cs (REVISED FOR AI) ---

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// ... (WheelInfo class is the same)
[System.Serializable]
public class WheelInfo { public WheelCollider collider; public Transform visual; public bool canSteer = false; public bool hasMotor = true; }

public class CarController : MonoBehaviour
{
    // --- ADD THIS NEW VARIABLE ---
    [Header("Control Type")]
    [Tooltip("If checked, this car will respond to player screen input. Uncheck for AI cars.")]
    public bool isPlayerControlled = true;
    // --- END OF ADDED VARIABLE ---

    [Header("Car Settings")]
    public float motorForce = 12000f;
    // ... (rest of variables are the same)
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

    private float verticalInput;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (centerOfMass != null)
        {
            rb.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
        }
        // NOTE: Fuel/Boost initialization is now handled in the Initialize method
    }

    public void Initialize(CarData data)
    {
        // ... (This method is the same as before)
        if (GameManager.Instance == null) return;
        foreach (var upgrade in data.upgrades)
        {
            int level = GameManager.Instance.GetUpgradeLevel(data.carID, upgrade.upgradeID);
            float finalValue = upgrade.baseValue + (upgrade.valuePerLevel * level);
            switch (upgrade.upgradeID)
            {
                case "engine_power": this.motorForce = finalValue; break;
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
        // --- MODIFY THIS BLOCK ---
        // Only handle screen input if this is the player's car
        if (isPlayerControlled)
        {
            HandleInput();
        }
        // --- END OF MODIFICATION ---

        UpdateWheelVisuals();
        HandleBoostRegen();
    }

    // --- ADD THIS NEW PUBLIC METHOD FOR AI ---
    /// <summary>
    /// Allows an external script (like an AI controller) to set the car's input.
    /// </summary>
    /// <param name="input">1 for accelerate, -1 for brake/reverse, 0 for coast.</param>
    public void SetVerticalInput(float input)
    {
        verticalInput = input;
    }
    // --- END OF NEW METHOD ---

    // ... (The rest of the script remains unchanged)
    void FixedUpdate() { CheckGroundedStatus(); ApplyDrivingForces(); ApplyAirControl(); CheckGameOverConditions(); }
    public void PerformFlip() { if (!isGrounded && Time.time > lastFlipTime + flipCooldown) { lastFlipTime = Time.time; rb.AddTorque(Vector3.forward * -flipTorque, ForceMode.Impulse); currentBoost = Mathf.Min(maxBoost, currentBoost + boostRewardForFlip); } }
    void HandleInput() { verticalInput = 0; var pointer = Pointer.current; if (pointer == null || !pointer.press.isPressed) return; if (pointer.position.ReadValue().x > Screen.width / 2) { verticalInput = 1; } else { verticalInput = -1; } }
    void ApplyDrivingForces() { if (verticalInput != 0) { rb.WakeUp(); } float motorInput = verticalInput > 0 && currentFuel > 0 ? verticalInput : 0; float finalMotorForce = motorForce; if (isBoosting && currentBoost > 0) { finalMotorForce += boostForce; currentBoost -= boostDepletionRate * Time.fixedDeltaTime; } float targetMotorTorque = finalMotorForce * motorInput; if (verticalInput > 0 && currentFuel > 0 && isPlayerControlled) { currentFuel -= fuelDepletionRate * Time.fixedDeltaTime; } float targetBrakeTorque = verticalInput < 0 ? activeBrakeForce : 0f; foreach (var wheel in wheels) { if (wheel.hasMotor) { wheel.collider.motorTorque = targetMotorTorque; } wheel.collider.brakeTorque = targetBrakeTorque; } }
    void CheckGameOverConditions() { if (GameManager.Instance == null || GameManager.Instance.currentState != GameManager.GameState.Playing) return; if (isPlayerControlled && currentFuel <= 0) { GameManager.Instance.EndGame("OUT OF FUEL"); return; } bool isFlipped = Vector3.Dot(transform.up, Vector3.down) > 0; bool isStopped = rb.linearVelocity.magnitude < stoppedSpeedThreshold; if (isPlayerControlled && isFlipped && isStopped) { timeSinceStuck += Time.fixedDeltaTime; if (timeSinceStuck >= stuckTimeThreshold) { GameManager.Instance.EndGame("STUCK"); } } else { timeSinceStuck = 0; } }
    void CheckGroundedStatus() { isGrounded = false; foreach (var wheel in wheels) { if (wheel.collider.isGrounded) { isGrounded = true; return; } } }
    void HandleBoostRegen() { if (!isBoosting && currentBoost < maxBoost) { currentBoost += boostRegenRate * Time.deltaTime; currentBoost = Mathf.Min(currentBoost, maxBoost); } }
    void ApplyAirControl() { if (!isGrounded) { rb.AddTorque(Vector3.forward * verticalInput * airControlTorque); } }
    void UpdateWheelVisuals() { foreach (var wheel in wheels) { Vector3 pos; Quaternion rot; wheel.collider.GetWorldPose(out pos, out rot); wheel.visual.position = pos; wheel.visual.rotation = rot; } }
    public void AddFuel(float amount) { currentFuel += amount; currentFuel = Mathf.Min(currentFuel, maxFuel); }
}