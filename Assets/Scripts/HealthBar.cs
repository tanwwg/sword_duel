using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerController playerController;
    public Image bar;

    private void Update()
    {
        bar.fillAmount = (float)playerController.health.Value / playerController.maxHealth;
    }
}
