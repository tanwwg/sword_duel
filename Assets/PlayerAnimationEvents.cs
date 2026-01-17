using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public bool canCombo = false;
    public bool isAttacking = false;
    
    public void StartCanCombo(AnimationEvent evt)
    {
        canCombo = true;
    }
    
    public void EndCanCombo(AnimationEvent evt)
    {
        canCombo = false;
    }
    
    public void StartAttack(AnimationEvent evt)
    {
        isAttacking = true;
    }
    
    public void EndAttack(AnimationEvent evt)
    {
        isAttacking = false;
    }
}
