using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private PlayerControllerInput inputs = PlayerControllerInput.zero;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        inputs.moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        inputs.isAttack = context.ReadValueAsButton();
    }

    public PlayerControllerInput ReadInputs()
    {
        var ret = inputs;
        inputs.isAttack = false;
        return ret;
    }
}
