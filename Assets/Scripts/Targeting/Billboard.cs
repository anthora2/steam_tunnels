using UnityEngine;

/// <summary>
/// Makes a GameObject always face the camera (billboard effect)
/// Perfect for VFX like lightning strikes, health bars, floating text, etc.
/// </summary>
public class Billboard : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The camera to face. If null, will find main camera automatically")]
    public Camera targetCamera;
    
    [Tooltip("Lock rotation on specific axes")]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
    
    [Header("Billboard Type")]
    [Tooltip("Full Billboard: Faces camera completely. Y-Axis Only: Only rotates around Y axis (stays upright)")]
    public BillboardType billboardType = BillboardType.Full;
    
    public enum BillboardType
    {
        Full,           // Faces camera completely (best for lightning/effects)
        YAxisOnly,      // Only rotates around Y axis (stays upright - good for health bars)
        Reverse         // Faces away from camera
    }
    
    void Start()
    {
        // Auto-find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            
            if (targetCamera == null)
            {
                Debug.LogWarning("Billboard: No camera found! Trying to find any camera...");
                targetCamera = FindObjectOfType<Camera>();
            }
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        Vector3 directionToCamera;
        
        switch (billboardType)
        {
            case BillboardType.Full:
                // Face the camera completely
                directionToCamera = targetCamera.transform.position - transform.position;
                
                if (directionToCamera.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
                    ApplyRotationWithLocks(targetRotation);
                }
                break;
                
            case BillboardType.YAxisOnly:
                // Only rotate around Y axis (billboard stays upright)
                directionToCamera = targetCamera.transform.position - transform.position;
                directionToCamera.y = 0; // Flatten to horizontal plane
                
                if (directionToCamera.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                    ApplyRotationWithLocks(targetRotation);
                }
                break;
                
            case BillboardType.Reverse:
                // Face away from camera
                directionToCamera = transform.position - targetCamera.transform.position;
                
                if (directionToCamera.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
                    ApplyRotationWithLocks(targetRotation);
                }
                break;
        }
    }
    
    void ApplyRotationWithLocks(Quaternion targetRotation)
    {
        // Apply axis locks if needed
        Vector3 eulerRotation = targetRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;
        
        if (lockX) eulerRotation.x = currentEuler.x;
        if (lockY) eulerRotation.y = currentEuler.y;
        if (lockZ) eulerRotation.z = currentEuler.z;
        
        transform.rotation = Quaternion.Euler(eulerRotation);
    }
}