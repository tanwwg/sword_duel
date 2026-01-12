using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;
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

    [Header("Debug")]
    public Image bar;

    public TextMeshProUGUI debugText;

    public GameObject showHitBox;

    public Color barColor = Color.white;
    public Color comboColor = Color.yellow;

    private void Start()
    {
        foreach (var ci in comboItems)
        {
            ci.stateHash = Animator.StringToHash(ci.stateName);
            animator.SetFloat(ci.animSpeedParam, ci.clip.length * (1.0f - ci.startTime) / (ci.animFrames / 60.0f));
        }
    }

    public bool IsPlaying => comboIndex >= 0;

    private void StartCombo(int idx)
    {
        // Debug.Log("Combo Start " + idx);
        comboIndex = idx;
        lastComboTime = Time.time;
        // comboItems[comboIndex].StartAnimation(animator);
        var ci = comboItems[comboIndex];
        //animator.Play(comboItems[comboIndex].stateHash, 0, ci.startTime);
        animator.CrossFade(comboItems[comboIndex].stateHash, ci.crossFadeTime, 0, ci.startTime);
        // animator.Update(0);
        
        weapon.ResetHit();
    }
    
    /// <summary>
    /// Handle click
    /// </summary>
    public void ComboClick()
    {
        isComboClick = true;
    }

    private void Update()
    {
        var isClickThisFrame = isComboClick;
        isComboClick = false;

        
        if (comboIndex < 0)
        {
            bar.fillAmount = 0;

            if (isClickThisFrame)
            {
                StartCombo(0);
            }
            return;
        }
        
        var ci = comboItems[comboIndex];
        var comboPct = Mathf.Min(1.0f, (Time.time - lastComboTime) * 60.0f / ci.animFrames);
        var canCombo = comboIndex < comboItems.Length - 1 && comboPct >= ci.comboStart && comboPct <= ci.comboEnd;
        var canHit = comboPct >= ci.attackStart && comboPct <= ci.attackEnd;
        if (comboPct >= ci.crossFadeTime)
        {
            animator.Play(ci.stateHash, 0, ci.startTime + Mathf.Lerp(0, 1.0f - ci.startTime, comboPct));
            // animator.Update(0);
        }
        
        showHitBox.SetActive(canHit);
        
        if (comboPct >= 1.0f)
        {
            comboIndex = -1;
            lastComboTime = -1;
            foreach (var c in comboItems)
            {
                c.Reset(animator);
            }

            bar.fillAmount = 0;
            return;
        }

        if (isClickThisFrame)
        {
            Debug.Log($"{ci.stateName} + comboClick {comboPct*100:F2}");
            if (canCombo)
            {
                StartCombo(comboIndex + 1);
            }
        }
        
        if (bar)
        {
            bar.fillAmount = comboPct;
            debugText.text = (comboPct * 100.0f).ToString("F0");
            bar.color = canCombo ? comboColor : barColor;
        }
    }

}
