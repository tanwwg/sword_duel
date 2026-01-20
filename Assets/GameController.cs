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

    public UnityEvent onPlayerDie;

    private WeaponHitInfo HandleHits(KnightInfo pc, KnightInfo opp)
    {
        var hitInfo = pc.weapon.GetHitInfo();
        if (hitInfo == null) return null;
        
        // Instantiate(hitInfo.weapon.hitPrefab, hitInfo.hitPoint, Quaternion.identity);
            
        var forceDir = pc.controller.transform.forward;
        forceDir.y = 0;
        forceDir = Quaternion.AngleAxis(hitInfo.weapon.hitAngle, Vector3.up) * forceDir * hitInfo.weapon.hitForce;
            
        hitInfo.hittable.playerController.HitStun(forceDir, hitInfo.weapon);
        return hitInfo;
    }

    void Tick(KnightInfo pc, PlayerTickResult result)
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
        pc.animator.Tick(result);
    }
    
    // Update is called once per frame
    void Update()
    {
        var enemyResult = new PlayerTickResult();
        var humanResult = new PlayerTickResult();
        
        enemyResult.hitInfo = HandleHits(human, enemy);
        humanResult.hitInfo = HandleHits(enemy, human);
        
        Tick(human, humanResult);
        Tick(enemy, enemyResult);
    }
}
