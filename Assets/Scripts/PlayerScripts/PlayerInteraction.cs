using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
<<<<<<< HEAD
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
=======
>>>>>>> parent of 14a199d (asdasd)
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

    private void Start()
    {
        // Inicializar crosshairs
        if (normalCrosshairGO != null) normalCrosshairGO.SetActive(true);
        if (interactCrosshairGO != null) interactCrosshairGO.SetActive(false);
    }

    private void Update()
    {
        // Obtener la Transform de la cámara para el Raycast
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;
        // Dibujar el Raycast para depuración
        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);

            // Actualizar crosshair dinámico
            HandleCrosshairVisuals(cameraTransform);
        }

        // Comprobar si estás mirando un objeto comentable (cada frame)
        HandleGazeComments(cameraTransform);

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

<<<<<<< HEAD
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
=======
    // Método para comentarios al mirar
>>>>>>> parent of 14a199d (asdasd)
    private void HandleGazeComments(Transform cameraTransform)
    {
        if (cameraTransform == null) return;

        int layerMask = LayerMask.GetMask("InteractableNumbers");

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, layerMask))
        {
            // LOG: Raycast hit algo
            Debug.Log("Raycast hit en: " + hit.collider.gameObject.name);

            CommentableObject commentable = hit.collider.GetComponent<CommentableObject>();
            if (commentable == null) commentable = hit.collider.GetComponentInParent<CommentableObject>();

            if (commentable != null)
            {
<<<<<<< HEAD
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
=======
                // LOG: Encontró CommentableObject
                Debug.Log("Encontró CommentableObject en " + commentable.gameObject.name + ". Llamando ShowRandomComment.");
                commentable.ShowRandomComment(); // Llama al método en el objeto
            }
            else
            {
                Debug.Log("No encontró CommentableObject en el hit.");
            }
>>>>>>> parent of 14a199d (asdasd)
        }
        else
        {
            // LOG: No hit
            Debug.Log("No raycast hit en InteractableNumbers layer.");
        }
    }

    private void HandleInteraction()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>().transform;
        int layerMask = LayerMask.GetMask("InteractableNumbers");

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, layerMask))
        {
            // 1. INTENTAR INTERACTUAR CON TERMINAL
            TerminalController terminal = hit.collider.GetComponent<TerminalController>();
            if (terminal == null)
            {
                terminal = hit.collider.GetComponentInParent<TerminalController>();
            }
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
                if (!success)
                {
                    Debug.Log("Puerta bloqueada o sin KeyCard adecuada.");
                }
                return;
            }
            // 3. INTENTAR RECOGER UN ÍTEM (Botiquín, Adrenalina, KeyCard, etc.)
            ItemConfig item = hit.collider.GetComponent<ItemConfig>();
            if (item != null)
            {
                PlayerInventory.Instance.TryAddItem(item);
                return;
            }
            // 4. INTENTAR INICIAR DIÁLOGO POR RADIO
            RadioDialogue radio = hit.collider.GetComponent<RadioDialogue>();
            if (radio == null)
            {
                radio = hit.collider.GetComponentInParent<RadioDialogue>();
            }
            if (radio != null)
            {
                radio.StartDialogue();
                return;
            }
            Debug.Log("No hay nada que interactuar aquí.");
        }
    }


<<<<<<< HEAD
=======
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
            return;
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
>>>>>>> parent of 14a199d (asdasd)
        if (playerMovement != null)
        {
            ConsumeItemEffect(itemTemplate, slotIndex);
        }
<<<<<<< HEAD

        isUsingItem = false;
    }

    #endregion
=======
        isUsingItem = false;
    }
>>>>>>> parent of 14a199d (asdasd)
}