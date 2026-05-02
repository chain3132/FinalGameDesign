using System;
using UnityEngine;

public class HackingTerminal : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject hackingUI;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private AlienAI[] aliensToAlert;

    private FlowPuzzleController puzzleController;
    private bool isHacked = false;

    private void Awake()
    {
        if (hackingUI != null)
            puzzleController = hackingUI.GetComponentInChildren<FlowPuzzleController>(true);
    }

    public string GetDescription() => isHacked ? "Terminal already compromised" : "Press E to hack terminal";

    public void Interact()
    {
        if (isHacked) return;
        OpenTerminal();
    }

    private void OpenTerminal()
    {
        hackingUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        puzzleController.StartMinigame(OnAllPuzzlesComplete, CloseTerminal);
    }

    public void CloseTerminal()
    {
        hackingUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
    }

    private void OnAllPuzzlesComplete()
    {
        isHacked = true;
        GameFlowManager.Instance?.CompleteHackingRoom();
        CloseTerminal();
        AlertAllAliens();
    }

    // เรียกผีทุกตัวใน aliensToAlert ให้ไล่ผู้เล่นทันที
    private void AlertAllAliens()
    {
        foreach (var alien in aliensToAlert)
        {
            if (alien != null)
                alien.ForceChase();
        }
    }
}
