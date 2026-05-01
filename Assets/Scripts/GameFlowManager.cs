using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Singleton ที่ควบคุม game flow ทั้งหมด
/// วางบน empty GameObject ใน Scene (DontDestroyOnLoad)
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    // ─── Stage flags ──────────────────────────────────────────────────────────
    [Header("Stage Progress (read-only in play mode)")]
    public bool BedroomComplete;
    public bool PowerRoomComplete;
    public bool OxygenRoomComplete;
    public bool ControlRoomComplete;
    public bool HackingRoomComplete;
    public bool SecurityRoomComplete;

    // ─── Alien ────────────────────────────────────────────────────────────────
    [Header("Alien Setup")]
    public GameObject alienPrefab;
    public Transform[] oxygenPatrolPoints;
    public Transform[] hackingPatrolPoints;

    [HideInInspector] public AlienAI currentAlien;

    // ─── Death UI ─────────────────────────────────────────────────────────────
    private Image deathOverlay;
    private TextMeshProUGUI deathText;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        BuildDeathUI();
    }

    private void Start()
    {
        if (OxygenEventManager.Instance != null)
        {
            OxygenEventManager.Instance.OnEventFailed    += HandleOxygenDeath;
            OxygenEventManager.Instance.OnEventCompleted += CompleteOxygenRoom;
        }
    }

    // ─── Stage completion ─────────────────────────────────────────────────────

    public void CompleteBedroom()
    {
        if (BedroomComplete) return;
        BedroomComplete = true;
    }

    public void CompletePowerRoom()
    {
        if (PowerRoomComplete) return;
        PowerRoomComplete = true;
        OxygenEventManager.Instance?.TriggerOxygenEvent();
        SpawnAlien(oxygenPatrolPoints);
    }

    public void CompleteOxygenRoom()
    {
        if (OxygenRoomComplete) return;
        OxygenRoomComplete = true;
        // เอเลี่ยนกลับ patrol ปกติ (ไม่ไล่แล้ว)
        if (currentAlien != null)
        {
            currentAlien.isAlerted    = false;
            currentAlien.currentState = AlienAI.State.Patrol;
        }
    }

    public void CompleteControlRoom()
    {
        if (ControlRoomComplete) return;
        ControlRoomComplete = true;
    }

    public void CompleteHackingRoom()
    {
        if (HackingRoomComplete) return;
        HackingRoomComplete = true;
        // เปลี่ยน patrol ของเอเลี่ยนมายังห้อง Hacking
        if (currentAlien != null && hackingPatrolPoints.Length > 0)
            currentAlien.patrolPoints = hackingPatrolPoints;
    }

    public void CompleteSecurityRoom()
    {
        if (SecurityRoomComplete) return;
        SecurityRoomComplete = true;
    }

    // ─── Sequential door check ────────────────────────────────────────────────
    /// <summary>
    /// LockedDoor เรียกเพื่อตรวจว่า stage ที่ต้องการผ่านแล้วหรือยัง
    /// ถ้า requiredStage เป็น "" หรือ "none" → ผ่านเสมอ
    /// </summary>
    public bool CanOpenDoor(string requiredStage)
    {
        return requiredStage switch
        {
            "Bedroom"      => BedroomComplete,
            "PowerRoom"    => PowerRoomComplete,
            "OxygenRoom"   => OxygenRoomComplete,
            "ControlRoom"  => ControlRoomComplete,
            "HackingRoom"  => HackingRoomComplete,
            "SecurityRoom" => SecurityRoomComplete,
            _              => true
        };
    }

    // ─── Alien management ────────────────────────────────────────────────────
    private void SpawnAlien(Transform[] patrolPoints)
    {
        if (alienPrefab == null) return;
        if (currentAlien != null) Destroy(currentAlien.gameObject);

        var go = Instantiate(alienPrefab);
        currentAlien = go.GetComponent<AlienAI>();
        if (currentAlien == null) return;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) currentAlien.player = playerGO.transform;
        if (patrolPoints.Length > 0) currentAlien.patrolPoints = patrolPoints;
    }

    // ─── Death / Restart ──────────────────────────────────────────────────────
    private void HandleOxygenDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // ล็อคการเคลื่อนที่
        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        // Fade to black
        deathOverlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            deathOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, t / 1.5f));
            yield return null;
        }
        deathText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ─── Build death UI ───────────────────────────────────────────────────────
    private void BuildDeathUI()
    {
        var canvasGO = new GameObject("DeathCanvas");
        DontDestroyOnLoad(canvasGO);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 998;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Black overlay
        var overlayGO = new GameObject("DeathOverlay");
        overlayGO.transform.SetParent(canvasGO.transform, false);
        deathOverlay = overlayGO.AddComponent<Image>();
        deathOverlay.color = new Color(0f, 0f, 0f, 0f);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayGO.SetActive(false);

        // Death message
        var textGO = new GameObject("DeathText");
        textGO.transform.SetParent(overlayGO.transform, false);
        deathText = textGO.AddComponent<TextMeshProUGUI>();
        deathText.text      = "ออกซิเจนหมด...\nระบบช่วยชีวิตล้มเหลว";
        deathText.fontSize  = 36;
        deathText.fontStyle = FontStyles.Bold;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.color     = new Color(0.8f, 0.15f, 0.15f);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.2f, 0.4f);
        textRect.anchorMax = new Vector2(0.8f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textGO.SetActive(false);
    }
}
