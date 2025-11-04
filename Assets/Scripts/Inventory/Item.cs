using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {
    public string itemName;
    public Sprite itemSprite;
    public bool stackable;
    public int maxStackSize;
    public int weight;

    public virtual void Use() {
        Debug.Log("Using " + itemName);
    }
}