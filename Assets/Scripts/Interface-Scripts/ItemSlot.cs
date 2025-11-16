using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ItemTemplate item;
    public Image highlight;

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUI.instance.SelectItem(this);
    }
}
