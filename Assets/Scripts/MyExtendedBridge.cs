using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MyExtendedBridge : NGOSteamBridge
{
    [SerializeField] TMP_Text serverIDText;
    public List<GameObject> outsideGameUI = new();
    public List<GameObject> insideGameUI = new();

    private static void ShowLogValue(string message, string extra = "")
    {
        ShowLogValue(nameof(MyExtendedBridge), message, extra);
    }

    void Start()
    {
        ActiveOutsideGameUI(true);
        ShowLogValue("Initialized");
    }

    protected override void OnLobbyEnteredCallback(Lobby lobby)
    {
        base.OnLobbyEnteredCallback(lobby);
        ActiveOutsideGameUI(false);
        if (serverIDText) serverIDText.text = $"Server ID: {lobby.Id.Value}";


    }

    public override void CreateLobby(bool isFriendOnly)
    {

    }

    public override void OnClientLeave(ulong clientID)
    {
        base.OnClientLeave(clientID);
        ActiveOutsideGameUI(true);
    }

    public void ActiveOutsideGameUI(bool isActive)
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
    }
}
