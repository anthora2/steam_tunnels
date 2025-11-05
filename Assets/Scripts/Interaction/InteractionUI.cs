using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject container;
    [SerializeField] public TMP_Text promptText;

    // Updates the interaction UI based on the current interactable
    private void Update()
    {
        // Check if InteractionDetector exists and is initialized
        if (InteractionDetector.Instance == null)
        {
            if (container != null)
                container.SetActive(false);
            return;
        }

        var target = InteractionDetector.Instance.CurrentInteractable;
        if (target != null && target.CanInteract())
        {
            if (container != null)
                container.SetActive(true);
            
            if (promptText != null)
                promptText.text = $"Press {InteractionDetector.Instance.interactKey} to interact with {target.GetInteractText()}";
        }
        else
        {
            if (container != null)
                container.SetActive(false);
        }
    }
}