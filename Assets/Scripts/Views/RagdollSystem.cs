using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RagdollPart
{
    public string name;
    public Transform transform;
    public Collider collider;
    public Rigidbody rigidbody;

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
    
    public List<RagdollPart> parts;
    
    [ContextMenu("Rebuild Children List")]
    private void RebuildChildrenList()
    {
        parts.Clear();
        Rebuild(ragdollRoot);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void Rebuild(Transform parent)
    {
        parts.Add(new RagdollPart()
        {
            name = parent.name,
            transform = parent,
            collider = parent.GetComponent<Collider>(),
            rigidbody = parent.GetComponent<Rigidbody>(),
            localPos = parent.localPosition,
            localRot = parent.localRotation,
        });
        foreach (Transform c in parent) Rebuild(c);
    }
    
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

    public void ResetRagdoll() {
        SetRagdoll(ragdollRoot, false); // stop physics

        foreach (var p in parts)
        {
            p.transform.localPosition = p.localPos;
            p.transform.localRotation = p.localRot;
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
        foreach (var p in parts)
        {
            if (p.rigidbody) p.rigidbody.isKinematic = !isEnabled;
            if (p.collider)
            {
                p.collider.isTrigger = !isEnabled;
                p.collider.enabled = isEnabled;
            }
        }
    }
}
