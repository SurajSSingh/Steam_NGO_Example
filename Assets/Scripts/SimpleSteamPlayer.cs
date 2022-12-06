using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Steamworks;
using UnityEngine.InputSystem;

public class SimpleSteamPlayer : NetworkBehaviour
{
    private Vector2 movement;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] TMP_Text playerName;
    [SerializeField] float speed = 5;

    void Start()
    {
        // Find playerName and Rigidbody if not already assigned
        if (playerName == null)
        {
            playerName = GetComponentInChildren<TMP_Text>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        // Check if Steam Client is valid (already initialized and connected)
        if (!SteamClient.IsValid)
        {
            Debug.LogError("You are not connected to Steam!");
        }
        else
        {
            UpdateText();
        }
    }

    public void UpdateText()
    {
        playerName.text = SteamClient.Name;
    }

    private void FixedUpdate()
    {
        // Update if you are the owner
        if (!IsOwner) return;
        rb.AddForce(movement * speed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        movement = context.ReadValue<Vector2>();
    }

}
