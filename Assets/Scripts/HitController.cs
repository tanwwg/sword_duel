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
        if (!hitInfo.hittable) return;
        if (player.weapon.isProcessed) return;

        player.weapon.MarkProcessed();
        
        var forceDir = player.transform.forward;
        forceDir.y = 0;
        forceDir = Quaternion.AngleAxis(hitInfo.weapon.hitAngle, Vector3.up) * forceDir * hitInfo.weapon.hitForce;
        
        var opp = hitInfo.hittable.playerController;

        if (opp.blockSystem.IsParry)
        {
            // opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * hitInfo.weapon.hitForce, 0, parryStun);
            hitAnimator.ShowHitClientRpc(HitType.Parry, hitInfo.hitPoint);
        } else if (opp.blockSystem.IsBlocking && !hitInfo.weapon.isHeavy) {
            opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * hitInfo.weapon.hitForce, 0, blockStun);
            hitAnimator.ShowHitClientRpc(HitType.Block, hitInfo.hitPoint);
        }
        else
        {
            opp.HitStun(forceDir, hitInfo.weapon.damage, hitInfo.weapon.stunTime);
            hitAnimator.ShowHitClientRpc(hitInfo.hitType, hitInfo.hitPoint);
        }
        
    }
}