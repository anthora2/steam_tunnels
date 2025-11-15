using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Mirror;

/// <summary>
/// Hotbar UI that displays and manages the player's inventory items
/// </summary>
public class HotbarUI : MonoBehaviour
{
    [Header("Inventory Reference")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("UI References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private Transform hotbarSlotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Hotbar Settings")]
    [SerializeField] private int hotbarSize = 5;
    [SerializeField] private KeyCode[] hotbarKeys = new KeyCode[] 
    { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    private HotbarSlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;
    private List<string> currentItems = new List<string>();
    
    // Item database to resolve item names to Item ScriptableObjects
    private static Dictionary<string, Item> itemDatabase;
    private static bool databaseInitialized = false;

    private void Start()
    {
        InitializeItemDatabase();
        ConnectToInventory();
        InitializeHotbar();
    }

    private void ConnectToInventory()
    {
        // If inventory is already assigned in inspector, use it
        if (playerInventory != null)
        {
            SubscribeToInventory();
            return;
        }

        // Otherwise, try to find it (fallback for runtime assignment)
        TryFindInventory();
        InvokeRepeating(nameof(TryFindInventory), 0.5f, 1f);
    }

    private void SubscribeToInventory()
    {
        if (playerInventory == null) return;

        playerInventory.OnInventoryChanged += OnInventoryChanged;
        Debug.Log("[HotbarUI] Subscribed to player inventory OnInventoryChanged event");
        
        // Request current inventory state if available
        playerInventory.SendCachedInventory();
    }

    private void TryFindInventory()
    {
        if (playerInventory != null) 
        {
            // Already found, cancel repeating invoke and subscribe
            CancelInvoke(nameof(TryFindInventory));
            SubscribeToInventory();
            return;
        }

        // Find local player's inventory
        NetworkIdentity[] allPlayers = FindObjectsOfType<NetworkIdentity>();
        foreach (NetworkIdentity player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                PlayerInventory inv = player.GetComponent<PlayerInventory>();
                if (inv != null)
                {
                    playerInventory = inv;
                    CancelInvoke(nameof(TryFindInventory));
                    SubscribeToInventory();
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    private void InitializeItemDatabase()
    {
        if (databaseInitialized) return;

        itemDatabase = new Dictionary<string, Item>();
        Item[] allItems = Resources.LoadAll<Item>("Items");

        foreach (Item item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                itemDatabase[item.itemName] = item;
            }
        }

        databaseInitialized = true;
    }

    private Item GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        itemDatabase.TryGetValue(itemName, out Item item);
        return item;
    }

    private void OnInventoryChanged(List<string> items)
    {
        Debug.Log($"[HotbarUI] OnInventoryChanged called with {items.Count} items: {string.Join(", ", items)}");
        currentItems = items;
        UpdateUI();
    }

    private void Update()
    {
        if (playerInventory == null) return;

        // Hotbar selection
        for (int i = 0; i < hotbarKeys.Length && i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i])) 
            {
                SelectHotbarSlot(i);
            }
        }

        // Use item in selected hotbar slot
        if (Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            UseSelectedItem();
        }
    }

    private void InitializeHotbar()
    {
        if (hotbarSlotsContainer == null || slotPrefab == null)
        {
            Debug.LogError("[HotbarUI] Hotbar slots container or slot prefab not assigned!");
            return;
        }

        hotbarSlots = new HotbarSlotUI[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, hotbarSlotsContainer);
            HotbarSlotUI slotUI = slotObject.GetComponent<HotbarSlotUI>();
            if (slotUI == null)
            {
                slotUI = slotObject.AddComponent<HotbarSlotUI>();
            }
            slotUI.Initialize(i);
            hotbarSlots[i] = slotUI;
        }
        SelectHotbarSlot(0);
    }

    private void UpdateUI()
    {
        if (hotbarSlots == null)
        {
            Debug.LogWarning("[HotbarUI] Hotbar slots not initialized!");
            return;
        }

        Debug.Log($"[HotbarUI] UpdateUI called with {currentItems.Count} items");

        // Update all hotbar slots
        for (int i = 0; i < hotbarSize; i++)
        {
            if (i < currentItems.Count)
            {
                string itemName = currentItems[i];
                Item item = GetItemByName(itemName);
                if (item != null)
                {
                    Debug.Log($"[HotbarUI] Updating slot {i} with item '{item.itemName}' (sprite: {(item.itemSprite != null ? item.itemSprite.name : "null")})");
                    hotbarSlots[i].UpdateSlot(new InventorySlot(item));
                }
                else
                {
                    Debug.LogWarning($"[HotbarUI] Could not find item '{itemName}' in database for slot {i}");
                    hotbarSlots[i].ClearSlot();
                }
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
            
            // Update selection state
            hotbarSlots[i].SetSelected(i == selectedSlotIndex);
        }
    }

    private void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        selectedSlotIndex = index;
        UpdateUI();
    }

    private void UseSelectedItem()
    {
        // Add validation before checking
        if (currentItems == null || currentItems.Count == 0)
        {
            Debug.Log("[HotbarUI] No items in inventory");
            return;
        }
        
        if (selectedSlotIndex < 0 || selectedSlotIndex >= currentItems.Count)
        {
            Debug.Log($"[HotbarUI] No item in selected slot {selectedSlotIndex} (inventory has {currentItems.Count} items)");
            return;
        }

        string itemName = currentItems[selectedSlotIndex];
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.Log($"[HotbarUI] Selected slot {selectedSlotIndex} has empty item name");
            return;
        }
        
        Item item = GetItemByName(itemName);
        
        if (item != null)
        {
            Debug.Log($"[HotbarUI] Using item '{item.itemName}' from hotbar slot {selectedSlotIndex + 1}");
            item.Use();
        }
        else
        {
            Debug.LogWarning($"[HotbarUI] Could not find item '{itemName}' in database");
        }
    }

    public InventorySlot GetSelectedHotbarSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < currentItems.Count)
        {
            string itemName = currentItems[selectedSlotIndex];
            Item item = GetItemByName(itemName);
            if (item != null)
            {
                return new InventorySlot(item);
            }
        }
        return null;
    }

    public Item GetSelectedHotbarItem()
    {
        InventorySlot slot = GetSelectedHotbarSlot();
        return slot != null ? slot.item : null;
    }
}
