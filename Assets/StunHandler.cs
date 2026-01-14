using System;
using UnityEngine;
using UnityEngine.UI;

public class StunHandler: MonoBehaviour
{
    public Rigidbody rb;
    public CharacterController controller;

    public AnimationClip hitClip;
    public Animator animator;

    public int maxHealth = 100;

    public Image healthBar;

    [Header("Runtime vars")] 
    public int health;
    public float stunTime;

    private void Start()
    {
        health = maxHealth;
        RefreshHealthBar();
    }

    void RefreshHealthBar()
    {
        healthBar.fillAmount = (float)health / maxHealth;
    }

    public void HitStun(Vector3 forceDir, WeaponData weapon)
    {
        stunTime += weapon.stunTime;
        health = Math.Max(0, health - weapon.damage);
        RefreshHealthBar();
        
        controller.enabled = false;
        rb.isKinematic = false;
        rb.AddForce(forceDir, ForceMode.Impulse);
        animator.SetFloat("OnHitSpeed", hitClip.length / stunTime);
        animator.Play("onhit", 0, 0f);
    }

    public bool UpdateStun()
    {
        if (stunTime <= 0) return false;
        
        stunTime -= Time.deltaTime;
        if (stunTime <= 0)
        {
            rb.isKinematic = true;
            controller.enabled = true;
            animator.CrossFade("Movement", 0.2f, 0);
        }
        return true;
    }
}
