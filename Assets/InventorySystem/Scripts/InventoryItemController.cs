using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemController : MonoBehaviour, IPointerUpHandler
{
    // When an inventory item is right-clicked, handle the item usage based on its name
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            try
            {
                HandleItemUsage(eventData);
            }
            catch (NullReferenceException)
            {
                // No-op
            }
        }
    }

    /*
     * 
     */
    private void HandleItemUsage(PointerEventData eventData)
    {
        // The index of the clicked slot
        int slotIndex = GetSlotIndex(eventData.pointerCurrentRaycast.gameObject);

        // The corresponding InventoryItem
        InventoryItem item = InventoryManager.instance.items[slotIndex];

        // If LeftControl is also clicked, use all items of the stack
        if (Input.GetKey(KeyCode.LeftControl))
        {
            UseAllItems(item, slotIndex);
        }
        // Otherwise, use the item only once
        else
        {
            UseSingleItem(item, slotIndex);
        }
    }

    /*
     * Get the slot index of the provided object, which should be a slot with name "Slot_x", where x is the index
     */
    private int GetSlotIndex(GameObject clickedObject)
    {
        return int.Parse(clickedObject.name.Split("_")[1]);
    }

    /*
     * Use the highest possible amount of items of the stack
     */
    private void UseAllItems(InventoryItem item, int slotIndex)
    {
        int itemsUsed = 0;

        switch (item.itemName)
        {
            case "Red Potion":
                itemsUsed = UseRedPotion(item.count);
                break;
            case "Blue Potion":
                itemsUsed = UseBluePotion(item.count);
                break;
            case "Green Potion":
                itemsUsed = UseGreenPotion(item.count);
                break;
            case "Skull":
                itemsUsed = UseSkull();
                break;
            default:
                return;
        }

        // If at least one item was used, update the inventory
        if (itemsUsed > 0)
        {
            UpdateInventory(item, itemsUsed, slotIndex);
        }
    }

    /*
     * Use a single item of the stack
     */
    private void UseSingleItem(InventoryItem item, int slotIndex)
    {
        bool itemUsed = false;

        switch (item.itemName)
        {
            case "Red Potion":
                itemUsed = UseRedPotion(1) > 0;
                break;
            case "Blue Potion":
                itemUsed = UseBluePotion(1) > 0;
                break;
            case "Green Potion":
                itemUsed = UseGreenPotion(1) > 0;
                break;
            case "Skull":
                itemUsed = UseSkull() > 0;
                break;
            default:
                return;
        }

        // If at least one item was used, update the inventory
        if (itemUsed)
        {
            UpdateInventory(item, 1, slotIndex);
        }
    }

    /*
     * Use the provided amount of red potions. Red potions increase the current health of the player by 25% of its max health. The "used" variable keeps track of the amount of potions actually used.
     */
    private int UseRedPotion(int quantity)
    {
        int used = 0;

        for (int i = 0; i < quantity && PlayerStatsManager.instance.health < PlayerStatsManager.instance.maxHealth; i++)
        {
            PlayerStatsManager.instance.health = Mathf.Min(PlayerStatsManager.instance.maxHealth, PlayerStatsManager.instance.health + PlayerStatsManager.instance.maxHealth / 4);
            PlayerStatsManager.instance.UpdateHealthBar();
            used++;
        }

        return used;
    }

    /*
     * Use the provided amount of blue potions. Blue potions increase the current mana of the player by 50% of its max mana. The "used" variable keeps track of the amount of potions actually used.
     */
    private int UseBluePotion(int quantity)
    {
        int used = 0;

        for (int i = 0; i < quantity && PlayerStatsManager.instance.mana < PlayerStatsManager.instance.maxMana; i++)
        {
            PlayerStatsManager.instance.mana = Mathf.Min(PlayerStatsManager.instance.maxMana, PlayerStatsManager.instance.mana + PlayerStatsManager.instance.maxMana / 2);
            PlayerStatsManager.instance.UpdateManaBar();
            used++;
        }

        return used;
    }

    /*
     * Use the provided amount of green potions. Green potions completely restore the stamina of the player. The "used" variable is kept for compatibility, but it's always 0 or 1.
     */
    private int UseGreenPotion(int quantity)
    {
        int used = 0;

        for (int i = 0; i < quantity && PlayerStatsManager.instance.stamina < PlayerStatsManager.instance.maxStamina; i++)
        {
            PlayerStatsManager.instance.stamina = PlayerStatsManager.instance.maxStamina;
            PlayerStatsManager.instance.UpdateStaminaBar();
            used++;
        }

        return used;
    }

    /*
     * Use the skull. A skull kills the player by setting its health to 0.
     */
    private int UseSkull()
    {
        PlayerStatsManager.instance.health = 0;
        PlayerStatsManager.instance.UpdateHealthBar();
        return 1;
    }

    /*
     * Updates the ItemCount information of the item, removing it from the inventory if ItemCount reaches 0. Please note that no control is performed on itemsUsed, so use carefully.
     */
    private void UpdateInventory(InventoryItem item, int itemsUsed, int slotIndex)
    {
        item.count -= itemsUsed;

        if (item.count == 0)
        {
            InventoryManager.instance.Remove(slotIndex);
        }

        InventoryManager.instance.RefreshInventory();  // TODO: why RefreshSlot(i) won't work?
    }
}
