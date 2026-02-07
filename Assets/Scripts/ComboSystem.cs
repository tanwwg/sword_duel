using UnityEngine;

public enum AttackState
{
    NotAttacking, Charge, HeavyCharged, Heavy, Light1, Light2
}

public class ComboSystem : MonoBehaviour
{
    public Weapon weapon;

    public float heavyChargeTime = 0.5f;
    
    [Header("Runtime Vars")] 
    public AttackState state;
    public float chargeTime;
    
    public void StopAttack()
    {
        state = AttackState.NotAttacking;
        chargeTime = 0;
    }

    public void Tick(bool isClick, PlayerAnimState animState)
    {
        weapon.gameObject.SetActive(animState.isAttacking);
        if (animState.isExitAttack)
        {
            StopAttack();
            return;
        }

        switch (state)
        {
            case AttackState.NotAttacking:
                if (isClick) state = AttackState.Charge;
                break;
            
            case AttackState.Charge:
                chargeTime += Time.deltaTime;
                if (chargeTime >= heavyChargeTime)
                {
                    state = AttackState.HeavyCharged;
                }
                else if (!isClick)
                {
                    state = AttackState.Light1;
                }
                break;
            
            case AttackState.HeavyCharged:
                if (!isClick)
                {
                    state = AttackState.Heavy;
                }

                break;
            
            case AttackState.Light1:
                if (animState.canCombo && isClick) state = AttackState.Light2;
                break;
            
            case AttackState.Light2:
                break;
            
            case AttackState.Heavy:
                break;
            
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

}
