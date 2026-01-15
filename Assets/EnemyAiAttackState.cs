public class EnemyAiAttackState: EnemyAiState
{
    public PlayerController target;

    public override void StartState()
    {
        target.isAttack = true;
    }

    public override void RunUpdate()
    {

    }
}