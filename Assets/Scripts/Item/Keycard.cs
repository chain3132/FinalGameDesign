using UnityEngine;

/// <summary>
/// วางบน GameObject ที่เป็น keycard ในห้องนอน
/// ผู้เล่นกด E เพื่อหยิบ → เพิ่มลง PlayerInventory + CompleteBedroom
/// </summary>
public class Keycard : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string keycardID = "keycard_bedroom";
    public string displayName = "Keycard";

    public string GetDescription() => $"กด E เพื่อหยิบ {displayName}";

    public void Interact()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var inv = player.GetComponent<PlayerInventory>();
        if (inv != null) inv.AddItem(keycardID);

        GameFlowManager.Instance?.CompleteBedroom();

        Destroy(gameObject);
    }
}
