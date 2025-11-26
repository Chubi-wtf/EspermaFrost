using UnityEngine;
// using static UnityEditor.Progress; // Esta línea puede causar errores si no estás en el editor, mejor eliminarla

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance; //SINGLETON

    public ItemTemplate[] inventory;
    public Inventory_UI_Slot[] inventory_UI_Slots;
    public GameObject[] activateObjectsBySlot = new GameObject[2];


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inventory = new ItemTemplate[2]; 
      //  inventory = new ItemTemplate[1];

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
                // Guardar el item
                inventory[i] = itemToAdd.itemTemplate;

                // Actualizar UI
                inventory_UI_Slots[i].SetSlot(itemToAdd);

                // --- ACTIVAR OBJETO SEGÚN EL SLOT ---
                if (activateObjectsBySlot[i] != null)
                {
                    activateObjectsBySlot[i].SetActive(true);
                    Debug.Log("Activando objeto del slot: " + i);
                }

                Destroy(itemToAdd.gameObject);
                return;
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
}