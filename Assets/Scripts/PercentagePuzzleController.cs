using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// มินิเกมกดปุ่มให้หลอดเต็มพอดี 100% — Sci-Fi theme, สร้าง UI ทั้งหมดใน code
/// </summary>
public class PercentagePuzzleController : MonoBehaviour
{
    // ─── Puzzle Presets ───────────────────────────────────────────────────────
    private static readonly int[][] presets =
    {
        new[] { 15, 20, 25, 30, 35 }, // skip 25 → 100
        new[] { 10, 20, 30, 40, 50 }, // หลายคำตอบ
        new[] {  5, 20, 25, 30, 45 }, // หลายคำตอบ
        new[] { 10, 15, 25, 35, 40 }, // 10+15+35+40=100
    };

    // ─── Sci-Fi Colors ────────────────────────────────────────────────────────
    private static readonly Color C_DARK_BG     = new Color(0.02f, 0.05f, 0.10f);
    private static readonly Color C_PANEL_BG    = new Color(0.03f, 0.07f, 0.13f);
    private static readonly Color C_CYAN        = new Color(0.00f, 0.85f, 0.95f);
    private static readonly Color C_CYAN_DIM    = new Color(0.00f, 0.45f, 0.55f);
    private static readonly Color C_GREEN_NEON  = new Color(0.10f, 0.95f, 0.30f);
    private static readonly Color C_RED_NEON    = new Color(0.95f, 0.12f, 0.12f);
    private static readonly Color C_AMBER       = new Color(0.95f, 0.72f, 0.00f);
    private static readonly Color C_BTN_BG      = new Color(0.04f, 0.09f, 0.17f);
    private static readonly Color C_BTN_USED_BG = new Color(0.05f, 0.14f, 0.06f);
    private static readonly Color C_BTN_BORDER  = new Color(0.00f, 0.55f, 0.68f);
    private static readonly Color C_BTN_BORDER_USED = new Color(0.08f, 0.30f, 0.10f);

    // ─── UI References ────────────────────────────────────────────────────────
    private Image             greenBar;
    private Image             redBar;
    private TextMeshProUGUI   percentText;
    private TextMeshProUGUI   statusText;
    private Button[]          buttons      = new Button[5];
    private Image[]           buttonBgs    = new Image[5];
    private Image[]           buttonBorders = new Image[5];
    private TextMeshProUGUI[] buttonLabels = new TextMeshProUGUI[5];
    private TextMeshProUGUI[] buttonSubs   = new TextMeshProUGUI[5];

    // ─── Game State ───────────────────────────────────────────────────────────
    private int[]  values;
    private bool[] pressed;
    private int    currentPercent;
    private bool   puzzleSolved;

    private Action onSuccess;
    private Action onClose;
    private bool   uiBuilt;
    private Sprite sprSquare;
    private Sprite sprCircle;

    // ─── Button Node IDs (ไม่บอก %) ─────────────────────────────────────────
    private static readonly string[] nodeIds = { "NODE·01", "NODE·02", "NODE·03", "NODE·04", "NODE·05" };

    // ─── Public API ───────────────────────────────────────────────────────────

    public void StartMinigame(Action onComplete, Action onCancel)
    {
        EnsureUI();
        onSuccess    = onComplete;
        onClose      = onCancel;
        puzzleSolved = false;
        PickValues();
        ResetAttempt();
    }

    // ─── Setup ────────────────────────────────────────────────────────────────

    private void EnsureUI()
    {
        if (uiBuilt) return;
        sprSquare = MakeSquareSprite();
        sprCircle = MakeCircleSprite(64);
        BuildUI();
        uiBuilt = true;
    }

