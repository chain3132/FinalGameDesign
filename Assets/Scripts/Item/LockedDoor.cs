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
                    if (inv.HasKey(requiredKeyID))
                    {
                        UnlockDoor();
                    }
                    else
                    {
                        Debug.Log("คุณไม่มีกุญแจสำหรับประตูนี้!");
                    }
                }
                else
                {
                    OpenDoor();
                }
            }
            else
            {
                Debug.LogError("หา Script PlayerInventory ในตัว Player ไม่เจอ!");
            }
        }
        else
        {
            Debug.LogError("หาตัว Player ในฉากไม่เจอ! (เช็ค Tag ดูนะ)");
        }
    }

    void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("ไขประตูสำเร็จ!");
    }

    void OpenDoor()
    {
        Debug.Log("ประตูเปิดออกแล้ว!");
        // ใส่ Code Animation หรือหมุนประตูตรงนี้
        Destroy(this.gameObject);
    }
}