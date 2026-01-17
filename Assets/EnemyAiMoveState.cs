using UnityEngine;

public class EnemyAiMoveState: EnemyAiState
{
    [Header("Movement")]
    public Vector2 moveInput;
    
    public override PlayerControllerInput Tick()
    {
        return new PlayerControllerInput()
        {
            moveInput = moveInput
        };
    }
}