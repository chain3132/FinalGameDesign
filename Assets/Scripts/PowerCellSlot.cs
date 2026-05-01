using UnityEngine;

/// <summary>
/// แท่นวาง Power Cell — ผู้เล่นกด E เพื่อวาง cell ที่มีในมือ
/// เมื่อวางแล้วเปลี่ยนสีเป็นสีฟ้า-เขียว และแจ้ง PowerRoomManager
/// </summary>
public class PowerCellSlot : MonoBehaviour, IInteractable
{
    [Header("Reference")]
    public PowerRoomManager roomManager;

    [Header("Visual — ลาก Renderer ของแท่นใส่")]
    public Renderer slotRenderer;

    public bool IsFilled { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    public string GetDescription()
    {
        if (IsFilled) return "[ วาง Power Cell แล้ว ]";

        var inv = GetPlayerInventory();
        return inv != null && inv.HasItem("power_cell")
            ? "กด E เพื่อวาง Power Cell"
            : "ต้องการ Power Cell";
    }

    public void Interact()
    {
        if (IsFilled) return;

        var inv = GetPlayerInventory();
        if (inv == null || !inv.HasItem("power_cell")) return;

        inv.RemoveItem("power_cell");
        IsFilled = true;

        // เปลี่ยนสีแท่นเป็นสีฟ้า-เขียวเพื่อแสดงว่าใส่แล้ว
        if (slotRenderer != null)
            slotRenderer.material.color = new Color(0f, 0.85f, 0.55f);

        roomManager?.OnCellPlaced();
    }

    private PlayerInventory GetPlayerInventory()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerInventory>() : null;
    }
}
