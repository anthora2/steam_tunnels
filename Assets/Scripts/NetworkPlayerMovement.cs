using Mirror;
using UnityEngine;

public class NetworkPlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Only allow local player to move themselves
        if (!isLocalPlayer) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move);
    }
}
