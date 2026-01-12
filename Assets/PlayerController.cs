using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    
    public Transform lockTarget;

    [Header("References")]
    public Animator animator;

    public CharacterController controller;
    public Vector2 moveInput;
    public Vector3 velocity;
    public float moveBlendTreeDamp = 0.12f;

    public ComboSystem comboSystem;

    void Update()
    {
        Move();
        // ApplyGravity();
    }

    void Move()
    {
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

        if (comboSystem.IsPlaying) return;
        
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // NEW INPUT SYSTEM CALLBACK
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            comboSystem.ComboClick();
        }
    }
}
