using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] public GameObject container;
    [SerializeField] public TMP_Text promptText;

    void Start()
    {
        container.SetActive(false);
    }

    public void Show(string prompt)
    {
        if (container != null)
            container.SetActive(true);
        if (promptText != null)
            promptText.text = prompt;
    }

    public void Hide()
    {
        if (container != null)
            container.SetActive(false);
    }
}