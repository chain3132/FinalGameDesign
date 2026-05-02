using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour playerLookScript;

    private GameObject uiRoot;
    private bool isOpen = false;
    private float openCooldown = 0f;

    // ─── Colours ──────────────────────────────────────────────────────────────
    private static readonly Color ColBg        = new Color(0.02f, 0.06f, 0.10f, 0.97f);
    private static readonly Color ColPanel     = new Color(0.03f, 0.10f, 0.16f, 0.98f);
    private static readonly Color ColCyan      = new Color(0.0f,  0.85f, 1.0f,  1.0f);
    private static readonly Color ColCyanDim   = new Color(0.0f,  0.55f, 0.75f, 1.0f);
    private static readonly Color ColHighlight = new Color(1.0f,  0.85f, 0.0f,  1.0f);
    private static readonly Color ColText      = new Color(0.75f, 0.92f, 1.0f,  1.0f);
    private static readonly Color ColSubText   = new Color(0.45f, 0.72f, 0.85f, 1.0f);
    private static readonly Color ColRed       = new Color(1.0f,  0.25f, 0.18f, 1.0f);
    private static readonly Color ColDivider   = new Color(0.0f,  0.65f, 0.90f, 0.45f);
    private static readonly Color ColClose     = new Color(0.85f, 0.15f, 0.10f, 1.0f);

    // ─── Conversation content ──────────────────────────────────────────────────
    private const string MSG_HEADER = "CREW MESSAGING SYSTEM  //  STATION DELTA-7";
    private const string MSG_META   = "CHANNEL: Internal Operations  |  CLASSIFICATION: RESTRICTED  |  NODE: COM-4419";

    private const string CONVERSATION = @"
<color=#7BCCDD>[ 06:14 ]  ENG. TORRES, M.</color>
Morning everyone. Just finished the third inspection cycle on Deck B. Coolant pressure is nominal but the secondary O2 regulator is showing some variance — nothing critical yet, but worth keeping an eye on. Logging it now.

<color=#7BCCDD>[ 06:31 ]  DR. CHEN, L.</color>
Thanks for the heads-up, Torres. I ran a diagnostic pass on the med-bay life support last night. Everything checks out on our end. We should probably schedule a joint inspection with Engineering before the next shift rotation. I've already sent a maintenance request to the system — ticket #MR-0882.

<color=#7BCCDD>[ 06:47 ]  CMDR. VASQUEZ, R.</color>
Morning crew. I've reviewed the overnight logs. The external hull sensor on grid F-12 went offline around 03:20. Possibly a power fluctuation from the solar array recalibration. Rodriguez — can you check it this afternoon? Also, I'll need the status report for Sector C finalized before 14:00. Command is monitoring our timeline closely.

<color=#7BCCDD>[ 07:02 ]  TECH. RODRIGUEZ, A.</color>
Copy that, Commander. I'll get on the hull sensor after breakfast. On a related note — I noticed the supply room on Deck A was left open again last night. I went ahead and secured it. The access log showed no unauthorized entry, just a missed lockout. We really need to be more careful about that.

<color=#7BCCDD>[ 07:15 ]  ENG. TORRES, M.</color>
Agreed. With the current situation, station security is priority one. Speaking of which — I had to borrow the room exit keycard earlier for a quick calibration run. I left it on the shelf next to the workbench on the left side when you walk in. Should still be there. Didn't want anyone panicking looking for it.

<color=#FFD700><b>► [ 07:15 ]  SYSTEM — ITEM LOCATION FLAGGED
   EXIT KEYCARD has been placed on the shelf beside the left workbench.
   Please retrieve it before securing the area.</b></color>

<color=#7BCCDD>[ 07:28 ]  DR. CHEN, L.</color>
Got it, Torres. Thanks for the note. I'll grab it after my rounds. Also — heads up to everyone: the galley refrigeration unit is acting up again. The temperature spiked to 12°C overnight. I've already flagged it for repair but don't leave anything temperature-sensitive in there until it's fixed.

