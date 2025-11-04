using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class WheelPickerMenu : MonoBehaviour
{
    [Header("Data")]
    public List<WheelPickerItem> items = new List<WheelPickerItem>();

    [Header("UI References")]
    public RectTransform wheelRoot;
    public GameObject wheelOptionPrefab;

    [Header("Settings")]
    public int maxItems = 8;
    public float optionPaddingDegrees = 1f;
    public float wheelRadius = 200f;

    [Header("Input")]
    public KeyCode openKey = KeyCode.Tab;
    [Tooltip("Scroll sensitivity - how fast scrolling changes selection")]
    public float scrollSensitivity = 0.5f;

    [Header("Events")]
    public UnityEvent<WheelPickerItem> onItemSelected;

    private List<WheelPickerOption> wheelOptions = new List<WheelPickerOption>();
    private bool isOpen = false;
    private int selectedIndex = 0;
    private Canvas canvas;
    private RectTransform rectTransform;

    // Intialize wheel with canvas and rect transform
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    // Generates the wheel options and closes by default
    private void Start()
    {
        GenerateWheel();
        Close();
    }

    // Handle the open and close input, scroll input, and mouse input
    private void Update()
    {
        HandleOpenCloseInput();
        
        // Process scroll input when menu is open, otherwise process mouse input
        if (isOpen) HandleScrollInput();
        else HandleMouseInput();
    }

    // Handle the mouse input
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) items[selectedIndex].OnClick();
    }

    // Handle the open and close input
    private void HandleOpenCloseInput()
    {
        if (!isOpen && Input.GetKeyDown(openKey)) Open();
        else if (isOpen && (Input.GetKeyUp(openKey) || Input.GetKeyDown(KeyCode.Escape))) Close();
    }

    // Open the wheel picker menu
    public void Open()
    {
        isOpen = true;

        // Show the wheel visually instead of activating GameObject
        if (wheelRoot != null) wheelRoot.gameObject.SetActive(true);

        // Initialize to first item (index 0)
        UpdateSelection();
    }

    // Close the wheel picker menu
    public void Close()
    {
        isOpen = false;

        // Hide the wheel object but keep the menu active for input detection
        if (wheelRoot != null) wheelRoot.gameObject.SetActive(false);
        ClearSelection();
    }

    // Handle the scroll input
    private void HandleScrollInput()
    {
        int count = Mathf.Clamp(items.Count, 0, maxItems);
        if (count < 1) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        
        // Apply sensitivity threshold to prevent accidental scrolling
        if (Mathf.Abs(scrollDelta) >= scrollSensitivity)
        {
            // Scroll up (positive) moves to next item, scroll down (negative) moves to previous
            int direction = scrollDelta > 0 ? 1 : -1;
            int newIndex = selectedIndex + direction;
            
            // Wrap around: if going past the end -> loop to beginning; if going before start -> loop to end
            if (newIndex >= count)
            {
                newIndex = 0;
            }
            else if (newIndex < 0)
            {
                newIndex = count - 1;
            }
            
            selectedIndex = newIndex;
            Debug.Log("Selected index: " + selectedIndex);
            UpdateSelection();
        }
    }

    // Update the selection of the current option
    private void UpdateSelection()
    {
        for (int i = 0; i < wheelOptions.Count; i++)
        {
            wheelOptions[i].SetSelected(i == selectedIndex);
        }
    }

    // Clear the selection of all options
    private void ClearSelection()
    {
        foreach (var option in wheelOptions)
        {
            if (option != null)
            {
                option.SetSelected(false);
            }
        }
    }

    // Generate the wheel options
    public void GenerateWheel()
    {
        ClearOptions();
        
        // Calculate the number of options to display
        int count = Mathf.Clamp(items.Count, 0, maxItems);
        if (count < 1) return;

        float optionAngle = 360f / count;
        float fillAmountNormalized = (optionAngle - optionPaddingDegrees) / 360f;

        for (int i = 0; i < count; i++)
        {
            float angle = optionAngle * i;
            GameObject obj = Instantiate(wheelOptionPrefab, wheelRoot, false);
            WheelPickerOption option = obj.GetComponent<WheelPickerOption>();
            if (option == null)
            {
                Debug.LogError("WheelPickerOption component not found on the wheel option prefab.");
                Destroy(obj);
                continue;
            }

            option.index = i;
            string label = (i < items.Count && items[i] != null) ? items[i].itemName : $"Item {i + 1}";
            option.Initialize(fillAmountNormalized, angle, label);
            wheelOptions.Add(option);
        }
    }

    // Clear the wheel options
    private void ClearOptions()
    {
        foreach (var option in wheelOptions)
        {
            if (option != null && option.gameObject != null) Destroy(option.gameObject);
        }
        wheelOptions.Clear();
    }
}