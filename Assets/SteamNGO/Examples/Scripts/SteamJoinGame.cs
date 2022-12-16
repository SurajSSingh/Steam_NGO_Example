using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SteamJoinGame : MonoBehaviour
{
    [field: SerializeField]
    public ulong SteamId { get; private set; }

    public string SteamName { get; private set; }

    public FacepunchTransport transport;

    void Start()
    {
        if (transport == null) transport = GameObject.FindObjectOfType<FacepunchTransport>();
    }

    public void AssignSteamUser(ulong steamId, string steamName)
    {
        SteamId = steamId;
        SteamName = steamName;
        if (GetComponentInChildren<TMP_Text>() is TMP_Text label)
        {
            label.text = $"Try Joining {SteamName}'s Game";
        }
    }
    public void RunTryJoinGame() => TryJoinGame();

    public bool TryJoinGame()
    {
        if (SteamId == 0)
        {
            Debug.LogError("Steam ID was not assigned!");
            return false;
        }
        var originalTarget = transport.targetSteamId;
        transport.targetSteamId = SteamId;

        if (NetworkManager.Singleton && NetworkManager.Singleton.StartClient())
        {
            Debug.Log($"Joined {SteamName}'s game!");
            return true;
        }
        Debug.LogError($"Could not join user {SteamName}!");
        // Return back to original
        transport.targetSteamId = originalTarget;
        return false;
    }

}
