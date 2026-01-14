using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    public float moveBlendTreeDamp = 0.12f;
    
    [Tooltip("How much to damp attack velocity")]
    public float attackInteria = 1.0f;

    public Transform lockTarget;

    [Header("References")]
    public Animator animator;

    public StunHandler stunHandler;

    public CharacterController controller;

    public ComboSystem comboSystem;

    public Rigidbody rb;
    
    [Header("Inputs")]
    public Vector2 moveInput;
    public bool isAttack;

    [Header("Runtime vars")]
    public Vector3 attackVelocity = Vector3.zero;

    void Update()
    {
        HandleGravity();
        Move();
    }

    void HandleGravity()
    {
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.up * gravity * Time.deltaTime);
        }
    }

    void Move()
    {
        if (stunHandler.UpdateStun())
        {
            return;
        }
        
        var isAttackThisFrame = isAttack;
        isAttack = false;

        // damp any attack velocity
        attackVelocity = Vector3.MoveTowards(attackVelocity, Vector3.zero, attackInteria * Time.deltaTime);

        if (isAttackThisFrame)
        {
            attackVelocity = moveInput.x * transform.right + moveInput.y * transform.forward;
            comboSystem.ComboClick();
            comboSystem.RunUpdate();
            return;
        }
        
        if (comboSystem.IsPlaying)
        {
            controller.Move(attackVelocity * Time.deltaTime);
            comboSystem.RunUpdate();
            return;
        }
        
        Vector3 move = moveInput.x * transform.right + moveInput.y * transform.forward;
        
        animator.SetFloat("Forward", moveInput.y, moveBlendTreeDamp, Time.deltaTime);
        animator.SetFloat("Strafe", moveInput.x, moveBlendTreeDamp, Time.deltaTime);        

        bool isMoving = move.sqrMagnitude > 0.01f;

        Vector3 dir = lockTarget.position - transform.position;
        dir.y = 0;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    public void FixedUpdate()
    {
        animator.SetFloat("HitRotation", rb.angularVelocity.magnitude);
    }
}
