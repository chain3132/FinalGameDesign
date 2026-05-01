using System.Collections;
using UnityEngine;

/// <summary>
/// ประตูที่เปิดได้ตามลำดับ Game Flow
/// - requiredKeyID  : ชื่อ item ใน inventory ที่ต้องมี (ว่างเปล่า = ไม่ต้องใช้กุญแจ)
/// - requiredStage  : stage ที่ต้องผ่านแล้วก่อนเปิดได้ ("Bedroom", "PowerRoom", ...)
/// Pivot ของ door object ควรอยู่ที่ขอบบานพับ (hinge edge)
/// </summary>
public class LockedDoor : MonoBehaviour, IInteractable
{
    [Header("Key / Stage Requirement")]
    public string requiredKeyID = "";       // ว่างเปล่า = ไม่ต้องใช้กุญแจ
    public string requiredStage = "";       // ว่างเปล่า = ไม่ต้องตรวจ stage

    [Header("Open Animation")]
    public float openAngle    = 90f;        // องศาที่หมุน (Y-axis)
    public float openDuration = 0.7f;       // วินาทีที่ใช้เปิด

    public bool isOpen { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    public string GetDescription()
    {
        if (isOpen) return "";

        if (!StageOK())   return "ยังไม่สามารถผ่านได้";
        if (!KeyOK())     return $"ต้องการ Keycard";
        return "กด E เพื่อเปิดประตู";
    }

    public void Interact()
    {
        if (isOpen) return;
        if (!StageOK() || !KeyOK()) return;

        StartCoroutine(OpenDoor());
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

    // ─── Animation ────────────────────────────────────────────────────────────
    private IEnumerator OpenDoor()
    {
        isOpen = true;

        // ปิด collider ทันทีเพื่อให้ผ่านได้
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Quaternion startRot = transform.localRotation;
        Quaternion endRot   = startRot * Quaternion.Euler(0f, openAngle, 0f);

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t / openDuration);
            yield return null;
        }
        transform.localRotation = endRot;
    }
}
