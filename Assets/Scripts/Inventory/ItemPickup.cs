using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class ItemPickup : MonoBehaviour
{
    public Item item;

    [Header("Sounds")]
    public AudioClip pickupSound;

    private void Start()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
		Inventory inventory = other.GetComponentInParent<Inventory>();
		if (inventory == null)
			return;

		bool added = inventory.AddItem(item, 1);    // Hardcoded quantity of 1 for now

		if (added)
		{
            if (pickupSound != null)
            {
                Debug.Log("Playing pickup sound");
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
			Debug.Log($"Picked up {item.itemName}");
			Destroy(gameObject);
		}
		else
		{
			Debug.Log("Inventory full or too heavy!");
		}
    }
}
