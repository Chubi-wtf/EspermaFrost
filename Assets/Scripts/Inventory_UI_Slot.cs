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

    // CORREGIDO (Arregla Errores 1, 10, 11, 12)
    // Cambiado de 'Item' a 'ItemConfig'
    public void SetSlot(ItemConfig itemToSet)
    {
        slotImage.color = itemToSet.itemTemplate.itemColor;
        slotText.text = itemToSet.itemTemplate.itemName;
    }

    // --- M TODO CORREGIDO ---
    // Este m todo ahora es llamado por PlayerInteraction DESPU S de usar el  tem.
    // Su  nica responsabilidad es limpiar la UI y el inventario.
    public void ClearSlot(int buttonIndex)
    {
        // 1. Limpiar la UI
        slotImage.color = Color.white;
        slotText.text = null;

        // 2. Limpiar el inventario de datos
        if (PlayerInventory.Instance.inventory[buttonIndex] != null)
        {
            PlayerInventory.Instance.inventory[buttonIndex] = null;
        }
    }
}