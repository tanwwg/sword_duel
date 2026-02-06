using Unity.Netcode;
using UnityEngine;

public class HitAnimator: NetworkBehaviour
{
    public GameObject hitPrefab;
    public GameObject blockPrefab;    
    
    [ClientRpc]
    public void ShowHitClientRpc(HitType hitType, Vector3 hitPos)
    {
        Debug.Log(hitType);
        var fab = hitType == HitType.Block ? blockPrefab : hitPrefab;
        Instantiate(fab, hitPos, Quaternion.identity);
    }
}