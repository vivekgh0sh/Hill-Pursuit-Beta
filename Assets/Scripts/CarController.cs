using UnityEngine;
using UnityEngine.InputSystem;

// THIS IS THE MISSING PART!
// This class defines what a "wheel" is for our controller.
[System.Serializable]
public class WheelInfo
{
    public WheelCollider collider;
    public Transform visual;
    public bool canSteer = false;
    public bool hasMotor = true;
}

// This is your main controller class
public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public Transform centerOfMass;

    [Header("Wheel Setup")]
    public WheelInfo[] wheels; // Now it knows what WheelInfo is

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
        ApplyMotorAndBrake();
        EnforceRotationConstraint();
    }

    void EnforceRotationConstraint()
    {
        // --- PART 1: KILL ROTATIONAL VELOCITY ---
        // Get the current angular velocity
        Vector3 currentAngularVelocity = rb.angularVelocity;

        // Create a new velocity with the Z component forced to zero
        Vector3 correctedAngularVelocity = new Vector3(currentAngularVelocity.x, currentAngularVelocity.y, 0f);

        // Apply the corrected velocity. This stops any existing roll dead in its tracks.
        rb.angularVelocity = correctedAngularVelocity;


        // --- PART 2: CORRECT THE ROTATION (as before) ---
        // Get the current rotation in Euler angles
        Vector3 currentEulerAngles = rb.rotation.eulerAngles;

        // Create the new, corrected rotation
        Quaternion correctedRotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, 0f);

        // Apply the corrected rotation
        rb.rotation = correctedRotation;
    } 

    void ApplyMotorAndBrake()
    {
        float currentBrakeForce = 0f;

        if (verticalInput < 0)
        {
            currentBrakeForce = brakeForce;
        }

        foreach (var wheel in wheels)
        {
            if (wheel.hasMotor)
            {
                wheel.collider.motorTorque = verticalInput * motorForce;
            }

            if (wheel.canSteer) // Front wheels
            {
                wheel.collider.brakeTorque = currentBrakeForce;
            }
            else // Rear wheels
            {
                wheel.collider.brakeTorque = currentBrakeForce * 0.5f;
            }
        }
    }

    void HandleInput()
    {
        verticalInput = 0;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.isPressed) return;

        if (pointer.position.ReadValue().x > Screen.width / 2)
        {
            verticalInput = 1; // Accelerate
        }
        else
        {
            verticalInput = -1; // Brake
        }
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