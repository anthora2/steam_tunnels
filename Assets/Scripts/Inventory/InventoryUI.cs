using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Mirror;

/// <summary>
/// Simple client-side inventory UI that displays items
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text inventoryText;
    
    [Header("Settings")]
    [SerializeField] private bool showItemCount = true;

    private PlayerInventory playerInventory;
    private List<string> currentItems = new List<string>();

    private void Start()
    {
        // Try to find local player's inventory
        FindPlayerInventory();
    }

    private void FindPlayerInventory()
    {
        // Wait a frame for player to spawn
        Invoke(nameof(FindInventoryDelayed), 0.5f);
    }

    private void FindInventoryDelayed()
    {
        // Find local player
        NetworkIdentity[] allPlayers = FindObjectsOfType<NetworkIdentity>();
        foreach (NetworkIdentity player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                playerInventory = player.GetComponent<PlayerInventory>();
                if (playerInventory != null)
                {
                    playerInventory.OnInventoryChanged += UpdateInventory;
                    Debug.Log("[InventoryUI] Connected to player inventory");
                    return;
                }
            }
        }

        // Retry if not found
        if (playerInventory == null)
        {
            Debug.LogWarning("[InventoryUI] Could not find local player inventory, retrying...");
            Invoke(nameof(FindInventoryDelayed), 1f);
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateInventory;
        }
    }

    public void UpdateInventory(List<string> items)
    {
        currentItems = items;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (inventoryText == null) return;

        if (currentItems.Count == 0)
        {
            inventoryText.text = "Inventory: (Empty)";
            return;
        }

        inventoryText.text = "Inventory:\n";
        
        // Group items by name and count them
        var itemGroups = currentItems.GroupBy(item => item);
        
        foreach (var group in itemGroups)
        {
            if (showItemCount && group.Count() > 1)
            {
                inventoryText.text += $"- {group.Key} x{group.Count()}\n";
            }
            else
            {
                inventoryText.text += $"- {group.Key}\n";
            }
        }
    }
}

