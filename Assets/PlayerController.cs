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

public class PlayerController : MonoBehaviour
{
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
    public Vector3 velocity = Vector3.zero;
    public PlayerState playerState = PlayerState.Move;
    
    void HandleGravity()
    {
        if (!controller.isGrounded && controller.enabled)
        {
            controller.Move(Vector3.up * gravity * Time.deltaTime);
        }
    }

    PlayerState ComputePlayerState()
    {
        return PlayerState.Move;
    }

    public void Tick(PlayerControllerInput frameInput)
    {
        this.playerState = ComputePlayerState();

        RotateToTarget();

        Vector3 move = frameInput.moveInput.x * transform.right + frameInput.moveInput.y * transform.forward;

        if (playerState == PlayerState.Move)
        {
            velocity = move;
        }
        else
        {
            // damp any residual velocity
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, attackInteria * Time.deltaTime);
        }
        
        controller.Move(move * (moveSpeed * Time.deltaTime));
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
