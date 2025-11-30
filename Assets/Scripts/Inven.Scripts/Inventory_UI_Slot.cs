using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_UI_Slot : MonoBehaviour
{
    public Image slotImage;
    public TextMeshProUGUI slotText;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
        slotText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Cambiado de 'Item' a 'ItemConfig'
    public void SetSlot(ItemConfig itemToSet)
    {
        slotImage.color = itemToSet.itemTemplate.itemColor;
        slotText.text = itemToSet.itemTemplate.itemName;
    }


    public void ClearSlot(int buttonIndex)
    {
        slotImage.color = Color.white;
        slotText.text = null;

        if (PlayerInventory.Instance.inventory[buttonIndex] != null)
        {
            PlayerInventory.Instance.inventory[buttonIndex] = null;
        }
    }
}