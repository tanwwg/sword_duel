using Unity.Netcode;
using UnityEngine;

public enum HitType
{
    Hit, Block
}

public class HitController: NetworkBehaviour
{
    public HitAnimator hitAnimator;
    
    /// <summary>
    /// Check if the weapon hit anyone
    /// </summary>
    /// <returns></returns>
    public void HandleWeaponHit(PlayerController player)
    {
        var hitInfo = player.weapon.hitInfo;
        if (hitInfo == null) return;
        if (player.weapon.isProcessed) return;

        var hit = hitInfo.Value;
        
        player.weapon.MarkProcessed();
        
        var forceDir = player.transform.forward;
        forceDir.y = 0;
        forceDir = Quaternion.AngleAxis(hit.weapon.hitAngle, Vector3.up) * forceDir * hit.weapon.hitForce;
            
        hit.hittable.playerController.HitStun(forceDir, hit.weapon);
        
        hitAnimator.ShowHitClientRpc(hit.hitType, hit.hitPoint);
    }
}