using UnityEngine;

public class Fixlight : MonoBehaviour, IInteractable
{
    public string requiredItem = "Energy Power";
    public int itemsRequired = 3;
    private int itemsInserted = 0;

    public bool isFixed = false;

    public string GetDescription() => isFixed ? "Light is fixed." : $"Press E to fix light {itemsInserted}/{itemsRequired}";

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerInventory inv = player.GetComponent<PlayerInventory>();

            if (!isFixed)
            {
                if (inv.HasItem(requiredItem))
                {
                    inv.RemoveItem(requiredItem);
                    itemsInserted++;

                    if (itemsInserted >= itemsRequired)
                    {
                        Fix();
                    }
                }
                else
                {
                    Debug.Log("You need " + requiredItem + " to fix this!");
                }
            }
        }
    }

    private void Fix()
    {
        isFixed = true;
        Debug.Log("Fixed!");
        // ตรงนี้ใส่ Code เปลี่ยนสีไฟ หรือเปิดไฟได้เลยครับ
    }
}