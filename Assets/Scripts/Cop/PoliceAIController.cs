// --- CREATE NEW FILE: PoliceAIController.cs ---

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CarController))]
public class PoliceAIController : MonoBehaviour
{
    [Header("AI Settings")]
    [Tooltip("The transform of the player car that the AI should chase.")]
    public Transform playerTarget;
    [Tooltip("How close the cop tries to get to the player before braking.")]
    public float targetDistance = 15f;
    [Tooltip("The distance at which the player is considered 'caught'.")]
    public float catchDistance = 5f;

    [Header("AI Behavior")]
    [Tooltip("How often, in seconds, the AI can use its boost to catch up.")]
    public float boostCooldown = 10f;
    [Tooltip("How long the boost lasts when activated.")]
    public float boostDuration = 2.0f;
    [Tooltip("The AI will boost if the player is further than this distance.")]
    public float boostActivationDistance = 40f;

    [Tooltip("How strongly the AI tries to level itself in the air.")]
    public float airControlStiffness = 200f;

    private CarController carController;
    private float lastBoostTime = -99f;
    private bool isStuck = false;

    void Awake()
    {
        // Get the CarController component attached to this same GameObject
        carController = GetComponent<CarController>();
    }

    void FixedUpdate()
    {
        // If we don't have a target, do nothing.
        if (playerTarget == null)
        {
            carController.SetVerticalInput(0); // Stop the car
            return;
        }

        // --- Core AI Logic ---
        HandleChasing();
        HandleBoosting();
        HandleFlippingAndStuck();
        HandleAirControl();
    }

    private void HandleAirControl()
    {
        if (!carController.isGrounded)
        {
            // If we are in the air, try to level out
            float angleCorrection = Vector3.SignedAngle(transform.right, Vector3.right, Vector3.forward);
            GetComponent<Rigidbody>().AddTorque(Vector3.forward * angleCorrection * airControlStiffness * Time.fixedDeltaTime);
        }
    }
    private void HandleChasing()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 1. Check if the player is caught
        if (distanceToPlayer < catchDistance)
        {
            GameManager.Instance.EndGame("YOU WERE CAUGHT!");
            return; // Stop processing AI once caught
        }

        // --- REVISED LOGIC ---
        // 2. Determine driving direction
        bool isPlayerInFront = playerTarget.position.x > transform.position.x;

        if (isPlayerInFront)
        {
            // Player is ahead of us. Full throttle!
            carController.SetVerticalInput(1f);
        }
        else
        {
            // We have overshot the player. Brake hard to turn around.
            carController.SetVerticalInput(-1f);
        }
        // --- END OF REVISED LOGIC ---
    }

    private void HandleBoosting()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // Check if we should boost
        if (distanceToPlayer > boostActivationDistance && Time.time > lastBoostTime + boostCooldown)
        {
            StartCoroutine(ActivateBoost());
        }
    }

    private IEnumerator ActivateBoost()
    {
        lastBoostTime = Time.time;
        carController.isBoosting = true;
        yield return new WaitForSeconds(boostDuration);
        carController.isBoosting = false;
    }

    private void HandleFlippingAndStuck()
    {
        // Use the same logic as the player for flipping
        bool isFlipped = Vector3.Dot(transform.up, Vector3.down) > 0;
        bool isStopped = GetComponent<Rigidbody>().linearVelocity.magnitude < 0.5f;

        if (isFlipped && isStopped)
        {
            carController.PerformFlip();
        }
    }
}