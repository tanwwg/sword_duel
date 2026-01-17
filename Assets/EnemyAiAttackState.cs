public class EnemyAiAttackState: EnemyAiState
{
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
                isAttack = true
            };
        }
        
        return PlayerControllerInput.zero;
    }
}