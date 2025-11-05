using UnityEngine;

// Interface for interactable objects
public interface IInteractable {
    public bool Interact();
    public bool CanInteract();
    public string GetInteractText();
    Transform Transform { get; }
}