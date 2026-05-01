using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// หน้าจอ Ending — เรียก Show() จาก EscapeCapsule
/// แสดง fade-in พร้อมข้อความจบเรื่อง จากนั้นปุ่ม "กลับเมนูหลัก"
/// วางบน empty GameObject ใน Scene (ปกติ disabled)
/// </summary>
public class EndingScreen : MonoBehaviour
{
    [Header("Main Menu Scene")]
    public string mainMenuSceneName = "MainMenu";

    private Canvas endCanvas;
    private Image  overlay;
    private CanvasGroup contentGroup;

    private static readonly string[] endingLines =
    {
        "Arm fires up the shuttle's engine...",
        "Station Na-se slowly shrinks in the viewport.",
        "Everything that happened there — still a mystery.",
        "But for now... he's safe."
    };

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake() => BuildUI();

    public void Show()
    {
        gameObject.SetActive(true);
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        StartCoroutine(PlayEnding());
    }

    // ─── Sequence ─────────────────────────────────────────────────────────────
    private IEnumerator PlayEnding()
    {
        // Fade screen to black
        float t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            overlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, t / 2f));
            yield return null;
        }

        // Fade in content
        contentGroup.gameObject.SetActive(true);
        t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            contentGroup.alpha = Mathf.Lerp(0f, 1f, t / 1.5f);
            yield return null;
        }
    }

    // ─── Build UI ─────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var canvasGO = new GameObject("EndingCanvas");
        canvasGO.transform.SetParent(transform, false);
        endCanvas = canvasGO.AddComponent<Canvas>();
        endCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        endCanvas.sortingOrder = 1000;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Black overlay (starts transparent)
        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        overlay = overlayGO.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0f);
        var oRect = overlayGO.GetComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero;
        oRect.anchorMax = Vector2.one;
        oRect.offsetMin = Vector2.zero;
        oRect.offsetMax = Vector2.zero;

        // Content group (text + button)
        var contentGO = new GameObject("EndingContent");
        contentGO.transform.SetParent(canvasGO.transform, false);
        contentGroup = contentGO.AddComponent<CanvasGroup>();
        contentGroup.alpha = 0f;
        var cRect = contentGO.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.2f, 0.25f);
        cRect.anchorMax = new Vector2(0.8f, 0.75f);
        cRect.offsetMin = Vector2.zero;
        cRect.offsetMax = Vector2.zero;

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(contentGO.transform, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text      = "ESCAPED";
        title.fontSize  = 52;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color     = new Color(0.3f, 0.85f, 1f);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.72f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Story text
        var storyGO = new GameObject("StoryText");
        storyGO.transform.SetParent(contentGO.transform, false);
        var story = storyGO.AddComponent<TextMeshProUGUI>();
        story.text      = string.Join("\n\n", endingLines);
        story.fontSize  = 22;
        story.fontStyle = FontStyles.Italic;
        story.alignment = TextAlignmentOptions.Center;
        story.color     = new Color(0.8f, 0.8f, 0.8f);
        story.enableWordWrapping = true;
        var storyRect = storyGO.GetComponent<RectTransform>();
        storyRect.anchorMin = new Vector2(0f, 0.22f);
        storyRect.anchorMax = new Vector2(1f, 0.70f);
        storyRect.offsetMin = Vector2.zero;
        storyRect.offsetMax = Vector2.zero;

        // Button — กลับเมนู
        var btnGO  = new GameObject("MenuButton");
        btnGO.transform.SetParent(contentGO.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.25f, 0.45f, 0.9f);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(ReturnToMenu);
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.3f, 0.02f);
        btnRect.anchorMax = new Vector2(0.7f, 0.18f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        var btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        var btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text      = "Return to Main Menu";
        btnText.fontSize  = 22;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color     = Color.white;
        var btnTextRect = btnTextGO.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        contentGO.SetActive(false);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
