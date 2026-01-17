using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 100;
    
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    [Tooltip("How much to damp attack velocity")]
    public float attackInteria = 1.0f;
    
    [Header("References")]
    public StunHandler stunHandler;

    public CharacterController controller;

    public ComboSystem comboSystem;

    public Rigidbody rb;

    public Transform lockTarget;

    [Header("Runtime vars")] 
    public int health;
    public float stunTime;
    public Vector3 velocity = Vector3.zero;
    public PlayerState playerState = PlayerState.Move;

    private void Awake()
    {
        this.health = this.maxHealth;
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
        if (health <= 0) return PlayerState.Death;
        if (stunTime > 0) return PlayerState.Stun;
        
        if (comboSystem.comboIndex == 0) return PlayerState.Attack1;
        if (comboSystem.comboIndex == 1) return PlayerState.Attack2;
        if (comboSystem.comboIndex == 2) return PlayerState.Attack3;
        
        return PlayerState.Move;
    }
    
    public void HitStun(Vector3 forceDir, WeaponData weapon)
    {
        stunTime += weapon.stunTime;
        health = Math.Max(0, health - weapon.damage);

        velocity = forceDir;
    }

    public void Tick(PlayerControllerInput frameInput, PlayerAnimState animState)
    {
        if (animState.isExitAttack) comboSystem.StopCombo();
        HandleGravity();
        
        this.playerState = ComputePlayerState();
        
        HandleMove(frameInput);
        stunTime = Math.Max(0, stunTime - Time.deltaTime);

        if (playerState == PlayerState.Move || playerState.IsAttack())
        {
            comboSystem.Tick(frameInput.isAttack, animState);
        }
        
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

        Vector3 dir = lockTarget.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );
        
    }

}
