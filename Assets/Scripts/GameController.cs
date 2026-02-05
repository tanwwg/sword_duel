using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public struct PlayerTickResult
{
    public WeaponHitInfo hitInfo;
    
    public static PlayerTickResult Empty =  new PlayerTickResult();
}

public class GameController : MonoBehaviour
{
    public KnightInfo[] knights;
    public PlayerTickResult[] tickResults = Array.Empty<PlayerTickResult>();
    
    public Transform[] spawnPoints;

    public float respawnWaitTime = 3;
    
    public float respawnTime = -1;

    public UnityEvent onStartGame;
    
    public void RebuildPlayerList()
    {
        knights = FindObjectsByType<KnightInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log("RebuildPlayers " + knights.Length);
        
        var nm = NetworkManager.Singleton;
        Debug.Log($"IsServer: {nm.IsServer} IsHost: {nm.IsHost} IsClient: {nm.IsClient}");

        for (var i = 0; i < knights.Length; i++)
        {
            knights[i].gameObject.name = $"Knight {i}";
            if (nm.IsServer)
            {
                Debug.Log("RebuildPlayers " + knights[i].gameObject.name);
                knights[i].controller.Respawn();
            }
        }

        if (knights.Length == 2)
        {
            knights[0].SetEnemy(knights[1]);
            knights[1].SetEnemy(knights[0]);
            onStartGame.Invoke();
            if (nm.IsServer)
            {
                Respawn();
            }
        }
    }

    public void Respawn()
    {
        for (var i = 0; i < knights.Length; i++)
        {
            Debug.Log($"Respawning {i} {knights[i].gameObject.name} at {spawnPoints[i].position}");
            knights[i].controller.controller.enabled = false;
            knights[i].transform.position = spawnPoints[i].position;
            knights[i].transform.rotation = spawnPoints[i].rotation;
            knights[i].controller.Respawn();
            knights[i].controller.controller.enabled = true;
        }

        this.respawnTime = -1;
    }
    
    void Tick(KnightInfo pc, KnightInfo opp)
    {
        // Debug.Log("GC.ReadInputs " + pc.inputHandler.name);
        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState, opp?.controller);
    }


    void CheckDeath()
    {
        if (respawnTime > 0) return;
        
        foreach (var knight in knights)
        {
            if (knight.controller.playerState.Value == PlayerState.Death)
            {
                Debug.Log(knight.gameObject.name + " has died");
                respawnTime = Time.time + respawnWaitTime;
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
        if (tickResults == null ||  tickResults.Length != knights.Length)
        {
            tickResults = new PlayerTickResult[knights.Length];
        }
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
    
    public void Update()
    {
        var nm = NetworkManager.Singleton;
        if (nm.IsServer)
        {
            this.Tick();
        }
        else
        {
            this.ClientTick();
        }
        
    }

}
