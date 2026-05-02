using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEditor.Rendering.MaterialUpgrader;

public class CassetteDeck : MonoBehaviour, IInteractable
{
    [Header("Speaker")]
    public string speakerName = "Crew";

    [Header("Dialogue Lines")]
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Options")]
    public bool lockPlayerWhileReading = true;
    public float typewriterSpeed = 38f;     // ตัวอักษรต่อวินาที

    private bool inputPressed;
    private PlayerMovement playerMovement;

    // UI refs
    private Canvas dialogCanvas;
    private GameObject dialogPanel;
    private TextMeshProUGUI speakerLabel;
    private TextMeshProUGUI dialogText;
    private TextMeshProUGUI promptText;

    public bool IsFilled { get; private set; }

    public string GetDescription()
    {
        if (IsFilled) return "[ Played ]";

        var inv = GetPlayerInventory();
        return inv != null && inv.HasItem("Tape")
            ? "Press E to Play Tape"
            : "Tape required";
    }

    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        BuildUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            inputPressed = true;
    }

    public void Interact()
    {
        if (IsFilled) return;

        var inv = GetPlayerInventory();
        if (inv == null || !inv.HasItem("Tape")) return;

        inv.RemoveItem("Tape");
        IsFilled = true;

        StartCoroutine(PlayDialogue());
    }

    private PlayerInventory GetPlayerInventory()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerInventory>() : null;
    }

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

        GameFlowManager.Instance?.CompleteSecurityRoom();
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
