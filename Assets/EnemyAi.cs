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
    
    public Rigidbody rb;
    public CharacterController controller;
    public float moveSpeed = 5f;
    
    public Vector2 moveInput = Vector2.zero;
    
    public Vector3 velocity = Vector3.zero;
    public float gravity = -9.81f;

    public float stunTime = 0.0f;
    public AnimationClip hitClip;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void HitStun(Vector3 forceDir, float time)
    {
        stunTime += time;
        controller.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(forceDir, ForceMode.Impulse);
        animator.SetFloat("OnHitSpeed", hitClip.length / stunTime);
        animator.Play("onhit", 0, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (stunTime > 0)
        {
            stunTime -= Time.deltaTime;
            if (stunTime <= 0)
            {
                rb.isKinematic = true;
                controller.enabled = true;
                animator.CrossFade("Movement", 0.2f, 0);
            }
            return;
        } 
        
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
