using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// สร้าง UI ทั้งหมดใน code — แค่ assign hackingUI (Panel ใน Canvas) แล้วใส่ FlowPuzzleController บน GameObject เดียวกัน
/// </summary>
public class FlowPuzzleController : MonoBehaviour
{
    // ─── Puzzle Definitions ───────────────────────────────────────────────────

    private struct ColorPair
    {
        public int r1, c1, r2, c2;
        public Color color;
        public ColorPair(int r1, int c1, int r2, int c2, Color color)
        { this.r1=r1; this.c1=c1; this.r2=r2; this.c2=c2; this.color=color; }
    }

    private struct PuzzleData
    {
        public int gridSize;
        public string title;
        public ColorPair[] pairs;
    }

    private static readonly Color C_CYAN    = new Color(0f,    1f,    1f);
    private static readonly Color C_PINK    = new Color(1f,    0f,    0.7f);
    private static readonly Color C_YELLOW  = new Color(1f,    0.9f,  0f);
    private static readonly Color C_ORANGE  = new Color(1f,    0.5f,  0f);
    private static readonly Color C_GREEN   = new Color(0.1f,  1f,    0.3f);
    private static readonly Color C_DARK_BG = new Color(0.03f, 0.06f, 0.1f);
    private static readonly Color C_CELL    = new Color(0.08f, 0.13f, 0.18f);

    private PuzzleData[] puzzles;

    // ─── UI ──────────────────────────────────────────────────────────────────

    private Transform gridContainer;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI statusText;
    private Image[] cellBg;
    private Image[] cellDot;
    private FlowCellUI[,] cellUIs;
    private int currentGridSize;
    private const float GRID_PX = 460f;

    // ─── Game State ──────────────────────────────────────────────────────────

    private int puzzleIndex;
    private int[,] grid;           // -1=empty, else colorIndex
    private int[,] epMap;          // -1=not endpoint, else colorIndex
    private Vector2Int[] ep1, ep2;
    private List<Vector2Int>[] paths;
    private bool[] connected;
    private int drawColor = -1;
    private int numColors;

    private Action onAllComplete;
    private Action onClose;

    private Sprite sprCircle;
    private Sprite sprSquare;
    private bool uiBuilt = false;

