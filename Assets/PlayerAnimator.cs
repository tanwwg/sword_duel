using System;
using UnityEngine;

public class PlayerAnimator: MonoBehaviour
{

    [Header("Animator Params")]
    public string forwardParam = "Forward";
    public string rightParam   = "Strafe";

    [Header("Smoothing")]
    public float dampTime = 0.1f;
    
    public PlayerController playerController;
    public Transform targetTransform;
    
    
    public Animator animator;
    
    private Vector3 lastPosition;

    private void Awake()
    {
        lastPosition = targetTransform.position;
    }

    public void Update()
    {
        var dt = Time.deltaTime;
        var worldDelta = targetTransform.position - lastPosition;
        
        // Convert to local space (right / forward)
        var localDelta = transform.InverseTransformDirection(worldDelta);
        
        float forwardSpeed = localDelta.z / dt;
        float rightSpeed   = localDelta.x / dt;

        // Normalize to -1..1 (percent of max speed)
        var maxSpeed = playerController.moveSpeed;
        float forward01 = Mathf.Clamp(forwardSpeed / maxSpeed, -1f, 1f);
        float right01   = Mathf.Clamp(rightSpeed   / maxSpeed,  -1f, 1f);
        
        // Feed animator (let Animator handle smoothing)
        animator.SetFloat(forwardParam, forward01, dampTime, dt);
        animator.SetFloat(rightParam,   right01,   dampTime, dt);
    }
}