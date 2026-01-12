using System;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private List<Hittable> hitCache = new();
    
    private void OnTriggerEnter(Collider other)
    {
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (hittable && !hitCache.Contains(hittable))
        {
            Debug.Log("HIT! " + other.gameObject.name);
            hitCache.Add(hittable);
            var animator = other.gameObject.GetComponent<Animator>();
            animator.SetTrigger("OnHit");
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
