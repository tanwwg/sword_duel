public class EnemyAiAttackState: EnemyAiState
{
    public PlayerController target;
    
    public override void RunUpdate()
    {
        target.isAttack = true;
    }
}