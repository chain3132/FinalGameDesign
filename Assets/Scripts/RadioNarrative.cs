using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Radio ในห้อง Hacking — กด E เพื่อฟัง
/// แสดงข้อความบรรยายแบบ typewriter, กด E/Space ข้ามได้
/// เล่นได้ครั้งเดียว (one-shot)
/// </summary>
public class RadioNarrative : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] lines = {
        "[ Static interference... ]",
        "\"...This is Station Helios-7... we received the SOS from Na-se...\"",
        "\"...Dispatching a rescue unit... ETA 72 hours...\"",
        "\"...Do NOT leave the station... Repeat, do NOT leave— \" [ Signal lost ]",
        "Arm: 72 hours... I may not last that long."
    };
    public float typewriterSpeed = 36f;

    private bool hasPlayed;
    private bool isPlaying;
    private bool inputPressed;

    // UI
    private Canvas radioCanvas;
    private GameObject panel;
    private TextMeshProUGUI speakerLabel;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI promptText;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start() => BuildUI();

    private void Update()
    {
        if (isPlaying && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
            inputPressed = true;
    }

    public string GetDescription() => hasPlayed ? "[ วิทยุ — ใช้งานแล้ว ]" : "กด E เพื่อฟังวิทยุ";

    public void Interact()
    {
        if (hasPlayed || isPlaying) return;
        hasPlayed = true;
        StartCoroutine(PlayRadio());
    }

    // ─── Dialogue sequence ────────────────────────────────────────────────────
    private IEnumerator PlayRadio()
    {
        isPlaying = true;
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        panel.SetActive(true);

        foreach (string line in lines)
        {
            inputPressed = false;
            promptText.text = "";

            // ชื่อผู้พูด: ถ้าขึ้นต้นด้วย '"' = สัญญาณวิทยุ, อื่นๆ = บรรยาย
            speakerLabel.text = line.StartsWith("\"") || line.StartsWith("[")
                ? "〔 Radio 〕"
                : "Arm";

            yield return TypeLine(line);
            promptText.text = "[ E / Space — Next ]";
            yield return new WaitUntil(() => inputPressed);
        }

        panel.SetActive(false);
        isPlaying = false;
        if (pm != null) pm.enabled = true;

        // แจ้ง GameFlow ว่า control room (ขอความช่วยเหลือ) เสร็จแล้ว
        // (Radio อยู่ในห้อง Hacking ซึ่งถือว่าผ่านห้อง Control มาแล้ว)
        GameFlowManager.Instance?.CompleteControlRoom();
    }

    private IEnumerator TypeLine(string text)
    {
        bodyText.text = "";
        foreach (char c in text)
        {
            if (inputPressed) { bodyText.text = text; inputPressed = false; yield break; }
            bodyText.text += c;
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
    }

    // ─── Build UI ─────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var canvasGO = new GameObject("RadioCanvas");
        radioCanvas = canvasGO.AddComponent<Canvas>();
        radioCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        radioCanvas.sortingOrder = 180;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        panel = new GameObject("RadioPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelImg  = panel.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.04f, 0.08f, 0.92f);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.04f);
        panelRect.anchorMax = new Vector2(0.95f, 0.30f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // เส้นขอบ (สีเหลืองทอง = วิทยุ)
        var borderGO  = new GameObject("Border");
        borderGO.transform.SetParent(panel.transform, false);
        var border    = borderGO.AddComponent<Image>();
        border.color  = new Color(0.85f, 0.65f, 0.1f, 0.9f);
        var borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0.94f);
        borderRect.anchorMax = new Vector2(1f, 1f);
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        // Speaker label
        var spGO  = new GameObject("SpeakerLabel");
        spGO.transform.SetParent(panel.transform, false);
        speakerLabel = spGO.AddComponent<TextMeshProUGUI>();
        speakerLabel.fontSize  = 17;
        speakerLabel.fontStyle = FontStyles.Bold;
        speakerLabel.color     = new Color(0.9f, 0.75f, 0.2f);
        var spRect = spGO.GetComponent<RectTransform>();
        spRect.anchorMin = new Vector2(0.02f, 0.76f);
        spRect.anchorMax = new Vector2(0.6f, 0.94f);
        spRect.offsetMin = Vector2.zero;
        spRect.offsetMax = Vector2.zero;

        // Body text
        var btGO  = new GameObject("BodyText");
        btGO.transform.SetParent(panel.transform, false);
        bodyText = btGO.AddComponent<TextMeshProUGUI>();
        bodyText.fontSize          = 21;
        bodyText.color             = new Color(0.88f, 0.88f, 0.88f);
        bodyText.enableWordWrapping = true;
        var btRect = btGO.GetComponent<RectTransform>();
        btRect.anchorMin = new Vector2(0.02f, 0.20f);
        btRect.anchorMax = new Vector2(0.98f, 0.76f);
        btRect.offsetMin = Vector2.zero;
        btRect.offsetMax = Vector2.zero;

        // Prompt
        var prGO  = new GameObject("PromptText");
        prGO.transform.SetParent(panel.transform, false);
        promptText = prGO.AddComponent<TextMeshProUGUI>();
        promptText.fontSize  = 14;
        promptText.alignment = TextAlignmentOptions.MidlineRight;
        promptText.color     = new Color(0.45f, 0.45f, 0.5f);
        var prRect = prGO.GetComponent<RectTransform>();
        prRect.anchorMin = new Vector2(0.6f, 0.02f);
        prRect.anchorMax = new Vector2(0.98f, 0.20f);
        prRect.offsetMin = Vector2.zero;
        prRect.offsetMax = Vector2.zero;

        panel.SetActive(false);
    }
}
