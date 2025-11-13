using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class HeartHUD : NetworkBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private GameObject heartPrefab; // UI Image or RawImage prefab for hearts
    [SerializeField] private Transform heartContainer; // Parent object for hearts
    [SerializeField] private int maxHearts = 3; // Maximum number of hearts
    [SerializeField] private Sprite heartSprite; // Only need one sprite now!
    
    [Header("Health Settings")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    private int currentHealth;
    
    private List<Image> heartImages = new List<Image>();
    
    void Start()
    {
        // Only create UI for local player
        if (!isLocalPlayer)
        {
            // Disable canvas for non-local players
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.enabled = false;
            return;
        }
        
        // Initialize health to max
        currentHealth = maxHearts;
        InitializeHearts();
    }
    
    void InitializeHearts()
    {
        // Clear existing hearts
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();
        
        // Create heart UI elements based on current health
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            if (heartImage != null && heartSprite != null)
            {
                heartImage.sprite = heartSprite;
            }
            heartImages.Add(heartImage);
        }
    }
    
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (!isLocalPlayer) return;
        UpdateHeartDisplay();
    }
    
    void UpdateHeartDisplay()
    {
        // If we have more hearts than needed, remove extras
        while (heartImages.Count > currentHealth)
        {
            int lastIndex = heartImages.Count - 1;
            Destroy(heartImages[lastIndex].gameObject);
            heartImages.RemoveAt(lastIndex);
        }
        
        // If we need more hearts (healing), add them
        while (heartImages.Count < currentHealth)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            if (heartImage != null && heartSprite != null)
            {
                heartImage.sprite = heartSprite;
            }
            heartImages.Add(heartImage);
        }
    }
    
    // Call this to take damage
    [Command]
    public void CmdTakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
    }
    
    // Call this to heal
    [Command]
    public void CmdHeal(int amount)
    {
        currentHealth = Mathf.Min(maxHearts, currentHealth + amount);
    }
    
    // Public method to set max hearts (for upgrades)
    [Command]
    public void CmdSetMaxHearts(int newMax)
    {
        maxHearts = newMax;
        if (isLocalPlayer)
        {
            InitializeHearts();
            currentHealth = Mathf.Min(currentHealth, maxHearts);
            UpdateHeartDisplay();
        }
    }
}