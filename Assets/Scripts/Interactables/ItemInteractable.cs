using UnityEngine;

// Interactable for picking up items
public class ItemInteractable : Interactable
{
    [SerializeField] private Item item;
    [SerializeField] private bool destroyOnPickup = true;

    public override bool CanInteract() => true;

    public override bool Interact()
    {
        // Get inventory from the InteractionDetector's GameObject (player)
        Inventory inventory = null;
        if (InteractionDetector.Instance != null)
        {
            inventory = InteractionDetector.Instance.GetComponent<Inventory>();
            if (inventory == null)
            {
                inventory = InteractionDetector.Instance.GetComponentInParent<Inventory>();
            }
        }

        if (inventory == null)
        {
            Debug.LogWarning("Could not find inventory component on player!");
            return false;
        }

        if (item == null)
        {
            Debug.LogWarning($"ItemInteractable on {gameObject.name} has no item assigned!");
            return false;
        }

        bool added = inventory.AddItem(item, 1);

        if (added)
        {
            Debug.Log("Picked up " + item.itemName);
            if (destroyOnPickup)
            {
                Debug.Log("Destroying game object");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Inventory full or too heavy!");
        }
        return added;
    }

    public override string GetInteractText() => $"Pick up {(item != null ? item.itemName : "item")}";
}