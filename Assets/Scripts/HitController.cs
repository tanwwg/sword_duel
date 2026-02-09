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
        var weapon = player.weapon;
        var hit = weapon.CheckHit();
        if (!hit || player.weapon.isProcessed) return;

        player.weapon.MarkProcessed();
        var dmg = weapon.weaponData;
        var hitPoint = hit.GetComponent<Collider>().ClosestPoint(weapon.transform.position);
        
        var forceDir = player.transform.forward;
        forceDir.y = 0;
        forceDir = Quaternion.AngleAxis(dmg.hitAngle, Vector3.up) * forceDir * dmg.hitForce;
        
        var opp = hit.playerController;

        if (opp.blockSystem.IsParry)
        {
            // opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * dmg.hitForce, 0, parryStun);
            hitAnimator.ShowHitClientRpc(HitType.Parry, hitPoint);
        } else if (opp.blockSystem.IsBlocking && !dmg.isHeavy) {
            opp.HitStun(forceDir, 0, 0.0f);
            player.HitStun(-player.transform.forward * dmg.hitForce, 0, blockStun);
            hitAnimator.ShowHitClientRpc(HitType.Block, hitPoint);
        }
        else
        {
            opp.HitStun(forceDir, dmg.damage, dmg.stunTime);
            hitAnimator.ShowHitClientRpc(HitType.Hit, hitPoint);
        }
        
    }
}