using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public PlayerController controller;
    
    // NEW INPUT SYSTEM CALLBACK
    public void OnMove(InputAction.CallbackContext context)
    {
        controller.moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            controller.isAttack = true;
        }
    }
}
