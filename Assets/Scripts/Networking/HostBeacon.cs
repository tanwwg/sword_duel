using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class HostBeaconPacket
{
    public string gameId { get; set; } = "sword_duel";
    public ushort gamePort { get; set; }
    public string device { get; set; }
}

public class HostBeacon : MonoBehaviour
{
    public int discoveryPort = 47777;
    public ushort gamePort;
    public float intervalSeconds = 1f;

    public UnityTransport transport;

    UdpClient _udp;
    float _t;

    void Awake()
    {
        _udp = new UdpClient();
        _udp.EnableBroadcast = true;
        gamePort = transport.ConnectionData.Port;
        InvokeRepeating(nameof(SendBeacon), 0.0f, intervalSeconds);
    }

    void OnDestroy() => _udp?.Dispose();

    void SendBeacon()
    {
        // gameId|gamePort|hostName
        var payload = JsonConvert.SerializeObject(new HostBeaconPacket()
        {
            gamePort = this.gamePort,
            device = SystemInfo.deviceName
        });
        var bytes = Encoding.UTF8.GetBytes(payload);

        foreach (var ep in GetBroadcastEndpoints(discoveryPort))
            _udp.Send(bytes, bytes.Length, ep);
    }
    
    public static List<IPEndPoint> GetBroadcastEndpoints(int port)
    {
        var eps = new List<IPEndPoint>();

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue; // IPv4 only
                if (ua.IPv4Mask == null) continue;

                var ipBytes = ua.Address.GetAddressBytes();
                var maskBytes = ua.IPv4Mask.GetAddressBytes();

                var bcast = new byte[4];
                for (int i = 0; i < 4; i++)
                    bcast[i] = (byte)(ipBytes[i] | (maskBytes[i] ^ 255));

                eps.Add(new IPEndPoint(new IPAddress(bcast), port));
            }
        }

        // Fallback broadcast (sometimes enough by itself)
        eps.Add(new IPEndPoint(IPAddress.Broadcast, port));
        return eps;
    }
}
