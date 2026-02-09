using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponData
{
    public int damage;

    public float hitForce = 100f;
    public float stunTime = 5f;
    public float hitAngle = -45f;

    public bool isHeavy;
}

[System.Serializable]
public struct WeaponHitInfo
{
    public Hittable hittable;
    public WeaponData weapon;
    public Vector3 hitPoint;
    public HitType hitType;

    public static WeaponHitInfo NoHit = new WeaponHitInfo();
}

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    public Hittable ignore;

    public WeaponHitInfo hitInfo = WeaponHitInfo.NoHit;
    public bool isProcessed;

    private void OnEnable()
    {
        ResetHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other.gameObject.name);
        if (hitInfo.hittable) return; // just track the first hit
        
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (!hittable) return;
        
        if (hittable == ignore) return;

        Debug.Log("MARK HIT " + other.gameObject.name);
        this.hitInfo = new WeaponHitInfo()
        {
            hittable = hittable,
            weapon = this.weaponData,
            hitPoint = other.ClosestPoint(this.transform.position)
        };
    }
    
    public void MarkProcessed() => isProcessed = true;

    public void ResetHit()
    {
        this.hitInfo = WeaponHitInfo.NoHit;
        isProcessed = false;
    }
    
    private void OnTriggerExit(Collider other)
    {

    }
}
