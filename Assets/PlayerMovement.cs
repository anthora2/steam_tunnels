using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed  = 7.0f;
    public float rotationLerp = 12f; // higher = snappier turns

    private Animator animator;
    private Transform cam;

    void Awake()
    {
        animator = GetComponent<Animator>();           // needs an Animator on this object
        cam = Camera.main != null ? Camera.main.transform : null;
    }

    void Update()
    {
        // --- 1) Read input ---
        float x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float z = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        Vector2 input = new Vector2(x, z);
        bool hasInput = input.sqrMagnitude > 0.0001f;

        // --- 2) Choose walk vs run ---
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = running ? runSpeed : walkSpeed;

        // --- 3) Camera-relative movement vector (XZ only) ---
        Vector3 moveDir;
        if (cam != null)
        {
            Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
            Vector3 right   = cam.right;   right.y   = 0f; right.Normalize();
            moveDir = (forward * z + right * x);
        }
        else
        {
            // fallback if no camera found
            moveDir = new Vector3(x, 0f, z);
        }
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // --- 4) Move ---
        transform.position += moveDir * targetSpeed * Time.deltaTime;

        // --- 5) Face movement direction ---
        if (hasInput && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerp * Time.deltaTime);
        }

        // --- 6) Drive Animator parameter ("speed" float you created) ---
        // Normalize 0..1 based on runSpeed so Animator thresholds are easy to set
        float normalizedSpeed = hasInput ? (targetSpeed / runSpeed) : 0f; // 0 idle, ~0.5 walk, 1 run
        if (animator != null) animator.SetFloat("speed", normalizedSpeed, 0.1f, Time.deltaTime);
        // (the last two args damp the change so blends are smooth)
    }
}
