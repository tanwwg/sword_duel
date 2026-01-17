using System;
using UnityEngine;

public struct PlayerAnimState
{
    public bool isExitAttack;
    public bool canCombo;
}

public class PlayerAnimator: MonoBehaviour
{

    [Header("Animator Params")]
    public string forwardParam = "Forward";
    public string rightParam   = "Strafe";

    [Header("Smoothing")]
    public float dampTime = 0.1f;
    
    public PlayerController playerController;
    public Transform targetTransform;
    
    
    public Animator animator;
    public PlayerAnimationEvents playerEvents;

    private PlayerState lastPlayerState;
    private Vector3 lastPosition;
    private AnimatorStateInfo lastAnim;

    private void Awake()
    {
        SaveStates();
    }

    void SaveStates()
    {
        lastPosition = targetTransform.position;
        lastPlayerState = playerController.playerState; 
        lastAnim = animator.GetCurrentAnimatorStateInfo(0);
    }

    public PlayerAnimState GetAnimState()
    {
        var state = new PlayerAnimState();
        
        var currentAnim = animator.GetCurrentAnimatorStateInfo(0);
        var isLastAttack = lastAnim.IsName("slash1") || lastAnim.IsName("slash2") || lastAnim.IsName("SpinAttack");
        if (currentAnim.IsName("Movement") && isLastAttack)
        {
            state.isExitAttack = true;
        }

        state.canCombo = playerEvents.canCombo;

        return state;
    }

    public void Tick()
    {
        var dt = Time.deltaTime;
        var worldDelta = targetTransform.position - lastPosition;
        
        // Convert to local space (right / forward)
        var localDelta = transform.InverseTransformDirection(worldDelta);
        
        float forwardSpeed = localDelta.z / dt;
        float rightSpeed   = localDelta.x / dt;

        // Normalize to -1..1 (percent of max speed)
        var maxSpeed = playerController.moveSpeed;
        float forward01 = Mathf.Clamp(forwardSpeed / maxSpeed, -1f, 1f);
        float right01   = Mathf.Clamp(rightSpeed   / maxSpeed,  -1f, 1f);
        
        // Feed animator (let Animator handle smoothing)
        animator.SetFloat(forwardParam, forward01, dampTime, dt);
        animator.SetFloat(rightParam,   right01,   dampTime, dt);

        var nowState = playerController.playerState;
        if (lastPlayerState != nowState)
        {
            if (nowState == PlayerState.Attack1)
            {
                animator.SetTrigger("Slash1");
                animator.SetBool("Slash2", false);
                animator.SetBool("SpinAttack", false);
            }
            else if (nowState == PlayerState.Attack2)
            {
                animator.SetBool("Slash2", true);
            }
            else if (nowState == PlayerState.Attack3)
            {
                animator.SetBool("SpinAttack", true);
            }
        }
        
        
        
        SaveStates();
    }
}