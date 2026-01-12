using UnityEngine;

public enum MoveEnum
{
    DoNothing, StrafeLeft, StrafeRight
}

public class EnemyAi : MonoBehaviour
{
    public Transform lockTarget;
    public float rotationSpeed = 10f;
    
    public Animator animator;

    public float moveBlendTreeDamp = 0.05f;

    public float evalEvery = 0.5f;

    public float lastEval = -100.0f;

    public MoveEnum moveState = MoveEnum.DoNothing;
    
    public CharacterController controller;
    public float moveSpeed = 5f;
    
    public Vector2 moveInput = Vector2.zero;
    
    public Vector3 velocity = Vector3.zero;
    public float gravity = -9.81f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyGravity();

        if (Time.time - lastEval > evalEvery)
        {
            lastEval = Time.time;
            var choices = new[] { MoveEnum.DoNothing, MoveEnum.DoNothing, MoveEnum.DoNothing, MoveEnum.DoNothing, MoveEnum.DoNothing, MoveEnum.StrafeLeft, MoveEnum.StrafeRight };
            var choice = choices[Random.Range(0, choices.Length)];
            moveState = choice;
        }

        switch (moveState)
        {
            case MoveEnum.StrafeLeft: moveInput = new Vector2(-0.5f, 0.25f); break;
            case MoveEnum.StrafeRight: moveInput = new Vector2(0.5f, 0.25f); break;
            case MoveEnum.DoNothing: moveInput = Vector2.zero; break;
        }
        
        Vector3 move = moveInput.x * transform.right + moveInput.y * transform.forward;
        
        animator.SetFloat("Forward", moveInput.y, moveBlendTreeDamp, Time.deltaTime);
        animator.SetFloat("Strafe", moveInput.x, moveBlendTreeDamp, Time.deltaTime);        

        // bool isMoving = move.sqrMagnitude > 0.01f;

        Vector3 dir = lockTarget.position - transform.position;
        dir.y = 0;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime
        );
        
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
    
    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
