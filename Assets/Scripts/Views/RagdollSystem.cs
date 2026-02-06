using System.Collections.Generic;
using UnityEngine;

struct BonePose {
    public Vector3 localPos;
    public Quaternion localRot;
}



public class RagdollSystem : MonoBehaviour
{
    public Transform ragdollRoot;

    /// <summary>
    /// animator needs to be disabled
    /// </summary>
    public Animator animator;
    
    /// <summary>
    /// disable existing character controller
    /// </summary>
    public CharacterController characterController;
    
    public void Start()
    {
        SetRagdoll(ragdollRoot, false);
    }

    public void StartRagdoll()
    {
        CachePose();
        SetRagdoll(ragdollRoot, true);
        if (animator) animator.enabled = false;
        if (characterController) characterController.enabled = false;
    }

    private Dictionary<Transform, BonePose> _cachedPose = new();

    void CachePose() {
        _cachedPose = new();
        foreach (var t in ragdollRoot.GetComponentsInChildren<Transform>()) {
            _cachedPose[t] = new BonePose {
                localPos = t.localPosition,
                localRot = t.localRotation
            };
        }
    }
    
    public void ResetRagdoll() {
        SetRagdoll(ragdollRoot, false); // stop physics
        foreach (var kv in _cachedPose) {
            kv.Key.localPosition = kv.Value.localPos;
            kv.Key.localRotation = kv.Value.localRot;
        }

        if (animator)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.enabled = true;
        }

        if (characterController) characterController.enabled = true;
    }
    
    
    private void SetRagdoll(Transform t, bool isEnabled)
    {
        var rbarr = t.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbarr) rb.isKinematic = !isEnabled;
        
        var collarr = t.GetComponentsInChildren<Collider>();
        foreach (var coll in collarr)
        {
            coll.isTrigger = !isEnabled;
            coll.enabled = isEnabled;
        }
    }
}
