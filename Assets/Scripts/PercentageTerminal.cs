using UnityEngine;

/// <summary>
/// วางบน GameObject ในฉาก → ผู้เล่นกด E เพื่อเปิดมินิเกม Percentage Bar
/// ใน Inspector: assign puzzleUI (Panel ที่มี PercentagePuzzleController) และ playerMovementScript
/// </summary>
public class PercentageTerminal : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject    puzzleUI;
    [SerializeField] private MonoBehaviour playerMovementScript;

    private PercentagePuzzleController puzzleController;
    private bool isSolved = false;

    /// <summary>PowerRoomManager subscribe เพื่อรับ callback เมื่อ puzzle สำเร็จ</summary>
    public event System.Action OnSolved;

    private void Awake()
    {
        if (puzzleUI != null)
            puzzleController = puzzleUI.GetComponentInChildren<PercentagePuzzleController>(true);
    }

    public string GetDescription() => isSolved ? "Already calibrated" : "Press E to calibrate";

    public void Interact()
    {
        if (isSolved) return;
        OpenPanel();
    }

    private void OpenPanel()
    {
        puzzleUI.SetActive(true);
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        puzzleController.StartMinigame(OnSuccess, ClosePanel);
    }

    public void ClosePanel()
    {
        puzzleUI.SetActive(false);
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void OnSuccess()
    {
        isSolved = true;
        ClosePanel();
        OnSolved?.Invoke();
    }
}
