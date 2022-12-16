using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System;
using Steamworks;
using Unity.Netcode;
using Unity.Collections;

public enum LobbyState
{
    PublicLobby,
    CreatingNew,
    PrivateRoom,
    NoLobby, // This is for when in main menu or game scenes
}

public class LobbyManager : MonoBehaviour
{
    private const int WAIT_TIMEOUT = 60;
    [Header("Lobby Screens")]
    [SerializeField] GameObject generalLobbyScreen;
    [SerializeField] GameObject currentRoom;
    [Header("Scroll Content")]
    [SerializeField] Transform joinableRoomsContent;
    [Header("Prefabs Content")]
    [SerializeField] GameObject joinGamePrefab;
    [SerializeField] GameObject roomManagerPrefab;

    [Header("Other UI")]
    [SerializeField] GameObject hostGame;
    private LobbyState currentLobbyState = LobbyState.PublicLobby;

    private LogLevel LogLevel => NetworkManager.Singleton ? NetworkManager.Singleton.LogLevel : LogLevel.Nothing;
    private void DeveloperLog(string msg)
    {
        if (LogLevel <= LogLevel.Developer) Debug.Log($"[{nameof(LobbyManager)}] {msg}");
    }
    void Start()
    {
        ShowLobbyScreen(LobbyState.PublicLobby);
        RefreshServerList();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForNetworkManagerSingletonToSubscrible());
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientEntered;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientExited;
            NetworkManager.Singleton.OnServerStarted -= ServerStarted;
            // SteamNetworkingSockets.OnConnectionStatusChanged -= OnConnectionChange;
        }
    }

    // Non-null variant to make it easier to use in inspector
    public void ShowLobbyScreenNonNull(LobbyState newLobby)
    {
        ShowLobbyScreen(newLobby);
    }

    public void ShowLobbyScreen(LobbyState? newLobby = null)
    {
        // Set current lobby state to new lobby state if it has a value, else leave unchanged
        // NOTE: ?? allowed here as LobbyState is a nullable enum type (don't do this for Unity objects like MonoBehaviour)
        currentLobbyState = newLobby ?? currentLobbyState;
        generalLobbyScreen.SetActive(currentLobbyState == LobbyState.PublicLobby);
        // createNewLobbyScreen.SetActive(currentLobbyState == LobbyState.CreatingNew);
        currentRoom.SetActive(currentLobbyState == LobbyState.PrivateRoom);
        if (currentLobbyState == LobbyState.PublicLobby)
        {
            RefreshServerList();
        }
    }

    public void StartHost() => TryStartHost();

    public bool TryStartHost()
    {
        DeveloperLog("Trying to host game.");
        bool successfullyStarted = NetworkManager.Singleton && NetworkManager.Singleton.StartHost();
        if (successfullyStarted)
        {
            DeveloperLog("Host was able to start!");
            // Currently go immideately to room, rather than lobby settings creation
        }
        else if (LogLevel <= LogLevel.Error && !successfullyStarted)
        {
            Debug.LogError("Could not start host!");
        }
        return successfullyStarted;
    }

    public void StartClient() => TryStartClient();
    public bool TryStartClient()
    {
        DeveloperLog("Trying to join game.");
        bool successfullyStarted = NetworkManager.Singleton && NetworkManager.Singleton.StartClient();
        if (successfullyStarted)
        {
            DeveloperLog("Client was able to join game!");
            ShowLobbyScreen(LobbyState.PrivateRoom);
        }
        else if (LogLevel <= LogLevel.Error && !successfullyStarted)
        {
            Debug.LogError("Could not join as client!");
        }
        return successfullyStarted;
    }

    public void LeaveGame()
    {
        DeveloperLog("Trying to leave game.");
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
            ShowLobbyScreen(LobbyState.PublicLobby);
        }
        else
        {
            Debug.LogWarning("Could not shutdown, network manager may already be shutdown!");
        }
    }

    private void ClientEntered(ulong clientId)
    {
        DeveloperLog($"Client {clientId} has started!");
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            ShowLobbyScreen(LobbyState.PrivateRoom);
        }
        // else if (NetworkManager.Singleton.IsHost)
        // {
        //     StartCoroutine(RunRefresh());
        // }
    }

    private void ServerStarted()
    {
        DeveloperLog("Server has started!");
    }

    private void ClientExited(ulong clientId)
    {
        DeveloperLog($"Client {clientId} has left!");
        // Show public lobby for person who left
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            ShowLobbyScreen(LobbyState.PublicLobby);
        }
    }

    public void RefreshServerList()
    {
        // Clear content
        var children = new List<GameObject>();
        foreach (Transform child in joinableRoomsContent) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        // NOTE: We only look at friends playing the same game because we use 
        //       default 480 App Id. If we had our own AppId, we won't need to
        //       do any pre-filtering. (480 is used by lots of people, would be
        //       nearly impossible to find the correct server without filtering). 
        foreach (var friend in SteamFriends.GetFriends())
        {
            // Debug.Log($"Friend: {friend.Name}");
            if (friend.IsPlayingThisGame)
            {
                var joinFriendGame = Instantiate(joinGamePrefab, joinableRoomsContent);
                if (joinFriendGame.GetComponent<SteamJoinGame>() is SteamJoinGame steamJoin)
                {
                    steamJoin.AssignSteamUser(friend.Id, friend.Name);
                }
            }
        }
    }

    private IEnumerator WaitForNetworkManagerSingletonToSubscrible()
    {
        if (hostGame) hostGame.SetActive(false);
        if (NetworkManager.Singleton == null)
        {
            for (int seconds = 0; seconds < WAIT_TIMEOUT; seconds++)
            {
                if (NetworkManager.Singleton != null) break;
                yield return new WaitForSeconds(1);
            }
        }
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientEntered;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientExited;
            NetworkManager.Singleton.OnServerStarted += ServerStarted;
            if (hostGame) hostGame.SetActive(true);
            DeveloperLog("Network Callbacks assigned!");
        }
        else if (LogLevel <= LogLevel.Error)
        {
            Debug.LogError("Network Manger Singleton could not be found!");
        }
    }
}
