using UnityEngine;
using TMPro;
using Steamworks;
using System;

public class SteamFPExample : MonoBehaviour
{
    // NOTE: Default app id of 480 (Spacewars) is provided by Steam for testing only.
    [SerializeField] uint app_id = 480;

    [Header("UI Objects")]
    [SerializeField] TMP_Text usernameText;

    void Start()
    {
        TrySteamInit();
        UpdateInfo();
    }

    void OnDestroy()
    {
        SteamClient.Shutdown();
        Debug.LogWarning($"Steam has been shutdown from {nameof(SteamFPExample)}");
    }

    private void TrySteamInit()
    {
        try
        {
            SteamClient.Init(app_id);
            Debug.Log("Successfully connected to Steam!");
        }
        catch (Exception e)
        {
            Debug.LogError($"From {nameof(SteamFPExample)} => could not initialize Steam, exception raised: {e}");
        }
    }

    private void UpdateInfo()
    {
        // Early return if Steam client not valid or not logged in
        if (!SteamClient.IsValid || !SteamClient.IsLoggedOn)
        {
            Debug.LogError("Steam has not been initialized or connected!");
            return;
        }
        if (usernameText)
        {
            usernameText.text = $"Username: {SteamClient.Name} [id={SteamClient.SteamId}]";
        }
    }
}
