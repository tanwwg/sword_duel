using Unity.Netcode;
using UnityEngine;

public class SinglePlayerGameScript : MonoBehaviour
{
    public NetworkObject player;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var nm = NetworkManager.Singleton;
        nm.StartHost();
        player.Spawn();
    }

}
