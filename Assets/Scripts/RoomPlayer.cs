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
    [SerializeField] Color notReadyColor = Color.magenta;
    [SerializeField] NetworkVariable<bool> isReady = new(false);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isReady.OnValueChanged += OnReadyChange;
        if (GetComponent<Button>() is Button btn)
        {
            btn.onClick.AddListener(ToggleIsReadyServerRpc);
        }
        background.color = notReadyColor;
        UpdateNameServerRPC($"{SteamClient.Name} (Not Ready)");
        Debug.Log("Room Player Created!");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (GetComponent<Button>() is Button btn)
        {
            btn.onClick.RemoveListener(ToggleIsReadyServerRpc);
        }
        isReady.OnValueChanged -= OnReadyChange;
    }

    private void OnReadyChange(bool previousIsReady, bool newIsReady)
    {
        background.color = newIsReady ? readyColor : notReadyColor;
        if (!IsOwner) return;
        string readyText = newIsReady ? "Ready" : "Not Ready";
        UpdateNameServerRPC($"{SteamClient.Name} ({readyText})");
    }

    [ServerRpc]
    public void ToggleIsReadyServerRpc()
    {
        isReady.Value = !isReady.Value;
    }

    [ServerRpc]
    public void UpdateNameServerRPC(string updatedName)
    {
        UpdateNameClientRpc(updatedName);
    }

    [ClientRpc]
    public void UpdateNameClientRpc(string updatedName)
    {
        playerName.text = updatedName;
    }
}
