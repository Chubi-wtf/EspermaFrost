using System.Collections;
using System.Collections.Generic; // Necesario para usar Listas
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private bool isUsingItem = false;
    private PlayerMovement playerMovement;

    [Header("CONFIGURACIÓN DE INTERACCIÓN")]
    public float interactionDistance = 3f;

    [Header("CONFIGURACIÓN CROSSHAIR")]
    public GameObject normalCrosshairGO;
    public GameObject interactCrosshairGO;
    public LayerMask interactableLayers;

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
        if (normalCrosshairGO != null) normalCrosshairGO.SetActive(true);
        if (interactCrosshairGO != null) interactCrosshairGO.SetActive(false);
    }

    private void Update()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>()?.transform;

        if (cameraTransform != null)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.red);
            HandleCrosshairVisuals(cameraTransform);
        }

        if (isUsingItem) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemFromSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemFromSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemFromSlot(2);

        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
    }

    private void HandleCrosshairVisuals(Transform camTransform)
    {
        if (normalCrosshairGO == null || interactCrosshairGO == null) return;

        RaycastHit hit;
        bool isInteractable = Physics.Raycast(camTransform.position, camTransform.forward, out hit, interactionDistance, interactableLayers);

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

    private void HandleInteraction()
    {
        Transform cameraTransform = playerMovement.GetComponentInChildren<Camera>().transform;
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance))
        {
            // 1. TERMINAL
            TerminalController terminal = hit.collider.GetComponent<TerminalController>();
            if (terminal == null) terminal = hit.collider.GetComponentInParent<TerminalController>();

            if (terminal != null)
            {
                terminal.ActivateTerminal();
                return;
            }

            // 2. PUERTA [MODIFICADO PARA USAR EL LLAVERO]
            DoorController door = hit.collider.GetComponent<DoorController>();
            if (door != null)
            {
                // Intentamos abrir la puerta con CUALQUIERA de las llaves que tengamos en el llavero
                if (TryOpenDoorWithKeyRing(door))
                {
                    Debug.Log("¡Puerta abierta con una llave del llavero!");
                }
                else
                {
                    // Si llegamos aquí, probamos todas las llaves y ninguna sirvió (o no tenemos llaves)
                    // Llamamos a interact con string vacío para que la puerta haga su sonido de "Bloqueado"
                    door.InteractDoor("");
                    Debug.Log("Está cerrada y no tienes la llave correcta.");
                }
                return;
            }

            // 3. RECOGER ÍTEM [MODIFICADO]
            ItemConfig item = hit.collider.GetComponent<ItemConfig>();
            if (item != null)
            {
                // [NUEVO] Verificamos si es una llave antes de meterla al inventario
                if (item.itemTemplate.itemType == ItemTemplate.ITEM_TYPE.KeyCard)
                {
                    // Es una llave: La mandamos al "Llavero" invisible
                    PlayerInventory.Instance.AddKey(item.itemTemplate.keyCardID);

                    // Destruimos el objeto del mundo porque ya lo "tenemos"
                    Destroy(item.gameObject);
                    Debug.Log("Llave recogida y guardada en el llavero permanente.");
                }
                else
                {
                    // No es llave (es poción, arma, etc): Va al inventario normal
                    PlayerInventory.Instance.TryAddItem(item);
                }
                return;
            }
        }
    }

    // [NUEVO] Función inteligente que prueba tus llaves en la puerta
    private bool TryOpenDoorWithKeyRing(DoorController door)
    {
        // Accedemos a la lista 'keyRing' que creaste en el PlayerInventory
        List<string> myKeys = PlayerInventory.Instance.keyRing;

        // Probamos cada llave que tenemos guardada
        foreach (string keyID in myKeys)
        {
            // Le preguntamos a la puerta: "¿Te abres con esta llave?"
            bool opened = door.InteractDoor(keyID);

            if (opened)
            {
                return true; // ¡Sí abrió! Dejamos de buscar
            }
        }

        return false; // Probamos todas y ninguna funcionó
    }

    // --- EL RESTO DE TUS MÉTODOS DE INVENTARIO SIGUEN IGUAL ---

    private void UseItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerInventory.Instance.inventory.Length) return;
        ItemTemplate itemToUse = PlayerInventory.Instance.inventory[slotIndex];

        if (itemToUse != null && PlayerInventory.Instance.CanUseItem(slotIndex))
        {
            // Ya no necesitamos validar KeyCard aquí porque nunca llegarán a los slots
            HandleItemAction(itemToUse, slotIndex);
        }
    }

    private void HandleItemAction(ItemTemplate itemTemplate, int slotIndex)
    {
        if (itemTemplate.itemType == ItemTemplate.ITEM_TYPE.Botiquin && playerMovement.currentHealth >= playerMovement.maxHealth)
        {
            Debug.Log("Vida al máximo.");
            return;
        }

        if (itemTemplate.useDuration > 0) StartCoroutine(UseItemWithDuration(itemTemplate, slotIndex));
        else ConsumeItemEffect(itemTemplate, slotIndex);
    }

    private void ConsumeItemEffect(ItemTemplate itemTemplate, int slotIndex)
    {
        bool shouldConsume = true;

        switch (itemTemplate.itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                if (playerMovement.Heal(itemTemplate.healAmount) <= 0) shouldConsume = false;
                break;
            case ItemTemplate.ITEM_TYPE.Adrenalina:
                playerMovement.ActivateAdrenaline(itemTemplate.adrenalineDuration);
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
        if (playerMovement != null) ConsumeItemEffect(itemTemplate, slotIndex);
        isUsingItem = false;
    }
}