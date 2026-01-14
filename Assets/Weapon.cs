using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponData
{
    public float hitForce = 100f;
    public float stunTime = 5f;
    public float hitAngle = -45f;
    public GameObject hitPrefab;
}

public class Weapon : MonoBehaviour
{
    private List<StunHandler> hitCache = new();

    public WeaponData weaponData;
    
    private void OnTriggerEnter(Collider other)
    {
        var hittable = other.gameObject.GetComponent<StunHandler>();
        if (hittable && !hitCache.Contains(hittable))
        {
            Debug.Log("HIT! " + other.gameObject.name);
            hitCache.Add(hittable);
            
            var hitPoint = other.ClosestPoint(this.transform.position);

            var forceDir = other.transform.position - hitPoint;
            forceDir.y = 0;
            forceDir = Quaternion.AngleAxis(weaponData.hitAngle, Vector3.up) * forceDir;
            
            Instantiate(weaponData.hitPrefab, hitPoint, Quaternion.Euler(forceDir));
            
            hittable.HitStun(forceDir.normalized * weaponData.hitForce, weaponData.stunTime);
        }
    }

    public void ResetHit()
    {
        hitCache.Clear();
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
