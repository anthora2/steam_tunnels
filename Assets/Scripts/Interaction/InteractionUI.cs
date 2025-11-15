using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// Shows interaction prompt when player is near items
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject container;
    [SerializeField] public TMP_Text promptText;
    
    [Header("Settings")]
    [SerializeField] private float checkRange = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float checkInterval = 0.2f;

    private float lastCheck = 0f;
    private WorldItem nearbyItem = null;

    private void Update()
    {
        // Only check periodically to avoid performance issues
        if (Time.time - lastCheck < checkInterval) return;
        lastCheck = Time.time;

        // Find local player
        NetworkIdentity localPlayer = FindLocalPlayer();
        if (localPlayer == null)
        {
            if (container != null)
                container.SetActive(false);
            return;
        }

        // Check for nearby items
        nearbyItem = FindNearbyItem(localPlayer.transform);

        if (nearbyItem != null && nearbyItem.CanBePickedUp())
        {
            if (container != null)
                container.SetActive(true);
            
            if (promptText != null)
            {
                string itemName = nearbyItem.Item != null ? nearbyItem.Item.itemName : "item";
                promptText.text = $"Press {interactKey} to pick up {itemName}";
            }
        }
        else
        {
            if (container != null)
                container.SetActive(false);
        }
    }

    private NetworkIdentity FindLocalPlayer()
    {
        NetworkIdentity[] allPlayers = FindObjectsOfType<NetworkIdentity>();
        foreach (NetworkIdentity player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                return player;
            }
        }
        return null;
    }

    private WorldItem FindNearbyItem(Transform playerTransform)
    {
        // Use camera for raycast if available, otherwise use player forward
        Transform rayOrigin = Camera.main != null ? Camera.main.transform : playerTransform;
        Vector3 rayDirection = Camera.main != null ? rayOrigin.forward : playerTransform.forward;

        Ray ray = new Ray(rayOrigin.position, rayDirection);
        
        // Use QueryTriggerInteraction.Collide to hit trigger colliders
        if (Physics.Raycast(ray, out RaycastHit hit, checkRange, -1, QueryTriggerInteraction.Collide))
        {
            WorldItem worldItem = hit.collider.GetComponent<WorldItem>();
            if (worldItem != null && worldItem.CanBePickedUp())
            {
                return worldItem;
            }
        }

        return null;
    }
}