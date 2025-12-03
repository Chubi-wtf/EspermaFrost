using UnityEngine;
using System.Collections.Generic; // [NUEVO] Necesario para usar Listas

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance; //SINGLETON

    public ItemTemplate[] inventory;
    public Inventory_UI_Slot[] inventory_UI_Slots;
    public GameObject[] activateObjectsBySlot = new GameObject[3];

    // [NUEVO] --- EL LLAVERO PERMANENTE ---
    // Aquí se guardarán las IDs de las llaves ("KeyCard_Blue", "Key_Basement", etc.)
    // No ocupa espacio en la UI y no tiene límite de tamaño.
    public List<string> keyRing = new List<string>();
    // -------------------------------------

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        inventory = new ItemTemplate[3];
    }

    // [NUEVO] --- FUNCION PARA AÑADIR LLAVES ---
    public void AddKey(string keyID)
    {
        // Solo la agregamos si no la tenemos ya (para evitar duplicados raros)
        if (!keyRing.Contains(keyID))
        {
            keyRing.Add(keyID);
            Debug.Log("LLAVERO: Se añadió la llave con ID: " + keyID);

            // Opcional: Aquí podrías reproducir un sonido de "Llaves tintineando"
            // AudioManager.Instance.Play("KeysPickup");
        }
    }
    // ------------------------------------------

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

                if (activateObjectsBySlot[i] != null)
                {
                    activateObjectsBySlot[i].SetActive(true);
                }

                Destroy(itemToAdd.gameObject);
                return;
            }
        }

        Debug.Log("Inventario lleno. No se pudo recoger el ítem.");
    }

    public bool CanUseItem(int itemIndex)
    {
        // Protección extra por si el índice es inválido o el slot está vacío
        if (itemIndex < 0 || itemIndex >= inventory.Length || inventory[itemIndex] == null)
            return false;

        switch (inventory[itemIndex].itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                // Tu lógica futura para validar condiciones
                return true;

            default:
                return true;
        }
    }
}