using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ท่อออกซิเจนที่ผู้เล่นต้องซ่อมโดยกด E ค้างไว้ 60 วินาที
/// ถ้าปล่อยมือ progress จะหยุดอยู่ที่เปอร์เซ็นนั้น ไม่รีเซ็ต
/// </summary>
[RequireComponent(typeof(Collider))]
public class OxygenPipe : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public float repairDuration = 60f;  // วินาทีที่ต้องกดค้างรวม
    public float interactRange  = 2.5f; // ระยะห่างสูงสุดจากท่อ

    public bool IsRepaired { get; private set; }

    private float       repairProgress; // วินาทีที่สะสมไว้
    private bool        isRepairing;
    private Transform   playerTransform;

    // Screen-space UI
    private GameObject        repairUIRoot;
    private Image             progressFill;
    private TextMeshProUGUI   percentText;

    // ─────────────────────────────────────────────
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        BuildRepairUI();
    }

    private void Update()
    {
        if (IsRepaired) return;

        // ถ้า event ยังไม่เปิด ปิด UI ทิ้ง
        if (!EventIsActive())
        {
            if (isRepairing) StopRepairing();
            return;
        }

        if (!isRepairing) return;

        bool eHeld  = Input.GetKey(KeyCode.E);
        bool inRange = playerTransform != null &&
                       Vector3.Distance(transform.position, playerTransform.position) <= interactRange;

        if (eHeld && inRange)
        {
            repairProgress = Mathf.Min(repairProgress + Time.deltaTime, repairDuration);
            RefreshUI();
            if (repairProgress >= repairDuration) FinishRepair();
        }
        else
        {
            // ปล่อยมือ — หยุดซ่อม แต่ไม่รีเซ็ต progress
            StopRepairing();
        }
    }

    // ─── IInteractable ───────────────────────────
    public string GetDescription()
    {
        if (IsRepaired) return "ท่อออกซิเจน  [ ซ่อมแล้ว ]";
        if (!EventIsActive()) return "";

        int pct = Mathf.RoundToInt((repairProgress / repairDuration) * 100f);
        return pct > 0
            ? $"กด E ค้าง เพื่อซ่อมท่อออกซิเจน  ({pct}%)"
            : "กด E ค้าง เพื่อซ่อมท่อออกซิเจน";
    }

    public void Interact()
    {
        if (IsRepaired || !EventIsActive()) return;
        isRepairing = true;
        repairUIRoot.SetActive(true);
        RefreshUI();
    }

    // ─── Internal ────────────────────────────────
    private void StopRepairing()
    {
        isRepairing = false;
        repairUIRoot.SetActive(false);
    }

    private void FinishRepair()
    {
        IsRepaired = true;
        StopRepairing();
        OxygenEventManager.Instance?.OnPipeRepaired();

        // เปลี่ยนสีท่อให้รู้ว่าซ่อมเสร็จแล้ว
        var rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(0.1f, 0.75f, 0.35f);
    }

    private bool EventIsActive() =>
        OxygenEventManager.Instance != null && OxygenEventManager.Instance.IsEventActive;

    private void RefreshUI()
    {
        float fill = repairProgress / repairDuration;
        progressFill.fillAmount = fill;
        progressFill.color = Color.Lerp(
            new Color(0f, 0.72f, 1f),
            new Color(0f, 1f, 0.45f),
            fill
        );
        percentText.text = $"{Mathf.RoundToInt(fill * 100f)}%";
    }

    // ─── Build UI ────────────────────────────────
    private void BuildRepairUI()
    {
        // Canvas screen-space overlay (เหมือนกับ puzzle อื่นๆ)
        var canvasGO = new GameObject("PipeRepairCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Root container — กึ่งกลางจอ เยื้องลงมาหน่อย
        repairUIRoot = new GameObject("RepairUIRoot");
        repairUIRoot.transform.SetParent(canvasGO.transform, false);
        var rootRect = repairUIRoot.AddComponent<RectTransform>();
        rootRect.anchorMin  = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax  = new Vector2(0.5f, 0.5f);
        rootRect.pivot      = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, -140f);
        rootRect.sizeDelta  = new Vector2(180f, 180f);

        Sprite circle = MakeCircleSprite(64);

        // วงกลมพื้นหลังสีเข้ม
        AddRingLayer(repairUIRoot.transform, circle,
            new Vector2(172f, 172f), new Color(0.05f, 0.05f, 0.1f, 0.85f));

        // วงกลม progress (Radial360)
        var fillGO = new GameObject("ProgressFill");
        fillGO.transform.SetParent(repairUIRoot.transform, false);
        progressFill = fillGO.AddComponent<Image>();
        progressFill.sprite        = circle;
        progressFill.color         = new Color(0f, 0.72f, 1f);
        progressFill.type          = Image.Type.Filled;
        progressFill.fillMethod    = Image.FillMethod.Radial360;
        progressFill.fillOrigin    = (int)Image.Origin360.Top;
        progressFill.fillClockwise = true;
        progressFill.fillAmount    = 0f;
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.sizeDelta       = new Vector2(172f, 172f);
        fillRect.anchoredPosition = Vector2.zero;

        // วงในทึบ — ทำให้ดูเหมือน ring ไม่ใช่ pie
        AddRingLayer(repairUIRoot.transform, circle,
            new Vector2(118f, 118f), new Color(0.04f, 0.04f, 0.08f, 0.95f));

        // ตัวเลข %
        var pctGO = new GameObject("PercentText");
        pctGO.transform.SetParent(repairUIRoot.transform, false);
        percentText = pctGO.AddComponent<TextMeshProUGUI>();
        percentText.text      = "0%";
        percentText.fontSize  = 34;
        percentText.fontStyle = FontStyles.Bold;
        percentText.alignment = TextAlignmentOptions.Center;
        percentText.color     = new Color(0f, 0.9f, 1f);
        var pctRect = pctGO.GetComponent<RectTransform>();
        pctRect.sizeDelta        = new Vector2(110f, 48f);
        pctRect.anchoredPosition = new Vector2(0f, 10f);

        // ข้อความใต้ตัวเลข
        var lblGO = new GameObject("LabelText");
        lblGO.transform.SetParent(repairUIRoot.transform, false);
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = "ซ่อมท่อ";
        lbl.fontSize  = 15;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color     = new Color(0.65f, 0.65f, 0.65f);
        var lblRect = lblGO.GetComponent<RectTransform>();
        lblRect.sizeDelta        = new Vector2(110f, 26f);
        lblRect.anchoredPosition = new Vector2(0f, -22f);

        repairUIRoot.SetActive(false);
    }

    private static void AddRingLayer(Transform parent, Sprite sprite, Vector2 size, Color color)
    {
        var go  = new GameObject("RingLayer");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite    = sprite;
        img.color     = color;
        img.type      = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillAmount = 1f;
        var r = go.GetComponent<RectTransform>();
        r.sizeDelta        = size;
        r.anchoredPosition = Vector2.zero;
    }

    private static Sprite MakeCircleSprite(int size)
    {
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float c  = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - c, dy = y - c;
                tex.SetPixel(x, y, (dx * dx + dy * dy) <= c * c ? Color.white : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
