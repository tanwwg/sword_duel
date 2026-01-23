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
        }
    }
    
    void Tick(KnightInfo pc)
    {
        var inputs = pc.inputHandler.ReadInputs();
        var animState = pc.animator.GetAnimState();
        pc.controller.Tick(inputs, animState);
    }
    
    // Update is called once per frame
    void Update()
    { 
        for(var i = 0; i < tickResults.Length; i++) tickResults[i] = new PlayerTickResult();
        
        if (knights.Length >= 2)
        {
            
        }

        foreach (var knight in knights)
        {
            Tick(knight);
        }
        for(var i = 0; i < tickResults.Length; i++)
        {
            knights[i].animator.Tick(tickResults[i]);
        }
        
        
        // enemyResult.hitInfo = human.controller.HandleWeaponHit();
        // humanResult.hitInfo = enemy.controller.HandleWeaponHit();
        //
        // Tick(human);
        // Tick(enemy);
        //
        // human.animator.Tick(humanResult);
        // enemy.animator.Tick(enemyResult);        
    }
}
