using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RpcInputs: NetworkBehaviour
{
    public PlayerControllerInput inputs = PlayerControllerInput.zero;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveServerRpc(context.ReadValue<Vector2>());
    }
    
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        OnLightAttackServerRpc(context.ReadValueAsButton());
    }
    
    public void OnBlock(InputAction.CallbackContext context)
    {
        OnBlockServerRpc(context.ReadValueAsButton());
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    private void OnMoveServerRpc(Vector2 input)
    {
        inputs.moveInput = input;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    private void OnLightAttackServerRpc(bool isDown)
    {
        inputs.isAttack = isDown;
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    private void OnBlockServerRpc(bool isDown)
    {
        inputs.isBlock = isDown;
    }

    public PlayerControllerInput ReadInputs()
    {
        var ret = inputs;
        return ret;
    }
}