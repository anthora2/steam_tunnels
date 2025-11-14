using UnityEngine;

public class TestDamageZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Checks the player and any parent object for HeartHUD
        HeartHUD hud = other.GetComponentInParent<HeartHUD>();
        if (hud != null)
        {
            hud.CmdTakeDamage(1);
        }
    }
}
