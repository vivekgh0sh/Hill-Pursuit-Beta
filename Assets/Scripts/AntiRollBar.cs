using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    public WheelCollider wheelL;
    public WheelCollider wheelR;
    public float antiRollForce = 5000f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = wheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;

        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

        float antiRollG = (travelL - travelR) * antiRollForce;

        if (groundedL)
            rb.AddForceAtPosition(wheelL.transform.up * -antiRollG, wheelL.transform.position);

        if (groundedR)
            rb.AddForceAtPosition(wheelR.transform.up * antiRollG, wheelR.transform.position);
    }
}