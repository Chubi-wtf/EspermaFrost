using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI instance;

    public ItemSlot selectedSlot;

    public Text itemNameText;
    public Image itemIcon;

    private void Awake()
    {
        instance = this;
    }

    public void SelectItem(ItemSlot slot)
    {
        selectedSlot = slot;

        itemNameText.text = slot.item.itemName;
     //   itemIcon.sprite = slot.item.icon;
    }
}

