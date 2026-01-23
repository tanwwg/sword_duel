using Unity.Netcode;
using UnityEngine;

public class NetworkController : MonoBehaviour
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
        // if (nm.IsServer)
        // {
        //     gameController.RebuildPlayerList();
        // }
        TryStartNetworkGame();
    }

    void TryStartNetworkGame()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;
        Debug.Log("Starting network game");
        networkSetupCanvas.SetActive(false);
    }
    
    
}
