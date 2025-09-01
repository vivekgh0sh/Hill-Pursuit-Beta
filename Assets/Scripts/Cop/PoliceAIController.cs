// --- START OF FILE PoliceAIController.cs (SIMPLIFIED & STABILIZED REVISION) ---

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CarController))]
public class PoliceAIController : MonoBehaviour
{
    [Header("AI Targeting")]
    public Transform playerTarget;
    [Tooltip("The distance at which the player is considered 'caught'.")]
    public float catchDistance = 5f;

    [Header("AI Behavior")]
    [Tooltip("AI accelerates if player is further than this distance.")]
    public float throttleDistance = 20f;
    [Tooltip("AI brakes if player is closer than this distance.")]
    public float brakeDistance = 15f;

    private CarController carController;
    private bool isAIActive = false; // Prevents AI from acting immediately on spawn

    void Awake()
    {
        carController = GetComponent<CarController>();
    }

    void Start()
    {
        // Start a coroutine to delay the AI's activation. This helps prevent flipping on spawn.
        StartCoroutine(ActivateAIWithDelay());
    }

    private IEnumerator ActivateAIWithDelay()
    {
        // Wait for 1.5 seconds to let the car settle on the ground
        yield return new WaitForSeconds(1.5f);
        isAIActive = true;
    }

    void FixedUpdate()
    {
        // Do nothing if the AI is not yet active or has no target
        if (!isAIActive || playerTarget == null)
        {
            carController.SetAI_MotorInput(0);
            carController.SetAI_BrakeInput(1); // Hold brakes while inactive
            return;
        }

        // --- CORE MOVEMENT LOGIC (Simplified) ---

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 1. Check if we caught the player
        if (distanceToPlayer < catchDistance)
        {
            GameManager.Instance.EndGame("YOU WERE CAUGHT!");
            carController.SetAI_BrakeInput(1); // Brake hard when caught
            carController.SetAI_MotorInput(0);
            return;
        }

        // 2. Decide to accelerate or brake based on distance
        bool isPlayerInFront = playerTarget.position.x > transform.position.x;

        if (isPlayerInFront)
        {
            // Player is in front, decide whether to hit the gas or the brakes
            if (distanceToPlayer > throttleDistance)
            {
                // Player is far away, full throttle!
                carController.SetAI_MotorInput(1f);
                carController.SetAI_BrakeInput(0);
            }
            else if (distanceToPlayer < brakeDistance)
            {
                // Player is too close, hit the brakes!
                carController.SetAI_MotorInput(0);
                carController.SetAI_BrakeInput(1f);
            }
            else
            {
                // We are in the sweet spot between braking and throttling, so coast.
                carController.SetAI_MotorInput(0);
                carController.SetAI_BrakeInput(0);
            }
        }
        else
        {
            // Player is behind us. We need to turn around.
            // Hit the REVERSE motor and DO NOT brake.
            carController.SetAI_MotorInput(-0.5f); // Use partial reverse to turn more easily
            carController.SetAI_BrakeInput(0);
        }
    }
}