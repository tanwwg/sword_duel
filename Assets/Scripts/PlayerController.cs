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
    Move, Attack, Stun, Death, Block
}

public class PlayerController : NetworkBehaviour
{
    public int maxHealth = 100;
    
    [Header("Movement")]
    public float moveSpeed = 5f;

    public float backSpeed = 2.5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    [Tooltip("How much to damp attack velocity")]
    public float attackInteria = 1.0f;
    
    [Header("References")]
    public CharacterController controller;

    public ComboSystem comboSystem;
    public BlockSystem blockSystem;
    
    [Header("Runtime vars")]
    
    public PlayerController lockTarget;

    // public int health;
    public Vector3 velocity = Vector3.zero;

    public NetworkVariable<PlayerState> playerState;
    public NetworkVariable<float> stunTimeNetwork;

    public Weapon weapon => comboSystem.weapon;

    public float stunTime
    {
        get => stunTimeNetwork.Value;
        private set => stunTimeNetwork.Value = value;
    }

    // public PlayerState playerState = PlayerState.Move;
    
    public NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void Respawn()
    {
        this.health.Value = this.maxHealth;
        this.playerState.Value = PlayerState.Move;
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

        if (blockSystem.isBlockSystemActive) return PlayerState.Block;

        if (comboSystem.state != AttackState.NotAttacking) return PlayerState.Attack;
        
        return PlayerState.Move;
    }
    
    public void HitStun(Vector3 forceDir, int damage, float stun)
    {
        stunTime += stun;
        health.Value = Math.Max(0, health.Value - damage);
        comboSystem.StopAttack();

        velocity = forceDir;
    }

    public void Tick(PlayerControllerInput frameInput, PlayerAnimState animState)
    {
        HandleGravity();
        
        this.playerState.Value = ComputePlayerState();
        if (this.playerState.Value == PlayerState.Death) return;
     
        HandleMove(frameInput);
        stunTime = Math.Max(0, stunTime - Time.deltaTime);
        
        // we can only block or attack, not both
        if (blockSystem.isBlockSystemActive)
        {
            if (!frameInput.isBlock)
            {
                blockSystem.StopBlock();
            }
        }
        else
        {
            if (frameInput.isBlock) 
            {
                blockSystem.StartBlock();
            }
            else
            {
                // var isAttack = frameInput.isAttack && playerState.Value is PlayerState.Move or PlayerState.Attack1 or PlayerState.Attack2;
                if (playerState.Value == PlayerState.Move || playerState.Value == PlayerState.Attack)
                {
                    comboSystem.Tick(frameInput.isAttack, animState);
                }
            }
        }
        
        this.playerState.Value = ComputePlayerState();
    }

    void HandleMove(PlayerControllerInput frameInput)
    {
        if (playerState.Value == PlayerState.Move)
        {
            float dy = frameInput.moveInput.y;
            float speed = dy > 0 ? moveSpeed : backSpeed;
            velocity = frameInput.moveInput.x * transform.right + dy * speed * transform.forward;
            velocity *= moveSpeed;
        }
        else
        {
            // damp any residual velocity
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, attackInteria * Time.deltaTime);
        }

        if (playerState.Value == PlayerState.Block || playerState.Value == PlayerState.Move)
        {
            RotateToTarget();
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
