using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkController : NetworkBehaviour
{
    public GameController gameController;
    public NetworkPlayer playerFab;

    private void PropagateHit()
    {
        for (var i = 0; i < gameController.tickResults.Length; i++)
        {
            var hit = gameController.tickResults[i].hitInfo;
            
            if (!(hit?.isHit ?? false)) continue;
            
            var np = gameController.knights[i].GetComponent<NetworkPlayer>();
            np.SpawnHitClientRpc(hit.hitPoint);
        }
    }


    public void StartAIGame()
    {
        NetworkManager.Singleton.StartServer();
        var ai = Instantiate(playerFab);
        ai.IsAi = true;
        ai.GetComponent<NetworkObject>().Spawn();
        var player = Instantiate(playerFab);
        player.GetComponent<NetworkObject>().Spawn();
    }

    public void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            gameController.Tick();
            PropagateHit();
        }
        else
        {
            gameController.ClientTick();
        }
        
    }
}
