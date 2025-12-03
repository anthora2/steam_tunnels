using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

// Serializable data structure for inventory items
// Only stores itemName string -> Mirror can't serialize ScriptableObject references
[System.Serializable]
public class InventoryItemData
{
    public string itemName;

    public InventoryItemData(string itemName)
    {
        this.itemName = itemName;
    }
}

// Server-authoritative inventory system
// Only the server can modify items -> clients are notified via TargetRpc when inventory changes
public class Inventory : NetworkBehaviour
{
    private const int MAX_SLOTS = 10;

    // Server-side storage: list of InventoryItemData
    private List<InventoryItemData> serverItems = new List<InventoryItemData>();

    // Client-side cache (updated via TargetRpc)
    private List<InventoryItemData> clientItems = new List<InventoryItemData>();

    // Event triggered when inventory changes (called on client after TargetRpc)
    public System.Action<List<InventoryItemData>> OnInventoryChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();
        serverItems = new List<InventoryItemData>();
    }

    // Server-only: Add an item to the inventory
    [Server]
    public void AddItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"[Inventory] Invalid item name: {itemName}");
            return;
        }

        // Check if inventory is full
        if (serverItems.Count >= MAX_SLOTS)
        {
            Debug.Log("[Inventory] Inventory is full");
            return;
        }

        // Add new item
        serverItems.Add(new InventoryItemData(itemName));
        NotifyClient();
        Debug.Log($"[Inventory] Server added {itemName} to inventory. Total items: {serverItems.Count}");
    }

    // Server-only: Remove an item from the inventory
    [Server]
    public void RemoveItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"[Inventory] Invalid item name: {itemName}");
            return;
        }

        int removed = serverItems.RemoveAll(item => item.itemName == itemName);
        if (removed > 0)
        {
            NotifyClient();
            Debug.Log($"[Inventory] Server removed {itemName} from inventory. Total items: {serverItems.Count}");
        }
    }

    private void NotifyClient()
    {
        NetworkIdentity netIdentity = GetComponent<NetworkIdentity>();
        if (netIdentity == null || netIdentity.connectionToClient == null)
        {
            Debug.LogError($"[Inventory] Cannot send TargetRpc: NetworkIdentity or connectionToClient is null!");
            return;
        }

        // Notify the client that owns this inventory
        TargetUpdateInventory(netIdentity.connectionToClient, SerializeInventory());

        // In host mode, also update client-side cache directly
        if (isClient && netIdentity.isLocalPlayer)
        {
            clientItems = DeserializeInventory(SerializeInventory());
            OnInventoryChanged?.Invoke(clientItems);
        }
    }

    // Get all items in the inventory (works on both server and client).
    public List<InventoryItemData> GetItems()
    {
        if (isServer)
            return new List<InventoryItemData>(serverItems);
        else
            return new List<InventoryItemData>(clientItems);
    }

    // Get a specific item by name (works on both server and client)
    public InventoryItemData GetItem(string itemName)
    {
        List<InventoryItemData> items = isServer ? serverItems : clientItems;
        return items.Find(item => item.itemName == itemName);
    }

    // Serialize inventory to string array for network transmission
    private string[] SerializeInventory()
    {
        return serverItems.Select(item => item.itemName).ToArray();
    }

    // Deserialize inventory from string array
    private List<InventoryItemData> DeserializeInventory(string[] serialized)
    {
        List<InventoryItemData> items = new List<InventoryItemData>();
        if (serialized == null || serialized.Length == 0)
            return items;

        foreach (string itemName in serialized)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                items.Add(new InventoryItemData(itemName));
            }
        }
        return items;
    }

    // TargetRpc: Notify the client when inventory changes
    // Only called on the client that owns this inventory.
    [TargetRpc]
    private void TargetUpdateInventory(NetworkConnection conn, string[] serializedInventory)
    {
        // Update client-side cache
        clientItems = DeserializeInventory(serializedInventory);

        // Trigger event so UI can update
        OnInventoryChanged?.Invoke(clientItems);
        Debug.Log($"[Inventory] Client received inventory update: {clientItems.Count} items");
    }
}