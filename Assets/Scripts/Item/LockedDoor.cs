using UnityEngine;

public class LockedDoor : MonoBehaviour, IInteractable
{
    public string requiredKeyID = "Lab_Room_01"; // กุญแจที่ประตูนี้ต้องการ
    public bool isLocked = true;

    public string GetDescription() 
    {
        return isLocked ? "This door locked" : "Press E to Open";
    }

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerInventory inv = player.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                if (isLocked)
                {
                    if (inv.HasItem(requiredKeyID))
                    {
                        UnlockDoor();
                    }
                    else
                    {
                        Debug.Log("No key!");
                    }
                }
                else
                {
                    OpenDoor();
                }
            }
        }
    }

    void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("Unlock Door!");
    }

    void OpenDoor()
    {
        Debug.Log("Open Door!");
        // ใส่ Code Animation หรือหมุนประตูตรงนี้
        Destroy(this.gameObject);
    }

    void CloseDoor()
    {
        //ใส่ปิดประตู
    }
}