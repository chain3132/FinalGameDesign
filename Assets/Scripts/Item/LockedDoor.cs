using System.Collections;
using UnityEngine;

/// <summary>
/// ประตูที่เปิดได้ตามลำดับ Game Flow
///
/// SplitSlide  — สองแผ่นเลื่อนออกซ้าย/ขวา (ห้องนอน)
///   ลาก LeftPanel และ RightPanel ใส่ Inspector
///   slideDistance = ระยะที่แต่ละแผ่นเลื่อนออกไป (local X)
///
/// SlideDown   — แผ่นเดียวเลื่อนลง (ห้องที่เหลือ)
///   slideDistance = ระยะที่ประตูจม (local Y)
/// </summary>
public class LockedDoor : MonoBehaviour, IInteractable
{
    public enum DoorType { SplitSlide, SlideDown }

    [Header("Door Type")]
    public DoorType doorType = DoorType.SlideDown;

    [Header("SplitSlide — ลาก child panels ใส่ (ใช้เฉพาะ SplitSlide)")]
    public Transform leftPanel;
    public Transform rightPanel;

    [Header("Slide Settings")]
    public float slideDistance = 2f;    // หน่วย Unity (เมตร)
    public float openDuration  = 0.6f;

    [Header("Key / Stage Requirement")]
    public string requiredKeyID = "";
    public string requiredStage = "";

    public bool isOpen { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    public string GetDescription()
    {
        if (isOpen)       return "";
        if (!StageOK())   return "[ Access denied ]";
        if (!KeyOK())     return "Keycard required";
        return "Press E to open";
    }

    public void Interact()
    {
        if (isOpen || !StageOK() || !KeyOK()) return;
        isOpen = true;
        DisableColliders();
        StartCoroutine(doorType == DoorType.SplitSlide ? OpenSplit() : OpenSlideDown());
    }

    // ─── Checks ───────────────────────────────────────────────────────────────
    private bool StageOK()
    {
        if (string.IsNullOrEmpty(requiredStage)) return true;
        return GameFlowManager.Instance != null &&
               GameFlowManager.Instance.CanOpenDoor(requiredStage);
    }

    private bool KeyOK()
    {
        if (string.IsNullOrEmpty(requiredKeyID)) return true;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        var inv = player.GetComponent<PlayerInventory>();
        return inv != null && inv.HasItem(requiredKeyID);
    }

    private void DisableColliders()
    {
        // ปิด collider ของ object นี้และ child ทั้งหมดทันที
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // ─── Animation: SplitSlide ────────────────────────────────────────────────
    private IEnumerator OpenSplit()
    {
        if (leftPanel == null || rightPanel == null) yield break;

        Vector3 leftStart  = leftPanel.localPosition;
        Vector3 rightStart = rightPanel.localPosition;
        // เลื่อนใน local X ของ panel นั้นๆ
        Vector3 leftEnd    = leftStart  + leftPanel.right  * -slideDistance;
        Vector3 rightEnd   = rightStart + rightPanel.right *  slideDistance;

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            float ease = Mathf.SmoothStep(0f, 1f, t / openDuration);
            leftPanel.localPosition  = Vector3.Lerp(leftStart,  leftEnd,  ease);
            rightPanel.localPosition = Vector3.Lerp(rightStart, rightEnd, ease);
            yield return null;
        }
        leftPanel.localPosition  = leftEnd;
        rightPanel.localPosition = rightEnd;
    }

    // ─── Animation: SlideDown ─────────────────────────────────────────────────
    private IEnumerator OpenSlideDown()
    {
        Vector3 startPos = transform.localPosition;
        // เลื่อนลงตาม local Y ของ parent
        Vector3 endPos   = startPos + Vector3.down * slideDistance;

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            float ease = Mathf.SmoothStep(0f, 1f, t / openDuration);
            transform.localPosition = Vector3.Lerp(startPos, endPos, ease);
            yield return null;
        }
        transform.localPosition = endPos;
    }
}