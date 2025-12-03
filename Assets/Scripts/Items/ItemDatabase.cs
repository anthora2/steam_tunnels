using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("All Items")]
    [SerializeField] private Item[] items;

    // Dictionary for fast lookup by name
    private Dictionary<string, Item> itemLookup;

    public void Initialize()
    {
        if (itemLookup != null) return; // Already initialized

        itemLookup = new Dictionary<string, Item>();
        if (items != null)
        {
            foreach (Item item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemName))
                {
                    itemLookup[item.itemName] = item;
                }
            }
        }
    }

    public Item GetItem(string itemName)
    {
        if (itemLookup == null)
        {
            Initialize();
        }

        if (string.IsNullOrEmpty(itemName))
            return null;

        itemLookup.TryGetValue(itemName, out Item item);
        return item;
    }

    public Item[] GetAllItems()
    {
        return items;
    }
}