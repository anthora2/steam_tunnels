using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private List<IInteractable> _interactablesInRange = new List<IInteractable>();

    [SerializeField] private float interactionRange = 2f;
    [SerializeField] public KeyCode interactKey = KeyCode.E;

    public IInteractable CurrentInteractable { get; private set; }
    public static InteractionDetector Instance { get; private set; }

    private void Awake() => Instance = this;

    // Checks for interactions and updates the current interactable
    void Update()
    {
        // Remove any interactables that are null
        _interactablesInRange.RemoveAll(interactable => interactable == null);

        CurrentInteractable = GetClosestInteractable();
        if (Input.GetKeyDown(interactKey) && _interactablesInRange.Count > 0)
        {
            if (CurrentInteractable != null && CurrentInteractable.CanInteract())
            {
                if (CurrentInteractable.Interact())
                {
                    _interactablesInRange.Remove(CurrentInteractable);
                }
            }
        }
    }

    // TODO: Implement logic to choose which interactable in game to interact with
    private IInteractable GetClosestInteractable()
    {
        if (_interactablesInRange.Count == 0)
            return null;
        return _interactablesInRange[0];
    }

    // Add the interactable to the list if it exists
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.name);
        if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            Debug.Log("Interactable found: " + interactable.GetInteractText());
        }
            _interactablesInRange.Add(interactable);
    }
 
    // Remove the interactable from the list if it exists
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
            _interactablesInRange.Remove(interactable);
    }
}