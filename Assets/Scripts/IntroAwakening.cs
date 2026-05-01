using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Opening awakening sequence — screen fades in from black, inner monologue appears,
/// camera shakes while waking up. Place on Player or GameManager in the first scene.
/// </summary>
public class IntroAwakening : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Timing")]
    public float fadeInDuration   = 5f;
    public float linePauseDuration = 2.2f;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.04f;   // max offset in units
    public float shakeDuration  = 6f;      // how long the shake lasts (covers fade + first lines)

    private Image blackScreen;
    private TextMeshProUGUI subtitleText;
    private CanvasGroup subtitleGroup;
    private Canvas introCanvas;
    private bool inputPressed;

    private static readonly string[] wakeLines =
    {
        "...",
        "My head is so heavy...",
        "What's that noise...?",
        "My badge... where did it go?",
        "Not a single sound. There should be a night shift here...",
        "I need to check it out. Start with the electrical control room."
    };

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        playerMovement.enabled = false;
        BuildUI();
        StartCoroutine(AwakeSequence());
        StartCoroutine(ShakeCamera());
        // lighting and sound effects can be triggered by the NarrativeTrigger in the bedroom, set to trigger on start
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            inputPressed = true;
    }

    // ─── Sequences ────────────────────────────────────────────────────────────
    private IEnumerator AwakeSequence()
    {
        // Phase 1: fade from black
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            blackScreen.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, t / fadeInDuration));
            yield return null;
        }
        blackScreen.color = Color.clear;

        yield return new WaitForSeconds(0.6f);

        // Phase 2: inner monologue
        foreach (string line in wakeLines)
        {
            inputPressed = false;
            yield return ShowLine(line);
        }

        // Phase 3: stand-up prompt
        inputPressed = false;
        subtitleText.text      = "[ Press  E  or  Space  to stand up ]";
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color     = new Color(0.5f, 0.85f, 1f);
        yield return FadeGroup(subtitleGroup, 0f, 1f, 0.4f);
        yield return new WaitUntil(() => inputPressed);

        yield return FadeGroup(subtitleGroup, 1f, 0f, 0.5f);
        Destroy(introCanvas.gameObject);

        playerMovement.enabled = true;
    }

    /// <summary>Camera sways like waking from unconsciousness, fading to stillness.</summary>
    private IEnumerator ShakeCamera()
    {
        var cam = playerMovement != null ? playerMovement.cameraTransform : null;
        if (cam == null) yield break;

        Vector3 originLocal = cam.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = Mathf.Lerp(shakeIntensity, 0f, elapsed / shakeDuration);
            // Low-frequency sway (sin) + high-frequency jitter (random)
            float swayX = Mathf.Sin(elapsed * 1.8f) * intensity * 0.6f;
            float swayY = Mathf.Sin(elapsed * 2.3f + 1f) * intensity * 0.4f;
            float jitX  = Random.Range(-intensity, intensity) * 0.4f;
            float jitY  = Random.Range(-intensity, intensity) * 0.25f;

            cam.localPosition = originLocal + new Vector3(swayX + jitX, swayY + jitY, 0f);
            yield return null;
        }

        cam.localPosition = originLocal;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private IEnumerator ShowLine(string text)
    {
        subtitleText.text      = text;
        subtitleText.fontStyle = FontStyles.Italic;
        subtitleText.color     = new Color(0.85f, 0.85f, 0.85f);
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

    // ─── Build UI ─────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        var canvasGO = new GameObject("IntroAwakeCanvas");
        DontDestroyOnLoad(canvasGO);
        introCanvas = canvasGO.AddComponent<Canvas>();
        introCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        introCanvas.sortingOrder = 999;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen black
        var bgGO = new GameObject("BlackScreen");
        bgGO.transform.SetParent(canvasGO.transform, false);
        blackScreen = bgGO.AddComponent<Image>();
        blackScreen.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Subtitle panel — add Image first so RectTransform is created
        var subGO = new GameObject("SubtitlePanel");
        subGO.transform.SetParent(canvasGO.transform, false);
        var panelBG = subGO.AddComponent<Image>();
        panelBG.color  = new Color(0f, 0f, 0f, 0.6f);
        subtitleGroup  = subGO.AddComponent<CanvasGroup>();
        subtitleGroup.alpha = 0f;
        var subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.15f, 0.07f);
        subRect.anchorMax = new Vector2(0.85f, 0.19f);
        subRect.offsetMin = Vector2.zero;
        subRect.offsetMax = Vector2.zero;

        var textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(subGO.transform, false);
        subtitleText = textGO.AddComponent<TextMeshProUGUI>();
        subtitleText.fontSize  = 26;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color     = new Color(0.85f, 0.85f, 0.85f);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.02f, 0.08f);
        textRect.anchorMax = new Vector2(0.98f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
