using UnityEngine;

[CreateAssetMenu(fileName = "WheelPickerItem", menuName = "WheelPicker/Item")]
public class WheelPickerItem : ScriptableObject
{
    public string itemName;

    public void OnClick()
    {
        Debug.Log("Using wheel item" + itemName);
    }
}
