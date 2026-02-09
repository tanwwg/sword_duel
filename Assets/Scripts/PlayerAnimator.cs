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

    public GameObject glintFab;
    
    public Animator animator;
    public PlayerAnimationEvents playerEvents;
    
    public AnimationClip onHitClip;
    
    public float onHitSpedMultiplier = 1.0f;

    public RagdollSystem ragdoll;
    

    [Header("Last states")]
    
    public bool isStartedAttacking;

    private PlayerState lastPlayerState;

    private AttackState lastAttackState;
    private Vector3 lastPosition;
    private AnimatorStateInfo lastAnim;
    private float lastStun;


    private void OnEnable()
    {
        animator.SetFloat("Slash1Speed", attackSpeed);
        animator.SetFloat("Slash2Speed", attackSpeed);        
        SaveStates();
    }

    void SaveStates()
    {
        lastPosition = targetTransform.position;
        lastAttackState = playerController.comboSystem.state;
        lastPlayerState = playerController.playerState.Value; 
        lastAnim = animator.GetCurrentAnimatorStateInfo(0);
        lastStun = playerController.stunTime;
    }

    private bool IsAttack(AnimatorStateInfo stateInfo)
    {
        return stateInfo.IsName("slash1") || stateInfo.IsName("slash2") || stateInfo.IsName("SpinAttack") || stateInfo.IsName("slash_charge") || stateInfo.IsName("slash_charge_idle");
    }

    public PlayerAnimState GetAnimState()
    {
        var state = new PlayerAnimState();
        
        var currentAnim = animator.GetCurrentAnimatorStateInfo(0);

        if (!IsAttack(currentAnim))
        {
            playerEvents.isAttacking = false;
            playerEvents.canCombo = false;

            if (isStartedAttacking)
            {
                state.isExitAttack = true;
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

    public void Tick()
    {
        var dt = Time.deltaTime;
        var worldDelta = targetTransform.position - lastPosition;

        // Convert to local space (right / forward)
        var localDelta = transform.InverseTransformDirection(worldDelta);

        float forwardSpeed = localDelta.z / dt;
        float rightSpeed = localDelta.x / dt;

        animator.SetFloat(forwardParam, forwardSpeed, dampTime, dt);
        animator.SetFloat(rightParam, rightSpeed, dampTime, dt);

        // if (tickResult.hitInfo != null)
        // {
        //     Instantiate(tickResult.hitInfo.weapon.hitPrefab, tickResult.hitInfo.hitPoint, Quaternion.identity);
        //     onHit.Invoke();
        //     animator.SetTrigger("OnHit");
        //     animator.SetFloat("OnHitSpeed", onHitClip.length / playerController.stunTime * onHitSpedMultiplier);
        // }

        if (playerController.stunTime > lastStun)
        {
            if (playerController.stunTime > 0.1f)
            {
                animator.SetTrigger("OnHit");
                animator.SetFloat("OnHitSpeed", onHitClip.length / playerController.stunTime * onHitSpedMultiplier);
            }
            else
            {
                animator.SetTrigger("OnSmallHit");
            }
            
        }

        var nowState = playerController.playerState.Value;
        if (lastPlayerState != nowState)
        {
            if (lastPlayerState == PlayerState.Death)
            {
                Debug.Log("Resetting the ragdoll!");
                ragdoll.ResetRagdoll();
            } 
            else if (lastPlayerState == PlayerState.Block)
            {
                animator.SetBool("IsBlock", false);
                animator.SetLayerWeight(1, 0f);
            }

            if (nowState != PlayerState.Attack)
            {
                glintFab.gameObject.SetActive(false);
            }

            if (nowState == PlayerState.Attack)
            {
                animator.SetBool("IsSlashCharge", true); 
                animator.SetBool("Slash1", false);
                animator.SetBool("Slash2", false);

            }            
            else if (nowState == PlayerState.Block)
            {
                animator.SetBool("IsBlock", true);
                animator.SetLayerWeight(1, 1f);
            }
            else if (nowState == PlayerState.Move)
            {
                animator.CrossFade("Movement", 0.1f);
            }
            else if (nowState == PlayerState.Death)
            {
                ragdoll.StartRagdoll();
            }
        }

        if (nowState == PlayerState.Attack)
        {
            var attackNow = playerController.comboSystem.state;
            if (lastAttackState != attackNow)
            {
                if (attackNow == AttackState.Light1)
                {
                    animator.SetBool("IsSlashCharge", false); 
                    animator.SetBool("Slash1", true);
                }
                else if (attackNow == AttackState.HeavyCharged)
                {
                    glintFab.gameObject.SetActive(true);
                    // var go = Instantiate(glintFab, glintFab.transform.position, Quaternion.identity);
                    // go.gameObject.SetActive(true);
                }
                else if (attackNow == AttackState.Light2)
                {
                    animator.SetBool("Slash2", true);
                }
                else if (attackNow == AttackState.Heavy)
                {
                    animator.SetBool("IsSlashCharge", false);                     
                    animator.SetBool("Slash1", true);
                }
            }
        }



        SaveStates();
    }
}