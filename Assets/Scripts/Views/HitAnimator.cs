using Unity.Netcode;
using UnityEngine;

public class HitAnimator: NetworkBehaviour
{
    public GameObject hitPrefab;
    
    [ClientRpc]
    public void ShowHitClientRpc(HitType hitType, Vector3 hitPos)
    {
        Instantiate(hitPrefab, hitPos, Quaternion.identity);
    }
}