    // ─── Init ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        BuildPuzzleData();
    }

    private void EnsureUI()
    {
        if (uiBuilt) return;
        sprSquare = MakeSquareSprite();
        sprCircle = MakeCircleSprite(64);
        BuildBaseUI();
        uiBuilt = true;
    }

    private void BuildPuzzleData()
    {
        puzzles = new PuzzleData[]
        {
            // Level 1 — 4×4, 3 สี (ง่าย)
            new PuzzleData { gridSize=4, title="BYPASS FIREWALL — LVL 1",
                pairs = new[]
                {
                    new ColorPair(0,0, 3,1, C_CYAN),
                    new ColorPair(0,2, 2,1, C_PINK),
                    new ColorPair(0,3, 3,3, C_YELLOW),
                }
            },
            // Level 2 — 5×5, 4 สี (ปานกลาง)
            // Solution: A=แถวบน, B=คอลัมน์ซ้าย+ล่างซ้าย, C=คอลัมน์ขวา+ล่างขวา, D=วนใน
            // Row0:AAAAA Row1:BDDDC Row2:BDDDC Row3:BDDDC Row4:BBBCC
            new PuzzleData { gridSize=5, title="BREAK ENCRYPTION — LVL 2",
                pairs = new[]
                {
                    new ColorPair(0,0, 0,4, C_CYAN),
                    new ColorPair(1,0, 4,2, C_ORANGE),
                    new ColorPair(1,4, 4,3, C_PINK),
                    new ColorPair(1,1, 3,3, C_YELLOW),
                }
            },
            // Level 3 — 6×6, 5 สี (ยาก)
            // Row0:AAAAAA Row1:BDDDDС Row2:BDDDDС Row3:BDDDDС Row4:BEEEEC Row5:BBBCCC
            new PuzzleData { gridSize=6, title="CORE ACCESS — LVL 3",
                pairs = new[]
                {
                    new ColorPair(0,0, 0,5, C_CYAN),
                    new ColorPair(1,0, 5,2, C_ORANGE),
                    new ColorPair(1,5, 5,3, C_PINK),
                    new ColorPair(1,1, 3,4, C_YELLOW),
                    new ColorPair(4,1, 4,4, C_GREEN),
                }
            },
        };
    }

    private void BuildBaseUI()
    {
        // Root background
        var rootImg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        rootImg.color = C_DARK_BG;
        rootImg.sprite = sprSquare;

        // Title
        titleText = MakeText("Title", transform, "HACKING TERMINAL",
            new Vector2(0,0.88f), new Vector2(1,1f), 20, Color.cyan);

        // Level indicator
        levelText = MakeText("Level", transform, "STAGE 1 / 3",
            new Vector2(0,0.82f), new Vector2(1,0.88f), 13, new Color(0.4f,1f,0.5f));

        // Grid container
        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(transform, false);
        var grt = gridGO.AddComponent<RectTransform>();
        gridContainer = grt;
        grt.anchorMin = new Vector2(0.5f,0.5f);
        grt.anchorMax = new Vector2(0.5f,0.5f);
        grt.sizeDelta = new Vector2(GRID_PX, GRID_PX);
        grt.anchoredPosition = new Vector2(0, -10f);
        var gbg = gridGO.AddComponent<Image>();
        gbg.color = new Color(0.05f,0.08f,0.12f);
        gbg.sprite = sprSquare;

        // Status
        statusText = MakeText("Status", transform, "",
            new Vector2(0,0.04f), new Vector2(1,0.12f), 13, new Color(1f,0.6f,0.1f));

        // Abort button
        var btnGO = new GameObject("AbortBtn");
        btnGO.transform.SetParent(transform, false);
        var brt = btnGO.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.75f, 0.91f);
        brt.anchorMax = new Vector2(0.98f, 0.99f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var bimg = btnGO.AddComponent<Image>();
        bimg.color = new Color(0.35f, 0.05f, 0.05f);
        bimg.sprite = sprSquare;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bimg;
        btn.onClick.AddListener(() => onClose?.Invoke());
        MakeText("BtnTxt", btnGO.transform, "ABORT",
            Vector2.zero, Vector2.one, 12, new Color(1f,0.3f,0.3f));
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void StartMinigame(Action onComplete, Action onCancel)
    {
        EnsureUI();
        onAllComplete = onComplete;
        onClose = onCancel;
        puzzleIndex = 0;
        LoadPuzzle(0);
    }

    // ─── Puzzle Loading ───────────────────────────────────────────────────────

    private void LoadPuzzle(int idx)
    {
        var pd = puzzles[idx];
        currentGridSize = pd.gridSize;
        numColors = pd.pairs.Length;

        grid  = new int[currentGridSize, currentGridSize];
        epMap = new int[currentGridSize, currentGridSize];
        paths = new List<Vector2Int>[numColors];
        connected = new bool[numColors];
        ep1 = new Vector2Int[numColors];
        ep2 = new Vector2Int[numColors];

        for (int r = 0; r < currentGridSize; r++)
            for (int c = 0; c < currentGridSize; c++)
            { grid[r,c] = -1; epMap[r,c] = -1; }

        for (int i = 0; i < numColors; i++)
        {
            paths[i] = new List<Vector2Int>();
            var p = pd.pairs[i];
            ep1[i] = new Vector2Int(p.r1, p.c1);
            ep2[i] = new Vector2Int(p.r2, p.c2);
            epMap[p.r1, p.c1] = i;
            epMap[p.r2, p.c2] = i;
            grid[p.r1, p.c1] = i;
            grid[p.r2, p.c2] = i;
        }

        drawColor = -1;
        titleText.text = pd.title;
        levelText.text = $"STAGE {idx + 1} / {puzzles.Length}";
        statusText.text = "";
        BuildGridUI(pd);
    }

    private void BuildGridUI(PuzzleData pd)
    {
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);

        int n = pd.gridSize;
        float gap = n <= 4 ? 4f : n <= 5 ? 3f : 2f;
        float cs  = (GRID_PX - gap * (n + 1)) / n;

        cellBg  = new Image[n * n];
        cellDot = new Image[n * n];
        cellUIs = new FlowCellUI[n, n];

        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                int idx = r * n + c;
                var go = new GameObject($"Cell_{r}_{c}");
                go.transform.SetParent(gridContainer, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cs, cs);
                float px =  gap + c * (cs + gap) + cs * 0.5f - GRID_PX * 0.5f;
                float py = -(gap + r * (cs + gap) + cs * 0.5f) + GRID_PX * 0.5f;
                rt.anchoredPosition = new Vector2(px, py);

                var bg = go.AddComponent<Image>();
                bg.sprite = sprSquare;
                bg.color  = C_CELL;
                bg.raycastTarget = true;
                cellBg[idx] = bg;

                // Circle overlay (แสดงเฉพาะ endpoint)
                var dotGO = new GameObject("Dot");
                dotGO.transform.SetParent(go.transform, false);
                var drt = dotGO.AddComponent<RectTransform>();
                float pad = cs * 0.12f;
                drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one;
                drt.offsetMin = new Vector2(pad, pad);
                drt.offsetMax = new Vector2(-pad, -pad);
                var dot = dotGO.AddComponent<Image>();
                dot.sprite = sprCircle;
                dot.color  = Color.clear;
                dot.raycastTarget = false;
                cellDot[idx] = dot;

                var cell = go.AddComponent<FlowCellUI>();
                cell.Init(r, c, this);
                cellUIs[r, c] = cell;
            }
        }

        RefreshVisuals();
    }

    // ─── Pointer Callbacks ────────────────────────────────────────────────────

    public void OnCellPointerDown(int r, int c)
    {
        int ec = epMap[r, c];
        if (ec >= 0)
        {
            ClearPath(ec);
            drawColor = ec;
            paths[ec].Add(new Vector2Int(r, c));
            grid[r, c] = ec;
            RefreshVisuals();
        }
        else
        {
            drawColor = -1;
        }
    }

    public void OnCellPointerEnter(int r, int c)
    {
        if (drawColor < 0) return;
        var path = paths[drawColor];
        if (path.Count == 0) return;

        var pos  = new Vector2Int(r, c);
        var last = path[path.Count - 1];
        if (pos == last) return;

        // Backtrack ถ้า cell นี้อยู่ใน path แล้ว
        int bIdx = path.IndexOf(pos);
        if (bIdx >= 0)
        {
            for (int i = path.Count - 1; i > bIdx; i--)
            {
                var rem = path[i];
                if (epMap[rem.x, rem.y] != drawColor) grid[rem.x, rem.y] = -1;
                path.RemoveAt(i);
            }
            connected[drawColor] = false;
            RefreshVisuals();
            return;
        }

        if (!IsAdjacent(last, pos)) return;

        // ถ้า cell มีสีอื่นอยู่แล้ว ลบ path สีนั้นออก (แต่ไม่ให้ทับ endpoint สีอื่น)
        int existing = grid[r, c];
        if (existing >= 0 && existing != drawColor)
        {
            if (epMap[r, c] >= 0 && epMap[r, c] != drawColor) return;
            ClearPath(existing);
        }

        path.Add(pos);
        grid[r, c] = drawColor;

        // ถึง endpoint ปลายทาง → สำเร็จ
        bool reached = (pos == ep1[drawColor] || pos == ep2[drawColor]) && path[0] != pos;
        if (reached)
        {
            connected[drawColor] = true;
            drawColor = -1;
            RefreshVisuals();
            CheckComplete();
            return;
        }

        RefreshVisuals();
    }

    public void OnCellPointerUp(int r, int c)
    {
        drawColor = -1;
    }

    // ─── Path Management ─────────────────────────────────────────────────────

    private void ClearPath(int ci)
    {
        foreach (var pos in paths[ci])
            if (epMap[pos.x, pos.y] != ci) grid[pos.x, pos.y] = -1;
        paths[ci].Clear();
        connected[ci] = false;
    }

    // ─── Visuals ─────────────────────────────────────────────────────────────

    private void RefreshVisuals()
    {
        int n = currentGridSize;
        var pd = puzzles[puzzleIndex];

        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                int idx = r * n + c;
                int ci  = grid[r, c];
                int ep  = epMap[r, c];

                if (ci < 0)
                {
                    cellBg[idx].color  = C_CELL;
                    cellDot[idx].color = Color.clear;
                }
                else
                {
                    Color col = pd.pairs[ci].color;
                    if (ep >= 0)
                    {
                        // Endpoint: วงกลมสว่าง
                        cellBg[idx].color  = col * 0.25f;
                        cellDot[idx].color = col;
                    }
                    else
                    {
                        // Path cell: สีเข้มกว่า endpoint
                        cellBg[idx].color  = col * 0.65f;
                        cellDot[idx].color = Color.clear;
                    }
                }
            }
        }
    }

    // ─── Completion ──────────────────────────────────────────────────────────

    private void CheckComplete()
    {
        foreach (bool c in connected)
            if (!c) return;

        StartCoroutine(PuzzleCompleteRoutine());
    }

    private IEnumerator PuzzleCompleteRoutine()
    {
        if (puzzleIndex < puzzles.Length - 1)
        {
            statusText.text = $"✓ STAGE {puzzleIndex + 1} CLEARED — NEXT LAYER...";
            yield return new WaitForSeconds(1.5f);
            puzzleIndex++;
            LoadPuzzle(puzzleIndex);
        }
        else
        {
            statusText.text = "✓ SYSTEM COMPROMISED — ACCESS GRANTED";
            yield return new WaitForSeconds(2f);
            onAllComplete?.Invoke();
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;

    // ─── UI Factory ──────────────────────────────────────────────────────────

    private TextMeshProUGUI MakeText(string name, Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(4, 4);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,4,4), new Vector2(0.5f,0.5f));
    }

    private Sprite MakeCircleSprite(int size)
    {
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        float c = size / 2f, r = c - 1f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x+.5f, y+.5f), new Vector2(c, c));
                px[y*size+x] = d <= r ? Color.white : Color.clear;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
    }
}
