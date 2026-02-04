using System;
using UnityEngine;
using UnityEngine.Events;

public class KnightInfo: MonoBehaviour
{
    public BaseInputHandler inputHandler;
    
    public PlayerController controller;
    public PlayerAnimator animator;

    public EnemyAi aiHandler;

    public void SetEnemy(KnightInfo enemy)
    {
        aiHandler.playerTransform =  enemy.controller.transform;
        controller.lockTarget = enemy.controller;
    }

    public void SetupAi()
    {
        inputHandler = aiHandler;
        aiHandler.gameObject.SetActive(true);
    }
}