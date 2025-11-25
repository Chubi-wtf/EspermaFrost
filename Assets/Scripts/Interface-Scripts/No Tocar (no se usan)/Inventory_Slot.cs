using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Slot : MonoBehaviour
{
    public Image icon;
    public ItemConfig storedItem;
    public int slotIndex;

    public void SetSlot(ItemConfig item)
    {
        storedItem = item;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        storedItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // CLICK DEL SLOT
    public void OnClickSlot()
    {
        if (storedItem == null) return;

        // Enviar a la hotbar
        HotBar.Instance.PlaceItemInHotbar(storedItem);

        Debug.Log("Mandado a HotBar: " + storedItem.itemTemplate.itemName);
    }
}
