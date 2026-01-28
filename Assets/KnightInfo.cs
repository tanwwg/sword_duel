using System;
using UnityEngine;
using UnityEngine.Events;

public class KnightInfo: MonoBehaviour
{
    public BaseInputHandler inputHandlerBehaviour;
    public BaseInputHandler inputHandler => inputHandlerBehaviour;
    
    public PlayerController controller;
    public PlayerAnimator animator;
    // public UnityEvent onDie;

    public Action OnRespawn;

}