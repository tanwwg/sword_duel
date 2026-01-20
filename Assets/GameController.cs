using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KnightInfo
{
    public BaseInputHandler inputHandler;
    public PlayerController controller;
    public PlayerAnimator animator;
    public Weapon weapon;
    public UnityEvent onDie;
    public UnityEvent onHit;
}

public struct PlayerTickResult
{
    public WeaponHitInfo hitInfo;
}

public class GameController : MonoBehaviour
{
    public KnightInfo human;
    public KnightInfo enemy;

    void Tick(KnightInfo pc)
    {
        if (pc.controller.playerState == PlayerState.Death)
        {
            pc.onDie.Invoke();
            this.enabled = false;
            return;
        }

        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState);
    }
    
    // Update is called once per frame
    void Update()
    {
        var enemyResult = new PlayerTickResult();
        var humanResult = new PlayerTickResult();
        
        enemyResult.hitInfo = human.controller.HandleWeaponHit();
        humanResult.hitInfo = enemy.controller.HandleWeaponHit();
        
        Tick(human);
        Tick(enemy);
        
        human.animator.Tick(humanResult);
        enemy.animator.Tick(enemyResult);        
    }
}
