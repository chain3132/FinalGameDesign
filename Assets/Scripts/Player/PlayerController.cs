using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    public TextMeshProUGUI interactionText;

    void Update()
    {
        CheckInteraction(); // เช็คทุกเฟรม

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract(); // กด E แล้วค่อยเรียกทำงาน
        }
    }

    void CheckInteraction()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // เอารายละเอียดจาก object มาโชว์ที่ UI
                interactionText.text = interactable.GetDescription();
                return; // จบฟังก์ชัน
            }
        }

        // ถ้าไม่ได้มองอะไร หรือมองไม่เห็น IInteractable ให้ซ่อนข้อความ
        interactionText.text = "";
    }

    void TryInteract()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) interactable.Interact();
        }
    }
}