using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class BeaconListener : MonoBehaviour
{
    public int discoveryPort = 47777;
    public string gameId = "MyGame-v1";
    public float serverTimeout = 4.0f;

    UdpClient _udp;

    public class FoundServer
    {
        public string ip; 
        public ushort port; 
        public string name; 
        public float lastSeen;
    }
    public readonly List<FoundServer> _servers = new();

    public Transform listParent;
    public NetworkListRow rowPrefab;

    void Start()
    {
        _udp = new UdpClient(discoveryPort);
        _udp.EnableBroadcast = true;
        _udp.BeginReceive(OnRecv, null);
    }

    void OnDestroy() => _udp?.Dispose();

    void RebuildList()
    {
        for (int i = listParent.childCount - 1; i >= 0; i--)
        {
            Destroy(listParent.GetChild(i).gameObject);
        }

        foreach (var server in _servers)
        {
            var row = Instantiate(rowPrefab);
            row.label.text = server.name;
            row.button.onClick.AddListener(() =>
            {
                ConnectTo(server);
            });
        }
        
    }

    void OnRecv(IAsyncResult ar)
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = _udp.EndReceive(ar, ref ep);
        _udp.BeginReceive(OnRecv, null);

        var s = Encoding.UTF8.GetString(data);
        try
        {
            var packet = JsonConvert.DeserializeObject<HostBeaconPacket>(s);
            _servers.Add(new FoundServer
            {
                ip = ep.Address.ToString(),
                port = packet.gamePort,
                name = packet.device,
                lastSeen = Time.time
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

    }

    public void ConnectTo(FoundServer server)
    {
        var nm = NetworkManager.Singleton;
        var utp = nm.GetComponent<UnityTransport>();

        utp.ConnectionData.Address = server.ip;  
        utp.ConnectionData.Port = server.port;

        nm.StartClient();                 
    }

    void Update()
    {
        var now = Time.time;
        var dead = _servers.Where(s => s.lastSeen + serverTimeout < now).ToList();
        if (dead.Count <= 0) return;
        
        foreach (var d in dead) _servers.Remove(d);
        
    }
}
