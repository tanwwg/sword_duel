using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KnightInfo
{
    public PlayerController controller;
    public PlayerAnimator animator;
    public Weapon weapon;
    public UnityEvent onDie;
}

public class GameController : MonoBehaviour
{
    public InputHandler inputHandler;
    public EnemyAi enemyAi;
    
    public KnightInfo human;
    public KnightInfo enemy;

    public UnityEvent onPlayerDie;

    private void HandleHits(KnightInfo pc)
    {
        var hitInfo = pc.weapon.GetHitInfo();
        if (hitInfo != null)
        {
            Debug.Log("HIT " + hitInfo.hittable.playerController.name);
            Instantiate(hitInfo.weapon.hitPrefab, hitInfo.hitPoint, Quaternion.identity);
            
            var forceDir = pc.controller.transform.forward;
            forceDir.y = 0;
            forceDir = Quaternion.AngleAxis(hitInfo.weapon.hitAngle, Vector3.up) * forceDir * hitInfo.weapon.hitForce;
            
            hitInfo.hittable.playerController.HitStun(forceDir, hitInfo.weapon);
           
        }
    }

    void Tick(KnightInfo pc, PlayerControllerInput inputs)
    {
        if (pc.controller.playerState == PlayerState.Death)
        {
            pc.onDie.Invoke();
            this.enabled = false;
            return;
        }
        
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState);
        pc.animator.Tick();
    }
    
    // Update is called once per frame
    void Update()
    {
        
        HandleHits(human);
        HandleHits(enemy);
        
        Tick(human, inputHandler.ReadInputs());
        Tick(enemy, enemyAi.Tick());
    }
}
