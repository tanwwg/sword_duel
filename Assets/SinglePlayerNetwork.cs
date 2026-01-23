using Unity.Netcode;
using UnityEngine;

public class SinglePlayerNetwork : MonoBehaviour
{
    public GameController gameController;

    public void Start()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Update()
    {
        gameController.Tick();
    }
}
