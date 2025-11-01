using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private Transform hotbarSlotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Hotbar Settings")]
    [SerializeField] private int hotbarSize = 5;        // hardcoded hotbar size for now
    [SerializeField] private KeyCode[] hotbarKeys = new KeyCode[]  // keys to bind to hotbar slots
    { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };

    private Inventory inventory;
    private InventorySlotUI[] hotbarSlots;
    private int selectedSlotIndex = 0;

    // Initalize the hotbar
    private void Start()
    {
        // Get the inventory component
        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("InventoryUI could not find the inventory component!");
            return;
        }

        // Subscribe to inventory event for UI updates
        inventory.OnInventoryChanged += UpdateUI;

        // Initialize the hotbar slots
        InitializeHotbar();

        // Update the UI to reflect the current inventory state
        UpdateUI();
    }

    // Handle input updates
    private void Update()
    {
        // Hotbar selection
        for (int i = 0; i < hotbarKeys.Length; i++)
        {
            if (Input.GetKeyDown(hotbarKeys[i]))
            {
                SelectHotbarSlot(i);
            }
        }

        // Mouse scroll wheel for hotbar selection
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int newIndex = selectedSlotIndex + (scroll > 0 ? 1 : -1);
            // Wrap around: if negative, go to last slot; if beyond size, go to first slot
            if (newIndex < 0)
            {
                newIndex = hotbarSize - 1;
            }
            else if (newIndex >= hotbarSize)
            {
                newIndex = 0;
            }
            SelectHotbarSlot(newIndex);
        }
    }

    // Initialize the hotbar slots
    private void InitializeHotbar()
    {
        hotbarSlots = new InventorySlotUI[hotbarSize];
        for (int i = 0; i < hotbarSize; i++)
        {
            // Create the slot object
            GameObject slotObject = Instantiate(slotPrefab, hotbarSlotsContainer);
            InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                slotUI = slotObject.AddComponent<InventorySlotUI>();
            }
            slotUI.Initialize(i);
            hotbarSlots[i] = slotUI;
        }
        // Set the first slot as selected
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
            // Update the slot with the item data
            if (i < hotbarSlots.Length)
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
}