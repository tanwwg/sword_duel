using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public struct PlayerAnimState
{
    public bool canCombo;
    
    /// <summary>
    /// Switch on weapon collider?
    /// </summary>
    public bool isAttacking;
    
    public bool isExitAttack;
}



public class PlayerAnimator: MonoBehaviour
{

    [Header("Animator Params")]
    public string forwardParam = "Forward";
    public string rightParam   = "Strafe";

    [Header("Animation Overrides")] 
    public float attackSpeed = 1.0f;

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

    public AnimationClip onHitClip;

    public bool isStartedAttacking;

    public UnityEvent onStartAttack;
    public UnityEvent onHit;
    public float onHitSpedMultiplier = 1.0f;

    public CinemachineCamera[] cameras;

    public RagdollSystem ragdoll;
    
 

    private void OnEnable()
    {
        animator.SetFloat("Slash1Speed", attackSpeed);
        animator.SetFloat("Slash2Speed", attackSpeed);        
        SaveStates();
    }

    void SaveStates()
    {
        lastPosition = targetTransform.position;
        lastPlayerState = playerController.playerState.Value; 
        lastAnim = animator.GetCurrentAnimatorStateInfo(0);
    }

    private bool IsAttack(AnimatorStateInfo stateInfo)
    {
        return stateInfo.IsName("slash1") || stateInfo.IsName("slash2") || stateInfo.IsName("SpinAttack");
    }

    public PlayerAnimState GetAnimState()
    {
        var state = new PlayerAnimState();
        
        var currentAnim = animator.GetCurrentAnimatorStateInfo(0);

        if (currentAnim.shortNameHash != lastAnim.shortNameHash && IsAttack(currentAnim))
        {
            onStartAttack.Invoke();
        }
        
        if (!IsAttack(currentAnim))
        {
            playerEvents.isAttacking = false;
            playerEvents.canCombo = false;

            if (isStartedAttacking)
            {
                state.isExitAttack = true;
                onExitAttack.Invoke();
            }

            isStartedAttacking = false;
        }
        else
        {
            
            isStartedAttacking = true;
        }
        
        state.isAttacking = playerEvents.isAttacking;
        state.canCombo = playerEvents.canCombo;

        return state;
    }

    void SetupCams()
    {
        if (!playerController.lockTarget) return;
        foreach (var cam in cameras)
        {
            cam.LookAt = playerController.lockTarget.lookTarget;
        }
    }

    public void Tick(PlayerTickResult tickResult)
    {
        var dt = Time.deltaTime;
        var worldDelta = targetTransform.position - lastPosition;

        SetupCams();
        
        // Convert to local space (right / forward)
        var localDelta = transform.InverseTransformDirection(worldDelta);
        
        float forwardSpeed = localDelta.z / dt;
        float rightSpeed   = localDelta.x / dt;
        
        animator.SetFloat(forwardParam, forwardSpeed, dampTime, dt);
        animator.SetFloat(rightParam,   rightSpeed,   dampTime, dt);

        if (tickResult.hitInfo != null)
        {
            Instantiate(tickResult.hitInfo.weapon.hitPrefab, tickResult.hitInfo.hitPoint, Quaternion.identity);
            onHit.Invoke();
            animator.SetTrigger("OnHit");
            animator.SetFloat("OnHitSpeed", onHitClip.length / playerController.stunTime * onHitSpedMultiplier);   
        }

        var nowState = playerController.playerState.Value;
        if (lastPlayerState != nowState)
        {
            if (lastPlayerState == PlayerState.Death)
            {
                Debug.Log("Resetting the ragdoll!");
                ragdoll.ResetRagdoll();
            }
            
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
            else if (nowState == PlayerState.Death)
            {
                ragdoll.StartRagdoll();
                onDie.Invoke();
            }
        }
        
        
        
        SaveStates();
    }
}