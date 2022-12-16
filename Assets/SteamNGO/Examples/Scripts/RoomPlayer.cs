using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Steamworks;

public class RoomPlayer : NetworkBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] Image background;
    [SerializeField] Color readyColor = Color.green;
    [SerializeField] Color notReadyColor = Color.red;
    [SerializeField] NetworkVariable<bool> isReady = new(false);

    private Button readyButton;
    private RoomManager roomManager;

    private void DeveloperLog(string msg)
    {
        if (NetworkManager.LogLevel <= LogLevel.Developer) Debug.Log($"[{nameof(RoomPlayer)}] {msg}");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isReady.OnValueChanged += OnReadyChange;
        StartCoroutine(RefreshRoomManager());
        DeveloperLog($"Room Player for Client {OwnerClientId} Created!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void NewButtonServerRpc()
    {
        DeveloperLog("Tell everyone to create a new button");
        // Server tells everyone to create a button for this room player
        NewButtonClientRpc();
    }

    [ClientRpc]
    public void NewButtonClientRpc()
    {
        if (readyButton)
        {
            DeveloperLog($"Destory old button for client {this.OwnerClientId}");
            Destroy(readyButton.gameObject);
            readyButton = null;
        }
        var newButtonGo = Instantiate(roomManager.ButtonPrefab, roomManager.PlayersInRoomContent);
        if (newButtonGo.GetComponent<Button>() is Button btn)
        {
            readyButton = btn;
            background = btn.GetComponent<Image>();
            playerName = btn.GetComponentInChildren<TMP_Text>();
            UpdateButton();
        }
        else
        {
            if (NetworkManager.LogLevel <= LogLevel.Error) Debug.LogError("Button could not be created, prefab has no button component!");
            Destroy(newButtonGo);
        }
        DeveloperLog($"Add listener for client {this.OwnerClientId}");
        readyButton.onClick.AddListener(ToggleIsReady);
        Debug.LogWarning($"Button listeners: {readyButton.onClick.GetPersistentEventCount()}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveButtonServerRpc()
    {
        DeveloperLog("Tell everyone to delete the ready button");
        RemoveButtonClientRpc();
    }
    [ClientRpc]
    public void RemoveButtonClientRpc()
    {
        if (readyButton)
        {
            DeveloperLog($"Remove button from client {this.OwnerClientId}");
            readyButton.onClick.RemoveListener(ToggleIsReadyServerRpc);
            Destroy(readyButton.gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isReady.OnValueChanged -= OnReadyChange;
        RemoveButtonServerRpc();
    }

    private void OnReadyChange(bool previousIsReady, bool newIsReady)
    {
        if (!IsOwner) return;
        DeveloperLog($"Client {OwnerClientId}'s is ready has changed from {previousIsReady} to {newIsReady}!");
        UpdateButton();
    }

    private void UpdateButton()
    {
        string readyText = isReady.Value ? "Ready" : "Not Ready";
        DeveloperLog($"Updating button with isReady = {isReady.Value}");
        string steamName = SteamClient.IsValid ? SteamClient.Name : $"Player {OwnerClientId + 1}";
        UpdateButtonServerRpc($"{steamName} ({readyText})", isReady.Value);
    }

    public void ToggleIsReady()
    {
        DeveloperLog($"Client {OwnerClientId}'s button was clicked, is owner? {IsOwner}!");
        if (!IsOwner) return;
        ToggleIsReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleIsReadyServerRpc()
    {
        DeveloperLog($"Client {OwnerClientId} toggled is ready!");
        isReady.Value = !isReady.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateButtonServerRpc(string updatedName, bool newIsReady)
    {
        DeveloperLog($"Updating button with new name = {updatedName}");
        UpdateNameClientRpc(updatedName, newIsReady);
    }

    [ClientRpc]
    public void UpdateNameClientRpc(string updatedName, bool newIsReady)
    {
        background.color = newIsReady ? readyColor : notReadyColor;
        playerName.text = updatedName;
    }
    private System.Collections.IEnumerator RefreshRoomManager()
    {
        roomManager = FindObjectOfType<RoomManager>();
        while (roomManager == null)
        {
            yield return new WaitForSeconds(0.2f);
            roomManager = FindObjectOfType<RoomManager>();
        }
        DeveloperLog($"Check if room manager is spawned: {roomManager.IsSpawned}");
        while (!roomManager.IsSpawned)
        {
            yield return new WaitForSeconds(0.2f);
        }
        NewButtonServerRpc();
    }
}
