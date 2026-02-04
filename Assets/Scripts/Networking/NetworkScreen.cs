using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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

    void SetStatus(string s)
    {
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
    
    public async void StartHost()
    {
        try
        {
            SetStatus("Creating relay...");
            var alloc = await RelayService.Instance.CreateAllocationAsync(2);
            SetStatus("Getting relay join code...");
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ToRelayServerData(RelayProtocol.WSS));
            transport.UseWebSockets = true;
            NetworkManager.Singleton.StartHost();

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new()
                {
                    { "relay", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };
            SetStatus("Creating lobby...");
            var lobbyName = $"{playerName}'s Lobby";
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
            SetStatus($"{lobbyName} created");
        }
        catch (Exception ex)
        {
            HandleException(ex);
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
            var relayCode = join.Data["relay"].Value;
            SetStatus("Getting relay join code...");
            var alloc = await RelayService.Instance.JoinAllocationAsync(relayCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(alloc.ToRelayServerData(RelayProtocol.WSS));
            transport.UseWebSockets = true;
            NetworkManager.Singleton.StartClient();
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
