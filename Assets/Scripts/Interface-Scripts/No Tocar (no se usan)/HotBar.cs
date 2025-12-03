using UnityEngine;
using UnityEngine.UI;

public class HotBar : MonoBehaviour
{
    public static HotBar Instance;

    public HotBarSlot[] hotbarSlots;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaceItemInHotbar(ItemConfig item)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (!hotbarSlots[i].isOccupied)
            {
                hotbarSlots[i].SetHotbarItem(item);
                return;
            }
        }

        Debug.Log("HotBar llena");
    }
}
