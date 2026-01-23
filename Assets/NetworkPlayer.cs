using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkPlayer : NetworkBehaviour
{
    public UnityEvent onServer;
    public UnityEvent onClient;    
    public UnityEvent onOwner;
    
    public KnightInfo knightInfo;
    public BaseInputHandler remoteInput;
    public BaseInputHandler localInput;
    
    public override void OnNetworkSpawn()
    {
        // Runs on server + clients when THIS object becomes spawned for that instance
        Debug.Log($"Spawned: {name} | IsServer={IsServer} IsOwner={IsOwner} OwnerClientId={OwnerClientId}");
        
        if (IsOwner)
        {
            onOwner.Invoke();
        }
        
        FindFirstObjectByType<NetworkController>().OnPlayerSpawned(this);
    }

}
