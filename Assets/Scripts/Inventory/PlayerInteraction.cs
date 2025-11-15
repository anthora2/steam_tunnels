using Mirror;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles player interaction with world items (pickup via trigger colliders)
/// </summary>
public class PlayerInteraction : NetworkBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private PlayerInventory inventory;
    private List<WorldItem> itemsInRange = new List<WorldItem>();
    private WorldItem currentItem = null;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("[PlayerInteraction] PlayerInventory component not found!");
        }
        
        // Ensure player has a trigger collider for detection
        SetupTriggerCollider();
    }

    private void SetupTriggerCollider()
    {
        // Check if we already have a trigger collider
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider != null && existingCollider.isTrigger)
        {
            return; // Already set up
        }

        // Add a sphere trigger collider
        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<SphereCollider>();
        }
        
        trigger.isTrigger = true;
        trigger.radius = 2f; // Pickup range
        trigger.center = Vector3.zero;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (inventory == null) return;

        // Clean up null references
        itemsInRange.RemoveAll(item => item == null || !item.CanBePickedUp());

        // Update current item (closest one)
        UpdateCurrentItem();

        // Handle pickup input
        if (Input.GetKeyDown(interactKey) && currentItem != null)
        {
            PickupItem(currentItem);
        }
    }

    private void UpdateCurrentItem()
    {
        if (itemsInRange.Count == 0)
        {
            currentItem = null;
            return;
        }

        // Find closest item
        Vector3 playerPos = transform.position;
        WorldItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (WorldItem item in itemsInRange)
        {
            if (item == null || !item.CanBePickedUp()) continue;

            float distance = Vector3.Distance(playerPos, item.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = item;
            }
        }

        currentItem = closest;
    }

    private void PickupItem(WorldItem worldItem)
    {
        if (worldItem == null || !worldItem.CanBePickedUp())
        {
            Debug.LogWarning("[PlayerInteraction] Cannot pick up item");
            return;
        }

        Debug.Log($"[PlayerInteraction] Attempting to pick up {worldItem.name} (netId: {worldItem.netId})");
        inventory.CmdRequestPickup(worldItem.netId);
    }

    // Trigger-based detection
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        WorldItem worldItem = other.GetComponent<WorldItem>();
        if (worldItem != null && worldItem.CanBePickedUp())
        {
            if (!itemsInRange.Contains(worldItem))
            {
                itemsInRange.Add(worldItem);
                Debug.Log($"[PlayerInteraction] Item {worldItem.name} entered range");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer) return;

        WorldItem worldItem = other.GetComponent<WorldItem>();
        if (worldItem != null)
        {
            itemsInRange.Remove(worldItem);
            Debug.Log($"[PlayerInteraction] Item {worldItem.name} exited range");
            
            if (currentItem == worldItem)
            {
                currentItem = null;
            }
        }
    }
}
