using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;

    public void Set(Sprite sprite, int count)
    {
        if (icon != null)
        {
            icon.enabled = sprite != null;
            icon.sprite = sprite;
        }
        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";
    }
}
