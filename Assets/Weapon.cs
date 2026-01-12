using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HIT! " + other.gameObject.name);
        var hittable = other.gameObject.GetComponent<Hittable>();
        if (hittable)
        {
            var animator = other.gameObject.GetComponent<Animator>();
            animator.SetTrigger("OnHit");
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
