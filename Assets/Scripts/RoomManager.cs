using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : NetworkBehaviour
{
    [SerializeField] Transform playersInRoomContent;
    [SerializeField] GameObject roomPlayerPrefab;
    [SerializeField] Button leaveButton;

    private List<GameObject> roomPlayers = new();

    private void OnEnable()
    {
        if (leaveButton)
        {
            leaveButton.onClick.AddListener(OnClickLeave);
        }
    }


    private void OnDisable()
    {
        if (leaveButton)
        {
            leaveButton.onClick.RemoveListener(OnClickLeave);
        }
    }

    public void RefreshPlayersInRoom()
    {
        if (IsHost || IsServer)
        {
            // RefreshPlayersInRoomClientRPC();
            Debug.Log("Called Refresh in room");
            foreach (var networkPlayer in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (networkPlayer.PlayerObject)
                {
                    networkPlayer.PlayerObject.TrySetParent(playersInRoomContent);
                }
            }
            // if (playersInRoomContent == null)
            // {
            //     playersInRoomContent = GameObject.FindGameObjectWithTag("PlayerList").transform;
            // }
            // // Clear content
            // roomPlayers.Clear();
            // foreach (Transform rp in playersInRoomContent) roomPlayers.Add(rp.gameObject);
            // roomPlayers.ForEach(roomPlayer => Destroy(roomPlayer));

            // if (NetworkManager)
            // {
            //     foreach (var clientId in NetworkManager.ConnectedClientsIds)
            //     {
            //         Debug.LogWarning($"Creating name tag for client {clientId}");
            //         var playerNameTag = Instantiate(roomPlayerPrefab, playersInRoomContent);
            //         if (playerNameTag.GetComponent<NetworkObject>() is NetworkObject roomPlayerObj)
            //         {

            //             roomPlayerObj.TrySetParent(playersInRoomContent);
            //             roomPlayerObj.SpawnWithOwnership(clientId);
            //         }
            //         roomPlayers.Add(playerNameTag);
            //     }
            // }
        }
    }

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
