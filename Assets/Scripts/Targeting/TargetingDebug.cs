using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class TargetingDebug : NetworkBehaviour
{
    [Header("Targeting Settings")]
    public LayerMask whatIsGround;
    public LayerMask targetLayers; // What can be targeted (enemies, objects, etc.)
    public float maxDistance = 50f;
    public float radius = 2f;
    public float height = 8f;
    
    [Header("Book Settings")]
    public GameObject bookObject;
    public Vector3 bookOffset = new Vector3(0.3f, -0.2f, 0.5f);
    public float bookScale = 1f;
    
    [Header("Visual Settings")]
    public Material cylinderMaterial; // Assign a transparent material
    public Color cylinderColor = new Color(0f, 0.6f, 1f, 0.3f);
    
    private Camera cam;
    private Vector3 hitPoint;
    private bool hasHit = false;
    
    [SyncVar(hook = nameof(OnAimingModeChanged))]
    private bool aimingMode = false;
    
    // Visual cylinder object (local player only)
    private GameObject targetingCylinder;
    private Renderer cylinderRenderer;
    
    // Collision tracking
    private List<Collider> detectedTargets = new List<Collider>();
    
    void Start()
    {
        // Only setup visuals for local player
        if (!isLocalPlayer)
        {
            // Non-local players just need to respond to SyncVar changes
            return;
        }
        
        // Find camera (local player only)
        cam = GetComponentInChildren<Camera>();
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("TargetingDebug: No camera found!");
            return;
        }
        
        // Validate book setup
        if (bookObject == null)
        {
            Debug.LogError("TargetingDebug: Book Object not assigned in Inspector!");
            return;
        }
        
        // Check if book has a renderer
        Renderer bookRenderer = bookObject.GetComponentInChildren<Renderer>();
        if (bookRenderer == null)
        {
            Debug.LogWarning("TargetingDebug: Book has no Renderer component!");
        }
        
        // Book starts hidden
        bookObject.SetActive(false);
        
        // Create targeting cylinder (local player only sees this)
        CreateTargetingCylinder();
        
        Debug.Log($"TargetingDebug initialized for local player. Camera: {cam.name}, Book: {bookObject.name}");
    }
    
    // Hook called when aimingMode changes on any client
    void OnAimingModeChanged(bool oldValue, bool newValue)
    {
        // Update book visibility for ALL clients
        if (bookObject != null)
        {
            bookObject.SetActive(newValue);
        }
        
        // Update cylinder visibility (local player only)
        if (isLocalPlayer && targetingCylinder != null)
        {
            targetingCylinder.SetActive(newValue);
        }
    }
    
    void CreateTargetingCylinder()
    {
        // Create cylinder GameObject
        targetingCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        targetingCylinder.name = "TargetingCylinder";
        
        // Remove collider (we don't want it to physically interact)
        Collider col = targetingCylinder.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        // Setup renderer
        cylinderRenderer = targetingCylinder.GetComponent<Renderer>();
        
        // Use provided material or create a transparent one
        if (cylinderMaterial != null)
        {
            cylinderRenderer.material = cylinderMaterial;
        }
        else
        {
            // Create a basic transparent material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = cylinderColor;
            // Make it transparent
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            cylinderRenderer.material = mat;
        }
        
        // Start hidden
        targetingCylinder.SetActive(false);
    }
    
    void Update()
    {
        // Only local player can control aiming
        if (!isLocalPlayer) return;
        
        if (cam == null || bookObject == null) return;
        
        // ---- TOGGLE AIM MODE ----
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Call command to update on server (which syncs to all clients)
            CmdSetAimingMode(!aimingMode);
            
            Debug.Log($"Shift pressed! Requesting aiming mode: {!aimingMode}");
        }
        
        // If not aiming, stop here
        if (!aimingMode)
            return;
        
        // ---- POSITION BOOK RELATIVE TO CAMERA ----
        UpdateBookPosition();
        
        // ---- RAYCAST FOR TARGETING ----
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxDistance, whatIsGround))
        {
            hitPoint = hit.point;
            hasHit = true;
            
            // Update cylinder position and scale
            UpdateCylinderVisual();
            
            // Detect targets in the area
            DetectTargets();
        }
        else
        {
            hasHit = false;
            if (targetingCylinder != null)
                targetingCylinder.SetActive(false);
        }
    }
    
    // Command: Client requests to change aiming mode, server updates it
    [Command]
    void CmdSetAimingMode(bool newMode)
    {
        aimingMode = newMode;
        // SyncVar automatically sends this to all clients
    }
    
    void UpdateBookPosition()
    {
        if (bookObject == null) return;
        
        // Set scale
        bookObject.transform.localScale = Vector3.one * bookScale;
        
        // If book is child of camera, just use local position
        if (bookObject.transform.parent == cam.transform)
        {
            bookObject.transform.localPosition = bookOffset;
            bookObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Fallback: position in world space relative to camera
            bookObject.transform.position = cam.transform.position + cam.transform.TransformDirection(bookOffset);
            bookObject.transform.rotation = cam.transform.rotation;
        }
        
        // Optional: Make book float slightly for effect
        float floatOffset = Mathf.Sin(Time.time * 2f) * 0.05f;
        bookObject.transform.localPosition += Vector3.up * floatOffset;
        
        // Optional: Rotate book slowly
        bookObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);
    }
    
    void UpdateCylinderVisual()
    {
        if (targetingCylinder == null) return;
        
        // Position at hitpoint, centered vertically
        Vector3 cylinderPos = hitPoint + Vector3.up * (height / 2f);
        targetingCylinder.transform.position = cylinderPos;
        
        // Scale to match radius and height
        // Unity cylinder default is 2 units tall, 1 unit diameter
        targetingCylinder.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        
        // Make sure it's visible
        if (!targetingCylinder.activeSelf)
            targetingCylinder.SetActive(true);
    }
    
    void DetectTargets()
    {
        // Clear previous detections
        detectedTargets.Clear();
        
        // Define capsule for overlap check
        Vector3 point1 = hitPoint; // Bottom of capsule
        Vector3 point2 = hitPoint + Vector3.up * height; // Top of capsule
        
        // Find all colliders in the capsule area
        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, targetLayers);
        
        if (hits.Length > 0)
        {
            Debug.Log($"=== Detected {hits.Length} targets ===");
            foreach (Collider hit in hits)
            {
                detectedTargets.Add(hit);
                
                // Log what we found
                Debug.Log($"Target: {hit.gameObject.name} | Tag: {hit.tag} | Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
                
                // You can add more detailed info here
                // For example, check if it has specific components:
                if (hit.GetComponent<Rigidbody>())
                    Debug.Log($"  - Has Rigidbody");
                
                // Check for enemy scripts, health components, etc.
                // Example: var enemy = hit.GetComponent<EnemyHealth>();
            }
        }
    }
    
    // Public method to get currently detected targets (for other scripts to use)
    public List<Collider> GetDetectedTargets()
    {
        return detectedTargets;
    }
    
    // Optional: Draw debug visualization in Scene view
    private void OnDrawGizmos()
    {
        if (!aimingMode || !hasHit) return;
        
        // Draw floor circle outline
        Gizmos.color = new Color(0f, 0.8f, 1f, 1f);
        const int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = Vector3.zero;
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 nextPoint = hitPoint + new Vector3(x, 0.01f, z);
            
            if (i > 0)
                Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
        
        // Draw camera ray
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cam.transform.position, cam.transform.forward * maxDistance);
        }
        
        // Draw detected targets
        Gizmos.color = Color.red;
        foreach (Collider target in detectedTargets)
        {
            if (target != null)
                Gizmos.DrawWireSphere(target.transform.position, 0.5f);
        }
    }
    
    void OnDestroy()
    {
        // Clean up cylinder when this script is destroyed
        if (targetingCylinder != null)
            Destroy(targetingCylinder);
    }
}