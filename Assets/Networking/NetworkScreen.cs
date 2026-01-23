using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkScreen : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
}
