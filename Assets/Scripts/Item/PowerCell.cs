using UnityEngine;

/// <summary>
/// Power Cell ที่วางกระจายอยู่ในห้องพลังงาน
/// ผู้เล่นกด E เพื่อหยิบ → เพิ่ม "power_cell" ลง inventory
/// </summary>
public class PowerCell : MonoBehaviour, IInteractable
{
    public string GetDescription() => "กด E เพื่อหยิบ Power Cell";

    public void Interact()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var inv = player.GetComponent<PlayerInventory>();
        if (inv != null) inv.AddItem("power_cell");

        Destroy(gameObject);
    }
}
