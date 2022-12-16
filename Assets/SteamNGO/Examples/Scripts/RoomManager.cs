using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : NetworkBehaviour
{
    [SerializeField] Transform playersInRoomContent;
    public Transform PlayersInRoomContent => playersInRoomContent;
    [SerializeField] GameObject buttonPrefab;
    public GameObject ButtonPrefab => buttonPrefab;
    [SerializeField] Button leaveButton;

    // private Dictionary<ulong, GameObject> roomPlayerBtns = new();


    // private NetworkList<RoomPlayer> allPlayers = new();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ClearAllButtons();
        foreach (var netObj in GetComponentsInChildren<NetworkObject>())
        {
            if (this.NetworkObject != netObj && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }
        }
        Debug.Log("Room Manager Spawned");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        // Delete all buttons
        ClearAllButtons();
        OnClickLeave();
    }

    private void ClearAllButtons()
    {
        List<GameObject> roomPlayers = new();
        foreach (Transform rp in playersInRoomContent.transform) roomPlayers.Add(rp.gameObject);
        roomPlayers.ForEach(roomPlayer => Destroy(roomPlayer));
    }

    private void OnEnable()
    {
        // NetworkManager.OnClientConnectedCallback += ClientConnected;
        // NetworkManager.OnClientDisconnectCallback += ClientDisconnected;
        if (leaveButton)
        {
            leaveButton.onClick.AddListener(OnClickLeave);
        }
    }

    private void OnDisable()
    {
        // NetworkManager.OnClientConnectedCallback -= ClientConnected;
        // NetworkManager.OnClientDisconnectCallback -= ClientDisconnected;
        if (leaveButton)
        {
            leaveButton.onClick.RemoveListener(OnClickLeave);
        }
    }

    // private void ClientConnected(ulong clientId)
    // {
    //     // RefreshPlayerContentServerRPC();
    //     var newPlayerButton = Instantiate(buttonPrefab, playersInRoomContent);
    //     if (IsLocalPlayer
    //     && newPlayerButton.GetComponent<Button>() is Button button
    //     && NetworkManager.LocalClient.PlayerObject.GetComponent<RoomPlayer>() is RoomPlayer roomPlayer)
    //     {
    //         roomPlayer.UpdateButtonRef(button);
    //     }
    //     roomPlayerBtns.Add(clientId, newPlayerButton);
    //     Debug.Log($"Refresh after client {clientId} enters");
    //     if (OwnerClientId == clientId) UpdateAllButtonsServerRpc();
    //     // RefreshPlayersInRoomClientRPC(clientId);
    // }
    // private void ClientDisconnected(ulong clientId)
    // {
    //     // RefreshPlayerContentServerRPC();
    //     Debug.Log($"Refresh after client {clientId} leave");
    //     if (roomPlayerBtns.TryGetValue(clientId, out var buttonGO))
    //     {
    //         Destroy(buttonGO);
    //     }
    //     roomPlayerBtns.Remove(clientId);
    //     // if(OwnerClientId != clientId){
    //     //     UpdateAllButtonsServerRpc();
    //     // }
    // }

    // [ServerRpc]
    // public void UpdateAllButtonsServerRpc(bool hasIgnoreId = false, ulong ignoreId = 0)
    // {

    // }
    // [ClientRpc]
    // public void UpdateAllButtonsClientRpc()
    // {

    // }

    // // [ServerRpc]
    // // public void AddButtonServerRpc(Button x)
    // // {

    // // }

    // // [ServerRpc]
    // // public Button CreateOrGetPlayerButtonServerRpc(ulong clientId)
    // // {
    // //     // Try finding button
    // //     foreach (var btn in roomPlayerBtns)
    // //     {
    // //         if (btn.GetComponent<NetworkObject>() is NetworkObject netObj && netObj.OwnerClientId == clientId)
    // //         {
    // //             return btn;
    // //         }
    // //     }
    // //     // Or create new if not found
    // //     var newPlayerButton = Instantiate(buttonPrefab, playersInRoomContent.transform);
    // //     if (newPlayerButton.GetComponent<Button>() is Button button)
    // //     {
    // //         roomPlayerBtns.Add(button);
    // //         return button;
    // //     }
    // //     return null;
    // // }

    // [ServerRpc(RequireOwnership = false)]
    // public void RefreshPlayerContentServerRPC()
    // {
    //     // Clear content
    //     // List<GameObject> roomPlayers = new();
    //     // foreach (Transform rp in playersInRoomContent.transform) roomPlayers.Add(rp.gameObject);
    //     // roomPlayers.ForEach(roomPlayer => Destroy(roomPlayer));
    //     // roomPlayerBtns.ForEach(roomPlayer => Destroy(roomPlayer));
    //     // roomPlayerBtns.Clear();
    //     RemoveAllButtonsClientRpc();
    //     Debug.Log("Called Refresh in room");
    //     foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
    //     {
    //         RefreshPlayersInRoomClientRPC(clientId);
    //     }
    //     // foreach (var networkPlayer in NetworkManager.Singleton.ConnectedClientsList)
    //     // {
    //     //     if (networkPlayer.PlayerObject
    //     //     && networkPlayer.PlayerObject.GetComponent<RoomPlayer>() is RoomPlayer roomPlayer)
    //     //     {
    //     //         var newPlayerButton = Instantiate(buttonPrefab, playersInRoomContent.transform);
    //     //         roomPlayerBtns.Add(newPlayerButton);
    //     //         if (newPlayerButton.GetComponent<NetworkObject>() is NetworkObject netObj)
    //     //         {
    //     //             netObj.SpawnWithOwnership(networkPlayer.ClientId);
    //     //         }
    //     //         if (newPlayerButton.GetComponent<Button>() is Button button)
    //     //         {
    //     //             roomPlayer.UpdateButton(button);
    //     //         }
    //     //     }
    //     // }
    // }

    // [ClientRpc]
    // public void RemoveAllButtonsClientRpc()
    // {
    //     // roomPlayerBtns.ForEach(roomPlayer => Destroy(roomPlayer));
    //     roomPlayerBtns.Clear();
    // }

    // [ClientRpc]
    // // Refresh Player keeps everyone in sync becasue all players execute the same code
    // public void RefreshPlayersInRoomClientRPC(ulong clientId)
    // {
    //     // Spawn a button
    //     var newPlayerButton = Instantiate(buttonPrefab, playersInRoomContent);
    //     // roomPlayerBtns.Add(newPlayerButton);

    //     // Wire button for player if the id matches
    //     if (newPlayerButton.GetComponent<Button>() is Button button)
    //     {
    //         if (OwnerClientId == clientId
    //         && this.NetworkManager.LocalClient.PlayerObject.GetComponent<RoomPlayer>() is RoomPlayer localRoomPlayer)
    //         {
    //             localRoomPlayer.UpdateButtonRef(button);
    //         }
    //         else if (OwnerClientId != clientId)
    //         {
    //             foreach (RoomPlayer otherPlayer in FindObjectsOfType<RoomPlayer>())
    //             {
    //                 if (otherPlayer.OwnerClientId == clientId)
    //                 {
    //                     otherPlayer.UpdateButtonRef(button, false);
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }

    // public void RefreshPlayersContent()
    // {
    //     if (IsHost || IsServer)
    //     {

    //         RefreshPlayerContentServerRPC();
    //         // if (playersInRoomContent == null)
    //         // {
    //         //     playersInRoomContent = GameObject.FindGameObjectWithTag("PlayerList").transform;
    //         // }


    //         // if (NetworkManager)
    //         // {
    //         //     foreach (var clientId in NetworkManager.ConnectedClientsIds)
    //         //     {
    //         //         Debug.LogWarning($"Creating name tag for client {clientId}");
    //         //         var playerNameTag = Instantiate(roomPlayerPrefab, playersInRoomContent);
    //         //         if (playerNameTag.GetComponent<NetworkObject>() is NetworkObject roomPlayerObj)
    //         //         {

    //         //             roomPlayerObj.TrySetParent(playersInRoomContent);
    //         //             roomPlayerObj.SpawnWithOwnership(clientId);
    //         //         }
    //         //         roomPlayers.Add(playerNameTag);
    //         //     }
    //         // }
    //     }
    //     else
    //     {
    //         RefreshPlayerContentServerRPC();
    //     }
    // }

    // [ClientRpc]
    // public void RefreshPlayersInRoomClientRPC()
    // {
    //     Debug.Log("Called Refresh in room");
    //     if (playersInRoomContent == null)
    //     {
    //         playersInRoomContent = GameObject.FindGameObjectWithTag("PlayerList").transform;
    //     }
    //     // Clear content
    //     roomPlayers.Clear();
    //     foreach (Transform rp in playersInRoomContent) roomPlayers.Add(rp.gameObject);
    //     roomPlayers.ForEach(roomPlayer => Destroy(roomPlayer));

    //     if (NetworkManager.Singleton is NetworkManager nm)
    //     {
    //         foreach (var clientId in nm.ConnectedClientsIds)
    //         {
    //             Debug.LogWarning($"Creating name tag for client {clientId}");
    //             var playerNameTag = Instantiate(roomPlayerPrefab, playersInRoomContent);
    //             if (playerNameTag.GetComponent<NetworkObject>() is NetworkObject roomPlayerObj)
    //             {
    //                 roomPlayerObj.SpawnWithOwnership(clientId, true);
    //             }
    //             roomPlayers.Add(playerNameTag);
    //         }
    //     }
    // }

    public void OnClickLeave()
    {
        if (FindObjectOfType<LobbyManager>() is LobbyManager lobby)
        {
            lobby.LeaveGame();
        }
        else
        {
            NetworkManager.Shutdown();
        }
    }
}
