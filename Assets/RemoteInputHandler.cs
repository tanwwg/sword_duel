using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RemoteInputHandler: BaseInputHandler
{
    public PlayerControllerInput inputs = PlayerControllerInput.zero;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveServerRpc(context.ReadValue<Vector2>());
    }
    
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnLightAttackServerRpc();
        }
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    private void OnMoveServerRpc(Vector2 input)
    {
        inputs.moveInput = input;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    private void OnLightAttackServerRpc()
    {
        inputs.isAttack = true;
    }

    public override PlayerControllerInput ReadInputs()
    {
        var ret = inputs;
        inputs.isAttack = false;
        return ret;
    }
}