<color=#7BCCDD>[ 07:41 ]  CMDR. VASQUEZ, R.</color>
Noted, Chen. Rodriguez, add the galley unit to the afternoon checklist as well. Torres, please include the O2 regulator variance in today's engineering report and send it directly to me. I want Command to have full visibility before the 15:00 briefing. We can't afford any surprises right now.

<color=#7BCCDD>[ 07:55 ]  TECH. RODRIGUEZ, A.</color>
Will do, Commander. Also — just a reminder that the external comms array will be in maintenance mode between 09:00 and 11:00. Any non-emergency transmissions should be queued before then or held until 11:30 to avoid interference with the diagnostic burst sequence.

<color=#7BCCDD>[ 08:10 ]  ENG. TORRES, M.</color>
Roger that. I'll have the engineering report to you by 13:30, Commander. The O2 variance has been consistent for the past 36 hours — it's not spiking, just sitting about 4% below threshold. My best guess is a partial blockage in the recirculation filter. I'll do a manual flush tonight during the low-activity window.

<color=#7BCCDD>[ 08:22 ]  DR. CHEN, L.</color>
Good thinking, Torres. If you need a second pair of hands for the flush, let me know — I can be free after 22:00. Rodriguez, did you have a chance to look at the med-bay door sensor? It's been intermittently failing to register badge scans. Low priority, I know, but it's been annoying.

<color=#7BCCDD>[ 08:35 ]  TECH. RODRIGUEZ, A.</color>
I'll take a look at it this evening, Chen. It might just be dust on the reader — happens every couple of months in these older units. I'll bring my toolkit. In other news, I finished cataloguing the equipment manifest for Sector C. Everything matches except for two portable oxygen tanks that seem to be unaccounted for. I've flagged it in the inventory system — ticket #INV-2241. Probably just miscounted during the last transfer, but logging it all the same.

<color=#7BCCDD>[ 08:50 ]  CMDR. VASQUEZ, R.</color>
Good catch, Rodriguez. Make sure that discrepancy is documented properly. Command has been strict about inventory accuracy this cycle after the supply audit last month. Also — I want everyone to double-check their personal access keycards before end of shift. We had two temporary keycards go missing last rotation and we can't afford that happening again during a restricted period.

<color=#7BCCDD>[ 09:05 ]  ENG. TORRES, M.</color>
Understood, Commander. And again — just to be clear for anyone reading back through the logs: the <b>exit keycard for this room is on the shelf</b>. Don't forget to pick it up.

<color=#7BCCDD>[ 09:18 ]  SYSTEM — AUTO REPLY</color>
This channel is now entering low-priority mode. Messages will be archived every 30 minutes. For emergencies, use COMM channel RED-1. Stay safe, crew.

