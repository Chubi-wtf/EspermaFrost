using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    #region Variables de Configuración

    [Header("CONFIGURACIÓN DE INTERACCIÓN")]
    [Tooltip("Distancia máxima para interactuar con objetos")]
    public float interactionDistance = 3f;

    [Header("CONFIGURACIÓN CROSSHAIR")]
    public GameObject normalCrosshairGO;
    public GameObject interactCrosshairGO;
    public LayerMask interactableLayers;

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

    private void Start()
    {
        // Inicializar crosshairs
        if (normalCrosshairGO != null) normalCrosshairGO.SetActive(true);
        if (interactCrosshairGO != null) interactCrosshairGO.SetActive(false);
    }

    private void Update()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;

        // Dibujar el Raycast para depuración (visible en Scene view)
        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);

            // Actualizar crosshair dinámico
            HandleCrosshairVisuals(cameraTransform);
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

    #region Sistema de Crosshair Dinámico

    /// <summary>
    /// Cambia el crosshair según si hay un objeto interactuable en la mira
    /// </summary>
    private void HandleCrosshairVisuals(Transform camTransform)
    {
        if (normalCrosshairGO == null || interactCrosshairGO == null) return;

        RaycastHit hit;
        bool isInteractable = Physics.Raycast(
            camTransform.position,
            camTransform.forward,
            out hit,
            interactionDistance,
            interactableLayers
        );

        if (isInteractable)
        {
            if (!interactCrosshairGO.activeSelf)
            {
                normalCrosshairGO.SetActive(false);
                interactCrosshairGO.SetActive(true);
            }
        }
        else
        {
            if (!normalCrosshairGO.activeSelf)
            {
                normalCrosshairGO.SetActive(true);
                interactCrosshairGO.SetActive(false);
            }
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

        // Prioridad 3: PUERTA (con sistema de llavero)
        if (TryInteractDoor(hit)) return;

        // Prioridad 4: RECOGER ÍTEM (con llavero automático)
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
    /// Intenta interactuar con una Puerta usando el sistema de llavero
    /// </summary>
    private bool TryInteractDoor(RaycastHit hit)
    {
        DoorController door = hit.collider.GetComponent<DoorController>();

        if (door != null)
        {
            // SISTEMA DE LLAVERO: Intentar con todas las llaves disponibles
            if (TryOpenDoorWithKeyRing(door))
            {
                Debug.Log("¡Puerta abierta con una llave del llavero!");
            }
            else
            {
                // Si no se pudo abrir, reproducir sonido de bloqueado
                door.InteractDoor("");
                Debug.Log("Está cerrada y no tienes la llave correcta.");
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Prueba todas las llaves del llavero en la puerta
    /// </summary>
    private bool TryOpenDoorWithKeyRing(DoorController door)
    {
        List<string> myKeys = PlayerInventory.Instance.keyRing;

        foreach (string keyID in myKeys)
        {
            bool opened = door.InteractDoor(keyID);
            if (opened)
            {
                return true; // Llave correcta encontrada
            }
        }

        return false; // Ninguna llave funcionó
    }

    /// <summary>
    /// Intenta recoger un ítem del suelo (con sistema de llavero automático)
    /// </summary>
    private bool TryPickupItem(RaycastHit hit)
    {
        ItemConfig item = hit.collider.GetComponent<ItemConfig>();

        if (item != null)
        {
            // Verificar si es una KeyCard
            if (item.itemTemplate.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
            {
                // Las llaves van al llavero permanente (no ocupan slots)
                PlayerInventory.Instance.AddKey(item.itemTemplate.keyCardID);
                Destroy(item.gameObject);
                Debug.Log("Llave recogida y guardada en el llavero permanente.");
            }
            else
            {
                // Ítems normales van al inventario
                PlayerInventory.Instance.TryAddItem(item);
                Debug.Log("Ítem recogido.");
            }
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

        // Las KeyCards ahora están en el llavero, no deberían llegar aquí
        if (itemToUse.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
        {
            Debug.Log("Las KeyCards se usan automáticamente con la tecla 'E' cerca de una puerta.");
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
}