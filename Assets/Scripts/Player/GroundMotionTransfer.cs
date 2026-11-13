using UnityEditor.Rendering;
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

    [Header("Platform Carry")]
    [Min(0.0f)]
    public float fullCarryAcceleration = 2.5f;
    [Min(0.0f)]
    public float carryCollisionSkin = 0.002f;


    private Transform trackedSupport;
    private Matrix4x4 previousSupportMatrix;
    private Vector3 previousSupportVelocity;
    private bool hasPreviousSupportSample;

    private static int lastCarrySyncFrame = -1;
    private static double lastCarrySyncFixedTime = -1.0;


    private Rigidbody playerBody;
    private GroundSpring groundSpring;


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
            ResetPlatformCarry();
            return;
        }

        Rigidbody supportBody = groundSpring.SupportBody;
        Transform supportTransform = groundSpring.SupportTransform;

        ApplyPlatformCarry(supportTransform, supportBody);

        Vector3 point = groundSpring.SupportPoint;
        Vector3 normal = groundSpring.GroundNormal.normalized;
        Vector3 supportVelocity = groundSpring.GetSupportVelocityAtPoint(point);

        float normalForce = groundSpring.SupportNormalForce;
        if (normalForce <= 0.0f)
            return;

        Vector3 relativeVelocity = Vector3.ProjectOnPlane(supportVelocity - playerBody.GetPointVelocity(point), normal);
        float relativeSpeed = relativeVelocity.magnitude;
        if (relativeSpeed < 0.0001f)
            return;

        Vector3 direction = relativeVelocity / relativeSpeed;
        float inverseEffectiveMass = GetInverseEffectiveMass(playerBody, point, direction);
        if (supportBody != null && !supportBody.isKinematic)
            inverseEffectiveMass += GetInverseEffectiveMass(supportBody, point, direction);
        if (inverseEffectiveMass <= 1.0e-6f)
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
        playerBody.AddForceAtPosition( impulse, point, ForceMode.Impulse);
        if (supportBody != null && !supportBody.isKinematic)
            supportBody.AddForceAtPosition(-impulse, point, ForceMode.Impulse);
    }

    private void ApplyPlatformCarry(Transform supportTransform, Rigidbody supportBody)
    {
        Matrix4x4 currentMatrix = supportTransform.localToWorldMatrix;
        Vector3 centerOfMass = playerBody.worldCenterOfMass;
        Vector3 supportVelocity = groundSpring.GetSupportVelocityAtPoint(centerOfMass);
        if (
            !hasPreviousSupportSample ||
            trackedSupport != supportTransform ||
            Mathf.Abs(previousSupportMatrix.determinant) < 1.0e-8f
        )
        {
            trackedSupport = supportTransform;
            previousSupportMatrix = currentMatrix;
            previousSupportVelocity = supportVelocity;
            hasPreviousSupportSample = true;
            return;
        }

        float dt = Mathf.Max(Time.fixedDeltaTime, 1.0e-6f);
        Vector3 supportDeltaVelocity = supportVelocity - previousSupportVelocity;
        float supportAcceleration = supportDeltaVelocity.magnitude / dt;
        float carryWeight = Mathf.SmoothStep(0.0f, 1.0f, supportAcceleration / Mathf.Max(fullCarryAcceleration, 0.01f));
        Matrix4x4 platformDelta = currentMatrix * previousSupportMatrix.inverse;
        Vector3 targetCenter = platformDelta.MultiplyPoint3x4(centerOfMass);
        Vector3 carryDisplacement = (targetCenter - centerOfMass) * carryWeight;

        previousSupportMatrix = currentMatrix;
        previousSupportVelocity = supportVelocity;
        if (carryDisplacement.sqrMagnitude > 1.0e-10f)
            ApplyCollisionAwareCarry(carryDisplacement, supportTransform, supportBody);
    }

    private void ApplyCollisionAwareCarry(
        Vector3 displacement,
        Transform supportTransform,
        Rigidbody supportBody
    )
    {
        SyncTransformsForCarry();

        Vector3 remaining = displacement;

        const int maxIterations = 4;
        const float minimumDistance = 1.0e-5f;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            float distance = remaining.magnitude;
            if (distance <= minimumDistance)
                return;
            
            Vector3 direction = remaining / distance;
            RaycastHit[] hits = playerBody.SweepTestAll(direction, distance, QueryTriggerInteraction.Ignore);
            bool foundBlocker = false;
            RaycastHit closestHit = default;
            float closestDistance = distance;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;
                if (IsSupportCollider(hit.collider, supportTransform, supportBody))
                    continue;
                if (hit.distance >= closestDistance)
                    continue;
                
                closestDistance = hit.distance;
                closestHit = hit;
                foundBlocker = true;
            }
            if (!foundBlocker)
            {
                playerBody.position += remaining;
                return;
            }

            float allowedDistance = Mathf.Clamp(closestDistance - carryCollisionSkin, 0.0f, distance);
            Vector3 allowedMovement = direction * allowedDistance;

            playerBody.position += allowedMovement;
            remaining -= allowedMovement;
            if (closestHit.normal.sqrMagnitude < 1.0e-6f)
                return;
            if (Vector3.Dot(remaining, closestHit.normal) >= -1.0e-6f)
                return;
            
            remaining = Vector3.ProjectOnPlane(remaining, closestHit.normal);
        }
    }

    private static bool IsSupportCollider(
        Collider collider,
        Transform supportTransform,
        Rigidbody supportBody
    )
    {
        if (
            supportBody != null &&
            collider.attachedRigidbody == supportBody
        )
            return true;
        return
            collider.transform == supportTransform ||
            collider.transform.IsChildOf(supportTransform);
    }

    private static void SyncTransformsForCarry()
    {
        int frame = Time.frameCount;
        double fixedTime = Time.fixedTimeAsDouble;
        if (
            lastCarrySyncFrame == frame &&
            lastCarrySyncFixedTime == fixedTime
        )
            return;
        
        Physics.SyncTransforms();

        lastCarrySyncFrame = frame;
        lastCarrySyncFixedTime = fixedTime;
    }
    
    private void ResetPlatformCarry()
    {
        trackedSupport = null;
        previousSupportMatrix = default;
        previousSupportVelocity = Vector3.zero;
        hasPreviousSupportSample= false;
    }

    private void Osable()
    {
        ResetPlatformCarry();
    }

    private static float GetInverseEffectiveMass(
        Rigidbody body,
        Vector3 point,
        Vector3 direction
    )
    {
        if (body == null || body.isKinematic || body.mass <= 1.0e-6f)
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
        if (torqueAxis.sqrMagnitude < 1.0e-6f)
            return Mathf.Max(0.0f, inverseLinearMass);
        
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
