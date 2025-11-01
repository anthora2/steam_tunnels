using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* UI Component for displaying a single inventory slot */
public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject selectedIndicator;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Sprite emptySlotSprite;

    private InventorySlot slot;
    private int slotIndex;
    private bool isSelected = false;

    // Initialize the slot with the given index and clear it
    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }

    // Refresh the UI to reflect the current slot state
    private void RefreshUI()
    {
        if (slot == null || slot.IsEmpty)
        {
            // Update UI to show empty slot
            if (itemImage != null)
            {
                if (emptySlotSprite != null)
                {
                    itemImage.sprite = emptySlotSprite;
                }
                // Optionally hide the image if empty slot sprite is not set
                itemImage.enabled = emptySlotSprite != null;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
            }
        }
        else 
        {
            // Update UI to show item
            if (itemImage != null)
            {
                if (slot.item != null && slot.item.itemSprite != null)
                {
                    itemImage.sprite = slot.item.itemSprite;
                    itemImage.enabled = true;
                }
                else
                {
                    itemImage.enabled = false;
                }
            }

            if (quantityText != null)
            {
                if (slot.quantity > 1 || slot.item.stackable)
                {
                    quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                }
                else
                {
                    quantityText.text = "";
                }
            }
        }

        // Preserve the selection state by updating background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    // Update slot with new slot data
    public void UpdateSlot(InventorySlot newSlot)
    {
        slot = newSlot;
        RefreshUI();
    }

    // Clear slot and reset UI
    public void ClearSlot()
    {
        slot = null;
        RefreshUI();
    }

    // Set the selected state of the slot
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    // Get slot
    public InventorySlot GetSlot()
    {
        return slot;
    }

    // Get slot index
    public int GetSlotIndex()
    {
        return slotIndex;
    }

}