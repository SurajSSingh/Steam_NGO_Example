using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

// Code adapted from https://github.com/bthomas2622/facepunch-steamworks-tutorial
public class NGOSteamBridge : MonoBehaviour
{
    [SerializeField] protected int maxMembers = 4;
    public static LogLevel LogLevel => NetworkManager.Singleton != null ? NetworkManager.Singleton.LogLevel : 0;
    public SteamId OpponentSteamId { get; set; }
    public bool LobbyPartnerDisconnected { get; set; }
    public Lobby CurrentLobby { get; private set; }

    private Lobby hostedMultiplayerLobby;
    private bool cleanedUp = false;

    private void Start()
    {
    }

    private void OnEnable()
    {
        // Network Manager Events
        if (NetworkManager.Singleton is NetworkManager netManager)
        {
            netManager.OnClientConnectedCallback += OnClientEnter;
            netManager.OnClientDisconnectCallback += OnClientLeave;
        }
        // Steam Events
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
    }

    // Update to run any callbacks
    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnDisable()
    {
        // Network Manager Events
        if (NetworkManager.Singleton is NetworkManager netManager)
        {
            netManager.OnClientConnectedCallback -= OnClientEnter;
            netManager.OnClientDisconnectCallback -= OnClientLeave;
        }
        // Steam Events
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnChatMessage -= OnChatMessageCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeaveCallback;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequestedCallback;
    }

    private void OnApplicationQuit()
    {
        GameCleanup();
    }

    private void OnDestroy()
    {
        GameCleanup();
    }

    public virtual async void CreateLobby(bool isFriendOnly)
    {
        try
        {
            NetworkManager.Singleton.StartHost();
            // Lobby? maybeCreatedLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
            // if (maybeCreatedLobby is Lobby createdLobby)
            // {
            //     LobbyPartnerDisconnected = false;
            //     hostedMultiplayerLobby = createdLobby;
            //     if (isFriendOnly) hostedMultiplayerLobby.SetFriendsOnly();
            //     else hostedMultiplayerLobby.SetPublic();
            //     hostedMultiplayerLobby.SetJoinable(true);
            //     // hostedMultiplayerLobby.SetData(staticDataString, lobbyParameters);
            //     CurrentLobby = hostedMultiplayerLobby;
            //     ShowLogValue($"Created Server", $"with isFriendOnly={isFriendOnly}");
            // }
            // else
            // {
            //     ShowLogWarning("Lobby created but not correctly instantiated");
            //     throw new Exception();
            // }
        }
        catch (Exception exception)
        {
            ShowLogWarning("Failed to create multiplayer lobby");
            ShowLogWarning(exception.ToString());
        }
    }

    public virtual async void TryJoiningLobby(ulong lobbyId)
    {
        try
        {
            Lobby? joinedLobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        }
        catch (Exception exception)
        {
            ShowLogWarning("Failed to join multiplayer lobby");
            ShowLogWarning(exception.ToString());
        }
    }

    public virtual void LeaveLobby()
    {
        ShowLogValue($"Trying to leave: {CurrentLobby.Id}");
        try
        {
            CurrentLobby.Leave();
        }
        catch (Exception e)
        {
            ShowLogError($"Error leaving current lobby: {e}");
        }
        finally
        {
            if (NetworkManager.Singleton) NetworkManager.Singleton.Shutdown();
        }
        ShowLogValue($"Checking Current Lobby ({CurrentLobby.Id}) is no longer valid: {!CurrentLobby.Id.IsValid}");
    }

    public virtual void OnClientEnter(ulong clientID)
    {
        ShowLogValue($"New client entered: {clientID}");
    }

    public virtual void OnClientLeave(ulong clientID)
    {
        ShowLogValue($"Client {clientID} has left");
    }

    protected virtual void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        ShowLogValue($"Running Lobby Created Callback", $"with result={result} lobby={lobby}");
        if (result == Result.OK)
        {
            // if (NetworkManager.Singleton.StartHost())
            // {
            //     ShowLogValue("Started Hosting Game");
            // }
            // else
            // {
            //     ShowLogError("Host Creation Error");
            // }
        }
        else
        {
            ShowLogError($"Lobby Creation Error with code: {result}");
        }
    }

    protected virtual void OnLobbyGameCreatedCallback(Lobby lobby, uint serverInt, ushort serverShort, SteamId steamId)
    {
        ShowLogValue($"Running Lobby Game Created Callback", $"with lobby = {lobby} ({serverInt}, {serverShort}, {steamId})");
    }

    protected virtual void OnLobbyEnteredCallback(Lobby lobby)
    {
        ShowLogValue($"Running Lobby Entered Callback", $"with lobby = {lobby}");
    }

    protected virtual void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId requester)
    {
        ShowLogValue($"Running Lobby Join Request Callback", $"with lobby = {lobby} and requester = {requester}");
    }
    protected virtual void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
    {
        ShowLogValue($"Running Member Joined Callback", $"with lobby = {lobby} and friend = {friend}");
    }

    protected virtual void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        ShowLogValue($"Running Chat Message Callback", $" with lobby = {lobby} and friend = {friend}");
    }

    protected virtual void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
    {
        ShowLogValue($"Running Member Leave Callback", $" with lobby = {lobby} and friend = {friend}");
    }

    protected virtual void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend friend)
    {
        ShowLogValue($"Running Member Disconnect Callback", $"with lobby = {lobby} and friend = {friend}");
    }

    protected virtual void GameCleanup()
    {
        if (!cleanedUp)
        {
            cleanedUp = true;
            LeaveLobby();
            SteamClient.Shutdown();
        }
    }

    public static void ShowLogError(string message)
    {
        if (LogLevel <= LogLevel.Error) Debug.LogError(message);
    }
    public static void ShowLogWarning(string message)
    {
        if (LogLevel <= LogLevel.Error) Debug.LogWarning(message);
    }

    private static void ShowLogValue(string message, string extra = "")
    {
        ShowLogValue(nameof(NGOSteamBridge), message, extra);
    }
    public static void ShowLogValue(string objectName, string message, string extra = "")
    {
        switch (LogLevel)
        {
            case LogLevel.Developer:
                Debug.Log($"[{objectName}] - {message} {extra}");
                break;
            case LogLevel.Normal:
                Debug.Log($"[{objectName}] - {message}");
                break;
            default:
                break;
        }
    }
}
