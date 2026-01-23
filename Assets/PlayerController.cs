using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct PlayerControllerInput
{
    public Vector2 moveInput;
    public bool isAttack;
    public bool isBlock;

    public static PlayerControllerInput zero = new PlayerControllerInput()
    {
        moveInput = Vector2.zero,
        isAttack = false,
        isBlock = false
    };

}

public enum PlayerState
{
    Move, Attack1, Attack2, Attack3, Stun, Death
}

public static class PlayerStateExtensions
{
    public static bool IsAttack(this PlayerState state)
    {
        return state is PlayerState.Attack1 or PlayerState.Attack2 or PlayerState.Attack3;
    }
}

public class PlayerController : NetworkBehaviour
{
    public int maxHealth = 100;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    [Tooltip("How much to damp attack velocity")]
    public float attackInteria = 1.0f;
    
    [Header("References")]
    public CharacterController controller;

    public ComboSystem comboSystem;
    
    public Transform lookTarget;

    [Header("Runtime vars")]
    
    public PlayerController lockTarget;

    // public int health;
    public float stunTime;
    public Vector3 velocity = Vector3.zero;
    public PlayerState playerState = PlayerState.Move;
    
    public NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        this.health.OnValueChanged += (value, newValue) =>
        {
            Debug.Log($"Health OnValueChanged: {value} -> {newValue}");
        };
    }

    public void Respawn()
    {
        this.health.Value = this.maxHealth;
    }

    void HandleGravity()
    {
        if (!controller.isGrounded && controller.enabled)
        {
            controller.Move(Vector3.up * gravity * Time.deltaTime);
        }
    }

    PlayerState ComputePlayerState()
    {
        if (health.Value <= 0) return PlayerState.Death;
        if (stunTime > 0) return PlayerState.Stun;
        
        if (comboSystem.comboIndex == 0) return PlayerState.Attack1;
        if (comboSystem.comboIndex == 1) return PlayerState.Attack2;
        if (comboSystem.comboIndex == 2) return PlayerState.Attack3;
        
        return PlayerState.Move;
    }
    
    public WeaponHitInfo HandleWeaponHit()
    {
        var hitInfo = comboSystem.weapon.GetHitInfo();
        if (hitInfo == null) return null;
        
        var forceDir = controller.transform.forward;
        forceDir.y = 0;
        forceDir = Quaternion.AngleAxis(hitInfo.weapon.hitAngle, Vector3.up) * forceDir * hitInfo.weapon.hitForce;
            
        hitInfo.hittable.playerController.HitStun(forceDir, hitInfo.weapon);
        return hitInfo;
    }

    
    public void HitStun(Vector3 forceDir, WeaponData weapon)
    {
        stunTime += weapon.stunTime;
        health.Value = Math.Max(0, health.Value - weapon.damage);
        comboSystem.StopCombo();

        velocity = forceDir;
    }

    public void Tick(PlayerControllerInput frameInput, PlayerAnimState animState, PlayerController opp)
    {
        HandleGravity();
        
        this.playerState = ComputePlayerState();
        if (this.playerState == PlayerState.Death) return;
     
        if (opp) this.lockTarget = opp;

        HandleMove(frameInput);
        stunTime = Math.Max(0, stunTime - Time.deltaTime);

        var isAttack = frameInput.isAttack && playerState is PlayerState.Move or PlayerState.Attack1 or PlayerState.Attack2;
        comboSystem.Tick(isAttack, animState);
        
        this.playerState = ComputePlayerState();
    }

    void HandleMove(PlayerControllerInput frameInput)
    {
        if (playerState == PlayerState.Move)
        {
            velocity = frameInput.moveInput.x * transform.right + frameInput.moveInput.y * transform.forward;
            velocity *= moveSpeed;
            RotateToTarget();
        }
        else
        {
            // damp any residual velocity
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, attackInteria * Time.deltaTime);
        }
        controller.Move(velocity * Time.deltaTime);
    }

    void RotateToTarget()
    {
        if (!lockTarget) return;

        Vector3 dir = lockTarget.transform.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );
        
    }

}
