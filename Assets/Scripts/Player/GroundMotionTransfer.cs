using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(GroundSpring))]
public class GroundMotionTransfer : MonoBehaviour
{
    [Header("Surface Friction")]
    [Min(0.0f)]
    public float staticFrictionCoefficient = 0.8f;
    [Min(0.0f)]
    public float dynamicFrictionCoefficient = 0.6f;

    private Rigidbody playerBody;
    private GroundSpring groundSpring;


    public bool HasRigidbodySupport
        => groundSpring.IsGrounded && groundSpring.SupportBody != null;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        groundSpring = GetComponent<GroundSpring>();
    }

    public Vector3 GetSupportVelocity()
    {
        if (!HasRigidbodySupport)
            return Vector3.zero;
        return groundSpring.SupportBody.GetPointVelocity(groundSpring.SupportPoint);
    }

    public void Apply()
    {
        if (!HasRigidbodySupport || groundSpring.SupportNormalForce <= 0.0f)
            return;
        
        Rigidbody supportBody = groundSpring.SupportBody;
        Vector3 point = groundSpring.SupportPoint;
        Vector3 normal = groundSpring.GroundNormal.normalized;
        Vector3 supportVelocity = supportBody.GetPointVelocity(point);
        Vector3 playerVelocity = playerBody.GetPointVelocity(point);
        Vector3 relativeVelocity = Vector3.ProjectOnPlane(supportVelocity - playerVelocity, normal);
        float relativeSpeed = relativeVelocity.magnitude;
        if (relativeSpeed < 0.0001f)
            return;
        
        Vector3 direction = relativeVelocity / relativeSpeed;
        float inverseEffectiveMass =
            GetInverseEffectiveMass(playerBody, point, direction) +
            GetInverseEffectiveMass(supportBody, point, direction);
        if (inverseEffectiveMass <= 0.0f)
            return;
        
        float requiredImpulse = relativeSpeed / inverseEffectiveMass;
        float normalForce = groundSpring.SupportNormalForce;
        float dt = Time.fixedDeltaTime;
        float maxStaticImpulse = staticFrictionCoefficient * normalForce * dt;
        float maxDynamicImpulse = Mathf.Min(dynamicFrictionCoefficient, staticFrictionCoefficient) * normalForce * dt;
        float impulseMagnitude = requiredImpulse <= maxStaticImpulse
            ? requiredImpulse : Mathf.Min(requiredImpulse, maxDynamicImpulse);
        if (impulseMagnitude <= 0.0f)
            return;
        
        Vector3 impulse = direction * impulseMagnitude;
        playerBody.AddForceAtPosition(impulse, point, ForceMode.Impulse);
        if (!supportBody.isKinematic)
            supportBody.AddForceAtPosition(-impulse, point, ForceMode.Impulse);
    }

    private static float GetInverseEffectiveMass(Rigidbody body, Vector3 point, Vector3 direction)
    {
        if (body == null || body.isKinematic)
            return 0.0f;
        
        float inverseMass = 1.0f / body.mass;
        Vector3 torqueAxis = Vector3.Cross(point - body.worldCenterOfMass, direction);
        if (torqueAxis.sqrMagnitude < 1.0e-6f)
            return inverseMass;
        
        Quaternion inertiaFrame = body.rotation * body.inertiaTensorRotation;
        Vector3 localTorque = Quaternion.Inverse(inertiaFrame) * torqueAxis;
        Vector3 inertia = body.inertiaTensor;
        Vector3 localAngularDelta = new(
            inertia.x > 1.0e-6f ? localTorque.x / inertia.x : 0.0f,
            inertia.y > 1.0e-6f ? localTorque.y / inertia.y : 0.0f,
            inertia.z > 1.0e-6f ? localTorque.z / inertia.z : 0.0f
        );
        Vector3 angularDeltaWorld = inertiaFrame * localAngularDelta;
        Vector3 angularDeltaLocal = Quaternion.Inverse(body.rotation) * angularDeltaWorld;
        RigidbodyConstraints constraints = body.constraints;
        if ((constraints & RigidbodyConstraints.FreezeRotationX) != 0)
            angularDeltaLocal.x = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezeRotationY) != 0)
            angularDeltaLocal.y = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
            angularDeltaLocal.z = 0.0f;

        angularDeltaWorld = body.rotation * angularDeltaLocal;
        float inverseRotationalMass = Vector3.Dot(torqueAxis, angularDeltaWorld);
        return inverseMass + Mathf.Max(0.0f, inverseRotationalMass);
    }
}
