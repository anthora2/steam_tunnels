using Mirror;
using UnityEngine;

/// <summary>
/// Represents an item in the world that can be picked up.
/// Server-authoritative: only server handles pickup logic.
/// </summary>
public class WorldItem : NetworkBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private Item item; // Reference to Item ScriptableObject
    
    [SyncVar(hook = nameof(OnPickedUpChanged))]
    private bool isPickedUp = false;

    public Item Item => item;
    public string ItemId => item != null ? item.itemName : "";

    public override void OnStartServer()
    {
        base.OnStartServer();
        EnsureCollider();
        Debug.Log($"[WorldItem] Server started for {gameObject.name} (netId: {netId}, item: {(item != null ? item.itemName : "null")})");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        EnsureCollider();
        Debug.Log($"[WorldItem] Client started for {gameObject.name} (netId: {netId}, item: {(item != null ? item.itemName : "null")})");
    }

    private void EnsureCollider()
    {
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        else if (!existingCollider.isTrigger)
        {
            existingCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// Called by server when item is picked up
    /// </summary>
    [Server]
    public void OnPickedUp()
    {
        isPickedUp = true;
        NetworkServer.Destroy(gameObject);
    }

    private void OnPickedUpChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            gameObject.SetActive(false);
        }
    }

    public bool CanBePickedUp() => !isPickedUp && item != null;
}

