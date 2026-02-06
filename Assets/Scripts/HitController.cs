using Unity.Netcode;
using UnityEngine;

public enum HitType
{
    Hit, Block, Parry
}

public class HitController: NetworkBehaviour
{
    public HitAnimator hitAnimator;

    public float blockStun = 0.05f;
    public float parryStun = 0.5f;    
    
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
        
        var opp = hit.hittable.playerController;

        if (opp.blockSystem.IsParry)
        {
            // opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * hit.weapon.hitForce, 0, parryStun);
            hitAnimator.ShowHitClientRpc(HitType.Parry, hit.hitPoint);
        } else if (opp.blockSystem.IsBlocking) {
            opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * hit.weapon.hitForce, 0, blockStun);
            hitAnimator.ShowHitClientRpc(HitType.Block, hit.hitPoint);
        }
        else
        {
            opp.HitStun(forceDir, hit.weapon.damage, hit.weapon.stunTime);
            hitAnimator.ShowHitClientRpc(hit.hitType, hit.hitPoint);
        }
        
    }
}