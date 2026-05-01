using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// วาง BoxCollider (Is Trigger) + script นี้ในแต่ละห้อง
/// กำหนดบทสนทนาใน Inspector (lines[]) แล้วจะแสดงเมื่อ Player เดินเข้า (one-shot)
///
/// ตัวอย่าง lines สำหรับแต่ละห้อง อยู่ในคอมเมนต์ท้าย file
/// </summary>
[RequireComponent(typeof(Collider))]
public class NarrativeTrigger : MonoBehaviour
{
    [Header("Speaker")]
    public string speakerName = "อาร์ม";

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Options")]
    public bool lockPlayerWhileReading = true;
    public float typewriterSpeed = 38f;     // ตัวอักษรต่อวินาที
    public bool triggerOnStart = false;     // เปิดเพื่อ auto-trigger เมื่อ scene โหลด (ห้องนอน)

    private bool triggered;
    private bool inputPressed;
    private PlayerMovement playerMovement;

    // UI refs
    private Canvas dialogCanvas;
    private GameObject dialogPanel;
    private TextMeshProUGUI speakerLabel;
    private TextMeshProUGUI dialogText;
    private TextMeshProUGUI promptText;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        GetComponent<Collider>().isTrigger = true;
        BuildUI();

        if (triggerOnStart)
        {
            triggered = true;
            StartCoroutine(PlayDialogue());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            inputPressed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(PlayDialogue());
    }

    // ─── Dialogue flow ────────────────────────────────────────────────────────
    private IEnumerator PlayDialogue()
    {
        if (lines == null || lines.Length == 0) yield break;

        if (lockPlayerWhileReading && playerMovement != null)
            playerMovement.enabled = false;

        dialogPanel.SetActive(true);
        speakerLabel.text = speakerName;

        foreach (string line in lines)
        {
            inputPressed = false;
            promptText.text = "";
            yield return TypeLine(line);

            promptText.text = "[ E / Space  —  Next ]";
            yield return new WaitUntil(() => inputPressed);
        }

        dialogPanel.SetActive(false);

        if (lockPlayerWhileReading && playerMovement != null)
            playerMovement.enabled = true;
    }

    private IEnumerator TypeLine(string text)
    {
        dialogText.text = "";
        foreach (char c in text)
        {
            if (inputPressed)           // กด E ระหว่างพิมพ์ → แสดงทั้งบรรทัดทันที
            {
                dialogText.text = text;
                inputPressed = false;
                yield break;
            }
            dialogText.text += c;
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
    }

    // ─── Build UI ─────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var canvasGO = new GameObject($"NarrativeCanvas_{name}");
        dialogCanvas = canvasGO.AddComponent<Canvas>();
        dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dialogCanvas.sortingOrder = 180;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // กรอบ dialog หลัก
        dialogPanel = new GameObject("DialogPanel");
        dialogPanel.transform.SetParent(canvasGO.transform, false);
        var panelImg = dialogPanel.AddComponent<Image>();
        panelImg.color = new Color(0.03f, 0.03f, 0.09f, 0.90f);
        var panelRect = dialogPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.04f);
        panelRect.anchorMax = new Vector2(0.95f, 0.30f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // เส้นขอบบนสีฟ้า
        var borderGO = new GameObject("TopBorder");
        borderGO.transform.SetParent(dialogPanel.transform, false);
        var border = borderGO.AddComponent<Image>();
        border.color = new Color(0.18f, 0.55f, 1f, 0.85f);
        var borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0.94f);
        borderRect.anchorMax = new Vector2(1f, 1f);
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        // ชื่อผู้พูด (บนซ้าย)
        var speakerGO = new GameObject("SpeakerLabel");
        speakerGO.transform.SetParent(dialogPanel.transform, false);
        speakerLabel = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerLabel.fontSize = 18;
        speakerLabel.fontStyle = FontStyles.Bold;
        speakerLabel.color = new Color(0.35f, 0.78f, 1f);
        var speakerRect = speakerGO.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0.02f, 0.76f);
        speakerRect.anchorMax = new Vector2(0.5f, 0.94f);
        speakerRect.offsetMin = Vector2.zero;
        speakerRect.offsetMax = Vector2.zero;

        // ข้อความ dialog หลัก
        var textGO = new GameObject("DialogText");
        textGO.transform.SetParent(dialogPanel.transform, false);
        dialogText = textGO.AddComponent<TextMeshProUGUI>();
        dialogText.fontSize = 22;
        dialogText.color = new Color(0.92f, 0.92f, 0.92f);
        dialogText.enableWordWrapping = true;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.02f, 0.20f);
        textRect.anchorMax = new Vector2(0.98f, 0.76f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // ข้อความ prompt (ล่างขวา)
        var promptGO = new GameObject("PromptText");
        promptGO.transform.SetParent(dialogPanel.transform, false);
        promptText = promptGO.AddComponent<TextMeshProUGUI>();
        promptText.fontSize = 15;
        promptText.alignment = TextAlignmentOptions.MidlineRight;
        promptText.color = new Color(0.45f, 0.45f, 0.55f);
        var promptRect = promptGO.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.6f, 0.02f);
        promptRect.anchorMax = new Vector2(0.98f, 0.20f);
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;

        dialogPanel.SetActive(false);
    }
}

/*
 ══════════════════════════════════════════════════════
  Example lines for each room (copy into Inspector)
 ══════════════════════════════════════════════════════

 ┌─ Bedroom ─────────────────────────────────────────────
 │ speakerName : Arm
 │ triggerOnStart : ✓ (auto-trigger after awakening)
 │
 │ "Emergency alarm... it's been going off for a while now."
 │ "My badge... where did it go?"
 │ "Not a single sound. There should be a night shift here..."
 │ "I need to check it out. Start with the electrical control room."
 └──────────────────────────────────────────────────────

 ┌─ Station Control Room (distress call) ───────────────
 │ speakerName : Arm
 │
 │ "This is the transmitter to Earth... but it's heavily damaged."
 │ "(Trying to power up) — Circuit breakers tripped. Almost every line."
 │ "If I can't send a distress signal directly..."
 │ "Wait — system logs mention an emergency shuttle at Docking Bay C."
 │ "I need to find a way to get there."
 └──────────────────────────────────────────────────────

 ┌─ Security Room (summary / Guard Log) ────────────────
 │ speakerName : [ Guard Log: Krit ]
 │
 │ "Guard Krit's log — March 14, 2387, 02:47."
 │ "Unknown life form breached through the ventilation ducts. Multiple crew members vanished overnight."
 │ "I sent an SOS to Station Helios-7. No response yet."
 │ "If anyone reads this — get to Docking Bay C. The emergency shuttle is still operational."
 │ "Don't waste time. It's still in the station."
 │
 │ ── add a second NarrativeTrigger right after ──
 │ speakerName : Arm
 │ "...So that's what happened."
 │ "I have to get out before it catches up."
 └──────────────────────────────────────────────────────
*/
