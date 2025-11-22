using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Scene UI (assign in inspector)")]
    public Canvas canvas;
    public HotbarUI hotbarUI;
    public InteractionUI interactionUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Debug.LogWarning("Multiple UIManager instances - destroying duplicate"); Destroy(gameObject); return; }

        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (hotbarUI == null) hotbarUI = FindObjectOfType<HotbarUI>(true);
        if (interactionUI == null) interactionUI = FindObjectOfType<InteractionUI>(true);

        // Hide initially; shown only for local player
        Hide();
    }

    // Called by PlayerController.OnStartLocalPlayer
    public void BindPlayer(PlayerController player)
    {
        if (player == null) { Debug.LogError("[UIManager] BindPlayer null"); return; }

        // Bind hotbar to player's inventory
        if (hotbarUI != null && player.inventory != null)
        {
            hotbarUI.BindInventory(player.inventory);
        }

        // Set interaction UI reference on player
        if (interactionUI != null)
        {
            player.SetInteractionUI(interactionUI);
        }
    }

    public void Show() { if (canvas != null) canvas.gameObject.SetActive(true); }
    public void Hide() { if (canvas != null) canvas.gameObject.SetActive(false); }
}
