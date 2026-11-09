using System;
using Unity.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CapsuleCollider))]
public class WallContactSmoother : MonoBehaviour
{
    [Header("Wall smoothing")]
    [Range(0.1f, 0.9f)]
    public float maxWallNormalY = 0.64f;
    [Range(0.01f, 1.0f)]
    public float normalResponse = 0.12f;
    [Min(0.01f)]
    public float contactMemory = 0.08f;

    private CapsuleCollider playerCollider;
    private int playerColliderId;

    private readonly object sync = new();

    private Vector3 stableWallNormal;
    private bool hasStableNormal;
    private float lastWallContactTime = float.NegativeInfinity;


    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
        playerColliderId = playerCollider.GetInstanceID();

        playerCollider.hasModifiableContacts = true;
    }

    private void OnEnable()
        => Physics.ContactModifyEvent += ModifyContacts;
    private void OnDisable()
        => Physics.ContactModifyEvent -= ModifyContacts;

    private void FixedUpdate()
    {
        lock (sync)
        {
            if (hasStableNormal && Time.fixedTime - lastWallContactTime > contactMemory)
                hasStableNormal = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
        => MarkWallContact(collision);
    private void OnCollisionStay(Collision collision)
        => MarkWallContact(collision);
    
    private void MarkWallContact(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (Math.Abs(normal.y) <= maxWallNormalY)
            {
                lock (sync)
                {
                    lastWallContactTime = Time.fixedTime;
                }
                return;
            }
        }
    }

    public bool TryGetWallNormal(out Vector3 normal)
    {
        lock (sync)
        {
            normal = stableWallNormal;
            return hasStableNormal;
        }
    }

    private void ModifyContacts(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
    {
        lock (sync)
        {
            Vector3 normalSum = Vector3.zero;
            for (int p = 0; p < pairs.Length; p++)
            {
                var pair = pairs[p];
                bool playerIsFirst = pair.colliderInstanceID == playerColliderId;
                bool playerIsSecond = pair.otherColliderInstanceID == playerColliderId;
                if (!playerIsFirst && !playerIsSecond)
                    continue;
                
                for (int i = 0; i < pair.contactCount; i++)
                {
                    Vector3 normal = pair.GetNormal(i);
                    if (playerIsSecond)
                        normal = -normal;
                    if (Mathf.Abs(normal.y) > maxWallNormalY)
                        continue;
                    
                    normal.y = 0.0f;
                    if (normal.sqrMagnitude < 0.0001f)
                        continue;
                    
                    normalSum += normal.normalized;
                }
            }

            if (normalSum.sqrMagnitude > 0.000f)
            {
                Vector3 candidate = normalSum.normalized;
                if (!hasStableNormal)
                {
                    stableWallNormal = candidate;
                    hasStableNormal = true;
                }
                else
                {
                    if (Vector3.Dot(candidate, stableWallNormal) < 0.0f)
                        candidate = -candidate;
                    stableWallNormal = Vector3.Slerp(
                        stableWallNormal,
                        candidate,
                        normalResponse
                    ).normalized;
                }
            }
            if (!hasStableNormal)
                return;
            
            for (int p = 0; p < pairs.Length; p++)
            {
                var pair = pairs[p];
                bool playerIsFirst = pair.colliderInstanceID == playerColliderId;
                bool playerIsSecond = pair.otherColliderInstanceID == playerColliderId;
                if (!playerIsFirst && !playerIsSecond)
                    continue;
                
                Vector3 replacementNormal = playerIsFirst
                    ? stableWallNormal : -stableWallNormal;
                for (int i = 0; i < pair.contactCount; i++)
                {
                    Vector3 originalNormal = pair.GetNormal(i);
                    if (playerIsSecond)
                        originalNormal = -originalNormal;
                    if (Mathf.Abs(originalNormal.y) > maxWallNormalY)
                        continue;
                    
                    pair.SetNormal(i, replacementNormal);
                }
            }
        }
    }
}
