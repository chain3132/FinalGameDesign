using UnityEngine;

/// <summary>
/// ควบคุม flow ของห้องพลังงาน:
/// 1) ผู้เล่นวาง Power Cell ครบ 3 อัน
/// 2) ปลดล็อค PercentageTerminal
/// 3) เมื่อ Terminal สำเร็จ → GameFlowManager.CompletePowerRoom()
///
/// วางบน empty GameObject ในห้อง ลาก slots[] + terminal ใน Inspector
/// </summary>
public class PowerRoomManager : MonoBehaviour
{
    [Header("Slots (ลาก PowerCellSlot ทั้ง 3 ใส่)")]
    public PowerCellSlot[] slots;

    [Header("Terminal (ล็อคอยู่จนกว่าจะวาง cell ครบ)")]
    public PercentageTerminal terminal;

    private int filledCount;

    private void Start()
    {
        // ปิด Terminal ก่อนจนกว่าจะวาง cell ครบ
        if (terminal != null) terminal.gameObject.SetActive(false);
    }

    /// <summary>เรียกโดย PowerCellSlot ทุกครั้งที่วาง cell สำเร็จ</summary>
    public void OnCellPlaced()
    {
        filledCount++;

        if (filledCount >= slots.Length)
            UnlockTerminal();
    }

    private void UnlockTerminal()
    {
        if (terminal == null) return;
        terminal.gameObject.SetActive(true);
        terminal.OnSolved += HandleTerminalSolved;
    }

    private void HandleTerminalSolved()
    {
        terminal.OnSolved -= HandleTerminalSolved;
        GameFlowManager.Instance?.CompletePowerRoom();
    }
}
