using System;
using Unity.Netcode;
using UnityEngine;

public class HitAnimator: NetworkBehaviour
{
    public GameObject hitPrefab;
    public GameObject blockPrefab;
    public GameObject parryPrefab;

    GameObject GetFab(HitType hitType)
    {
        switch (hitType)
        {
            case HitType.Hit: return hitPrefab;
            case HitType.Block: return blockPrefab;
            case HitType.Parry: return parryPrefab;
            default: throw new InvalidOperationException();
        }
    }
    
    [ClientRpc]
    public void ShowHitClientRpc(HitType hitType, Vector3 hitPos)
    {
        var fab = GetFab(hitType);
        Instantiate(fab, hitPos, Quaternion.identity);
    }
}