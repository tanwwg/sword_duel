using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkScreen : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnClientConnectedCallback += (l) =>
        {
            NetworkManager.Singleton.SceneManager
                .LoadScene("NetworkScene", LoadSceneMode.Single);
        };
    }
}
