using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OxygenEventManager : MonoBehaviour
{
    public static OxygenEventManager Instance { get; private set; }

    [Header("Settings")]
    public float countdownDuration = 240f; // 4 minutes

    public bool IsEventActive { get; private set; }
    public bool IsEventCompleted { get; private set; }

    // GameFlowManager subscribe เพื่อรับ callback
    public event System.Action OnEventFailed;
    public event System.Action OnEventCompleted;

    private float remainingTime;
    private Image alertPanel;
    private TextMeshProUGUI alertText;
    private TextMeshProUGUI timerText;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        BuildAlertUI();
        TriggerOxygenEvent();
    }

    public void TriggerOxygenEvent()
    {
        if (IsEventActive || IsEventCompleted) return;
        IsEventActive = true;
        remainingTime = countdownDuration;
        alertPanel.gameObject.SetActive(true);
        blinkCoroutine = StartCoroutine(BlinkBackground());
    }

    private void Update()
    {
        if (!IsEventActive) return;

        remainingTime -= Time.deltaTime;

        int minutes = Mathf.Max(0, Mathf.FloorToInt(remainingTime / 60f));
        int seconds = Mathf.Max(0, Mathf.FloorToInt(remainingTime % 60f));
        timerText.text = $"{minutes}:{seconds:00}";
        timerText.color = remainingTime < 60f
            ? new Color(1f, 0.15f, 0.15f)
            : new Color(1f, 0.4f, 0.4f);

        if (remainingTime <= 0f) HandleFailure();
    }

    public void OnPipeRepaired()
    {
        HandleSuccess();
    }

    private void HandleSuccess()
    {
        IsEventActive = false;
        IsEventCompleted = true;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        OnEventCompleted?.Invoke();
        alertPanel.color = new Color(0f, 0.28f, 0.14f, 0.92f);
        alertText.text = "◈  OXYGEN RESTORED  ◈";
        alertText.color = new Color(0f, 1f, 0.5f);
        timerText.text = "";
        StartCoroutine(FadeOutAlert(3f));
    }

    private void HandleFailure()
    {
        IsEventActive = false;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        OnEventFailed?.Invoke();
        alertPanel.color = new Color(0.5f, 0f, 0f, 0.95f);
        alertText.text = "✕  OXYGEN DEPLETED  ✕";
        alertText.color = Color.white;
        timerText.text = "0:00";
        timerText.color = Color.red;
        // เพิ่ม logic game over ได้ที่นี่
    }

    private IEnumerator BlinkBackground()
    {
        Color bright = new Color(0.55f, 0.05f, 0.05f, 0.92f);
        Color dim    = new Color(0.30f, 0.02f, 0.02f, 0.88f);
        while (true)
        {
            alertPanel.color = bright;
            yield return new WaitForSeconds(0.55f);
            alertPanel.color = dim;
            yield return new WaitForSeconds(0.55f);
        }
    }

    private IEnumerator FadeOutAlert(float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsed = 0f;
        const float fadeDuration = 1f;
        Color start = alertPanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, elapsed / fadeDuration);
            alertPanel.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
        alertPanel.gameObject.SetActive(false);
    }

    private void BuildAlertUI()
    {
        var canvasGO = new GameObject("OxygenAlertCanvas");
        DontDestroyOnLoad(canvasGO);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // แถบแจ้งเตือนบนหน้าจอ
        var panelGO = new GameObject("OxygenAlertPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        alertPanel = panelGO.AddComponent<Image>();
        alertPanel.color = new Color(0.55f, 0.05f, 0.05f, 0.92f);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.22f, 0.918f);
        panelRect.anchorMax = new Vector2(0.78f, 0.992f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // ข้อความเตือน
        var alertGO = new GameObject("AlertText");
        alertGO.transform.SetParent(panelGO.transform, false);
        alertText = alertGO.AddComponent<TextMeshProUGUI>();
        alertText.text = "⚠  OXYGEN SYSTEM FAILURE  ⚠";
        alertText.fontSize = 21;
        alertText.fontStyle = FontStyles.Bold;
        alertText.alignment = TextAlignmentOptions.MidlineLeft;
        alertText.color = new Color(1f, 0.85f, 0f);
        var alertRect = alertGO.GetComponent<RectTransform>();
        alertRect.anchorMin = new Vector2(0.02f, 0f);
        alertRect.anchorMax = new Vector2(0.72f, 1f);
        alertRect.offsetMin = Vector2.zero;
        alertRect.offsetMax = Vector2.zero;

        // นับถอยหลัง
        var timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(panelGO.transform, false);
        timerText = timerGO.AddComponent<TextMeshProUGUI>();
        timerText.text = "4:00";
        timerText.fontSize = 30;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.MidlineRight;
        timerText.color = new Color(1f, 0.4f, 0.4f);
        var timerRect = timerGO.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.72f, 0f);
        timerRect.anchorMax = new Vector2(0.97f, 1f);
        timerRect.offsetMin = Vector2.zero;
        timerRect.offsetMax = Vector2.zero;

        panelGO.SetActive(false);
    }
}
