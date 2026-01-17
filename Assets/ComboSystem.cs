using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class AttackComboItem
{
    public string stateName;
    public int stateHash;
    
    public AnimationClip clip;
    
    [Range(0f, 1f)]
    public float startTime = 0;
    
    public float animFrames;
    public string animSpeedParam;
    
    public CapsuleCollider hitCollider;

    [Range(0f, 1f)]    
    public float crossFadeTime;
    
    [Tooltip("Combo start %")]
    [Range(0f, 1f)]
    public float comboStart;
    
    [Tooltip("Combo end %")]
    [Range(0f, 1f)]
    public float comboEnd;

    [Range(0f, 1f)]
    public float attackStart;
    
    [Range(0f, 1f)]
    public float attackEnd;
    
    public string animBool;
    public string animTrigger;

    public WeaponData weaponData;

    public UnityEvent onComboStart;

    public void StartAnimation(Animator animator)
    {
        if (!string.IsNullOrWhiteSpace(animBool))
        {
            animator.SetBool(this.animBool, true);
        }

        if (!string.IsNullOrWhiteSpace(animTrigger))
        {
            animator.SetTrigger(this.animTrigger);
        }
    }

    public void Reset(Animator animator)
    {
        if (!string.IsNullOrWhiteSpace(animBool))
        {
            animator.SetBool(this.animBool, false);
        }

        if (!string.IsNullOrWhiteSpace(animTrigger))
        {
            animator.ResetTrigger(this.animTrigger);
        }
    }
}

public class ComboSystem : MonoBehaviour
{
    public Animator animator;
    public AttackComboItem[] comboItems;

    private bool isComboClick = false;

    public float lastComboTime = -1;
    public int comboIndex = -1;

    public Weapon weapon;
    // public float comboFrame;
    
    [Header("Runtime Vars")] 
    
    public bool canCombo;

    public bool isEndAttack = false;

    public bool isComboFailed = false;

    public bool IsPlaying => comboIndex >= 0;

    private void StartCombo(int idx)
    {
        comboIndex = idx;
    }

    public void StartCanCombo()
    {
        canCombo = true;
    }

    public void StopCanCombo()
    {
        canCombo = false;
    }
    
    /// <summary>
    /// Handle click
    /// </summary>
    public void ComboClick()
    {
        isComboClick = true;
    }

    public void Tick(bool isClick, PlayerAnimState animState)
    {
        if (!IsPlaying)
        {
            if (isClick)
            {
                StartCombo(0);
            }
        }
        else
        {
            if (animState.canCombo && isClick)
            {
                StartCombo(comboIndex + 1);
            }
            if (animState.isExitAttack)
            {
                comboIndex = -1;
            }
        }
    }

}
