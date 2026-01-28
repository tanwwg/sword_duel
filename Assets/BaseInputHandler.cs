using Unity.Netcode;
using UnityEngine;

public abstract class BaseInputHandler: NetworkBehaviour
{
    public abstract PlayerControllerInput ReadInputs();
}