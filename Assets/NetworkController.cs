using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkController : NetworkBehaviour
{
    public GameController gameController;
    public GameObject networkSetupCanvas;

    private void Start()
    {
        var nm =  NetworkManager.Singleton;
        nm.OnClientConnectedCallback += OnClientConnectedToServer;
        
        networkSetupCanvas.SetActive(true);
    }

    void OnClientConnectedToServer(ulong clientId)
    {
        var nm =  NetworkManager.Singleton;
        Debug.Log($"Client connected to server clientId={clientId} localId={nm.LocalClientId}");
        
        TryStartNetworkGame();
    }

    public void OnPlayerSpawned(NetworkPlayer player)
    {
        var nm =  NetworkManager.Singleton;

        gameController.RebuildPlayerList();
        foreach (var k in gameController.knights)
        {
            k.OnRespawn = () =>
            {
                k.GetComponent<NetworkTransform>().Teleport(k.transform.position, k.transform.rotation, k.transform.localScale);
            };
        }
        
        if (nm.IsServer)
        {
            gameController.Respawn();
        }
    }

    void TryStartNetworkGame()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;
        Debug.Log("Starting network game");
        networkSetupCanvas.SetActive(false);
    }

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
