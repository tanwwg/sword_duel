using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public struct PlayerTickResult
{
    public WeaponHitInfo hitInfo;
    
    public static PlayerTickResult Empty =  new PlayerTickResult();
}

public class GameController : MonoBehaviour
{
    public KnightInfo[] knights;
    public PlayerTickResult[] tickResults;
    
    public Transform[] spawnPoints;

    private void Start()
    {
        RebuildPlayerList();
    }

    public void RebuildPlayerList()
    {
        knights = FindObjectsByType<KnightInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        tickResults = new PlayerTickResult[knights.Length];

        for (var i = 0; i < tickResults.Length; i++)
        {
            knights[i].gameObject.name = $"Knight {i}";
        }

        if (knights.Length == 2)
        {
            knights[0].controller.lockTarget = knights[1].controller;
            knights[1].controller.lockTarget = knights[0].controller;            
        }
    }

    public void Respawn()
    {
        for (var i = 0; i < tickResults.Length; i++)
        {
            knights[i].transform.position = spawnPoints[i].position;
            knights[i].transform.rotation = spawnPoints[i].rotation;
            knights[i].controller.Respawn();
        }
    }
    
    void Tick(KnightInfo pc, KnightInfo opp)
    {
        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState, opp?.controller);
    }

    public float respawnTime = -1;

    void CheckDeath()
    {
        if (respawnTime > 0) return;
        
        foreach (var knight in knights)
        {
            if (knight.controller.playerState.Value == PlayerState.Death)
            {
                respawnTime = Time.time + 3.0f;
                break;
            }
        }
    }
    
    void CheckRespawn()
    {
        if (respawnTime >= 0)
        {
            if (Time.time > respawnTime)
            {
                this.Respawn();
            }
        }
    }
    
    public void Tick()
    {
        for(var i = 0; i < tickResults.Length; i++) tickResults[i] = new PlayerTickResult();
        
        CheckRespawn();
        CheckDeath();
        
        if (knights.Length == 2)
        {
            tickResults[0].hitInfo = knights[1].controller.HandleWeaponHit();
            tickResults[1].hitInfo = knights[0].controller.HandleWeaponHit();
            Tick(knights[0], knights[1]);
            Tick(knights[1], knights[0]);            
        }
        else
        {
            foreach (var knight in knights)
            {
                Tick(knight, null);
            }
        }

        for(var i = 0; i < tickResults.Length; i++)
        {
            knights[i].animator.Tick(tickResults[i]);
        }
    }

    public void ClientTick()
    {
        for(var i = 0; i < tickResults.Length; i++)
        {
            knights[i].animator.Tick(PlayerTickResult.Empty);
        }
    }
}
