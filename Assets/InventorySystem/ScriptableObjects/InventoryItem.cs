using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Scriptable Objects/New Inventory Item")]
public class InventoryItem : ScriptableObject
{
    public int id;
    public string itemName;
    public int value;
    public Sprite icon;
    public int count;
    public int maxStackSize;


    // Code below for testing only: makes the "count" field reset after each run

    private int countDefault;
    private void Awake()
    {
        countDefault = count;
    }

    public void ResetCount()
    {
        count = countDefault;
    }
}
