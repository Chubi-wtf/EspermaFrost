using UnityEngine;

public class Slot : MonoBehaviour
{
    public ScrollCenterOnSelect scroller;

    public void SelectThisSlot()
    {
        scroller.CenterOnItem(GetComponent<RectTransform>());
    }
}
