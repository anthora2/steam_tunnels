using UnityEngine;

// Abstract class for interactable objects
public abstract class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] protected string interactText = "Interact";

    public Transform Transform => transform;
    public virtual string GetInteractText() => interactText;

    public abstract bool CanInteract();
    public abstract bool Interact();
}
