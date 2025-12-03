using UnityEngine;
using Mirror;

public class DoorInteractable : NetworkBehaviour, IInteractable
{
    [SyncVar] bool isOpen = false;

    public bool CanInteract(PlayerController player) => true;

    public string GetInteractPrompt() => isOpen ? "Close door" : "Open door";

    [Command(requiresAuthority = false)]
    public void Interact(PlayerController player)
    {
        isOpen = !isOpen;
        RpcPlayDoorAnimation(isOpen);
    }

    [ClientRpc]
    void RpcPlayDoorAnimation(bool isOpen)
    {
        
    }
}