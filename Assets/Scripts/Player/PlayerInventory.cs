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

    public bool HasKey(string keyID)
    {
        InventoryItem item = inventory.Find(i => i.itemName == keyID);
        return item != null && item.count > 0;
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
