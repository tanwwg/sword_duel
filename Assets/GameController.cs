using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public struct PlayerTickResult
{
    public WeaponHitInfo hitInfo;
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
            knights[i].transform.position = spawnPoints[i].position;
            knights[i].transform.rotation = spawnPoints[i].rotation;
            
            var netobj = knights[i].GetComponent<NetworkObject>();
            var isOwner = netobj?.IsOwner ?? false;
            knights[i].gameObject.name = $"Knight {i} {(isOwner ? "owner" : "")}";
        }
    }
    
    void Tick(KnightInfo pc, KnightInfo opp)
    {
        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState, opp?.controller);
    }
    
    // Update is called once per frame
    void Update()
    { 
        for(var i = 0; i < tickResults.Length; i++) tickResults[i] = new PlayerTickResult();
        
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
}
