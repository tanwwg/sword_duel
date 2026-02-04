using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class KnightInfo: MonoBehaviour
{
    public BaseInputHandler inputHandler;
    
    public PlayerController controller;
    public PlayerAnimator animator;

    public EnemyAi aiHandler;

    public CinemachineCamera[] cams;

    public void SetEnemy(KnightInfo enemy)
    {
        aiHandler.playerTransform =  enemy.controller.transform;
        controller.lockTarget = enemy.controller;
        foreach(var c in cams) c.LookAt = enemy.controller.transform;
    }

    public void SetupAi()
    {
        Debug.Log("Setup Ai");
        inputHandler = aiHandler;
        aiHandler.gameObject.SetActive(true);
    }
}