using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RemoteInputHandler: BaseInputHandler
{
    public RpcInputs rpcInputs;

    public override PlayerControllerInput ReadInputs()
    {
        return rpcInputs.ReadInputs();
    }
}