using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class WheelPickerOption : MonoBehaviour
{
    public int index = -1;
    private bool isSelected = false;

    private Image image;
    private Color backgroundColor = Color.white;
    private Color selectedColor = new Color(1f, 0.8f, 0.3f, 1f);

    // Intialize the wheel option
    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            // Store the background color of the image
            backgroundColor = image.color;

            // Set image options to create wheel effect
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = 0;
            image.fillClockwise = true;
        }
    }

    // Initialize the wheel option with the fill amount, rotation, and label text
    public void Initialize(float fillAmountNormalized, float rotationDegrees, string labelText)
    {
        if (image == null) image = GetComponent<Image>();
        
        if (image != null) image.fillAmount = fillAmountNormalized;
        
        // Set the rotation of the wheel option
        transform.localRotation = Quaternion.Euler(0, 0, -rotationDegrees);
    }

    // Set the selected state of the wheel option
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (image != null) image.color = selected ? selectedColor : backgroundColor;
    }
}
