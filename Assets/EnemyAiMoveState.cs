using UnityEngine;

public class EnemyAiMoveState: EnemyAiState
{
    public Vector2 moveInput;
    public PlayerController target;
    
    public override void RunUpdate()
    {
        target.moveInput = moveInput;
    }
}