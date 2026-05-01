using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ลำดับตื่นนอนตอนเริ่มเกม — จอดำค่อยๆ สว่าง, ข้อความ inner monologue ทยอยขึ้น
/// วางบน Player หรือ GameManager object ใน Scene แรก
/// </summary>
public class IntroAwakening : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Timing")]
    public float fadeInDuration = 5f;
    public float linePauseDuration = 2.2f;

    private Image blackScreen;
    private TextMeshProUGUI subtitleText;
    private CanvasGroup subtitleGroup;
    private Canvas introCanvas;
    private bool inputPressed;

    private static readonly string[] wakeLines =
    {
        "...",
        "หัวมันหนักมาก...",
        "มีเสียงอะไรดังอยู่...",
        "นี่คือ... สถานีอวกาศ Na-se ใช่ไหม?",
        "ทำไมถึงไม่มีใครตอบสัญญาณวิทยุ..."
    };

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        playerMovement.enabled = false;
        BuildUI();
        StartCoroutine(AwakeSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            inputPressed = true;
    }

    private IEnumerator AwakeSequence()
    {
        // Phase 1: จอดำค่อยๆ จางออก
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0.0f, t / fadeInDuration));
            yield return null;
        }
        blackScreen.color = Color.clear;

        yield return new WaitForSeconds(0.6f);

        // Phase 2: inner monologue ทยอยขึ้น
        foreach (string line in wakeLines)
        {
            inputPressed = false;
            yield return ShowLine(line);
        }

        // Phase 3: prompt ให้กด E
        inputPressed = false;
        subtitleText.text = "[ กด  E  หรือ  Space  เพื่อลุกขึ้น ]";
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color = new Color(0.5f, 0.85f, 1f);
        yield return FadeGroup(subtitleGroup, 0f, 1f, 0.4f);
        yield return new WaitUntil(() => inputPressed);

        yield return FadeGroup(subtitleGroup, 1f, 0f, 0.5f);
        Destroy(introCanvas.gameObject);

        playerMovement.enabled = true;
    }

    private IEnumerator ShowLine(string text)
    {
        subtitleText.text = text;
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color = new Color(0.85f, 0.85f, 0.85f);
        yield return FadeGroup(subtitleGroup, 0f, 1f, 0.5f);
        yield return new WaitForSeconds(linePauseDuration);
        yield return FadeGroup(subtitleGroup, 1f, 0f, 0.5f);
    }

    private IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        cg.alpha = to;
    }

    // ─── Build UI ────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var canvasGO = new GameObject("IntroAwakeCanvas");
        DontDestroyOnLoad(canvasGO);
        introCanvas = canvasGO.AddComponent<Canvas>();
        introCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 999;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // จอดำเต็มหน้าจอ
        var bgGO = new GameObject("BlackScreen");
        bgGO.transform.SetParent(canvasGO.transform, false);
        blackScreen = bgGO.AddComponent<Image>();
        blackScreen.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // แถบคำบรรยายกลางล่าง
        var subGO = new GameObject("SubtitlePanel");
        subGO.transform.SetParent(canvasGO.transform, false);
        // ต้อง add Image ก่อน เพื่อให้ RectTransform ถูกสร้าง
        var panelBG = subGO.AddComponent<Image>();
        panelBG.color = new Color(0f, 0f, 0f, 0.6f);
        subtitleGroup = subGO.AddComponent<CanvasGroup>();
        subtitleGroup.alpha = 0f;
        var subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.15f, 0.07f);
        subRect.anchorMax = new Vector2(0.85f, 0.19f);
        subRect.offsetMin = Vector2.zero;
        subRect.offsetMax = Vector2.zero;

        

        var textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(subGO.transform, false);
        subtitleText = textGO.AddComponent<TextMeshProUGUI>();
        subtitleText.fontSize = 26;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.85f, 0.85f, 0.85f);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.02f, 0.08f);
        textRect.anchorMax = new Vector2(0.98f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
