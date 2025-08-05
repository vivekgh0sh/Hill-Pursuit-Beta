using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Assign your Player object here

    [Header("Settings")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // This determines the isometric distance and angle

    void LateUpdate()
    {
        if (target == null) return;

        // The camera's desired position is the target's position plus the offset
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Make the camera always look at the target
        transform.LookAt(target);
    }
}