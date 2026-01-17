using UnityEngine;

public class RagdollSystem : MonoBehaviour
{
    public Transform ragdollRoot;
    
    public void Start()
    {
        SetRagdoll(ragdollRoot, false);
    }

    public void StartRagdoll()
    {
        SetRagdoll(ragdollRoot, true);
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