    private void PickValues()
    {
        int[] src = presets[UnityEngine.Random.Range(0, presets.Length)];
        values = (int[])src.Clone();
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    private void ResetAttempt()
    {
        pressed        = new bool[5];
        currentPercent = 0;
        puzzleSolved   = false;

        for (int i = 0; i < 5; i++)
        {
            if (buttonLabels[i] != null) buttonLabels[i].text = nodeIds[i];
            if (buttonSubs[i]   != null) buttonSubs[i].text   = "[ STANDBY ]";
        }

        if (statusText != null) statusText.text = "";
        RefreshBarAndButtons();
    }

    // ─── Game Logic ───────────────────────────────────────────────────────────

    private void OnButtonPressed(int idx)
    {
        if (pressed[idx] || puzzleSolved) return;
        pressed[idx]    = true;
        currentPercent += values[idx];
        statusText.text = "";
        RefreshBarAndButtons();
        EvaluateState();
    }

    private void EvaluateState()
    {
        if (currentPercent == 100)
        {
            puzzleSolved    = true;
            statusText.text = "◈ CALIBRATION COMPLETE ◈";
            statusText.color = C_GREEN_NEON;
            for (int i = 0; i < 5; i++) buttons[i].interactable = false;
            StartCoroutine(SuccessRoutine());
            return;
        }

        statusText.color = C_AMBER;

        if (currentPercent > 100)
        {
            statusText.text = $"⚠ OVERFLOW DETECTED  [{currentPercent}%]  — RESET REQUIRED";
            return;
        }

        int remaining = 0;
        for (int i = 0; i < 5; i++)
            if (!pressed[i]) remaining += values[i];

        if (currentPercent + remaining < 100)
            statusText.text = "⚠ TARGET UNREACHABLE — RESET REQUIRED";
    }

    private IEnumerator SuccessRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        onSuccess?.Invoke();
    }

    // ─── UI Refresh ───────────────────────────────────────────────────────────

    private void RefreshBarAndButtons()
    {
        greenBar.fillAmount = Mathf.Min(currentPercent, 100) / 100f;
        redBar.fillAmount   = Mathf.Max(currentPercent - 100, 0) / 100f;

        if (percentText != null)
            percentText.text = $"{currentPercent}%";

        for (int i = 0; i < 5; i++)
        {
            bool used = pressed[i];
            buttonBgs[i].color     = used ? C_BTN_USED_BG : C_BTN_BG;
            buttonBorders[i].color = used ? C_BTN_BORDER_USED : C_BTN_BORDER;
            buttonLabels[i].color  = used ? C_GREEN_NEON * 0.6f : C_CYAN;
            if (buttonSubs[i] != null)
                buttonSubs[i].text  = used ? "[ ✓ ENGAGED ]" : "[ STANDBY ]";
            if (buttonSubs[i] != null)
                buttonSubs[i].color = used ? C_GREEN_NEON : C_CYAN_DIM;
            buttons[i].interactable = !used && !puzzleSolved;
        }
    }

    // ─── UI Construction ─────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Panel background
        var rootImg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        rootImg.color  = C_PANEL_BG;
        rootImg.sprite = sprSquare;

