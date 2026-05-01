using UnityEngine;
using UnityEngine.EventSystems;

public class FlowCellUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    [HideInInspector] public int row, col;
    private FlowPuzzleController controller;

    public void Init(int r, int c, FlowPuzzleController ctrl)
    {
        row = r; col = c; controller = ctrl;
    }

    public void OnPointerDown(PointerEventData eventData) => controller?.OnCellPointerDown(row, col);
    public void OnPointerEnter(PointerEventData eventData) => controller?.OnCellPointerEnter(row, col);
    public void OnPointerUp(PointerEventData eventData) => controller?.OnCellPointerUp(row, col);
}
