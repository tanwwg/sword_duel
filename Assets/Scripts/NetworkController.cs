using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkController : NetworkBehaviour
{
    public GameController gameController;
    public NetworkPlayer playerFab;
    
    public void StartAIGame()
    {
        NetworkManager.Singleton.OnServerStarted += () => 
        {
            var ai = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
            var np = ai.GetComponent<NetworkPlayer>();
            np.IsAi = true;
            ai.GetComponent<NetworkObject>().Spawn();
        };
        NetworkManager.Singleton.StartHost();
        // ai.GetComponent<NetworkObject>().Spawn();
        // var player = Instantiate(playerFab);
        // player.GetComponent<NetworkObject>().Spawn();
    }

    
}
