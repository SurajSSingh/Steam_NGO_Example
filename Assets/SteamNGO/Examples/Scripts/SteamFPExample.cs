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
    [SerializeField] TMP_Text userInfoText;
    [SerializeField] TMP_Text friendText;

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
            usernameText.text = $"Username: {SteamClient.Name} [ID={SteamClient.SteamId}]";
        }
        if (userInfoText)
        {
            userInfoText.text = $@"
            Logged-In: {SteamClient.IsLoggedOn}
            Valid User: {SteamClient.IsValid}
            Current State: {SteamClient.State}
            Current Level: {SteamUser.SteamLevel}

            ";
        }
        if (friendText)
        {
            friendText.text = "List of Friends:\n";
            foreach (var friend in SteamFriends.GetFriends())
            {
                friendText.text += $"{friend.Name} [{friend.Id}] - {friend.State} - Level: {friend.SteamLevel}\n";
                Debug.Log(friend.GameInfo.HasValue ? friend.GameInfo.Value : "none");
            }

        }
    }
}
