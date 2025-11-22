using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;

/*
Hotbar UI component that displays the first few items from the local player's inventory.
Uses ItemDatabase to look up item icons and metadata.
Only displays for the local player.
*/
public class HotbarUI : MonoBehaviour
{
    private const int HOTBAR_SIZE = 5;

    [Header("UI References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private Transform hotbarSlotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Hotbar Settings")]
    [SerializeField] private KeyCode[] hotbarKeys = new KeyCode[] 
    { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    private HotbarSlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;
    private List<InventoryItemData> currentItems = new List<InventoryItemData>();
    
    private void Start()
    {
        // Initialize ItemDatabase if assigned
        if (itemDatabase != null)
        {
            itemDatabase.Initialize();
        }
        else
        {
            Debug.LogError("[HotbarUI] ItemDatabase not assigned!");
        }

        InitializeHotbar();
        
        // Subscribe to inventory changes if inventory is already assigned
        // Otherwise, UIManager will subscribe after binding
        if (inventory != null)
        {
            inventory.OnInventoryChanged += OnInventoryChanged;
            Debug.Log("[HotbarUI] Subscribed to inventory changes in Start()");
        }
    }

    public void BindInventory(Inventory inventory)
    {
        this.inventory = inventory;
        inventory.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from inventory changes
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    private void Update()
    {
        // Check if this is the local player's inventory by checking the parent NetworkIdentity
        if (inventory != null)
        {
            NetworkIdentity netIdentity = inventory.GetComponent<NetworkIdentity>();
            if (netIdentity != null && !netIdentity.isLocalPlayer) return;
        }

        // Hotbar selection
        for (int i = 0; i < hotbarKeys.Length && i < HOTBAR_SIZE; i++)
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

    // Called when inventory changes (via TargetRpc from server).
    // Updates the hotbar UI using ItemDatabase to get item icons.
    public void OnInventoryChanged(List<InventoryItemData> items)
    {
        if (inventory != null)
        {
            NetworkIdentity netIdentity = inventory.GetComponent<NetworkIdentity>();
            if (netIdentity == null || !netIdentity.isLocalPlayer)
            {
                Debug.Log("[HotbarUI] Ignoring inventory update - not local player's inventory");
                return;
            }
        }
        
        Debug.Log($"[HotbarUI] OnInventoryChanged called with {items.Count} items for local player");
        currentItems = items;
        UpdateUI(items);
    }

    private void InitializeHotbar()
    {
        if (hotbarSlotsContainer == null || slotPrefab == null)
        {
            Debug.LogError("[HotbarUI] Hotbar slots container or slot prefab not assigned!");
            return;
        }

        hotbarSlots = new HotbarSlotUI[HOTBAR_SIZE];
        for (int i = 0; i < HOTBAR_SIZE; i++)
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

    // Update the hotbar UI with current inventory items
    // Uses ItemDatabase to look up item data
    private void UpdateUI(List<InventoryItemData> items)
    {
        if (hotbarSlots == null)
        {
            Debug.LogWarning("[HotbarUI] Hotbar slots not initialized!");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("[HotbarUI] ItemDatabase not assigned!");
            return;
        }

        // Update all hotbar slots
        for (int i = 0; i < HOTBAR_SIZE; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                InventoryItemData itemData = items[i];
                
                // Look up the Item ScriptableObject from ItemDatabase
                Item item = itemDatabase.GetItem(itemData.itemName);
                
                if (item != null)
                {
                    InventorySlot slot = new InventorySlot(item);
                    hotbarSlots[i].UpdateSlot(slot);
                }
                else
                {
                    Debug.LogWarning($"[HotbarUI] Item '{itemData.itemName}' not found in ItemDatabase!");
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
        
        // This is especially important when multiple clients are present
        Canvas.ForceUpdateCanvases();
        
        // Also force layout rebuild if using layout groups
        if (hotbarSlotsContainer != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(hotbarSlotsContainer as RectTransform);
        }
    }

    private void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_SIZE) return;
        selectedSlotIndex = index;
        UpdateUI(currentItems);
    }

    private void UseSelectedItem()
    {
        if (currentItems == null || currentItems.Count == 0)
        {
            Debug.Log("[HotbarUI] No items in inventory");
            return;
        }
        
        if (selectedSlotIndex < 0 || selectedSlotIndex >= currentItems.Count)
        {
            Debug.Log($"[HotbarUI] No item in selected slot {selectedSlotIndex}");
            return;
        }

        InventoryItemData itemData = currentItems[selectedSlotIndex];
        if (itemData != null && itemDatabase != null)
        {
            Item item = itemDatabase.GetItem(itemData.itemName);
            if (item != null)
            {
                Debug.Log($"[HotbarUI] Using item '{item.itemName}' from hotbar slot {selectedSlotIndex + 1}");
                item.Use();
            }
        }
    }

    public InventorySlot GetSelectedHotbarSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < currentItems.Count && currentItems[selectedSlotIndex] != null)
        {
            InventoryItemData itemData = currentItems[selectedSlotIndex];
            if (itemDatabase != null)
            {
                Item item = itemDatabase.GetItem(itemData.itemName);
                if (item != null)
                {
                    return new InventorySlot(item);
                }
            }
        }
        return null;
    }

    public Item GetSelectedHotbarItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < currentItems.Count && currentItems[selectedSlotIndex] != null)
        {
            InventoryItemData itemData = currentItems[selectedSlotIndex];
            if (itemDatabase != null)
            {
                return itemDatabase.GetItem(itemData.itemName);
            }
        }
        return null;
    }
}

