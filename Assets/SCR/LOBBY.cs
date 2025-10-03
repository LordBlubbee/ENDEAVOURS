using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LOBBY : MonoBehaviour
{
    string currentLobby = ""; 
    public static LOBBY lobby;
    public NetworkObject spawnCO;
    public Transform lobbyGrid;
    public LobbyButton spawnLobbyButton;
    public GameObject lobbyScreenConnect;
    public GameObject lobbyHostSettings;
    private List<LobbyButton> LobbyButtons = new List<LobbyButton>();

    

    [Header("Hosting Settings")]
    public TMP_InputField HostNameInput;

    private bool StartedJoining = false;
    public void StartHost()
    {
        //if (StartedJoining) return;
        //StartedJoining = true;
        string hos = HostNameInput.text;
        CreateLobby(hos);
    }
    private void Start()
    {
        lobby = this;
        InitLobby();

    }
    async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    public async void InitLobby()
    {
        await UnityServices.InitializeAsync();
        await SignUpAnonymouslyAsync();
        RefreshLobbyList();
    }
    public async void RefreshLobbyList()
    {
        if (StartedJoining) return;
        if (!AuthenticationService.Instance.IsAuthorized) return;
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 99;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
        {
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0")
        };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
        {
            new QueryOrder(
                asc: false,
                field: QueryOrder.FieldOptions.Created)
        };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (LobbyButton but in LobbyButtons)
            {
                Destroy(but.gameObject);
            }
            LobbyButtons = new List<LobbyButton>();
            foreach (Lobby lobby in lobbies.Results)
            {
                //Debug.Log($"Lobby found: {lobby.Name}");
                LobbyButton but = Instantiate(spawnLobbyButton, lobbyGrid);
                but.SetLobbyButton(lobby);
                LobbyButtons.Add(but);
            }

            //...
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public void PressJoinLobby(string ID)
    {
        if (StartedJoining) return;
        StartedJoining = true;
        JoinLobby(ID);
    }
    public void PressPlayAlone()
    {
        transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");

        NetworkManager.Singleton.StartHost();
        OpenGameInterface();

        Instantiate(spawnCO, Vector3.zero, Quaternion.identity).Spawn();
    }
    async void JoinLobby(string ID)
    {
        try
        {
            currentLobby = ID;
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(ID);

            IntroducePlayer();

            string relayJoinCode = joinedLobby.Data["joinCode"].Value;
            Debug.Log("Getting join code: " + relayJoinCode);

            // 2. Use it to join Relay
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
            NetworkManager.Singleton.StartClient();
            OpenGameInterface();

            StartedJoining = false;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            StartedJoining = false;
        }
    }
    public UnityTransport transport; 
    ConcurrentQueue<string> createdLobbyIds = new ConcurrentQueue<string>();
    public async void CreateLobby(string name)
    {
        if (name == "") name = "Main Lobby";
        Debug.Log($"Lobby created: {name}");
        string lobbyName = name;
        int maxPlayers = 6;
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = false;

        // 1. Authenticate (you already do this)

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        options.Data = new Dictionary<string, DataObject>()
        {
             {
                "joinCode", new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: joinCode
                )
            }
        };
        // 2. Create Lobby & attach joinCode to data
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        currentLobby = lobby.Id; 
        createdLobbyIds.Enqueue(lobby.Id);
        StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

        Debug.Log(lobby);
        IntroducePlayer();

        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        NetworkManager.Singleton.StartHost();
        OpenGameInterface();

        Instantiate(spawnCO, Vector3.zero, Quaternion.identity).Spawn();
    }

    void OpenGameInterface()
    {
        UI.ui.SelectScreen(UI.ui.LoadingStartGameUI);
    }
    void OnApplicationQuit()
    {
        while (createdLobbyIds.TryDequeue(out var lobbyId))
        {
            LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
    }
    async void IntroducePlayer()
    {
        //Introduce a new player!
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();

            Debug.Log("Player added");
            //Ensure you sign-in before calling Authentication Instance
            //See IAuthenticationService interface
            string playerId = AuthenticationService.Instance.PlayerId;

            var lobby = await LobbyService.Instance.UpdatePlayerAsync(currentLobby, playerId, options);

            /*foreach (Player play in lobby.Players)
            {
                if (play.Id == playerId) currentPlayer = play;
            }*/
            //...
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (currentLobby == lobbyId)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}
