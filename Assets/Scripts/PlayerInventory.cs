using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance; //SINGLETON

    public ItemTemplate[] inventory;
    public Inventory_UI_Slot[] inventory_UI_Slots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inventory = new ItemTemplate[3];
    }

    private void AddItem(ItemConfig itemToAdd)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = itemToAdd.itemTemplate;
                inventory_UI_Slots[i].SetSlot(itemToAdd);
                Destroy(itemToAdd.gameObject);
                break;
            }
        }
    }

    public bool CanUseItem(int itemIndex)
    {
        switch (inventory[itemIndex].itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                //IF EN COLLIDER CON EL CANDADO: return true
                //else: return false;
                return true;

            default:
                return true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ItemConfig>())
        {
            AddItem(other.GetComponent<ItemConfig>());
        }
    }
}