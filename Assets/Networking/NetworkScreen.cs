using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviour
{
    public TMP_InputField ipField;
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void ConnectByIp()
    {
        var nm = NetworkManager.Singleton;
        var utp = nm.GetComponent<UnityTransport>();

        utp.ConnectionData.Address = ipField.text;
        utp.ConnectionData.Port = utp.ConnectionData.Port;

        nm.StartClient();  
    }
}
