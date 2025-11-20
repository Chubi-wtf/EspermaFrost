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
        // --- VISUALIZACIÓN DEBUG (RAYCAST) ---
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;
        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);
        }

        if (isUsingItem) return;

        // --- USO DE ÍTEMS DE INVENTARIO (1, 2, 3) ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemFromSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemFromSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemFromSlot(2);

        // --- INTERACCIÓN CON EL ENTORNO (E) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>().transform;
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance))
        {
            // 1. INTENTAR INTERACTUAR CON TERMINAL
            TerminalController terminal = hit.collider.GetComponent<TerminalController>();
            if (terminal == null) terminal = hit.collider.GetComponentInParent<TerminalController>();

            if (terminal != null)
            {
                terminal.ActivateTerminal();
                return;
            }

            // 2. INTENTAR INTERACTUAR CON PUERTA
            DoorController door = hit.collider.GetComponent<DoorController>();
            if (door != null)
            {
                string keyID = GetHeldKeyCardID();
                bool success = door.InteractDoor(keyID);

                if (!success) Debug.Log("Puerta bloqueada o sin KeyCard adecuada.");
                return;
            }

            // 3. INTENTAR RECOGER UN ÍTEM
            ItemConfig item = hit.collider.GetComponent<ItemConfig>();
            if (item != null)
            {
                PlayerInventory.Instance.TryAddItem(item);
                return;
            }

            Debug.Log("No hay nada interactuable aquí.");
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
                // Evitar usar KeyCards con botones numéricos
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
        // Chequeo especial para no gastar botiquín si la vida está llena
        if (itemTemplate.itemType == ItemTemplate.ITEM_TYPE.Botiquin && playerMovement.currentHealth >= playerMovement.maxHealth)
        {
            Debug.Log("Vida al máximo. No se puede usar el Botiquín.");
            return;
        }

        // Si el ítem tiene tiempo de uso (animación/delay), usar corrutina
        if (itemTemplate.useDuration > 0)
        {
            StartCoroutine(UseItemWithDuration(itemTemplate, slotIndex));
        }
        else
        {
            // Uso instantáneo
            ConsumeItemEffect(itemTemplate, slotIndex);
        }
    }

    private IEnumerator UseItemWithDuration(ItemTemplate itemTemplate, int slotIndex)
    {
        isUsingItem = true;
        Debug.Log($"Iniciando uso de {itemTemplate.itemName}. Tiempo: {itemTemplate.useDuration}s");

        yield return new WaitForSeconds(itemTemplate.useDuration);

        // Verificar que el jugador siga existiendo tras la espera
        if (playerMovement != null)
        {
            ConsumeItemEffect(itemTemplate, slotIndex);
        }

        isUsingItem = false;
    }

    private void ConsumeItemEffect(ItemTemplate itemTemplate, int slotIndex)
    {
        bool shouldConsume = true;

        switch (itemTemplate.itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                float healed = playerMovement.Heal(itemTemplate.healAmount);
                // Si devuelve 0 o menos, significa que no curó nada (probablemente vida llena que cambió durante el delay)
                if (healed <= 0)
                {
                    Debug.Log("Botiquín usado pero no curó (vida llena). Slot no se vacía.");
                    shouldConsume = false;
                }
                break;

            case ItemTemplate.ITEM_TYPE.Adrenalina:
                // Llama al método unificado en PlayerMovement
                playerMovement.ActivateAdrenaline(itemTemplate.adrenalineDuration);
                break;

            default:
                break;
        }

        // Solo eliminar del inventario si se usó exitosamente
        if (shouldConsume)
        {
            PlayerInventory.Instance.inventory_UI_Slots[slotIndex].ClearSlot(slotIndex);
        }
    }
}