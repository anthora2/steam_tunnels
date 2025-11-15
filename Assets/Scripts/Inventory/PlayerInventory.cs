using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Server-authoritative inventory system.
/// Inventory state lives ONLY on the server.
/// Clients request changes via Commands, server validates and updates.
/// </summary>
public class PlayerInventory : NetworkBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSize = 5;

    // Server-only truth - the actual inventory list
    private readonly List<string> items = new List<string>();

    // Client-side cache of last received inventory (for late subscribers)
    private List<string> cachedInventory = new List<string>();

    // Event fired when inventory changes (only for local player)
    public event Action<List<string>> OnInventoryChanged;

    /// <summary>
    /// Called by client when attempting to pick up an item
    /// </summary>
    [Command]
    public void CmdRequestPickup(uint itemNetId)
    {
        Debug.Log($"[PlayerInventory] CmdRequestPickup called for netId {itemNetId} by {gameObject.name}");
        
        // Validate item exists
        if (!NetworkServer.spawned.TryGetValue(itemNetId, out NetworkIdentity identity))
        {
            Debug.LogWarning($"[PlayerInventory] Item with netId {itemNetId} not found in NetworkServer.spawned. Total spawned objects: {NetworkServer.spawned.Count}");
            return;
        }

        WorldItem worldItem = identity.GetComponent<WorldItem>();
        if (worldItem == null)
        {
            Debug.LogWarning($"[PlayerInventory] Object {identity.name} is not a WorldItem");
            return;
        }

        // Check if item can be picked up
        if (!worldItem.CanBePickedUp())
        {
            Debug.LogWarning($"[PlayerInventory] Item {worldItem.name} cannot be picked up");
            return;
        }

        // Optional: check distance
        float distance = Vector3.Distance(transform.position, worldItem.transform.position);
        if (distance > 5f)
        {
            Debug.LogWarning($"[PlayerInventory] Item too far away: {distance}m");
            return;
        }

        // Check inventory space
        if (items.Count >= maxSize)
        {
            Debug.LogWarning($"[PlayerInventory] Inventory full ({items.Count}/{maxSize})");
            return;
        }

        // Add to server inventory
        string itemId = worldItem.ItemId;
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogError($"[PlayerInventory] WorldItem {worldItem.name} has no itemId");
            return;
        }

        items.Add(itemId);
        Debug.Log($"[PlayerInventory] Added '{itemId}' to {gameObject.name}'s inventory ({items.Count}/{maxSize})");

        // Remove world item from server
        worldItem.OnPickedUp();

        // Update only THIS player's UI
        // Handle host mode correctly - host client needs special handling
        Debug.Log($"[PlayerInventory] Sending inventory update. connectionToClient: {connectionToClient?.connectionId}, isHost: {NetworkServer.activeHost}, localConnection: {NetworkServer.localConnection?.connectionId}");
        
        if (NetworkServer.activeHost && connectionToClient == NetworkServer.localConnection)
        {
            // Host mode - the connection should work, but add extra validation
            Debug.Log($"[PlayerInventory] Host mode: Sending inventory to local connection");
            TargetUpdateInventory(connectionToClient, items.ToArray());
        }
        else
        {
            // Remote client
            Debug.Log($"[PlayerInventory] Remote client: Sending inventory to connection {connectionToClient?.connectionId}");
            TargetUpdateInventory(connectionToClient, items.ToArray());
        }
    }

    /// <summary>
    /// Called by client to remove an item from inventory
    /// </summary>
    [Command]
    public void CmdRemoveItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.LogWarning($"[PlayerInventory] Invalid index {index}");
            return;
        }

        string itemId = items[index];
        items.RemoveAt(index);
        Debug.Log($"[PlayerInventory] Removed '{itemId}' from {gameObject.name}'s inventory");

        // Update UI
        Debug.Log($"[PlayerInventory] Sending inventory update after removal. connectionToClient: {connectionToClient?.connectionId}");
        TargetUpdateInventory(connectionToClient, items.ToArray());
    }

    /// <summary>
    /// Sends updated inventory to the local client UI
    /// </summary>
    [TargetRpc]
    void TargetUpdateInventory(NetworkConnection target, string[] newItems)
    {
        List<string> itemsList = new List<string>(newItems);
        
        // Cache the inventory on client
        cachedInventory = itemsList;
        
        Debug.Log($"[PlayerInventory] TargetUpdateInventory called with {itemsList.Count} items: {string.Join(", ", itemsList)}");
        
        int subscriberCount = OnInventoryChanged?.GetInvocationList().Length ?? 0;
        Debug.Log($"[PlayerInventory] OnInventoryChanged has {subscriberCount} subscribers");
        
        if (subscriberCount == 0)
        {
            Debug.LogWarning("[PlayerInventory] No subscribers to OnInventoryChanged! HotbarUI may not be connected yet. Inventory cached for later.");
        }
        
        OnInventoryChanged?.Invoke(itemsList);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (isLocalPlayer)
        {
            // Give HotbarUI time to subscribe, then send cached inventory if available
            Invoke(nameof(SendCachedInventory), 1f);
        }
    }

    /// <summary>
    /// Public method to resend cached inventory to subscribers (called by HotbarUI when it subscribes late)
    /// </summary>
    public void SendCachedInventory()
    {
        if (isLocalPlayer && cachedInventory.Count > 0)
        {
            Debug.Log($"[PlayerInventory] Sending cached inventory state with {cachedInventory.Count} items to late subscribers");
            OnInventoryChanged?.Invoke(new List<string>(cachedInventory));
        }
    }

    /// <summary>
    /// Get current inventory items (client-side, only for local player)
    /// </summary>
    public List<string> GetItems()
    {
        // This is a placeholder - actual items come from OnInventoryChanged event
        // In a real implementation, you'd cache the last received inventory
        return new List<string>();
    }

    /// <summary>
    /// Get the number of items in the inventory
    /// </summary>
    public int Count
    {
        get { return items.Count; }
    }
}

