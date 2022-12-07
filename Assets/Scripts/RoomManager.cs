using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    [SerializeField] Transform playersInRoomContent;
    [SerializeField] GameObject roomPlayerPrefab;

    private List<GameObject> roomPlayers = new();
    public void InitializeRoom(Transform roomContentParent, GameObject roomPrefab)
    {
        playersInRoomContent = roomContentParent;
        roomPlayerPrefab = roomPrefab;
    }

    public void RefreshPlayersInRoom()
    {
        Debug.Log("Called Refresh in room");
        // if (!IsOwner) return;
        // Debug.Log("Server Refresh");
        // Clear content
        roomPlayers.Clear();
        foreach (Transform rp in playersInRoomContent) roomPlayers.Add(rp.gameObject);
        roomPlayers.ForEach(roomPlayer => Destroy(roomPlayer));

        if (NetworkManager.Singleton is NetworkManager nm)
        {
            foreach (var clientId in nm.ConnectedClientsIds)
            {
                Debug.LogWarning($"Creating name tag for client {clientId}");
                var playerNameTag = Instantiate(roomPlayerPrefab, playersInRoomContent);
                if (playerNameTag.GetComponent<NetworkObject>() is NetworkObject roomPlayerObj)
                {
                    roomPlayerObj.Spawn();
                }
                roomPlayers.Add(playerNameTag);
            }
        }
    }
}