        // Inner dark background (slightly lighter border effect)
        BuildPanelFrame();
        BuildTitle();
        BuildProgressBar();
        BuildButtons();
        BuildStatusText();
        BuildRefreshButton();
        BuildCloseButton();
    }

    private void BuildPanelFrame()
    {
        // เส้นขอบ cyan บาง ๆ รอบ panel
        var frameGO = new GameObject("Frame");
        frameGO.transform.SetParent(transform, false);
        var frt = frameGO.AddComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(4, 4); frt.offsetMax = new Vector2(-4, -4);
        var fImg = frameGO.AddComponent<Image>();
        fImg.color  = C_BTN_BORDER;
        fImg.sprite = sprSquare;

        // พื้นที่ภายในกรอบ
        var innerGO = new GameObject("Inner");
        innerGO.transform.SetParent(frameGO.transform, false);
        var irt = innerGO.AddComponent<RectTransform>();
        irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(2, 2); irt.offsetMax = new Vector2(-2, -2);
        var iImg = innerGO.AddComponent<Image>();
        iImg.color  = C_DARK_BG;
        iImg.sprite = sprSquare;

        // มุมประดับ (corner markers)
        string[] corners = { "◤", "◥", "◣", "◢" };
        Vector2[] cAnchors = {
            new Vector2(0.01f, 0.95f), new Vector2(0.92f, 0.95f),
            new Vector2(0.01f, 0.01f), new Vector2(0.92f, 0.01f),
        };
        for (int i = 0; i < 4; i++)
            MakeText($"Corner_{i}", transform, corners[i],
                cAnchors[i], new Vector2(cAnchors[i].x + 0.07f, cAnchors[i].y + 0.05f),
                12, C_CYAN_DIM);
    }

    private void BuildTitle()
    {
        MakeText("Title", transform, "◈  POWER CALIBRATION UNIT  ◈",
            new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f),
            14, C_CYAN);

        MakeText("Subtitle", transform, "SELECT NODES TO REACH TARGET ENERGY LEVEL",
            new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.89f),
            9, C_CYAN_DIM);
    }

    private void BuildProgressBar()
    {
        // label
        MakeText("BarLabel", transform, "ENERGY LEVEL",
            new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.80f),
            9, C_CYAN_DIM);

        // กรอบ cyan
        var borderGO = new GameObject("BarBorder");
        borderGO.transform.SetParent(transform, false);
        var brt = borderGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.08f, 0.64f);
        brt.anchorMax = new Vector2(0.92f, 0.75f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color  = C_BTN_BORDER;
        borderImg.sprite = sprSquare;

        // พื้นหลังมืด
        var bgGO = new GameObject("BarBg");
        bgGO.transform.SetParent(borderGO.transform, false);
        var bgrt = bgGO.AddComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = new Vector2(3, 3); bgrt.offsetMax = new Vector2(-3, -3);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color  = new Color(0.01f, 0.03f, 0.06f);
        bgImg.sprite = sprSquare;

        // หลอดเขียว
        var greenGO = new GameObject("GreenFill");
        greenGO.transform.SetParent(bgGO.transform, false);
        greenBar = greenGO.AddComponent<Image>();
        greenBar.color      = C_GREEN_NEON;
        greenBar.sprite     = sprSquare;
        greenBar.type       = Image.Type.Filled;
        greenBar.fillMethod = Image.FillMethod.Horizontal;
        greenBar.fillOrigin = 0;
        greenBar.fillAmount = 0f;
        var grt = greenGO.GetComponent<RectTransform>();
        grt.anchorMin = Vector2.zero; grt.anchorMax = Vector2.one;
        grt.offsetMin = grt.offsetMax = Vector2.zero;

        // หลอดแดง (ทับจากขวา)
        var redGO = new GameObject("RedFill");
        redGO.transform.SetParent(bgGO.transform, false);
        redBar = redGO.AddComponent<Image>();
        redBar.color      = C_RED_NEON;
        redBar.sprite     = sprSquare;
        redBar.type       = Image.Type.Filled;
        redBar.fillMethod = Image.FillMethod.Horizontal;
        redBar.fillOrigin = 1;
        redBar.fillAmount = 0f;
        var rrt = redGO.GetComponent<RectTransform>();
        rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
        rrt.offsetMin = rrt.offsetMax = Vector2.zero;

        // เปอร์เซ็นต์กลางหลอด
        percentText = MakeText("PctText", bgGO.transform, "0%",
            Vector2.zero, Vector2.one, 12, Color.white);
    }

    private void BuildButtons()
    {
        float bW = 0.28f, bH = 0.175f;
        Vector2[] anchors =
        {
            new Vector2(0.04f, 0.44f),
            new Vector2(0.36f, 0.44f),
            new Vector2(0.68f, 0.44f),
            new Vector2(0.04f, 0.23f),
            new Vector2(0.36f, 0.23f),
        };

        for (int i = 0; i < 5; i++)
        {
            int idx  = i;
            var amin = anchors[i];
            var amax = new Vector2(amin.x + bW, amin.y + bH);

            // กรอบ cyan
            var borderGO = new GameObject($"BtnBorder_{i}");
            borderGO.transform.SetParent(transform, false);
            var brt = borderGO.AddComponent<RectTransform>();
            brt.anchorMin = amin; brt.anchorMax = amax;
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color  = C_BTN_BORDER;
            borderImg.sprite = sprSquare;
            buttonBorders[i] = borderImg;

            // ปุ่มด้านใน
            var btnGO = new GameObject($"Btn_{i}");
            btnGO.transform.SetParent(borderGO.transform, false);
            var btnrt = btnGO.AddComponent<RectTransform>();
            btnrt.anchorMin = Vector2.zero; btnrt.anchorMax = Vector2.one;
            btnrt.offsetMin = new Vector2(2, 2); btnrt.offsetMax = new Vector2(-2, -2);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color  = C_BTN_BG;
            btnImg.sprite = sprSquare;
            buttonBgs[i]  = btnImg;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => OnButtonPressed(idx));
            var cb = btn.colors;
            cb.highlightedColor = new Color(0.06f, 0.14f, 0.24f);
            cb.pressedColor     = new Color(0.02f, 0.06f, 0.10f);
            cb.disabledColor    = new Color(0.04f, 0.09f, 0.05f);
            btn.colors = cb;
            buttons[i] = btn;

            // ชื่อ Node (บนปุ่ม)
            buttonLabels[i] = MakeText($"Label_{i}", btnGO.transform, nodeIds[i],
                new Vector2(0f, 0.52f), Vector2.one, 14, C_CYAN);

            // สถานะ (ล่างปุ่ม)
            buttonSubs[i] = MakeText($"Sub_{i}", btnGO.transform, "[ STANDBY ]",
                Vector2.zero, new Vector2(1f, 0.52f), 9, C_CYAN_DIM);
        }
    }

    private void BuildStatusText()
    {
        statusText = MakeText("Status", transform, "",
            new Vector2(0.05f, 0.09f), new Vector2(0.70f, 0.20f),
            10, C_AMBER);
        statusText.enableWordWrapping = true;
    }

    private void BuildRefreshButton()
    {
        var go = new GameObject("RefreshBtn");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.76f, 0.04f);
        rt.anchorMax = new Vector2(0.96f, 0.21f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color  = new Color(0.50f, 0.06f, 0.06f);
        img.sprite = sprCircle;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = new Color(0.72f, 0.08f, 0.08f);
        cb.pressedColor     = new Color(0.35f, 0.04f, 0.04f);
        btn.colors = cb;
        btn.onClick.AddListener(() =>
        {
            if (!puzzleSolved)
            {
                statusText.text  = "";
                statusText.color = C_AMBER;
                ResetAttempt();
            }
        });

        MakeText("RTxt", go.transform, "↺",
            Vector2.zero, Vector2.one, 26, Color.white);
    }

    private void BuildCloseButton()
    {
        var go = new GameObject("CloseBtn");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.76f, 0.23f);
        rt.anchorMax = new Vector2(0.96f, 0.35f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color  = new Color(0.06f, 0.12f, 0.20f);
        img.sprite = sprSquare;

        // กรอบปุ่ม exit
        var borderGO = new GameObject("CloseBorder");
        borderGO.transform.SetParent(go.transform, false);
        var brt = borderGO.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var bImg = borderGO.AddComponent<Image>();
        bImg.color  = new Color(0.30f, 0.08f, 0.08f);
        bImg.sprite = sprSquare;

        var innerGO = new GameObject("CloseInner");
        innerGO.transform.SetParent(borderGO.transform, false);
        var irt = innerGO.AddComponent<RectTransform>();
        irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(2, 2); irt.offsetMax = new Vector2(-2, -2);
        var iImg = innerGO.AddComponent<Image>();
        iImg.color  = new Color(0.08f, 0.03f, 0.03f);
        iImg.sprite = sprSquare;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClose?.Invoke());

        MakeText("ExitTxt", borderGO.transform, "EXIT",
            Vector2.zero, Vector2.one, 11, new Color(0.85f, 0.25f, 0.25f));
    }

    // ─── UI Helpers ───────────────────────────────────────────────────────────

    private TextMeshProUGUI MakeText(string name, Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text               = text;
        tmp.fontSize           = size;
        tmp.color              = color;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(4, 4);
        var px  = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    private Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        float c = size / 2f, r = c - 1f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(c, c));
                px[y * size + x] = d <= r ? Color.white : Color.clear;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
