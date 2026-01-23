using UnityEngine;

public class KnightInfo: MonoBehaviour
{
    public MonoBehaviour inputHandlerBehaviour;
    public BaseInputHandler inputHandler => inputHandlerBehaviour as BaseInputHandler;
    
    public PlayerController controller;
    public PlayerAnimator animator;
    // public UnityEvent onDie;
        
}