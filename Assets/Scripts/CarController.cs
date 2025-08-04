// 04-08-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of forward/backward movement
    public float turnSpeed = 50f; // Speed of turning

    private CarInputActions carInputActions; // Reference to the input actions
    private Vector2 moveInput; // Stores the input from the player

    private void Awake()
    {
        // Initialize the input actions
        carInputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        // Enable the input actions
        carInputActions.Enable();

        // Subscribe to the Move action
        carInputActions.CarControls.Move.performed += OnMovePerformed;
        carInputActions.CarControls.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from the Move action
        carInputActions.CarControls.Move.performed -= OnMovePerformed;
        carInputActions.CarControls.Move.canceled -= OnMoveCanceled;

        // Disable the input actions
        carInputActions.Disable();
    }

    private void Update()
    {
        // Move the car forward/backward
        transform.Translate(Vector3.forward * moveInput.y * moveSpeed * Time.deltaTime);

        // Rotate the car left/right
        transform.Rotate(Vector3.up, moveInput.x * turnSpeed * Time.deltaTime);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Read the input value when the Move action is performed
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // Reset the input value when the Move action is canceled
        moveInput = Vector2.zero;
    }
}
