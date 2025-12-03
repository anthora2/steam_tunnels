using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the visual faith bar UI
/// Updates the fill amount based on faith percentage
/// </summary>
public class FaithBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image faithBarFill;
    
    [Header("Animation Settings")]
    [SerializeField] private float smoothSpeed = 5f; // How fast the bar animates
    
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;
    
    void Start()
    {
        // Validate references
        if (faithBarFill == null)
        {
            Debug.LogError("FaithBarUI: FaithBarFill Image is not assigned!");
            return;
        }
        
        // Make sure the fill is set up correctly
        if (faithBarFill.type != Image.Type.Filled)
        {
            Debug.LogWarning("FaithBarUI: FaithBarFill should be Image Type 'Filled'!");
        }
        
        // Initialize at full
        faithBarFill.fillAmount = 1f;
        
        Debug.Log("FaithBarUI initialized");
    }
    
    void Update()
    {
        // Smoothly animate the fill amount toward the target
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            faithBarFill.fillAmount = currentFillAmount;
        }
    }
    
    /// <summary>
    /// Updates the faith bar to show a specific percentage
    /// </summary>
    /// <param name="percent">Value between 0 and 1 (0 = empty, 1 = full)</param>
    public void UpdateFaithBar(float percent)
    {
        // Clamp to valid range
        percent = Mathf.Clamp01(percent);
        targetFillAmount = percent;
    }

    private void OnEnable()
    {
        FaithManager.OnFaithChanged += HandleFaithChanged;
    }

    private void OnDisable()
    {
        FaithManager.OnFaithChanged -= HandleFaithChanged;
    }

    private void HandleFaithChanged(float current, float max)
    {
        float percent = current / max;
        UpdateFaithBar(percent);
    }
    
    /// <summary>
    /// Instantly set the faith bar without animation
    /// </summary>
    public void SetFaithBarInstant(float percent)
    {
        percent = Mathf.Clamp01(percent);
        targetFillAmount = percent;
        currentFillAmount = percent;
        faithBarFill.fillAmount = percent;
    }
    
    // Optional: For testing in Inspector
    [ContextMenu("Test - Set to 75%")]
    void TestSetTo75()
    {
        UpdateFaithBar(0.75f);
    }
    
    [ContextMenu("Test - Set to 50%")]
    void TestSetTo50()
    {
        UpdateFaithBar(0.5f);
    }
    
    [ContextMenu("Test - Set to 25%")]
    void TestSetTo25()
    {
        UpdateFaithBar(0.25f);
    }
    
    [ContextMenu("Test - Set to Full")]
    void TestSetToFull()
    {
        UpdateFaithBar(1f);
    }
}