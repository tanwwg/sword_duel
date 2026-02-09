using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    public HitController hitController;
    
    public KnightInfo[] knights;
    
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
    
    void Tick(KnightInfo pc)
    {
        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        if (animState.isAttacking)
        {
            hitController.HandleWeaponHit(pc.controller);
        }
        else
        {
            pc.controller.comboSystem.weapon.ResetHit();
        }
        pc.controller.Tick(inputs, animState);
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
    
    private void Tick()
    {
        if (IsServer)
        {
            CheckRespawn();
            CheckDeath();
            ServerTick();
        }
        ClientTick();
    }

    private void ServerTick()
    {
        foreach (var knight in knights) Tick(knight);
    }

    public void ClientTick()
    {
        foreach (var knight in knights)
        {
            knight.animator.Tick();
        }
    }
    
    public void Update()
    {
        this.Tick();
    }

}
