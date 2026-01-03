using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;
using NUnit.Framework.Internal;

public class InventoryDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameObject draggedObject;
    private Canvas draggedObjectCanvas;
    private Camera mainCamera;
    private Vector3 offset;

    private InventoryItem[] items;
    private bool[] slotsOccupied;

    void Start()
    {
        mainCamera = Camera.main;
        items = InventoryManager.instance.items;
        slotsOccupied = InventoryManager.instance.slotsOccupied;
    }

    /*
     * Update the info about the mouse position during the drag
     */
    void Update()
    {
        if (draggedObject != null)
        {
            draggedObject.transform.position = GetMousePosition() + offset;
        }
    }

    /* 
     * In inventory, when hovering an item, if you click the mouse left button, start dragging and enable the "Discard Item" section
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            StartDragging(eventData);
        }
    }

    private void StartDragging(PointerEventData eventData)
    {
        // Compute the offset
        offset = transform.position - GetMousePosition();

        // Get a reference on the clicked object
        draggedObject = eventData.pointerCurrentRaycast.gameObject;

        // Add a new Canvas component with a high sortingOrder to prevent it from being hidden
        draggedObjectCanvas = AddCanvasToDraggedObject(draggedObject);

        // Activate the "Discard Item" section
        InventoryManager.instance.DiscardItemImage.gameObject.SetActive(true);
    }

    private Canvas AddCanvasToDraggedObject(GameObject obj)
    {
        Canvas canvas = obj.GetOrAddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999;
        return canvas;
    }

    /* 
     * When "dropping" the item, several cases may occur:
     * • Case 1: the drop occurs on the Discard Item section -> discard the item
     * • Case 2: the drop occurs on an inventory slot. Now:
     *      • Case 2.1: if the slot is the same, just refresh the inventory with a no-op
     *      • Case 2.2: if the new slot contains the same item
     *          • Case 2.2.1: if both the slots stack contain the same count of the item, just refresh the inventory with a no-op
     *          • Case 2.2.2: if the counts sum up to a number that is <= to the max stack size, free the starting slot and transfer its count to the dest-slot stack
     *          • Case 2.2.3: if the counts sum up to a number that is > than the max stack size, transfer the highest possible amount of items to the dest-slot
     *      • Case 2.3: if the starting slot contains a non-stackable item OR the dest slot is free OR the item IDs are different -> swap the items
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        try
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                HandleDrop(eventData);
            }
        }
        catch (IndexOutOfRangeException)
        {
            // GridLayoutGroup: if the drop occurs on an out-of-bound cell of the group, just refresh the starting slot
            InventoryManager.instance.RefreshSlot(GetSlotIndex(draggedObject.name));
            ResetDraggedObject();
        }
    }

    private void HandleDrop(PointerEventData eventData)
    {
        // Get a reference to the GameObject caught by the dropping
        GameObject newSlot = eventData.pointerCurrentRaycast.gameObject;

        // Case 1: Discard the item
        if (IsDropOnDiscard(newSlot))
        {
            DiscardItem();
            return;
        }

        // If not discarded, deactivate the Discard Item section
        InventoryManager.instance.DiscardItemImage.gameObject.SetActive(false);

        // Case 2: the drop occurs on an inventory slot
        if (draggedObject != null && newSlot != null && newSlot.name.StartsWith("Slot") && eventData.button == PointerEventData.InputButton.Left)
        {
            ProcessSlotSwap(eventData, newSlot);
        }

        // Reset the info about the dragged object
        ResetDraggedObject();

        // Refresh the inventory in any case
        InventoryManager.instance.RefreshInventory();  // TODO: why RefreshSlot(i) and RefreshSlot(j) won't work?
    }

    /*
     * Returns true if, while using the inventory, an inventory item is dragged and it's dropped on the Discard Item section.
     */
    private bool IsDropOnDiscard(GameObject newSlot)
    {
        return draggedObject != null && newSlot != null && newSlot.name == "DiscardItem";
    }

    /*
     * The item discard operation removes it from the inventory.
     */
    private void DiscardItem()
    {
        // Get the slot index based on the object name
        int index = GetSlotIndex(draggedObject.name);

        // Remove the item from the inventory based on its index
        InventoryManager.instance.Remove(index);

        // Reset the info about the dragged object
        ResetDraggedObject();
    }

    private void ProcessSlotSwap(PointerEventData eventData, GameObject newSlot)
    {
        // Get a reference to the starting slot and the destination slot
        int startingSlot = GetSlotIndex(draggedObject.name);
        int destSlot = GetSlotIndex(newSlot.name);

        // Case 2.1: no-op
        if (startingSlot == destSlot)
        {
            return;
        }

        // Case 2.2: different slots, but same item
        if (AreSameStackableItems(startingSlot, destSlot))
        {
            HandleStackableItemSwap(startingSlot, destSlot);
        }
        // Case 2.3: swap the slots content
        else
        {
            SwapSlotsContent(startingSlot, destSlot, newSlot);
        }
    }

    private int GetSlotIndex(string slotName)
    {
        return int.Parse(slotName.Split("_")[1]);
    }

    private bool AreSameStackableItems(int startingSlot, int destSlot)
    {
        return items[startingSlot].maxStackSize > 1 && items[destSlot] != null && items[startingSlot].id == items[destSlot].id;
    }

    private void HandleStackableItemSwap(int startingSlot, int destSlot)
    {
        // Case 2.2.1: same item, same item count = max stack size -> no-op
        if (items[startingSlot].count == items[startingSlot].maxStackSize && items[destSlot].count == items[destSlot].maxStackSize)
        {
            return;
        }

        // Case 2.2.2: free the starting slot and update the destination slot
        if ((items[startingSlot].count + items[destSlot].count) <= items[startingSlot].maxStackSize)
        {
            items[destSlot].count += items[startingSlot].count;
            InventoryManager.instance.Remove(startingSlot);
        }
        // Case 2.2.3: transfer the highest possible amount of items to the destination slot
        else
        {
            int tmpCount = items[destSlot].count;
            items[destSlot].count = items[destSlot].maxStackSize;
            items[startingSlot].count -= (items[destSlot].maxStackSize - tmpCount); // Case 2.2.3
        }
    }

    private void SwapSlotsContent(int startingSlot, int destSlot, GameObject newSlot)
    {
        // Swap the items in the items array
        (items[startingSlot], items[destSlot]) = (items[destSlot], items[startingSlot]);

        // Swap the slotsOccupied info
        (slotsOccupied[startingSlot], slotsOccupied[destSlot]) = (slotsOccupied[destSlot], slotsOccupied[startingSlot]);

        // Swap the transform.position of the objects
        (newSlot.transform.position, draggedObject.transform.position) = (draggedObject.transform.position, newSlot.transform.position);
    }

    private void ResetDraggedObject()
    {
        Destroy(draggedObjectCanvas);
        draggedObject = null;
        draggedObjectCanvas = null;
    }

    private Vector3 GetMousePosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
}
