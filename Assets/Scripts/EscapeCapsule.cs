using UnityEngine;

/// <summary>
/// กระสวยหนีฉุกเฉิน — กด E เมื่อผ่านทุก stage แล้ว → จบเกม (แสดง EndingScreen)
/// วางบน capsule GameObject ในห้องสุดท้าย
/// </summary>
public class EscapeCapsule : MonoBehaviour, IInteractable
{
    [Header("Reference")]
    public EndingScreen endingScreen;

    public string GetDescription()
    {
        if (!AllStagesClear())
            return "[ Escape Shuttle — not ready ]";

        return "Press E to escape the station";
    }

    public void Interact()
    {
        if (!AllStagesClear()) return;
        endingScreen?.Show();
    }

    private bool AllStagesClear()
    {
        var gf = GameFlowManager.Instance;
        return gf != null && gf.SecurityRoomComplete;
    }
}
