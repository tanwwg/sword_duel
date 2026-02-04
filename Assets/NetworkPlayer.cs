using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour
{
    public UnityEvent onOwner;

    public GameObject hitPrefab;
    
    public PlayerController controller;

    public KnightInfo knightInfo;
    public PlayerInput localInput;
    public RemoteInputHandler remoteInput;

    [Header("Runtime Var")]
    public bool IsAi;
    
    public override void OnNetworkSpawn()
    {
        // Runs on server + clients when THIS object becomes spawned for that instance
        Debug.Log($"Spawned: {name} | IsServer={IsServer} IsOwner={IsOwner} OwnerClientId={OwnerClientId}");
        
        if (IsOwner)
        {
            onOwner.Invoke();
            if (IsAi)
            {
                knightInfo.SetupAi();
            }
            else
            {
                localInput.gameObject.SetActive(true);
            }
            knightInfo.inputHandler = remoteInput;
        } 
        else if (IsServer)
        {
            knightInfo.inputHandler = remoteInput;
        }
        
        var gc = FindFirstObjectByType<GameController>();
        if (gc) gc.RebuildPlayerList();
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
