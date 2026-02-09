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

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public Hittable ignore;
    public CapsuleCollider capsule;

    [Header("Runtime data")]
    public bool isProcessed;
    private readonly Collider[] _hits = new Collider[5];

    private void OnEnable()
    {
        ResetHit();
    }

    public Hittable CheckHit()
    {
        GetCapsuleWorldPoints(capsule, out var p0, out var p1, out var r);
        var numHits = Physics.OverlapCapsuleNonAlloc(p0, p1, r, _hits);
        for (var i = 0; i < numHits; i++)
        {
            if (_hits[i].TryGetComponent<Hittable>(out var hit) && hit != ignore) return hit;
        }
        return null;
        
    }
    
    private static void GetCapsuleWorldPoints(CapsuleCollider c, out Vector3 p0, out Vector3 p1, out float radius)
    {
        Transform t = c.transform;

        // CapsuleCollider.direction: 0=X, 1=Y, 2=Z
        Vector3 axis = c.direction == 0 ? t.right : (c.direction == 1 ? t.up : t.forward);

        Vector3 lossy = t.lossyScale;

        // radius scales by the max of the two perpendicular axes
        float rScale = c.direction == 0 ? Mathf.Max(lossy.y, lossy.z)
            : c.direction == 1 ? Mathf.Max(lossy.x, lossy.z)
            : Mathf.Max(lossy.x, lossy.y);

        radius = c.radius * rScale;

        // height scales along the capsule axis
        float hScale = c.direction == 0 ? lossy.x : (c.direction == 1 ? lossy.y : lossy.z);
        float height = Mathf.Max(c.height * hScale, radius * 2f);

        Vector3 center = t.TransformPoint(c.center);
        float offset = (height * 0.5f) - radius;

        p0 = center + axis * offset;
        p1 = center - axis * offset;
    }


    public void MarkProcessed() => isProcessed = true;

    public void ResetHit()
    {
        isProcessed = false;
    }
    
    private void OnTriggerExit(Collider other)
    {

    }
}
