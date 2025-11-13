using System.Collections;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private bool isUsingItem = false;
    private PlayerMovement playerMovement;

    [Header("CONFIGURACIÓN DE INTERACCIÓN")]
    public float interactionDistance = 3f;


    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerInteraction requiere el script PlayerMovement en el mismo GameObject.");
        }
    }

    private void Update()
    {
        // Obtener la Transform de la cámara para el Raycast
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;

        // Dibujar el Raycast para depuración
        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);
        }

        if (isUsingItem) return;

        // Lógica de uso de ítems consumibles (1, 2, 3)
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemFromSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemFromSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemFromSlot(2);

        // Lógica de INTERACCIÓN (Puertas y Recogida de Ítems)
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction(); // Llamamos al nuevo método unificado
        }
    }

    // NUEVO MÉTODO: Maneja la interacción Raycast unificada
    private void HandleInteraction()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>().transform;
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance))
        {
            // 1. INTENTAR INTERACTUAR CON PUERTA
            DoorController door = hit.collider.GetComponent<DoorController>();
            if (door != null)
            {
                string keyID = GetHeldKeyCardID();
                bool success = door.InteractDoor(keyID);

                if (!success)
                {
                    Debug.Log("Puerta bloqueada o sin KeyCard adecuada.");
                }
                return; // Salir después de intentar la interacción con la puerta
            }

            // 2. INTENTAR RECOGER UN ÍTEM (Botiquín, Adrenalina, KeyCard, etc.)
            ItemConfig item = hit.collider.GetComponent<ItemConfig>();
            if (item != null)
            {
                // Llamamos al método de recogida en el inventario
                PlayerInventory.Instance.TryAddItem(item);
                return; // Salir después de intentar recoger el ítem
            }

            // Si el Raycast golpea algo, pero no es interactuable
            Debug.Log("No hay nada que interactuar aquí.");
        }
    }
    private string GetHeldKeyCardID()
    {
        for (int i = 0; i < PlayerInventory.Instance.inventory.Length; i++)
        {
            ItemTemplate item = PlayerInventory.Instance.inventory[i];

            if (item != null && item.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
            {
                return item.keyCardID;
            }
        }
        return string.Empty;
    }


    private void UseItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerInventory.Instance.inventory.Length) return;

        ItemTemplate itemToUse = PlayerInventory.Instance.inventory[slotIndex];

        if (itemToUse != null)
        {
            if (PlayerInventory.Instance.CanUseItem(slotIndex))
            {
                if (itemToUse.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
                {
                    Debug.Log("Las KeyCards se usan con la tecla de Interacción ('E') cerca de una puerta.");
                    return;
                }

                HandleItemAction(itemToUse, slotIndex);
            }
        }
    }

    private void HandleItemAction(ItemTemplate itemTemplate, int slotIndex)
    {
        if (itemTemplate.itemType == ItemTemplate.ITEM_TYPE.Botiquin && playerMovement.currentHealth >= playerMovement.maxHealth)
        {
            Debug.Log("Vida al máximo. No se puede usar el Botiquín.");
            return;
        }

        if (itemTemplate.useDuration > 0)
        {
            StartCoroutine(UseItemWithDuration(itemTemplate, slotIndex));
        }
        else
        {
            ConsumeItemEffect(itemTemplate, slotIndex);
        }
    }

    private void ConsumeItemEffect(ItemTemplate itemTemplate, int slotIndex)
    {
        switch (itemTemplate.itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                playerMovement.Heal(itemTemplate.healAmount);
                break;

            case ItemTemplate.ITEM_TYPE.Adrenalina:
                playerMovement.ActivateAdrenaline(itemTemplate.adrenalineDuration);
                break;
        }

        PlayerInventory.Instance.inventory_UI_Slots[slotIndex].ClearSlot(slotIndex);
    }


    private IEnumerator UseItemWithDuration(ItemTemplate itemTemplate, int slotIndex)
    {
        isUsingItem = true;
        Debug.Log($"Iniciando uso de {itemTemplate.itemName}. Tiempo: {itemTemplate.useDuration}s");

        yield return new WaitForSeconds(itemTemplate.useDuration);

        if (playerMovement != null)
        {
            ConsumeItemEffect(itemTemplate, slotIndex);
        }

        isUsingItem = false;
    }
}