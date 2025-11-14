using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

public class HeartHUD : NetworkBehaviour
{
    [Header("Heart Settings")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private Sprite heartSprite;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    
    [SyncVar(hook = nameof(OnHealthChanged))]
    private int currentHealth;
    
    private List<Image> heartImages = new List<Image>();
    
    // SERVER ONLY: Initialize health when spawned on server
    public override void OnStartServer()
    {
        base.OnStartServer();
        // Server initializes the authoritative health value
        currentHealth = maxHearts;
    }
    
    // CLIENT ONLY: Setup UI for local player
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Local player sets up their UI
        InitializeHearts();
    }
    
    // Called on non-local player clients when they see this player spawn
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Disable canvas for non-local players
        if (!isLocalPlayer)
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }
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
            CreateHeart();
        }
    }
    
    void CreateHeart()
    {
        GameObject heartObj = Instantiate(heartPrefab, heartContainer);
        Image heartImage = heartObj.GetComponent<Image>();
        if (heartImage != null && heartSprite != null)
        {
            heartImage.sprite = heartSprite;
        }
        heartImages.Add(heartImage);
    }
    
    // Hook called whenever currentHealth changes (on clients)
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Only update UI for local player
        if (!isLocalPlayer) return;
        
        UpdateHeartDisplay();
    }
    
    void UpdateHeartDisplay()
    {
        // Remove excess hearts
        while (heartImages.Count > currentHealth)
        {
            int lastIndex = heartImages.Count - 1;
            if (heartImages[lastIndex] != null)
            {
                Destroy(heartImages[lastIndex].gameObject);
            }
            heartImages.RemoveAt(lastIndex);
        }
        
        // Add missing hearts
        while (heartImages.Count < currentHealth)
        {
            CreateHeart();
        }
    }
    
    // CLIENT calls this, SERVER executes it
    [Command]
    public void CmdTakeDamage(int damage)
    {
        // Server updates authoritative health value
        currentHealth = Mathf.Max(0, currentHealth - damage);
    }
    
    // CLIENT calls this, SERVER executes it
    [Command]
    public void CmdHeal(int amount)
    {
        // Server updates authoritative health value
        currentHealth = Mathf.Min(maxHearts, currentHealth + amount);
    }
    
    // CLIENT calls this, SERVER executes it
    [Command]
    public void CmdSetMaxHearts(int newMax)
    {
        // Server updates max hearts
        maxHearts = newMax;
        // Clamp current health to new max
        currentHealth = Mathf.Min(currentHealth, maxHearts);
        // The SyncVar hook will handle updating the client UI
    }
    
    // Public method to get current health (read-only for gameplay logic)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHearts()
    {
        return maxHearts;
    }
}