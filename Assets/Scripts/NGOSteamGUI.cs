using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NGOSteamGUI : MonoBehaviour
{
    [SerializeField] FacepunchTransport steamTransport;
    [SerializeField] TMP_Text serverIDText;
    [SerializeField] TMP_InputField serverField;
    public List<GameObject> outsideGameUI = new();
    public List<GameObject> insideGameUI = new();

    private ulong defaultServerId;

    void Start()
    {
        if (steamTransport == null) steamTransport = GetComponent<FacepunchTransport>();
        ActiveOutsideGameUI(true);
    }

    public void HostGame()
    {
        if (!steamTransport)
        {
            Debug.LogError("Steam Transport not found");
            return;
        }
        if (defaultServerId == 0)
        {
            defaultServerId = steamTransport.targetSteamId != 0 ? steamTransport.targetSteamId : Steamworks.SteamClient.SteamId;
        }
        if (NetworkManager.Singleton && NetworkManager.Singleton.StartHost())
        {
            ActiveOutsideGameUI(false, defaultServerId);
            NetworkLog.LogInfo("Started Host");
        }
        else
        {
            NetworkLog.LogError("Could not start host");
        }
    }
    public void JoinGame()
    {
        if (!steamTransport)
        {
            Debug.LogError("Steam Transport not found");
            return;
        }
        if (!serverField)
        {
            Debug.LogError("No Input Field for inputting server ID found");
            return;
        }
        if (defaultServerId == 0)
        {
            defaultServerId = steamTransport.targetSteamId != 0 ? steamTransport.targetSteamId : Steamworks.SteamClient.SteamId;
        }
        if (ulong.TryParse(serverField.text, out ulong targetID))
        {
            steamTransport.targetSteamId = targetID;
            if (NetworkManager.Singleton && NetworkManager.Singleton.StartClient())
            {
                ActiveOutsideGameUI(false, targetID);
                NetworkLog.LogInfo($"Started Client with server: {targetID}");
                return;
            }
        }
        // Fallback case: Just start hosting your own server
        steamTransport.targetSteamId = defaultServerId;
        NetworkLog.LogError("Could connect to server");
        NetworkLog.LogWarning($"Trying to host a game instead on {steamTransport.targetSteamId}");
        HostGame();

    }
    public void LeaveGame()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.Shutdown();
            ActiveOutsideGameUI(true);
            NetworkLog.LogInfo($"Left Game");
        }
    }

    public void ActiveOutsideGameUI(bool isActive, ulong? serverId = null)
    {
        if (serverIDText) serverIDText.gameObject.SetActive(!isActive);
        foreach (GameObject ui in outsideGameUI)
        {
            ui.SetActive(isActive);
        }
        foreach (GameObject ui in insideGameUI)
        {
            ui.SetActive(!isActive);
        }
        serverIDText.text = serverId.HasValue ? $"Server: {serverId.Value}" : "No Server";
    }

    public void CopyServerId()
    {
        if (!steamTransport)
        {
            Debug.LogError("Steam Transport not found");
            return;
        }
        TextEditor textEditor = new()
        {
            text = (defaultServerId != 0 ? defaultServerId : steamTransport.ServerClientId).ToString()
        };
        textEditor.SelectAll();
        textEditor.Copy(); //Copy string from textEditor.text to Clipboard
    }

    public void PasteServerId()
    {
        if (!serverField)
        {
            Debug.LogError("No Input Field for inputting server ID found");
            return;
        }
        TextEditor textEditor = new()
        {
            multiline = true
        };
        textEditor.Paste(); //Copy string from Clipboard to textEditor.text
        serverField.text = textEditor.text;
    }
}
