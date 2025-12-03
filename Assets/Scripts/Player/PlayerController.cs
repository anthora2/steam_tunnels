using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Assign in prefab")]
    public Inventory inventory;    // NetworkBehaviour on player prefab
    public Camera cam;             // disabled by default in prefab
    public float interactRange = 3f;

    InteractionUI interactionUI;  // assigned at runtime by UIManager

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Enable local camera
        if (cam != null) cam.gameObject.SetActive(true);

        // Bind UI to this local player
        if (UIManager.Instance != null)
        {
            UIManager.Instance.BindPlayer(this);
            UIManager.Instance.Show();
        }
        else
        {
            Debug.LogError("[PlayerController] UIManager.Instance is null");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isLocalPlayer && UIManager.Instance != null) UIManager.Instance.Hide();
    }

    // called by UIManager when binding
    public void SetInteractionUI(InteractionUI ui)
    {
        interactionUI = ui;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (interactionUI == null) return;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, interactRange))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                // show prompt
                interactionUI.Show(interactable.GetInteractPrompt());

                if (Input.GetKeyDown(KeyCode.E))
                {
                    var id = hit.transform.GetComponentInParent<NetworkIdentity>();
                    if (id != null)
                        CmdInteract(id.netId);
                }
                return;
            }
        }

        interactionUI.Hide();
    }

    [Command]
    void CmdInteract(uint targetNetId)
    {
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var obj))
            return;

        if (obj.TryGetComponent(out IInteractable interactable))
        {
            if (interactable.CanInteract(this))
            {
                // Run server-side behavior
                interactable.Interact(this);
            }
        }
    }
}
