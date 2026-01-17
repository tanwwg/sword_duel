using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    public Weapon weapon;  
    
    public WeaponData[] comboData;
    
    
    [Header("Runtime Vars")] 
    public int comboIndex = -1;
    
    public bool IsPlaying => comboIndex >= 0;

    private void StartCombo(int idx)
    {
        comboIndex = idx;
        weapon.weaponData = comboData[idx];
    }

    public void StopCombo()
    {
        weapon.gameObject.SetActive(false);
        comboIndex = -1;
    }
    
    public void Tick(bool isClick, PlayerAnimState animState)
    {
        weapon.gameObject.SetActive(animState.isAttacking);
        
        if (!IsPlaying)
        {
            if (isClick)
            {
                StartCombo(0);
            }
        }
        else
        {
            if (animState.canCombo && isClick && comboIndex < comboData.Length - 1)
            {
                StartCombo(comboIndex + 1);
            }
        }
    }

}
