using UnityEngine;

public class Zone : MonoBehaviour
{
    public string zoneName = "Unnamed Zone";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered the zone: {other.name}");
        if (other.CompareTag("Player") || 
        (other.transform.root.CompareTag("Player")))
        {
            Debug.Log($"✅ Player entered zone: {zoneName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"🚪 Player left zone: {zoneName}");
        }
    }
}