— END OF LOG —
";

    // ─── IInteractable ─────────────────────────────────────────────────────────
    public string GetDescription() => "Press E to access tablet";

    public void Interact()
    {
        if (isOpen && openCooldown <= 0f) CloseUI();
        else if (!isOpen) OpenUI();
    }

    // ─── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start() => BuildUI();

    private void Update()
    {
        if (openCooldown > 0f) openCooldown -= Time.unscaledDeltaTime;
    }

    // ─── Open / Close ──────────────────────────────────────────────────────────
    private void OpenUI()
    {
        isOpen = true;
        openCooldown = 0.3f;
        uiRoot.SetActive(true);
        // Rebuild layout after canvas becomes active so ContentSizeFitter calculates correctly
        Canvas.ForceUpdateCanvases();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerLookScript     != null) playerLookScript.enabled     = false;
    }

    private void CloseUI()
    {
        isOpen = false;
        uiRoot.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (playerLookScript     != null) playerLookScript.enabled     = true;
    }

    // ─── UI Construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        // Canvas
        uiRoot = new GameObject("TabletUI");
        DontDestroyOnLoad(uiRoot);
        Canvas canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        uiRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        uiRoot.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        uiRoot.AddComponent<GraphicRaycaster>();

        // Full-screen dim overlay
        MakeImage(uiRoot, "Overlay", new Color(0, 0, 0, 0.65f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // ── Outer frame ───────────────────────────────────────────────────────
        GameObject outer = MakeImage(uiRoot, "OuterFrame", ColBg,
            new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f),
            Vector2.zero, Vector2.zero);
        AddOutline(outer, ColCyan, 2f);

        // Corner decorators
        AddCornerDeco(outer, ColCyan);

        // ── Top header bar ────────────────────────────────────────────────────
        GameObject headerBar = MakeImage(outer, "HeaderBar", ColCyan,
            new Vector2(0f, 0.92f), Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform hbrt = headerBar.GetComponent<RectTransform>();
        hbrt.sizeDelta = new Vector2(0, 36);
        hbrt.anchorMin = new Vector2(0, 1);
        hbrt.anchorMax = new Vector2(1, 1);
        hbrt.anchoredPosition = new Vector2(0, -18);

        // Title text inside header
        GameObject title = MakeText(headerBar, "Title", "  " + MSG_HEADER,
            18, FontStyles.Bold, ColPanel, TextAlignmentOptions.MidlineLeft);
        StretchRect(title);

        // ── Status strip ──────────────────────────────────────────────────────
        GameObject statusBar = MakeImage(outer, "StatusBar", new Color(0f, 0.25f, 0.35f, 1f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform sbrt = statusBar.GetComponent<RectTransform>();
        sbrt.anchorMin = new Vector2(0, 1); sbrt.anchorMax = new Vector2(1, 1);
        sbrt.sizeDelta = new Vector2(0, 24);
        sbrt.anchoredPosition = new Vector2(0, -54);
        MakeDivider(outer, 0.88f, ColCyanDim);

        // Meta text
        GameObject metaTxt = MakeText(statusBar, "Meta", "  " + MSG_META,
            10, FontStyles.Normal, ColSubText, TextAlignmentOptions.MidlineLeft);
        StretchRect(metaTxt);

        // ── Right-side status panel ───────────────────────────────────────────
        GameObject sidePanel = MakeImage(outer, "SidePanel", ColPanel,
            new Vector2(0.82f, 0.03f), new Vector2(0.99f, 0.85f),
            Vector2.zero, Vector2.zero);
        AddOutline(sidePanel, ColCyanDim, 1f);

        MakeText(sidePanel, "SideTitle", "SYSTEM\nSTATUS", 11, FontStyles.Bold,
            ColCyan, TextAlignmentOptions.Top).GetComponent<RectTransform>()
            .anchoredPosition = new Vector2(0, -14);
        StretchRect(sidePanel.transform.Find("SideTitle").gameObject);

        // Status rows
        string[] statusLabels = { "NODE", "AUTH", "ENCRYPT", "UPTIME", "CREW", "SIGNAL" };
        string[] statusVals   = { "COM-4419", "ACTIVE", "AES-256", "847:32:11", "4 / 4", "NOMINAL" };
        Color[]  statusCols   = { ColCyan, ColCyan, ColCyan, ColSubText, ColCyan, ColCyan };
        for (int i = 0; i < statusLabels.Length; i++)
        {
            float y = -56 - i * 34f;
            MakeSideRow(sidePanel, statusLabels[i], statusVals[i], statusCols[i], y);
        }

        // Blinking alert
        GameObject alert = MakeText(sidePanel, "Alert",
            "! KEYCARD\n  LOCATED",
            11, FontStyles.Bold, ColHighlight, TextAlignmentOptions.Center);
        RectTransform art = alert.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0.05f, 0f);
        art.anchorMax = new Vector2(0.95f, 0f);
        art.sizeDelta = new Vector2(0, 48);
        art.anchoredPosition = new Vector2(0, 36);
        alert.AddComponent<BlinkEffect>();

        MakeDivider(sidePanel, 0.22f, ColHighlight);

        // ── Close button ──────────────────────────────────────────────────────
        GameObject closeBtn = MakeImage(outer, "CloseBtn", ColClose,
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        RectTransform cbrt = closeBtn.GetComponent<RectTransform>();
        cbrt.anchorMin = cbrt.anchorMax = new Vector2(0.91f, 0.025f);
        cbrt.sizeDelta = new Vector2(110, 30);
        AddOutline(closeBtn, new Color(1, 0.5f, 0.4f), 1f);

        GameObject closeTxt = MakeText(closeBtn, "CloseTxt", "[E]  CLOSE",
            12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        StretchRect(closeTxt);

        Button btn = closeBtn.AddComponent<Button>();
        btn.onClick.AddListener(() => CloseUI());

        // ── Scroll area (main message) ────────────────────────────────────────
        GameObject scrollObj = new GameObject("ScrollArea");
        scrollObj.transform.SetParent(outer.transform, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.01f, 0.08f);
        scrollRT.anchorMax = new Vector2(0.80f, 0.87f);
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;

        // Viewport — must have a non-zero alpha Image for Mask stencil to write
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vrt = viewport.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(6, 6);
        vrt.offsetMax = new Vector2(-14, -6);
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0f, 0f, 0f, 0.004f); // nearly invisible but writes stencil
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content — TMP IS the content (no extra wrapper needed)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot     = new Vector2(0f, 1f);
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;

        // TMP text directly on Content
        TextMeshProUGUI msgTmp = content.AddComponent<TextMeshProUGUI>();
        msgTmp.text               = CONVERSATION.Trim();
        msgTmp.fontSize           = 13.5f;
        msgTmp.color              = ColText;
        msgTmp.lineSpacing        = 6f;
        msgTmp.richText           = true;
        msgTmp.enableWordWrapping = true;
        msgTmp.overflowMode       = TextOverflowModes.Overflow;
        msgTmp.margin             = new Vector4(10f, 8f, 10f, 8f);

        // ContentSizeFitter lets Content grow to fit all text vertically
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // ScrollRect
        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.viewport          = vrt;
        sr.content           = crt;
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.scrollSensitivity = 30f;
        sr.movementType      = ScrollRect.MovementType.Clamped;
        sr.inertia           = true;
        sr.decelerationRate  = 0.135f;

        // Scrollbar track
        GameObject sbGo = new GameObject("Scrollbar");
        sbGo.transform.SetParent(scrollObj.transform, false);
        RectTransform sbRt = sbGo.AddComponent<RectTransform>();
        sbRt.anchorMin        = new Vector2(1f, 0f);
        sbRt.anchorMax        = new Vector2(1f, 1f);
        sbRt.pivot            = new Vector2(1f, 0.5f);
        sbRt.sizeDelta        = new Vector2(10f, 0f);
        sbRt.anchoredPosition = Vector2.zero;
        sbGo.AddComponent<Image>().color = new Color(0.05f, 0.15f, 0.22f, 0.9f);

        // Sliding area
        GameObject sliding = new GameObject("SlidingArea");
        sliding.transform.SetParent(sbGo.transform, false);
        RectTransform slrt = sliding.AddComponent<RectTransform>();
        slrt.anchorMin = Vector2.zero; slrt.anchorMax = Vector2.one;
        slrt.offsetMin = slrt.offsetMax = Vector2.zero;

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliding.transform, false);
        RectTransform hrt2 = handle.AddComponent<RectTransform>();
        hrt2.anchorMin = hrt2.anchorMax = new Vector2(0.5f, 0.5f);
        hrt2.sizeDelta = new Vector2(10f, 60f);
        Image hImg = handle.AddComponent<Image>();
        hImg.color = ColCyan;

        Scrollbar scrollbar = sbGo.AddComponent<Scrollbar>();
        scrollbar.handleRect    = hrt2;
        scrollbar.direction     = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = hImg;
        sr.verticalScrollbar    = scrollbar;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        // Bottom divider above close button
        MakeDivider(outer, 0.07f, ColCyanDim);

        uiRoot.SetActive(false);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────
    private GameObject MakeImage(GameObject parent, string name, Color col,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = col;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        return go;
    }

    private GameObject MakeText(GameObject parent, string name, string text,
        float size, FontStyles style, Color col, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = col;
        tmp.alignment = align;
        tmp.richText  = true;
        return go;
    }

    private void StretchRect(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
    }

    private void AddOutline(GameObject go, Color col, float width)
    {
        Outline o = go.AddComponent<Outline>();
        o.effectColor    = col;
        o.effectDistance = new Vector2(width, -width);
        o.useGraphicAlpha = false;
    }

    private void MakeDivider(GameObject parent, float anchorY, Color col)
    {
        GameObject d = new GameObject("Divider");
        d.transform.SetParent(parent.transform, false);
        Image img = d.AddComponent<Image>();
        img.color = col;
        RectTransform rt = d.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.01f, anchorY);
        rt.anchorMax = new Vector2(0.99f, anchorY);
        rt.sizeDelta = new Vector2(0, 1.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    private void MakeSideRow(GameObject parent, string label, string value, Color valCol, float yPos)
    {
        GameObject row = new GameObject("Row_" + label);
        row.transform.SetParent(parent.transform, false);
        RectTransform rrt = row.AddComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0, 1); rrt.anchorMax = new Vector2(1, 1);
        rrt.sizeDelta = new Vector2(0, 30); rrt.anchoredPosition = new Vector2(0, yPos);

        // Label (left)
        GameObject lbl = MakeText(row, "Lbl", label, 9, FontStyles.Normal,
            ColSubText, TextAlignmentOptions.MidlineLeft);
        RectTransform lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.04f, 0); lrt.anchorMax = new Vector2(0.48f, 1);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        // Value (right)
        GameObject val = MakeText(row, "Val", value, 9, FontStyles.Bold, valCol,
            TextAlignmentOptions.MidlineRight);
        RectTransform vrt = val.GetComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0.5f, 0); vrt.anchorMax = new Vector2(0.96f, 1);
        vrt.offsetMin = vrt.offsetMax = Vector2.zero;
    }

    // Corner bracket decoration (4 corners)
    private void AddCornerDeco(GameObject parent, Color col)
    {
        float size = 18f;
        float thick = 2.5f;
        // Each corner: horizontal + vertical bar
        Vector2[][] corners = new Vector2[][]
        {
            new Vector2[]{ new Vector2(0,1), new Vector2(0,1) },   // top-left
            new Vector2[]{ new Vector2(1,1), new Vector2(1,1) },   // top-right
            new Vector2[]{ new Vector2(0,0), new Vector2(0,0) },   // bot-left
            new Vector2[]{ new Vector2(1,0), new Vector2(1,0) },   // bot-right
        };
        float[] signs = new float[] { 1, -1, 1, -1 };   // x direction
        float[] signsY = new float[] { -1, -1, 1, 1 };  // y direction

        for (int i = 0; i < 4; i++)
        {
            Vector2 anchor = corners[i][0];
            float sx = signs[i];   float sy = signsY[i];

            // Horizontal bar
            GameObject h = new GameObject("CornerH" + i);
            h.transform.SetParent(parent.transform, false);
            Image hi = h.AddComponent<Image>(); hi.color = col;
            RectTransform hrt = h.GetComponent<RectTransform>();
            hrt.anchorMin = hrt.anchorMax = anchor;
            hrt.pivot = new Vector2(i < 2 ? (i == 0 ? 0 : 1) : (i == 2 ? 0 : 1), sy < 0 ? 1 : 0);
            hrt.sizeDelta = new Vector2(size, thick);
            hrt.anchoredPosition = Vector2.zero;

            // Vertical bar
            GameObject v = new GameObject("CornerV" + i);
            v.transform.SetParent(parent.transform, false);
            Image vi = v.AddComponent<Image>(); vi.color = col;
            RectTransform vrt = v.GetComponent<RectTransform>();
            vrt.anchorMin = vrt.anchorMax = anchor;
            vrt.pivot = new Vector2(i < 2 ? (i == 0 ? 0 : 1) : (i == 2 ? 0 : 1), sy < 0 ? 1 : 0);
            vrt.sizeDelta = new Vector2(thick, size);
            vrt.anchoredPosition = Vector2.zero;
        }
    }
}

// ─── Blink component (keycard alert) ──────────────────────────────────────────
public class BlinkEffect : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private float timer;
    private bool state = true;

    private void Start() => tmp = GetComponent<TextMeshProUGUI>();

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= 0.7f)
        {
            state = !state;
            if (tmp != null) tmp.alpha = state ? 1f : 0.25f;
            timer = 0f;
        }
    }
}
