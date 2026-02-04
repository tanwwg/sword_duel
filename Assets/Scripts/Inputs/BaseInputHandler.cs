using Unity.Netcode;
using UnityEngine;

public class BaseInputHandler: MonoBehaviour
{
    public virtual PlayerControllerInput ReadInputs()
    {
        return PlayerControllerInput.zero;
    }
}