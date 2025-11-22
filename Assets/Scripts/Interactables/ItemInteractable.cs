using UnityEngine;
using Mirror;

public class ItemInteractable : NetworkBehaviour, IInteractable
{
    public Item item;

    public bool CanInteract(PlayerController player) => true;

    public string GetInteractPrompt() => "Pick up " + item.itemName;

    // This is called by the PlayerController on the server
    public void Interact(PlayerController player)
    {
        if (!isServer) return;
        if (!CanInteract(player)) return;
        
        // Add item to player's inventory before destroying
        if (player.inventory != null && item != null)
        {
            player.inventory.AddItem(item.itemName);
        }
        
        NetworkServer.Destroy(gameObject);
    }
}
