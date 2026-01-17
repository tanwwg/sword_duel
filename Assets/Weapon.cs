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
    public GameObject hitPrefab;
}

[System.Serializable]
public class WeaponHitInfo
{
    public bool isHit = false;
    public bool isProcessed = false;
    
    public Hittable hittable;
    public Collider hitCollider;
    public WeaponData weapon;
    public Vector3 hitPoint;
}

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    public WeaponHitInfo hitInfo = null;

    private void OnEnable()
    {
        ResetHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (hitInfo.isHit) return; // just track the first hit
        
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (!hittable) return;

        this.hitInfo = new WeaponHitInfo()
        {
            isHit = true,
            hittable = hittable,
            hitCollider = other,
            weapon = this.weaponData,
            hitPoint = other.ClosestPoint(this.transform.position)
        };
    }

    public void ResetHit()
    {
        this.hitInfo = new WeaponHitInfo();
    }

    public WeaponHitInfo GetHitInfo()
    {
        if (this.hitInfo.isHit && !this.hitInfo.isProcessed)
        {
            this.hitInfo.isProcessed = true;
            return this.hitInfo;
        }
        return null;
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
