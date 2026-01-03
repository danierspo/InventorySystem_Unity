# InventorySystem_Unity
A simple inventory system made in Unity, as part of an evaluation test of seven days to assess my programming skills. This test was the first step of a job interview for a position of Junior Unity Developer. Everything in this repo was produced in roughly 30 hours of work (from 2025/04/10 to 2025/04/16), as shown in the .xlsx file.

https://github.com/user-attachments/assets/8b42f48c-835f-4a74-a9a1-94fb7b6c830b

## Test Instructions and Tasks
"This test is designed to assess the programming skills of a Unity developer. Complete the following programming tasks by demonstrating a deep understanding of Unity development tools and techniques. Provide the complete source code along with detailed comments to explain your choices and implementations.
* Create an inventory system that allows the player to collect, view, and use items.
* Inventory items should be collectible from pickups and usable.
* Implement at least two different types of items with unique effects when used.
* Enable the player to drag and drop items within the inventory for organization.

Additional notes:
* Use C# Scripting
* Pay special attention to code organization, efficiency, and maintainability
* Provide comprehensive comments to explain design choices, complex implementations, and solutions to encountered problems."

## Explanation of implemented mechanics and other info
The .xlsx file shows the activities carried out on each day. Getting into the task activities and implemented mechanics:
* The inventory has 24 slots. Each slot can contain a STACK of items. Each item, implemented as a ScriptableObject, contains information such as the number of items in the stack and the maximum number of stackable items of the same type. E.g.: "RedPotions" max 7 per stack, while "Skull" is a unique item (max 1 per stack)
* Right-click on an item in the scene to add it to the inventory ("pickup" action). If there's a stack of the same item available, the new item is added to the stack. Otherwise, it is placed in an available slot by creating a new stack. If the inventory is full, the item is not added to the inventory and remains in the scene instead
* In the inventory, items are draggable within the inventory by using the left mouse button. The system is inspired by Metin2 inventory: the developed system has all of its features except for separating items in a stack. So you can move an existing stack of items into an empty slot, swap items slots, discard items, transfer the max quantity of items from one stack to another, and so on.
* In the inventory, items are usable by using the right mouse button. Pressing CTRL while right-clicking uses all possible items in the stack. That is, if you have 30/100 HPs and 5 red pots in your inventory, right-clicking uses 1, while CTRL+right-clicking uses 3. [*This was not included in the demo video to avoid confusion]

The red potion restores 25% of max HPs. The blue potion restores 50% of max MPs. The green potion completely restores stamina. The skull sets HPs to 0.
