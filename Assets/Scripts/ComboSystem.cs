using UnityEngine;

public enum AttackState
{
    NotAttacking, Charge, Heavy, Light1, Light2
}

public class ComboSystem : MonoBehaviour
{
    public Weapon weapon;
    
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
                if (!isClick) state = AttackState.Light1;
                break;
            
            case AttackState.Light1:
                if (animState.canCombo && isClick) state = AttackState.Light2;
                break;
            
            case AttackState.Light2:
                break;
            
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

}
