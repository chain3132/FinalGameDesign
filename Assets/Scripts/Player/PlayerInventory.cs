using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private List<InventoryItem> inventory = new List<InventoryItem>();

    public void AddItem(string item)
    {
        InventoryItem existingItem = inventory.Find(i => i.itemName == item);

        if (existingItem != null)
        {
            existingItem.count++;
        }
        else
        {
            inventory.Add(new InventoryItem(item));
        }
    }

    public bool HasItem(string invitem)
    {
        InventoryItem item = inventory.Find(i => i.itemName == invitem);
        return item != null && item.count > 0;
    }

    public void RemoveItem(string itemName)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);

        if (item != null && item.count > 0)
        {
            item.count--;

            if (item.count <= 0)
            {
                inventory.Remove(item);
            }
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int count;

    public InventoryItem(string name)
    {
        itemName = name;
        count = 1;
    }
}

