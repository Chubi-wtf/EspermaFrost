using System.Collections;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private bool isUsingItem = false;
    private PlayerMovement playerMovement;

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
        if (isUsingItem) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemFromSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemFromSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemFromSlot(2);
    }

    private void UseItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerInventory.Instance.inventory.Length) return;

        ItemTemplate itemToUse = PlayerInventory.Instance.inventory[slotIndex];

        if (itemToUse != null)
        {
            if (PlayerInventory.Instance.CanUseItem(slotIndex))
            {
                HandleItemAction(itemToUse, slotIndex);
            }
        }
    }

    private void HandleItemAction(ItemTemplate itemTemplate, int slotIndex)
    {
        switch (itemTemplate.itemType)
        {
            case ItemTemplate.ITEM_TYPE.Botiquin:
                if (playerMovement.currentHealth < playerMovement.maxHealth)
                {
                    StartCoroutine(UseBotiquin(itemTemplate, slotIndex));
                }
                else
                {
                    Debug.Log("Vida al máximo. No se puede usar el Botiquín.");
                }
                break;

            case ItemTemplate.ITEM_TYPE.Adrenalina:
                Debug.Log("Usando Adrenalina: Implementar efecto...");
                break;
        }
    }

    private IEnumerator UseBotiquin(ItemTemplate botiquinTemplate, int slotIndex)
    {
        isUsingItem = true;
        Debug.Log($"Iniciando uso de {botiquinTemplate.itemName}. Tiempo: {botiquinTemplate.useDuration}s");

        yield return new WaitForSeconds(botiquinTemplate.useDuration);

        if (playerMovement != null)
        {
            playerMovement.Heal(botiquinTemplate.healAmount);

            PlayerInventory.Instance.inventory_UI_Slots[slotIndex].ClearSlot(slotIndex);
        }

        isUsingItem = false;
    }
}