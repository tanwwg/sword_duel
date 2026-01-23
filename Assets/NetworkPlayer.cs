using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetworkPlayer : NetworkBehaviour
{
    public UnityEvent onOwner;

    public GameObject hitPrefab;
    
    public PlayerController controller;
    
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

    public void Respawn()
    {
        this.controller.Respawn();
    }

    [ClientRpc]
    public void SpawnHitClientRpc(Vector3 pos)
    {
        if (IsServer && IsHost) return;  // don't play hit prefab if on server
        
        Instantiate(hitPrefab, pos, Quaternion.identity);
    }

}
