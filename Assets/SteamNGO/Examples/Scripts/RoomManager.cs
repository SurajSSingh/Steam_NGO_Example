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
