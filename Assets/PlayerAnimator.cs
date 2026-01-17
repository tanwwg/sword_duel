using System;
using UnityEngine;
using UnityEngine.Events;

public struct PlayerAnimState
{
    public bool isExitAttack;
    public bool canCombo;
    public bool isAttacking;
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

    public UnityEvent onSlash2;
    public UnityEvent onSlash3;
    public UnityEvent onExitAttack;

    public UnityEvent onDie;

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
            onExitAttack.Invoke();
        }

        state.isAttacking = playerEvents.isAttacking;
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
        
        animator.SetFloat(forwardParam, forwardSpeed, dampTime, dt);
        animator.SetFloat(rightParam,   rightSpeed,   dampTime, dt);

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
                onSlash2.Invoke();
            }
            else if (nowState == PlayerState.Attack3)
            {
                animator.SetBool("SpinAttack", true);
                onSlash3.Invoke();
            } 
            else if (nowState == PlayerState.Stun)
            {
                animator.SetTrigger("OnHit");
            } 
            else if (nowState == PlayerState.Death)
            {
                onDie.Invoke();
            }
        }
        
        
        
        SaveStates();
    }
}