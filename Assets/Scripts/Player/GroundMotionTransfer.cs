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

    private Transform trackedSupport;

    public Vector3 CarriedVelocity { get; private set; }


    public bool HasSupport
        => groundSpring.IsGrounded && groundSpring.SupportTransform != null;
    
    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        groundSpring = GetComponent<GroundSpring>();
    }
    
    public Vector3 GetSupportVelocity()
        => HasSupport ? groundSpring.SupportVelocity : Vector3.zero;
    
    public void Apply()
{
    if (!HasSupport)
    {
        trackedSupport = null;
        CarriedVelocity = Vector3.zero;
        return;
    }

    Transform supportTransform = groundSpring.SupportTransform;
    if (trackedSupport != supportTransform)
    {
        trackedSupport = supportTransform;
        CarriedVelocity = Vector3.zero;
    }

    float normalForce = groundSpring.SupportNormalForce;
    if (normalForce <= 0.0f)
        return;

    Rigidbody supportBody = groundSpring.SupportBody;
    Vector3 point = groundSpring.SupportPoint;
    Vector3 normal = groundSpring.GroundNormal.normalized;
    Vector3 supportVelocity = groundSpring.GetSupportVelocityAtPoint(point);
    Vector3 relativeVelocity = Vector3.ProjectOnPlane(supportVelocity - CarriedVelocity, normal);
    float relativeSpeed = relativeVelocity.magnitude;
    if (relativeSpeed < 0.0001f)
        return;

    Vector3 direction = relativeVelocity / relativeSpeed;
    float inverseEffectiveMass =GetInverseEffectiveMass(playerBody, point, direction);
    if (supportBody != null && !supportBody.isKinematic)
        inverseEffectiveMass += GetInverseEffectiveMass(supportBody, point, direction);
    if (inverseEffectiveMass <= 0.000001f)
        return;

    float requiredImpulse = relativeSpeed / inverseEffectiveMass;
    float dt = Time.fixedDeltaTime;
    float staticFriction = Mathf.Max(0.0f, staticFrictionCoefficient);
    float dynamicFriction = Mathf.Clamp(dynamicFrictionCoefficient, 0.0f, staticFriction);
    float maxStaticImpulse = staticFriction * normalForce * dt;
    float maxDynamicImpulse = dynamicFriction * normalForce * dt;
    float impulseMagnitude = requiredImpulse <= maxStaticImpulse
        ? requiredImpulse : Mathf.Min(requiredImpulse, maxDynamicImpulse);
    if (impulseMagnitude <= 0.0f)
        return;

    Vector3 impulse = direction * impulseMagnitude;
    playerBody.AddForceAtPosition(impulse, point, ForceMode.Impulse);
    if (supportBody != null && !supportBody.isKinematic)
        supportBody.AddForceAtPosition(-impulse, point, ForceMode.Impulse);

    CarriedVelocity += impulse / playerBody.mass;
}

    private static float GetInverseEffectiveMass(
        Rigidbody body,
        Vector3 point,
        Vector3 direction
    )
    {
        if (body == null || body.isKinematic || body.mass <= 0.000001f)
            return 0.0f;
        
        Vector3 allowedDirectionLocal = Quaternion.Inverse(body.rotation) * direction;
        RigidbodyConstraints constraints = body.constraints;
        if ((constraints & RigidbodyConstraints.FreezePositionX) != 0)
            allowedDirectionLocal.x = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezePositionY) != 0)
            allowedDirectionLocal.y = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezePositionZ) != 0)
            allowedDirectionLocal.z = 0.0f;
        
        Vector3 allowedDirectionWorld = body.rotation * allowedDirectionLocal;
        float inverseLinearMass = Vector3.Dot(direction, allowedDirectionWorld) / body.mass;
        Vector3 torqueAxis = Vector3.Cross(point - body.worldCenterOfMass, direction);
        if (torqueAxis.sqrMagnitude < 0.000001f)
            return Mathf.Max(0.0f, inverseLinearMass);
        
        Quaternion inertiaFrame = body.rotation * body.inertiaTensorRotation;
        Vector3 localTorque = Quaternion.Inverse(inertiaFrame) * torqueAxis;
        Vector3 inertia = body.inertiaTensor;
        Vector3 localAngularDelta = new(
            inertia.x > 0.000001f ? localTorque.x / inertia.x : 0.0f,
            inertia.y > 0.000001f ? localTorque.y / inertia.y : 0.0f,
            inertia.z > 0.000001f ? localTorque.z / inertia.z : 0.0f
        );
        Vector3 angularDeltaWorld = inertiaFrame * localAngularDelta;
        Vector3 angularDeltaLocal = Quaternion.Inverse(body.rotation) * angularDeltaWorld;
        if ((constraints & RigidbodyConstraints.FreezeRotationX) != 0)
            angularDeltaLocal.x = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezeRotationY) != 0)
            angularDeltaLocal.y = 0.0f;
        if ((constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
            angularDeltaLocal.z = 0.0f;

        angularDeltaWorld = body.rotation * angularDeltaLocal;
        float inverseRotationalMass = Vector3.Dot(torqueAxis, angularDeltaWorld);
        return Mathf.Max(0.0f, inverseLinearMass) + Mathf.Max(0.0f, inverseRotationalMass);
    }
}
