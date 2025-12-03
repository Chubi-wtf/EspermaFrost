using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    #region Variables de Configuración

    [Header("CONFIGURACIÓN DE INTERACCIÓN")]
    [Tooltip("Distancia máxima para interactuar con objetos")]
    public float interactionDistance = 3f;

    #endregion

    #region Referencias Internas

    private PlayerMovement playerMovement;
    private bool isUsingItem = false;

    #endregion

    #region Métodos de Unity

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
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;

        // Dibujar el Raycast para depuración (visible en Scene view)
        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);
        }

        // Comprobar objetos comentables (cada frame)
        HandleGazeComments(cameraTransform);

        // Si está usando un ítem, no permitir otras acciones
        if (isUsingItem) return;

        // Uso de ítems consumibles con teclas 1, 2, 3
        HandleItemUsage();

        // Interacción con objetos (Tecla E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction(cameraTransform);
        }
    }

    #endregion

    #region Sistema de Comentarios por Mirada

    /// <summary>
    /// Maneja los comentarios automáticos al mirar objetos
    /// </summary>
    private void HandleGazeComments(Transform cameraTransform)
    {
        if (cameraTransform == null) return;

        int layerMask = LayerMask.GetMask("InteractableNumbers");
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, layerMask))
        {
            CommentableObject commentable = hit.collider.GetComponent<CommentableObject>();
            if (commentable == null)
                commentable = hit.collider.GetComponentInParent<CommentableObject>();

            if (commentable != null)
            {
                commentable.ShowRandomComment();
            }
        }
    }

    #endregion

    #region Sistema de Interacción (Tecla E)

    /// <summary>
    /// Maneja todas las interacciones con objetos (Puertas, Terminales, Radio, Ítems)
    /// </summary>
    private void HandleInteraction(Transform cameraTransform)
    {
        if (cameraTransform == null) return;

        int layerMask = LayerMask.GetMask("InteractableNumbers");
        RaycastHit hit;

        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, layerMask))
        {
            Debug.Log("No hay nada en el rango de interacción.");
            return;
        }

        // Prioridad 1: TERMINAL
        if (TryInteractTerminal(hit)) return;

        // Prioridad 2: RADIO DIALOGUE
        if (TryInteractRadio(hit)) return;

        // Prioridad 3: PUERTA
        if (TryInteractDoor(hit)) return;

        // Prioridad 4: RECOGER ÍTEM
        if (TryPickupItem(hit)) return;

        Debug.Log("No hay nada interactuable aquí.");
    }

    #endregion

    #region Interacciones Específicas

    /// <summary>
    /// Intenta interactuar con un Terminal
    /// </summary>
    private bool TryInteractTerminal(RaycastHit hit)
    {
        TerminalController terminal = hit.collider.GetComponent<TerminalController>();
        if (terminal == null)
            terminal = hit.collider.GetComponentInParent<TerminalController>();

        if (terminal != null)
        {
            terminal.ActivateTerminal();
            Debug.Log("Terminal activado.");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Intenta interactuar con una Radio (Diálogo)
    /// </summary>
    private bool TryInteractRadio(RaycastHit hit)
    {
        RadioDialogue radio = hit.collider.GetComponent<RadioDialogue>();
        if (radio == null)
            radio = hit.collider.GetComponentInParent<RadioDialogue>();

        if (radio != null)
        {
            Debug.Log("Intentando iniciar diálogo por radio...");
            radio.StartDialogue();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Intenta interactuar con una Puerta
    /// </summary>
    private bool TryInteractDoor(RaycastHit hit)
    {
        DoorController door = hit.collider.GetComponent<DoorController>();

        if (door != null)
        {
            string keyID = GetHeldKeyCardID();
            bool success = door.InteractDoor(keyID);

            if (!success)
            {
                Debug.Log("Puerta bloqueada o sin KeyCard adecuada.");
            }
            else
            {
                Debug.Log("Puerta interactuada correctamente.");
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Intenta recoger un ítem del suelo
    /// </summary>
    private bool TryPickupItem(RaycastHit hit)
    {
        ItemConfig item = hit.collider.GetComponent<ItemConfig>();

        if (item != null)
        {
            PlayerInventory.Instance.TryAddItem(item);
            Debug.Log("Ítem recogido.");
            return true;
        }
        return false;
    }

    #endregion

    #region Sistema de Uso de Ítems

    /// <summary>
    /// Maneja el uso de ítems desde el inventario con las teclas 1, 2, 3
    /// </summary>
    private void HandleItemUsage()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemFromSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemFromSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemFromSlot(2);
    }

    private void UseItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerInventory.Instance.inventory.Length) return;

        ItemTemplate itemToUse = PlayerInventory.Instance.inventory[slotIndex];
        if (itemToUse == null) return;

        if (!PlayerInventory.Instance.CanUseItem(slotIndex)) return;

        // Las KeyCards no se usan así
        if (itemToUse.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
        {
            Debug.Log("Las KeyCards se usan con la tecla de Interacción ('E') cerca de una puerta.");
            return;
        }

        HandleItemAction(itemToUse, slotIndex);
    }

    private void HandleItemAction(ItemTemplate itemTemplate, int slotIndex)
    {
        // No usar botiquín si la vida está al máximo
        if (itemTemplate.itemType == ItemTemplate.ITEM_TYPE.Botiquin &&
            playerMovement.currentHealth >= playerMovement.maxHealth)
        {
            Debug.Log("Vida al máximo. No se puede usar el Botiquín.");
            return;
        }

        // Si tiene duración de uso, esperar
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
        bool shouldConsume = true;

        switch (itemTemplate.itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                float healed = playerMovement.Heal(itemTemplate.healAmount);
                if (healed <= 0)
                {
                    Debug.Log("Botiquín usado pero no curó. Slot no se vacía.");
                    shouldConsume = false;
                }
                break;

            case ItemTemplate.ITEM_TYPE.Adrenalina:
                playerMovement.ActivateAdrenaline(itemTemplate.adrenalineDuration);
                break;

            default:
                break;
        }

        if (shouldConsume)
        {
            PlayerInventory.Instance.inventory_UI_Slots[slotIndex].ClearSlot(slotIndex);
        }
    }

    private IEnumerator UseItemWithDuration(ItemTemplate itemTemplate, int slotIndex)
    {
        isUsingItem = true;
        yield return new WaitForSeconds(itemTemplate.useDuration);

        if (playerMovement != null)
        {
            ConsumeItemEffect(itemTemplate, slotIndex);
        }

        isUsingItem = false;
    }

    #endregion

    #region Métodos de Utilidad

    /// <summary>
    /// Obtiene el ID de la KeyCard que está en el inventario
    /// </summary>
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

    #endregion
}