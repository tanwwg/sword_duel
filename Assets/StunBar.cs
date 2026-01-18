using UnityEngine;
using UnityEngine.UI;

public class StunBar : MonoBehaviour
{
    public PlayerController playerController;
    
    public GameObject stunBar;
    public Image stunBarImage;

    public float maxStunTime = 0;
    
    // Update is called once per frame
    void Update()
    {
        if (!playerController)
        {
            return;
        }
        
        if (playerController.stunTime > 0)
        {
            stunBar.SetActive(true);
            if (playerController.stunTime > maxStunTime)
            {
                maxStunTime = playerController.stunTime;
            }
            stunBarImage.fillAmount = playerController.stunTime / maxStunTime;
        }
        else
        {
            stunBar.SetActive(false);
            maxStunTime = 0;
        }
    }
}
