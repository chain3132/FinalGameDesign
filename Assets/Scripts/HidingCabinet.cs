using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ตู้ซ่อนตัว — กด E เพื่อเข้า, กด E อีกครั้งเพื่อออก
/// วาง HidePoint (child) ไว้ตรงกลางด้านในตู้
/// วาง ExitPoint (child) ไว้ด้านหน้าประตูตู้
/// </summary>
public class HidingCabinet : MonoBehaviour, IInteractable
{
    [Header("Positions (Child Transforms)")]
    public Transform hidePoint;   // จุดที่ผู้เล่นยืนอยู่ในตู้
    public Transform exitPoint;   // จุดที่ผู้เล่นออกมาหน้าตู้

    [Header("References")]
    public PlayerMovement playerMovement; // ลาก Player ใส่ Inspector

    // ─── state ───────────────────────────────────
    public bool IsHiding { get; private set; }

    private CharacterController cc;
    private Transform           playerTransform;
    private TextMeshProUGUI     exitHintText;
    private bool                exitHandledThisFrame;

    // ─────────────────────────────────────────────
    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        cc = player.GetComponent<CharacterController>();

        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();

        BuildExitHintUI();
    }

    private void Update()
    {
        exitHandledThisFrame = false;

        if (!IsHiding) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            exitHandledThisFrame = true;
            ExitCabinet();
        }
    }

    // ─── IInteractable ───────────────────────────
    public string GetDescription()
    {
        return IsHiding ? "Press E to exit the cabinet" : "Press E to hide inside";
    }

    public void Interact()
    {
        // ป้องกันการ exit ซ้ำในเฟรมเดียวกันกับ Update()
        if (IsHiding)
        {
            if (!exitHandledThisFrame) ExitCabinet();
            return;
        }
        EnterCabinet();
    }

    // ─── Enter / Exit ─────────────────────────────
    private void EnterCabinet()
    {
        // 1. ปิด CharacterController เพื่อ teleport ได้โดยไม่ติด collider
        cc.enabled = false;

        // 2. ย้ายผู้เล่นเข้าไปในตู้
        playerTransform.position = hidePoint.position;
        playerTransform.rotation = hidePoint.rotation;

        // 3. เปิด CharacterController กลับ (ไม่ขยับเองเพราะ PlayerMovement จะปิด)
        cc.enabled = true;

        // 4. ล็อคการเคลื่อนที่และกล้อง
        playerMovement.enabled = false;

        // 5. Reset มุมกล้อง ไม่ให้มองทะลุฝาตู้
        playerMovement.xRotation = 0f;
        if (playerMovement.cameraTransform != null)
            playerMovement.cameraTransform.localRotation = Quaternion.identity;

        IsHiding = true;
        exitHintText.gameObject.SetActive(true);
    }

    private void ExitCabinet()
    {
        // 1. ปิด CharacterController เพื่อ teleport ออก
        cc.enabled = false;

        // 2. ย้ายออกมาหน้าตู้
        playerTransform.position = exitPoint.position;
        playerTransform.rotation = exitPoint.rotation;

        // 3. เปิด CharacterController
        cc.enabled = true;

        // 4. คืน input ให้ผู้เล่น
        playerMovement.xRotation = 0f;
        if (playerMovement.cameraTransform != null)
            playerMovement.cameraTransform.localRotation = Quaternion.identity;

        playerMovement.enabled = true;

        IsHiding = false;
        exitHintText.gameObject.SetActive(false);
    }

    // ─── UI ──────────────────────────────────────
    private void BuildExitHintUI()
    {
        var canvasGO = new GameObject("CabinetHintCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // แผ่นพื้นหลังโปร่งใส กึ่งกลางล่าง
        var bgGO = new GameObject("HintBG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.3f, 0.06f);
        bgRect.anchorMax = new Vector2(0.7f, 0.12f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // ข้อความ
        var textGO = new GameObject("HintText");
        textGO.transform.SetParent(bgGO.transform, false);
        exitHintText = textGO.AddComponent<TextMeshProUGUI>();
        exitHintText.text = "[E]  Exit cabinet";
        exitHintText.fontSize = 22;
        exitHintText.fontStyle = FontStyles.Bold;
        exitHintText.alignment = TextAlignmentOptions.Center;
        exitHintText.color = new Color(0.85f, 0.85f, 0.85f);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        bgGO.SetActive(false);
    }
}
