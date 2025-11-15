using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* UI Component for displaying a single hotbar slot */
public class HotbarSlotUI : MonoBehaviour
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
    public bool isSelected = false;

    // Initialize the hotbar slot with the given index and clear it
    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }

    // Refresh the UI to reflect the current hotbar slot state
    private void RefreshUI()
    {
        if (slot == null || slot.IsEmpty)
        {
            // Update UI to show empty hotbar slot
            if (itemImage != null)
            {
                if (emptySlotSprite != null)
                {
                    itemImage.sprite = emptySlotSprite;
                    itemImage.enabled = true;
                }
                else
                {
                    itemImage.enabled = false;
                }
            }

            if (quantityText != null)
            {
                quantityText.text = "";
            }
        }
        else 
        {
            // Update UI to show item in hotbar slot
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
                quantityText.text = "";
            }
        }

        // Preserve the selection state by updating background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    // Update hotbar slot with new slot data
    public void UpdateSlot(InventorySlot newSlot)
    {
        slot = newSlot;
        RefreshUI();
    }

    // Clear hotbar slot and reset UI
    public void ClearSlot()
    {
        slot = null;
        RefreshUI();
    }

    // Set the selected state of the hotbar slot
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

    // Get hotbar slot
    public InventorySlot GetSlot()
    {
        return slot;
    }

    // Get hotbar slot index
    public int GetSlotIndex()
    {
        return slotIndex;
    }

}