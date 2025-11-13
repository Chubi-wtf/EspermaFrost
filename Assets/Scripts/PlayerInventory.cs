using UnityEngine;
// using static UnityEditor.Progress; // Esta línea puede causar errores si no estás en el editor, mejor eliminarla

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

    // NUEVO: Método público llamado por PlayerInteraction.cs
    public void TryAddItem(ItemConfig itemToAdd)
    {
        AddItem(itemToAdd);
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
                Debug.Log($"Ítem {itemToAdd.itemTemplate.itemName} recogido con éxito.");
                return; // Salir después de añadir
            }
        }
        Debug.Log("Inventario lleno. No se puede recoger el ítem.");
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
}