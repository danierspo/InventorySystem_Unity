using UnityEngine;

public class ItemActions : MonoBehaviour
{
    /*
     * ItemActions provides functions to drag an item in the scene (mouse left button) and to pick and add it to the inventory (mouse right button).
     */

    public InventoryItem item;

    private Vector3 offset;
    private Camera mainCamera;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        mainCamera = Camera.main;
    }

    /*
     * The item pickup operation is simple: if the inventory has an available slot for the item, store it in the inventory and remove it from the scene.
     */
    private void Pickup()
    {
        if (InventoryManager.instance.Add(item))
        {
            Destroy(gameObject);
        }
    }

    /*
     * If the mouse pointer is over the item and the player presses the right button, pickup the item. Please note that a collider is needed.
     */
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Pickup();
        }
    }

    /*
     * If the mouse left button is pressed, start dragging the item. This method only works with the left button.
     */
    public void OnMouseDown()
    {
        StartDrag();
    }

    /*
     * When the drag operation is started, get a reference to the mouse offset.
     */
    private void StartDrag()
    {
        offset = transform.position - GetMousePosition();
    }

    /*
     * This method updates the position of the item based on the mouse position.
     */
    private void UpdateObjectPosition()
    {
        transform.position = GetMousePosition() + offset;
    }

    /*
     * When the player is dragging an item, update its position. Please note that a collider is needed.
     */
    public void OnMouseDrag()
    {
        UpdateObjectPosition();
    }

    /*
     * Input.mousePosition returns the mouse position in pixel coords, which must be converted to world coords.
     */
    private Vector3 GetMousePosition()
    {
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}
