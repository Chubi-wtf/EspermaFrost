using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HotBarSlot : MonoBehaviour
{
    public Image icon;
    public bool isOccupied;
    public ItemConfig storedItem;

    public void SetHotbarItem(ItemConfig item)
    {
        storedItem = item;
        icon.enabled = true;
        isOccupied = true;
    }

    public void UseItem()
    {
        if (storedItem == null) return;

        // Si el item es activable, lo activamos
        if (storedItem.objectToActivate != null)
        {
            storedItem.objectToActivate.SetActive(true);
        }

        Debug.Log("Item activado desde HotBar: " + storedItem.itemTemplate.itemName);
    }
}
