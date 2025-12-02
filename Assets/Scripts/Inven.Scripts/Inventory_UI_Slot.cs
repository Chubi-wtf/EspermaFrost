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
        slotImage.sprite = itemToSet.itemTemplate.itemSprite;
    }


    public void ClearSlot(int buttonIndex)
    {
        slotImage.sprite = null;
        gameObject.SetActive(false);

        if (PlayerInventory.Instance.inventory[buttonIndex] != null)
        {
            PlayerInventory.Instance.inventory[buttonIndex] = null;
        }
    }
}