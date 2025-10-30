using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public int TotalWeight => item.Weight * quantity;

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public bool IsFull => quantity >= item.MaxStackSize;
    public bool IsEmpty => quantity <= 0;
}

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 36;  // hardcoded inventory size for now
    public int maxWeight = 100;     // hardcoded max weight for now

    [Header("Current State")]
    public List<InventorySlot> items = new List<InventorySlot>();
    public int currentWeight = 0;

    // Attempts to add an item to the inventory. Returns true if all items were added successfully.
    public bool AddItem(Item item, int quantity)
    {
        if (item == null || quantity <= 0)
            return false;

        // Determine effective stack size (non-stackable items count as 1 per slot, and guard against 0)
        int effectiveMaxStack = item.IsStackable ? Mathf.Max(1, item.MaxStackSize) : 1;

        int remaining = quantity;

        // Try to add to existing inventory slots first
        foreach (var slot in items)
        {
            if (slot.item == item && item.IsStackable && slot.quantity < effectiveMaxStack)
            {
                int space = effectiveMaxStack - slot.quantity;
                int toAdd = Mathf.Min(space, remaining);

                // Check weight before adding
                int additionalWeight = item.Weight * toAdd;
                if (currentWeight + additionalWeight > maxWeight)
                    return false;

                slot.quantity += toAdd;
                remaining -= toAdd;
                currentWeight += additionalWeight;

                if (remaining <= 0)
                    return true;
            }
        }

        // Add new slots if space allows
        while (remaining > 0 && items.Count < inventorySize)
        {
            int toAdd = Mathf.Min(effectiveMaxStack, remaining);
            if (toAdd <= 0)
                break;

            int additionalWeight = item.Weight * toAdd;
            if (currentWeight + additionalWeight > maxWeight)
                return false;

            items.Add(new InventorySlot(item, toAdd));
            remaining -= toAdd;
            currentWeight += additionalWeight;
        }

        // Return whether all items fit
        return remaining <= 0;
    }

    // Removes an item from the inventory and updates total weight.
    public void RemoveItem(Item item, int quantity)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var slot = items[i];
            if (slot.item == item)
            {
                int toRemove = Mathf.Min(quantity, slot.quantity);

                slot.quantity -= toRemove;
                currentWeight -= item.Weight * toRemove;
                quantity -= toRemove;

                if (slot.IsEmpty)
                {
                    items.RemoveAt(i);
                    i--;
                }

                if (quantity <= 0)
                    return;
            }
        }
    }

    // Returns the remaining weight capacity.
    public int RemainingWeightCapacity()
    {
        return Mathf.Max(0, maxWeight - currentWeight);
    }

    // Returns the number of empty slots available.
    public int EmptySlots()
    {
        return inventorySize - items.Count;
    }
}
