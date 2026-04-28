using UnityEngine;

public class Note : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject noteUI;

    [SerializeField]
    private MonoBehaviour playerMovementScript;

    public string GetDescription() => "Press E to read note ";
    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            ReadNote();
        }
    }
    public void ReadNote()
    {
        noteUI.SetActive(true);

        // 1. โชว์เมาส์และปลดล็อคเมาส์
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2. ปิด Script หมุนกล้อง/เดิน (ถ้ามี)
        if (playerMovementScript != null) playerMovementScript.enabled = false;
    }

    public void CloseNote()
    {
        noteUI.SetActive(false);

        // 1. ซ่อนเมาส์และล็อคเมาส์กลับที่เดิม
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 2. เปิด Script หมุนกล้อง/เดิน ให้ทำงานต่อ
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }
}
