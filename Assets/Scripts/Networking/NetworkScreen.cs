using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviour
{
    public TMP_InputField ipField;

    public string playerName;

    public TMP_Text statusText;

    public Transform listParent;
    public UnityEvent onSignIn;
    public NetworkListRow rowPrefab;

    public GameObject lobbiesPanel;

    private Lobby _hostLoby;

    void SetStatus(string s)
    {
        Debug.Log(s);
        statusText.text = s;
    }

    void HandleException(Exception e)
    {
        Debug.LogException(e);
        SetStatus(e.Message);
    }

    async void Start()
    {
        try
        {
            SetupNetworkLogging();

            SetStatus("Init UGS");
            await UnityServices.InitializeAsync();

            SetStatus("Signing in...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

            SetStatus($"Signed in as {playerName}");
            onSignIn.Invoke();
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void OnDisable()
    {
        if (_hostLoby != null) LobbyService.Instance.DeleteLobbyAsync(_hostLoby.Id);
    }

    private static void SetupNetworkLogging()
    {
        var nm = NetworkManager.Singleton;
        nm.NetworkConfig.EnableNetworkLogs = true;
        nm.OnClientConnectedCallback += clientId =>
        {
            Debug.Log($"Client connected: {clientId}");
        };

        nm.OnClientDisconnectCallback += clientId =>
        {
            Debug.LogError($"Client disconnected: {clientId}");
        };

        nm.OnTransportFailure += () =>
        {
            Debug.LogError("Transport failure (Relay / UTP failed)");
        };
    }

    public async void StartHost()
    {
        try
        {
            SetStatus("Fetching regions...");
            var regions = await RelayService.Instance.ListRegionsAsync();
            var region = regions.First(rr => rr.Id == "asia-southeast1");
            
            SetStatus("Creating relay for asia-southeast1...");
            var alloc = await RelayService.Instance.CreateAllocationAsync(2, region.Id);
            
            SetStatus("Getting relay join code...");
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ToRelayServerData(RelayProtocol.WSS));
            transport.UseWebSockets = true;
            var connect = NetworkManager.Singleton.StartHost();
            if (!connect) throw new Exception("Failed to start host");

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new()
                {
                    { "relay", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };
            SetStatus("Creating lobby...");
            var lobbyName = $"{playerName}'s Lobby";
            _hostLoby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
            SetStatus($"{lobbyName} created");
            StartCoroutine(HeartbeatHostLobby());
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private IEnumerator HeartbeatHostLobby()
    {
        var delay = new WaitForSecondsRealtime(5.0f);
        while (_hostLoby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(_hostLoby.Id);
            yield return delay;
        }
    }

    public async void QueryLobbies()
    {
        try {
            SetStatus("Finding lobbies...");
            try
            {
                var lobbies = await NetworkScreen.QueryLobbiesAsync();
                RebuildList(lobbies.Results);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
                SetStatus(e.Message);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private static async Task<QueryResponse> QueryLobbiesAsync()
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions()
        {
            Count = 5,
            Filters = new List<QueryFilter>()
            {
                new(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            },
            Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            }
        };
        QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
        return lobbies;
    }
    
    void RebuildList(List<Lobby> lobbies)
    {
        for (var i = listParent.childCount - 1; i >= 0; i--)
        {
            Destroy(listParent.GetChild(i).gameObject);
        }
        SetStatus($"Found {lobbies.Count} lobbies");

        foreach (var lobby in lobbies)
        {
            var row = Instantiate(rowPrefab, listParent);
            row.label.text = lobby.Name;
            row.button.onClick.AddListener(() =>
            {
                ConnectToLobby(lobby);
            });
        }
    }

    private async void ConnectToLobby(Lobby lobby)
    {
        lobbiesPanel.SetActive(false);
        try
        {
            SetStatus("Joining lobby...");
            var join = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            var fresh = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
            var relayCode = fresh.Data["relay"].Value;
            SetStatus("Getting relay join code...");
            var alloc = await RelayService.Instance.JoinAllocationAsync(relayCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ToRelayServerData(RelayProtocol.WSS));
            transport.UseWebSockets = true;
            var connect = NetworkManager.Singleton.StartClient();
            if (!connect) throw new Exception("StartClient() failed");
            SetStatus("Starting client...");
        }
        catch (Exception ex)
        {
            lobbiesPanel.SetActive(true);
            HandleException(ex);
        }
    }

    public void ConnectByIp()
    {
        var nm = NetworkManager.Singleton;
        var utp = nm.GetComponent<UnityTransport>();

        utp.ConnectionData.Address = ipField.text;

        nm.StartClient();  
    }
}
