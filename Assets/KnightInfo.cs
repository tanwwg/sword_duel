using System;
using UnityEngine;
using UnityEngine.Events;

public class KnightInfo: MonoBehaviour
{
    public MonoBehaviour inputHandlerBehaviour;
    public BaseInputHandler inputHandler => inputHandlerBehaviour as BaseInputHandler;
    
    public PlayerController controller;
    public PlayerAnimator animator;
    // public UnityEvent onDie;

    public Action OnRespawn;

}