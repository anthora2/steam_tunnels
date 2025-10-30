using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject {
    public string ItemName;
    public Sprite ItemIcon;
    public bool IsStackable;
    public int MaxStackSize;
    public int Weight;

    public virtual void Use() {
        Debug.Log("Using " + ItemName);
    }
}