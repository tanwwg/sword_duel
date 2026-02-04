using UnityEngine;

public class EnemyAiAttackState: EnemyAiState
{
    [Header("Movement")]
    public Vector2 moveInput;
    public bool isTriggered;

    public override void StartState()
    {
        isTriggered = false;
    }

    public override PlayerControllerInput Tick()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            return new PlayerControllerInput()
            {
                moveInput = this.moveInput,
                isAttack = true
            };
        }
        
        return PlayerControllerInput.zero;
    }
}