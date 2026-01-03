using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;  // Static object freely accessible in other scripts, being the inventory a unique object by design

    public const int MAX_ITEMS = 24;  // By design, the inventory can contain up to 24 elements without the possibility to increase its capacity
    public InventoryItem[] items;  // The items stored in the inventory
    public bool[] slotsOccupied;  // Each element is "true" if the slot is currently occupied by an item, "false" otherwise
    public int n_items;  // An optimization that prevents the iteration of the entire inventory, placed on-top of the Add() method

    public GameObject inventoryItem;  // From the editor, drag the Item prefab
    public Transform itemContent;  // From the editor, drag the Content object, child of the Inventory viewport
    public Image DiscardItemImage;  // From the editor, drag the DiscardItem object


    private void Awake()
    {
        instance = this;
        items = new InventoryItem[MAX_ITEMS];
        slotsOccupied = new bool[MAX_ITEMS];
        n_items = 0;
    }

    /*
     * Add an item to the inventory. There are several cases:
     *  • Case 1: If the item is not stackable and the inventory is full, cannot add
     *  • Case 2: If the item is stackable, iterate over the array to:
     *      • Case 2.1: Find a stack of the same item
     *      • Case 2.2: Find an empty slot
     */
    public bool Add(InventoryItem newItem)
    {
        // Case 1: Inventory is full and item is not stackable
        if (n_items == MAX_ITEMS && newItem.maxStackSize == 1)
            return false;

        // Check if the inventory is open
        bool isInventoryOpen = IsInventoryOpen();

        // Case 2: Stackable item
        if (newItem.maxStackSize > 1)
            return HandleStackableItem(newItem, isInventoryOpen);

        // Case 3: Non-stackable item
        return HandleNonStackableItem(newItem, isInventoryOpen);
    }

    /*
     * Checks if the inventory is open. GameObject.Find() is used because this method is only used when adding an item.
     * Just as an alternative to adding the Inventory from the editor (no variable stored).
     * 
     * The inventory is closed on start and can be opened with the Open Inventory button. If opened, it can be closed again
     * with the Close button. So, with this configuration:
     *  • If the inventory is up, then it's necessarily active (and vice versa)
     *  • If the inventory is not up, it's necessarily inactive, so Find() will return null
     */
    private bool IsInventoryOpen()
    {
        try
        {
            return GameObject.Find("Inventory").activeSelf;
        }
        catch (NullReferenceException)
        {
            return false;
        }
    }

    /*
     * This method covers the case of adding a stackable item. Even if the inventory is full, it must be
     * iterated anyway to search for a possible available stack of the same item.
     * TODO: implement a {item_id -> inventory_slots_where_it's_stored} hashmap to avoid the O(n) search
     */
    private bool HandleStackableItem(InventoryItem newItem, bool isInventoryOpen)
    {
        int emptySlot = -1;  // A reference to a possible empty slot

        // Iterate over the inventory
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            // If the slot is empty, save its index in emptySlot
            if (!slotsOccupied[i])
            {
                if (emptySlot == -1)
                    emptySlot = i;
            }
            // If the slot is occupied, check if it contains an available stack of the same item and store it there
            else
            {
                InventoryItem invItem = items[i];
                if (invItem.id == newItem.id && invItem.count < invItem.maxStackSize)
                {
                    invItem.count++;
                    if (isInventoryOpen) RefreshSlot(i);
                    return true;
                }
            }
        }

        // If no slot is available, return false (item not added); otherwise, store the item in a new stack and return true
        return emptySlot == -1 ? false : CreateNewStack(newItem, emptySlot, isInventoryOpen);
    }

    /*
     * Creates a new "stack" to store the item. If the inventory is open, refreshes the slot.
     * (optimization: before this, the entire inventory was refreshed)
     */
    private bool CreateNewStack(InventoryItem newItem, int emptySlot, bool isInventoryOpen)
{
        InventoryItem newStack = Instantiate(newItem);
        newStack.count = 1;
        items[emptySlot] = newStack;
        slotsOccupied[emptySlot] = true;
        n_items++;

        if (isInventoryOpen) RefreshSlot(emptySlot);
        return true;
    }

    /*
     * This method covers the case of adding a non-stackable item. The inventory must be iterated to search for a possible empty slot.
     * If no slot is found (the inventory is full), return false.
     */
    private bool HandleNonStackableItem(InventoryItem newItem, bool isInventoryOpen)
    {
        // Iterate the inventory to search for the first empty slot
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            // If an empty slot is found, instantiate the item "stack" with count 1 and add it to the slot.
            if (!slotsOccupied[i])
            {
                InventoryItem newStack = Instantiate(newItem);
                newStack.count = 1;
                items[i] = newStack;
                slotsOccupied[i] = true;
                n_items++;

                if (isInventoryOpen) RefreshSlot(i);

                return true;
            }
        }

        return false; // The inventory is full: cannot add the item
    }

    /*
     * Removes an item at the specified index from the inventory. This is done by clearing the slot data and refreshing the slot.
     */
    public void Remove(int index)
    {
        ClearSlotData(index);
        RefreshSlot(index);
    }

    /*
     * Frees the index slot by destroying the item and updating the slotsOccupied and items arrays
     */
    private void ClearSlotData(int index)
    {
        slotsOccupied[index] = false;
        Destroy(items[index]);
        items[index] = null;
    }

    /*
     * Instantiates the index slot with the default ItemIcon, ItemName and ItemCount.
     */
    private GameObject InstantiateSlot(int index)
    {
        GameObject slot = Instantiate(inventoryItem, itemContent);
        slot.name = "Slot_" + index.ToString();
        return slot;
    }

    /*
     * This method populates the specified slot fields with the provided inventory item. Being a non-empty slot, the
     * InventoryDragHandler (discard, drag, swap operations) and the InventoryItemController (use operations) components must be added.
     */
    private void PopulateSlot(GameObject slot, InventoryItem item)
    {
        AddRequiredComponents(slot);

        var itemName = slot.transform.Find("ItemName").GetComponent<Text>();
        var itemIcon = slot.transform.Find("ItemIcon").GetComponent<Image>();
        var itemCount = slot.transform.Find("ItemCount").GetComponent<Text>();

        itemName.text = item.itemName;
        itemIcon.sprite = item.icon;
        itemIcon.color = new Color(1, 1, 1, 1);
        itemCount.text = item.count.ToString();
    }

    /*
     * This method resets the ItemName, ItemIcon and ItemCount properties in order to view an empty slot.
     */
    private void ClearSlotVisual(GameObject slot)
    {
        var itemIcon = slot.transform.Find("ItemIcon").GetComponent<Image>();
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0);

        var itemName = slot.transform.Find("ItemName").GetComponent<Text>();
        itemName.text = "";

        var itemCount = slot.transform.Find("ItemCount").GetComponent<Text>();
        itemCount.text = "";
    }

    /*
     * Adds the InventoryDragHandler and the InventoryItemController components to the specified slot.
     */
    private void AddRequiredComponents(GameObject slot)
    {
        if (slot.GetComponent<InventoryDragHandler>() == null)
            slot.AddComponent<InventoryDragHandler>();
        if (slot.GetComponent<InventoryItemController>() == null)
            slot.AddComponent<InventoryItemController>();
    }

    /*
     * A method used in RefreshInventory(), and in the OnClick() method of the Open Inventory button aswell.
     * This method instantiates each inventory slot of the GridLayoutGroup: if the i-th slot is occupied,
     * populate it with the corresponding item; otherwise, clear its visual.
     */
    public void ListItems()
    {
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            GameObject slot = InstantiateSlot(i);

            if (slotsOccupied[i])
                PopulateSlot(slot, items[i]);
            else
                ClearSlotVisual(slot);
        }
    }

    /*
     * Destroys all the items transforms in the inventory Content. Used in RefreshInventory().
     */
    public void DestroyItems()
    {
        foreach (Transform item in itemContent)
            Destroy(item.gameObject);
    }

    /*
     * This method provides a (inefficient) way to refresh the inventory slots after some update about their content.
     * This is done by destroying all the Content items with DestroyItems() and refilling each slot with the correct
     * items info. However, the main operations with the inventory involve one item (use, change slot, discard) or
     * two items (swap slots, update stack counts, etc.) at most; but all the inventory slots are involved, so there's
     * a lot of overhead going on regarding the Destroy and Instantiate operations.
     * (optimization: refresh the involved slots with RefreshSlot(index))
     */
    public void RefreshInventory()
    {
        DestroyItems();
        ListItems();
    }

    /*
     * This method refreshes a single inventory slot by Destroying and Instantiating it with the updated information.
     * (Optimization: instead of using RefreshInventory(), it's possible to refresh a single slot to avoid unnecessary
     * Destroy and Instantiate operations on all the other slots.)
     */
    public void RefreshSlot(int index)
    {
        Transform slot = itemContent.GetChild(index);

        if (slotsOccupied[index])
            PopulateSlot(slot.gameObject, items[index]);
        else
        {
            InstantiateSlot(index);
            ClearSlotVisual(slot.gameObject);
        }
    }
}
