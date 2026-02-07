using UnityEngine;

public class EnemyAiBlockState: EnemyAiState
{
    [Header("Movement")]
    public Vector2 moveInput;

    public override void StartState()
    {
    }

    public override PlayerControllerInput Tick()
    {
        return new PlayerControllerInput()
        {
            moveInput = moveInput,
            isBlock = true
        };
    }
}