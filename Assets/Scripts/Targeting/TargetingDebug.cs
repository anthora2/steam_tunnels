using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class TargetingDebug : NetworkBehaviour
{
    [Header("Targeting Settings")]
    public LayerMask whatIsGround;
    public LayerMask targetLayers;
    public float maxDistance = 50f;
    public float radius = 2f;
    public float height = 8f;
    
    [Header("Book Settings")]
    public GameObject bookObject;
    public Vector3 bookOffset = new Vector3(0.3f, -0.2f, 0.5f);
    public float bookScale = 1f;
    public string currentAttack = "Lightning";
    
    [Header("Visual Settings")]
    public Material cylinderMaterial;
    public Color cylinderColor = new Color(0f, 0.6f, 1f, 0.15f);
    
    [Header("Lightning Settings")]
    public GameObject lightningPrefab;
    public float lightningDuration = 0.3f;
    public float lightningHeight = 0.1f;
    
    private Camera cam;
    private Vector3 hitPoint;
    private bool hasHit = false;
    
    [SyncVar(hook = nameof(OnAimingModeChanged))]
    private bool aimingMode = false;
    
    private GameObject targetingCylinder;
    private Renderer cylinderRenderer;
    private GameObject edgeRing;
    private LineRenderer ringLineRenderer;
    private List<Collider> detectedTargets = new List<Collider>();
    private List<Collider> previousTargets = new List<Collider>();
    
    private bool canCast = true;
    private Dictionary<string, AttackData> attackMap;
    private FaithManager faithManager;
    
    void Start()
    {
        InitializeAttackData();
        faithManager = FindFirstObjectByType<FaithManager>();

        if (!isLocalPlayer)
        {
            return;
        }
    
        cam = GetComponentInChildren<Camera>();
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("TargetingDebug: No camera found!");
            return;
        }

        if (faithManager == null)
        {
            Debug.LogError("Could not find FaithManager");
            return;
        }


        if (bookObject == null)
        {
            Debug.LogError("TargetingDebug: Book Object not assigned in Inspector!");
            return;
        }
        
        Renderer bookRenderer = bookObject.GetComponentInChildren<Renderer>();
        if (bookRenderer == null)
        {
            Debug.LogWarning("TargetingDebug: Book has no Renderer component!");
        }
        
        bookObject.SetActive(false);
        CreateTargetingCylinder();
        CreateEdgeRing();
        
        Debug.Log($"TargetingDebug initialized for local player. Camera: {cam.name}, Book: {bookObject.name}");
    }

    void InitializeAttackData()
        {
            attackMap = new Dictionary<string, AttackData>();

            attackMap["Lightning"] = new AttackData(
                attackName: "Lightning",
                cost: 80f,
                cooldown: 1.5f,
                damage: 40f,
                targetType: AttackTargetType.Point
            )
            {
                vfxPrefab = lightningPrefab,
                vfxScale = 15f,
                vfxDuration = lightningDuration
            };
        }
    
    void OnAimingModeChanged(bool oldValue, bool newValue)
    {
        if (bookObject != null)
        {
            bookObject.SetActive(newValue);
        }
        
        if (isLocalPlayer && targetingCylinder != null)
        {
            targetingCylinder.SetActive(newValue);
        }
        
        if (isLocalPlayer && edgeRing != null)
        {
            edgeRing.SetActive(newValue);
        }
    }
    
    void CreateTargetingCylinder()
    {
        targetingCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        targetingCylinder.name = "TargetingCylinder";
        
        Collider col = targetingCylinder.GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        cylinderRenderer = targetingCylinder.GetComponent<Renderer>();
        
        if (cylinderMaterial != null)
        {
            cylinderRenderer.material = cylinderMaterial;
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = cylinderColor;
                
                if (shader.name.Contains("Universal"))
                {
                    mat.SetFloat("_Surface", 1);
                    mat.SetFloat("_Blend", 0);
                    mat.SetFloat("_AlphaClip", 0);
                    mat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetFloat("_ZWrite", 0);
                    mat.renderQueue = 3000;
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                }
                else
                {
                    mat.SetFloat("_Mode", 3);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
                
                cylinderRenderer.material = mat;
            }
            else
            {
                Debug.LogWarning("Could not find shader for transparent material. Cylinder may appear pink.");
            }
        }
        
        targetingCylinder.SetActive(false);
    }
    
    void CreateEdgeRing()
    {
        edgeRing = new GameObject("TargetingEdgeRing");
        ringLineRenderer = edgeRing.AddComponent<LineRenderer>();
        
        ringLineRenderer.startWidth = 0.1f;
        ringLineRenderer.endWidth = 0.1f;
        ringLineRenderer.loop = true;
        ringLineRenderer.useWorldSpace = true;
        ringLineRenderer.positionCount = 64;
        
        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = new Color(0f, 0.8f, 1f, 1f);
        ringLineRenderer.material = lineMat;
        
        edgeRing.SetActive(false);
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;

        if (attackMap != null 
            && attackMap.ContainsKey(currentAttack)
            && faithManager != null)
        {
            float required = attackMap[currentAttack].cost;
            float current = faithManager.GetCurrentFaith();

            bool newCanCast = current >= required;

            if (newCanCast != canCast)
            {
                 Debug.Log($"State canCast has changed, is now: {newCanCast}");
            }

            canCast = newCanCast;
        }

        
        if (cam == null || bookObject == null) return;
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            CmdSetAimingMode(!aimingMode);
            Debug.Log($"Shift pressed! Requesting aiming mode: {!aimingMode}");
        }
        
        if (!aimingMode)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            AttemptAttack();
        }
        
        UpdateBookPosition();
        
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxDistance, whatIsGround))
        {
            hitPoint = hit.point;
            hasHit = true;
            UpdateCylinderVisual();
            UpdateEdgeRing();
            DetectTargets();
        }
        else
        {
            hasHit = false;
            if (targetingCylinder != null)
                targetingCylinder.SetActive(false);
            if (edgeRing != null)
                edgeRing.SetActive(false);
        }
    }
    
    [Command]
    void CmdSetAimingMode(bool newMode)
    {
        aimingMode = newMode;
    }
    
    void UpdateBookPosition()
    {
        if (bookObject == null) return;
        
        bookObject.transform.localScale = Vector3.one * bookScale;
        
        if (bookObject.transform.parent == cam.transform)
        {
            bookObject.transform.localPosition = bookOffset;
            bookObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            bookObject.transform.position = cam.transform.position + cam.transform.TransformDirection(bookOffset);
            bookObject.transform.rotation = cam.transform.rotation;
        }
        
        float floatOffset = Mathf.Sin(Time.time * 2f) * 0.05f;
        bookObject.transform.localPosition += Vector3.up * floatOffset;
        bookObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);
    }
    
    void UpdateCylinderVisual()
    {
        if (targetingCylinder == null) return;
        
        Vector3 cylinderPos = hitPoint + Vector3.up * (height / 2f);
        targetingCylinder.transform.position = cylinderPos;
        targetingCylinder.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);
        
        if (!targetingCylinder.activeSelf)
            targetingCylinder.SetActive(true);
    }
    
    void UpdateEdgeRing()
    {
        if (ringLineRenderer == null || !hasHit) return;
        
        int segments = ringLineRenderer.positionCount;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 point = hitPoint + new Vector3(x, 0.05f, z);
            ringLineRenderer.SetPosition(i, point);
        }
        
        if (!edgeRing.activeSelf && aimingMode)
            edgeRing.SetActive(true);
        else if (!aimingMode && edgeRing.activeSelf)
            edgeRing.SetActive(false);
    }
    
    void DetectTargets()
    {
        previousTargets.Clear();
        previousTargets.AddRange(detectedTargets);
        detectedTargets.Clear();
        
        Vector3 point1 = hitPoint;
        Vector3 point2 = hitPoint + Vector3.up * height;
        
        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, targetLayers);
        
        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                detectedTargets.Add(hit);
                
                if (!previousTargets.Contains(hit))
                {
                    Debug.Log($"🎯 <color=green>TARGET ENTERED</color>: {hit.gameObject.name}");
                    LogTargetDetails(hit);
                }
            }
            
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"╔══════════════════════════════════════════╗");
                Debug.Log($"║  {hits.Length} TARGET(S) IN AREA");
                Debug.Log($"╚══════════════════════════════════════════╝");
                
                for (int i = 0; i < hits.Length; i++)
                {
                    Debug.Log($"[{i + 1}] {hits[i].gameObject.name}");
                }
            }
        }
        
        foreach (Collider previousTarget in previousTargets)
        {
            if (previousTarget != null && !detectedTargets.Contains(previousTarget))
            {
                Debug.Log($"🚫 <color=red>TARGET LEFT</color>: {previousTarget.gameObject.name}");
            }
        }
    }
    
    void LogTargetDetails(Collider hit)
    {
        Debug.Log($"    └─ Tag: '{hit.tag}'");
        Debug.Log($"    └─ Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");
        Debug.Log($"    └─ Position: {hit.transform.position}");
        
        if (hit.GetComponent<Rigidbody>())
            Debug.Log($"    └─ ✓ Has Rigidbody");
        
        var netIdentity = hit.GetComponent<Mirror.NetworkIdentity>();
        if (netIdentity != null)
            Debug.Log($"    └─ ✓ Network Object (NetID: {netIdentity.netId})");
        
        float distanceFromCenter = Vector3.Distance(hit.transform.position, hitPoint);
        Debug.Log($"    └─ Distance from center: {distanceFromCenter:F2}m");
    }

    void AttemptAttack()
    {
        //Client side check first
        if (!canCast)
        {
            Debug.Log("Not enough faith to cast, blocking attack");
            return;
        }
       

       //Server side request after
        CmdRequestAttack(currentAttack, hitPoint);

        //this is to test how it looks after
        SpawnLocalAttackVFX();

    }

    void SpawnLocalAttackVFX()
    {
        AttackData currAttackData = attackMap[currentAttack];

        if (currAttackData.vfxPrefab == null)
        {
            Debug.Log("No VFX prefab assigned for attack: " + currentAttack);
            return;
        }

        Vector3 pos = hitPoint + Vector3.up * lightningHeight;

        GameObject vfx = Instantiate(currAttackData.vfxPrefab, pos,  Quaternion.identity);
        vfx.transform.localScale = Vector3.one * currAttackData.vfxScale;
        Destroy(vfx, currAttackData.vfxDuration);
    }
    
    public List<Collider> GetDetectedTargets()
    {
        return detectedTargets;
    }
    
    // Command: Client sends spawn request to server
    // [Command]
    // void CmdSpawnLightning(Vector3 position)
    // {
    //     // Server tells all clients to spawn lightning
    //     RpcSpawnLightning(position);
    // }

    // [Command]
    // void CmdAttack(Vector3 position)
    // {
    //     RpcAttack(position);
    // }

    [Command]
    void CmdRequestAttack(string attackName, Vector3 position, NetworkConnectionToClient sender = null)
    {
        
        Debug.Log($"[SERVER] CmdRequestAttack RECEIVED from {sender.connectionId}");

        if (attackMap == null)
        {
            Debug.LogError("[SERVER] attackMap is NULL");
            return;
        }
        
        if (!attackMap.ContainsKey(attackName))
        {
            Debug.Log($"Invalid attack request: {attackName}");
            return;
        }

        AttackData currAttackData = attackMap[attackName];

        //Server Faith Check
        if (faithManager.GetCurrentFaith() < currAttackData.cost)
        {
            canCast = false;
            Debug.Log("Not enough faith!");
            return;
        }

        faithManager.ReduceFaith(currAttackData.cost);

        RpcPerformAttack(attackName, position);
    }

    [ClientRpc]
    void RpcPerformAttack(string attackName, Vector3 position)
    {
        // Step 1: Check if attack exists
        if (!attackMap.ContainsKey(attackName))
        {
            Debug.LogError($"[CLIENT] attackMap missing key: {attackName}");
            return;
        }
    
        AttackData currAttackData = attackMap[attackName];
        
        // Step 2: Check if prefab exists
        if (currAttackData.vfxPrefab == null)
        {
            Debug.LogError($"[CLIENT] No VFX prefab for {attackName}");
            return;
        }
        
        // Step 3: CREATE the vfx GameObject FIRST
        Vector3 pos = position + Vector3.up * lightningHeight;
        GameObject vfx = Instantiate(currAttackData.vfxPrefab, pos, Quaternion.identity);
        vfx.transform.localScale = Vector3.one * currAttackData.vfxScale;
        
        // Step 4: NOW use vfx (after it exists)
        Camera clientCam = FindLocalPlayerCamera();
        Billboard billboard = vfx.GetComponent<Billboard>();
        
        if (billboard != null && clientCam != null)
        {
            billboard.targetCamera = clientCam;
            Debug.Log($"[CLIENT] Assigned camera {clientCam.name} to lightning billboard");
        }
    
        // Step 5: Destroy it
        Destroy(vfx, currAttackData.vfxDuration);
    }

    Camera FindLocalPlayerCamera()
    {
        TargetingDebug[] allPlayers = FindObjectsOfType<TargetingDebug>();
        
        foreach (TargetingDebug player in allPlayers)
        {
            if (player.isLocalPlayer)
            {
                Camera playerCam = player.GetComponentInChildren<Camera>();
                if (playerCam != null)
                {
                    return playerCam;
                }
            }
        }
        
        return FindObjectOfType<Camera>();
    }
    
    // ClientRpc: All clients spawn lightning at the same position
    // [ClientRpc]
    // void RpcSpawnLightning(Vector3 position)
    // {
    //     SpawnLightningLocal(position);
    // }
    
    // [ClientRpc]
    // void RpcAttack(Vector3 position)
    // {
    //     //will take in an attack and then will do the same thing as spawn lightning
        
    // }


    // void SpawnLightningLocal(Vector3 position)
    // {
    //     if (lightningPrefab == null)
    //     {
    //         Debug.LogWarning("Cannot spawn lightning: Prefab not assigned");
    //         return;
    //     }
        
    //     Vector3 spawnPosition = position + Vector3.up * lightningHeight;
    //     GameObject lightning = Instantiate(lightningPrefab, spawnPosition, Quaternion.identity);
    //     lightning.transform.localScale = Vector3.one * 15f;
        
    //     // Manually assign camera to billboard
    //     Billboard billboard = lightning.GetComponent<Billboard>();
    //     if (billboard != null && cam != null)
    //     {
    //         billboard.targetCamera = cam;
    //     }
        
    //     // Destroy after duration
    //     Destroy(lightning, lightningDuration);
        
    //     // Only log on local player
    //     if (isLocalPlayer)
    //     {
    //         Debug.Log($"⚡ Lightning spawned at {spawnPosition}");
            
    //         if (detectedTargets.Count > 0)
    //         {
    //             Debug.Log($"⚡ Lightning struck {detectedTargets.Count} target(s)!");
    //         }
    //     }
    // }
    
    private void OnDrawGizmos()
    {
        if (!aimingMode || !hasHit) return;
        
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
        
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cam.transform.position, cam.transform.forward * maxDistance);
        }
        
        Gizmos.color = Color.red;
        foreach (Collider target in detectedTargets)
        {
            if (target != null)
                Gizmos.DrawWireSphere(target.transform.position, 0.5f);
        }
    }
    
    void OnDestroy()
    {
        if (targetingCylinder != null)
            Destroy(targetingCylinder);
        if (edgeRing != null)
            Destroy(edgeRing);
    }
}