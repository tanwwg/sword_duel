using UnityEngine;

public class StunHandler: MonoBehaviour
{
    public Rigidbody rb;
    public CharacterController controller;

    public AnimationClip hitClip;
    public Animator animator;

    [Header("Runtime vars")] 
    public float stunTime;

    public void HitStun(Vector3 forceDir, float time)
    {
        stunTime += time;
        controller.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(forceDir, ForceMode.Impulse);
        animator.SetFloat("OnHitSpeed", hitClip.length / stunTime);
        animator.Play("onhit", 0, 0f);
    }

    public bool UpdateStun()
    {
        if (stunTime <= 0) return false;
        
        stunTime -= Time.deltaTime;
        if (stunTime <= 0)
        {
            rb.isKinematic = true;
            controller.enabled = true;
            animator.CrossFade("Movement", 0.2f, 0);
        }
        return true;
    }
}
