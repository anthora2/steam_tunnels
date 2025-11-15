using UnityEngine;

/// <summary>
/// Simple container for an item in the inventory (used by UI)
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public Item item;

    public InventorySlot(Item item)
    {
        this.item = item;
    }

    public bool IsEmpty => item == null;
}

