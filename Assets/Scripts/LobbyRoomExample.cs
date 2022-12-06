using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;
using System;
using Steamworks;
using Unity.Netcode;

public enum LobbyState
{
    PublicLobby,
    CreatingNew,
    PrivateRoom,
}

public class LobbyRoomExample : MonoBehaviour
{
    [Header("Lobby Screens")]
    [SerializeField] GameObject generalLobbyScreen;
    [SerializeField] GameObject createNewLobbyScreen;
    [SerializeField] GameObject roomScreen;

    [Header("Scroll Content")]
    [SerializeField] Transform joinableRoomsContent;
    [SerializeField] Transform playersInRoomContent;



    private LobbyState currentLobbyState = LobbyState.PublicLobby;
    private LogLevel LogLevel => NetworkManager.Singleton.LogLevel;

    void Start()
    {
        ShowLobbyScreen(LobbyState.PublicLobby);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientEntered;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientExited;
            NetworkManager.Singleton.OnServerStarted += ServerStarted;
        }
    }


    private void OnDisable()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientEntered;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientExited;
            NetworkManager.Singleton.OnServerStarted -= ServerStarted;
        }
    }

    // Non-null variant to make it easier to use in inspector
    public void ShowLobbyScreen(LobbyState newLobby)
    {
        ShowLobbyScreen(newLobby);
    }

    public void ShowLobbyScreen(LobbyState? newLobby = null)
    {
        // Set current lobby state to new lobby state if it has a value, else leave unchanged
        // NOTE: ?? allowed here as LobbyState is a nullable enum type (don't do this for Unity objects like MonoBehaviour)
        currentLobbyState = newLobby ?? currentLobbyState;
        generalLobbyScreen.SetActive(currentLobbyState == LobbyState.PublicLobby);
        createNewLobbyScreen.SetActive(currentLobbyState == LobbyState.CreatingNew);
        roomScreen.SetActive(currentLobbyState == LobbyState.PrivateRoom);
    }

    public void StartHost() => TryStartHost();

    public bool TryStartHost()
    {
        bool successfullyStarted = NetworkManager.Singleton && NetworkManager.Singleton.StartHost();
        if (LogLevel <= LogLevel.Normal && successfullyStarted)
        {
            Debug.Log("Host was able to start!");
        }
        else if (LogLevel <= LogLevel.Error && !successfullyStarted)
        {
            Debug.LogError("Could not start host!");
        }
        return successfullyStarted;
    }

    // public void StartClient() => TryStartClient();

    // public bool TryStartClient()
    // {
    //     bool successfullyStarted = NetworkManager.Singleton && NetworkManager.Singleton.StartClient();
    //     if (LogLevel <= LogLevel.Normal && successfullyStarted)
    //     {
    //         Debug.Log("Client was able to connect!");
    //     }
    //     else if (LogLevel <= LogLevel.Error && !successfullyStarted)
    //     {
    //         Debug.LogError("Could not connect client to host!");
    //     }
    //     return successfullyStarted;
    // }

    public void LeaveGame()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            Debug.LogWarning("Could not shutdown, network manager may already be shutdown!");
        }
    }

    private void ServerStarted()
    {
        if (LogLevel <= LogLevel.Normal) Debug.Log("Server has started!");
    }

    private void ClientEntered(ulong clientId)
    {
        if (LogLevel <= LogLevel.Normal) Debug.Log($"Client {clientId} has started!");
        ShowLobbyScreen(LobbyState.PrivateRoom);
    }

    private void ClientExited(ulong clientId)
    {
        if (LogLevel <= LogLevel.Normal) Debug.Log($"Client {clientId} has left!");
        ShowLobbyScreen(LobbyState.PublicLobby);

    }



}
