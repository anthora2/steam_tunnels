using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{

    [Header("Inventory Reference")]
    [SerializeField] private Inventory inventory;

    [Header("UI References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private Transform hotbarSlotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Hotbar Settings")]
    [SerializeField] private int hotbarSize = 5;        // hardcoded hotbar size for now
    [SerializeField] private KeyCode[] hotbarKeys = new KeyCode[]  // keys to bind to hotbar slots
    { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    private HotbarSlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;

    private void Start()
    {
        if (inventory == null)
        {
            Debug.LogError("HotbarUI could not find the inventory component! Please assign it in the inspector.");
            return;
        }

        // Subscribe to inventory event for UI updates
        inventory.OnInventoryChanged += UpdateUI;
        InitializeHotbar();
        UpdateUI();
    }

    // Unsubscribe from inventory event on destroy
    private void OnDestroy()
    {
        if (inventory != null) inventory.OnInventoryChanged -= UpdateUI;
    }

    private void Update()
    {
        // Hotbar selection
        for (int i = 0; i < hotbarKeys.Length; i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i])) SelectHotbarSlot(i);
        }

        // Use item in selected hotbar slot
        if (Input.GetKeyDown(KeyCode.E)) UseSelectedItem();
    }

    // Initialize the hotbar slots
    private void InitializeHotbar()
    {
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

    // Update the UI to reflect the current inventory state
    private void UpdateUI()
    {
        if (hotbarSlots == null)
        {
            Debug.LogError("Hotbar slots are not initialized!");
            return;
        }
        if (inventory == null)
        {
            Debug.LogError("Inventory is not initialized!");
            return;
        }
        // Update the hotbar slots - iterate through ALL slots
        for (int i = 0; i < hotbarSize; i++)
        {
            // Update the slot with the item data (or null if slot is empty)
            // NOTE: Hotbar slots and inventory slots are not the same thing, so we need to add a check to make sure the slot is not null
            // Consider refactoring this to decouple inventory slots from hotbar slots
            if (i < inventory.items.Count)
            {
                hotbarSlots[i].UpdateSlot(inventory.items[i]);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
            // Update selection state for all slots
            hotbarSlots[i].SetSelected(i == selectedSlotIndex);
        }
    }

    // Select the hotbar slot at the given index
    private void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= hotbarSize)
        {
            Debug.LogError("Invalid hotbar slot index!");
            return;
        }
        selectedSlotIndex = index;
        UpdateUI();
    }

    // Get selected hotbar slot
    public InventorySlot GetSelectedHotbarSlot()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSize)
        {
            return hotbarSlots[selectedSlotIndex].GetSlot();
        }
        return null;
    }

    // Get selected hotbar slot item
    public Item GetSelectedHotbarItem()
    {
        InventorySlot slot = GetSelectedHotbarSlot();
        return slot != null ? slot.item : null;
    }

    // Use the item in the currently selected hotbar slot
    private void UseSelectedItem()
    {
        InventorySlot slot = GetSelectedHotbarSlot();
        if (slot != null && slot.item != null && !slot.IsEmpty)
        {
            Debug.Log($"Using item '{slot.item.itemName}' from hotbar slot {selectedSlotIndex + 1}");
            slot.item.Use();
        }
        else Debug.Log($"No item in hotbar slot {selectedSlotIndex + 1} to use.");
    }
}