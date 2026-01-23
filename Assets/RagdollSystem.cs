using UnityEngine;

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
        SetRagdoll(ragdollRoot, true);
        if (animator) animator.enabled = false;
        if (characterController) characterController.enabled = false;
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
