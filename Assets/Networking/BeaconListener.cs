using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class BeaconListener : MonoBehaviour
{
    public int discoveryPort = 47777;
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

    private SynchronizationContext unityContext;

    private void Awake()
    {
        unityContext = SynchronizationContext.Current;
    }

    void Start()
    {
        _udp = new UdpClient(discoveryPort);
        _udp.EnableBroadcast = true;
        Debug.Log("Udp Client started");
        _udp.BeginReceive(OnRecv, null);
    }

    void OnDestroy() => _udp?.Dispose();

    void RebuildList()
    {
        for (var i = listParent.childCount - 1; i >= 0; i--)
        {
            Destroy(listParent.GetChild(i).gameObject);
        }

        foreach (var server in _servers)
        {
            var row = Instantiate(rowPrefab, listParent);
            row.label.text = server.name;
            row.button.onClick.AddListener(() =>
            {
                ConnectTo(server);
            });
        }
    }

    void OnRecvMainThread(string data, string ip)
    {
        // Debug.Log("RecvMainThread: " + data);        
        try
        {
            var packet = JsonConvert.DeserializeObject<HostBeaconPacket>(data);
            var serv = _servers.FirstOrDefault(s => 
                s.port == packet.gamePort && s.ip == ip && s.name == packet.device);
            if (serv != null)
            {
                serv.lastSeen = Time.time;
            }
            else
            {
                _servers.Add(new FoundServer
                {
                    ip = ip,
                    port = packet.gamePort,
                    name = packet.device,
                    lastSeen = Time.time
                });
            }
            
            RebuildList();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    void OnRecv(IAsyncResult ar)
    {
        // Debug.Log("Recv");
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = _udp.EndReceive(ar, ref ep);
        _udp.BeginReceive(OnRecv, null);

        var s = Encoding.UTF8.GetString(data);
        unityContext.Post(_ => OnRecvMainThread(s, ep.Address.ToString()), null);
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
        RebuildList();
    }
}
