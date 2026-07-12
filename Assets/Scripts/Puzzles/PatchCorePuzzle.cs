using UnityEngine;
using UnityEngine.UI;

public class PatchCorePuzzle : PuzzleSceneBase
{
    [SerializeField] private int totalPieces = 6;

    private GameObject canvas;
    private Text statusText;
    private GameObject[] slots;
    private GameObject[] pieces;
    private int[] slotContents;
    private int[] pieceOrder;
    private int selectedPiece = -1;
    private Color[] pieceColors;

    private static readonly Color[] COLORS = {
        new Color(1f, 0.35f, 0.35f),
        new Color(1f, 0.71f, 0.27f),
        new Color(1f, 0.9f, 0.35f),
        new Color(0.47f, 0.9f, 0.59f),
        new Color(0.35f, 0.82f, 1f),
        new Color(0.59f, 0.59f, 1f),
        new Color(0.86f, 0.51f, 1f),
        new Color(1f, 0.51f, 0.75f),
        new Color(0.51f, 0.96f, 0.9f),
        new Color(0.86f, 0.9f, 0.94f),
    };

    protected override void Start()
    {
        base.Start();
        puzzleName = "PATCHCORD ROTO";
        timeLimit = 45f;
        timer = timeLimit;
        canvas = FindOrCreateCanvas();
        CreateUI();
    }

    private GameObject FindOrCreateCanvas()
    {
        GameObject go = GameObject.Find("PuzzleCanvas");
        if (go != null) return go;
        go = new GameObject("PuzzleCanvas");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 200;
        CanvasScaler sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        go.AddComponent<GraphicRaycaster>();
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        return go;
    }

    private void CreateUI()
    {
        pieceColors = new Color[totalPieces];
        for (int i = 0; i < totalPieces; i++) pieceColors[i] = COLORS[i % COLORS.Length];

        slotContents = new int[totalPieces];
        for (int i = 0; i < totalPieces; i++) slotContents[i] = -1;

        int[] shuffled = new int[totalPieces];
        for (int i = 0; i < totalPieces; i++) shuffled[i] = i;
        for (int i = totalPieces - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int t = shuffled[i]; shuffled[i] = shuffled[j]; shuffled[j] = t;
        }

        slots = new GameObject[totalPieces];
        pieces = new GameObject[totalPieces];

        float slotW = 0.9f / totalPieces;
        float slotStartX = 0.05f;
        float slotY = 0.72f;
        float slotH = 0.12f;

        GameObject patchcordBase = new GameObject("PatchcordBase");
        patchcordBase.transform.SetParent(canvas.transform, false);
        Image patchcordImg = patchcordBase.AddComponent<Image>();
        patchcordImg.color = new Color(0.15f, 0.15f, 0.2f);
        patchcordImg.raycastTarget = false;
        RectTransform pRect = patchcordBase.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(slotStartX - 0.01f, slotY - 0.02f);
        pRect.anchorMax = new Vector2(slotStartX + slotW * totalPieces + 0.01f, slotY + slotH + 0.02f);
        pRect.sizeDelta = Vector2.zero;

        for (int i = 0; i < totalPieces; i++)
        {
            float x = slotStartX + i * slotW;
            CreateSlot(i, x, slotY, slotW - 0.005f, slotH);
        }

        float bankY = 0.2f;
        float bankPieceW = 0.9f / totalPieces;
        for (int i = 0; i < totalPieces; i++)
        {
            int pieceIdx = shuffled[i];
            float x = 0.05f + i * bankPieceW;
            CreatePiece(pieceIdx, x, bankY, bankPieceW - 0.005f, 0.1f);
        }

        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(canvas.transform, false);
        statusText = statusObj.AddComponent<Text>();
        statusText.text = "PATCHCORD ROTO  |  Coloca piezas en orden  |  0/" + totalPieces;
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 20;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(0.9f, 0.95f, 1f);
        Outline ol = statusObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.05f, 0.05f, 0.15f);
        ol.effectDistance = new Vector2(1, -1);
        RectTransform sRect = statusObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.05f, 0.92f);
        sRect.anchorMax = new Vector2(0.95f, 0.98f);
        sRect.sizeDelta = Vector2.zero;

        GameObject hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(canvas.transform, false);
        Text hint = hintObj.AddComponent<Text>();
        hint.text = "Click pieza, luego click slot para colocar";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 14;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(0.6f, 0.65f, 0.7f);
        RectTransform hRect = hintObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0.05f, 0.87f);
        hRect.anchorMax = new Vector2(0.95f, 0.92f);
        hRect.sizeDelta = Vector2.zero;
    }

    private void CreateSlot(int idx, float x, float y, float w, float h)
    {
        GameObject go = new GameObject("Slot_" + idx);
        go.transform.SetParent(canvas.transform, false);
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(x, y);
        rect.anchorMax = new Vector2(x + w, y + h);
        rect.sizeDelta = Vector2.zero;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        int capturedIdx = idx;
        btn.onClick.AddListener(() => OnSlotClicked(capturedIdx));

        GameObject numGO = new GameObject("Num");
        numGO.transform.SetParent(go.transform, false);
        Text numText = numGO.AddComponent<Text>();
        numText.text = (idx + 1).ToString("D2");
        numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numText.fontSize = 16;
        numText.fontStyle = FontStyle.Bold;
        numText.alignment = TextAnchor.MiddleCenter;
        numText.color = new Color(0.4f, 0.4f, 0.5f);
        RectTransform nRect = numGO.GetComponent<RectTransform>();
        nRect.anchorMin = Vector2.zero;
        nRect.anchorMax = Vector2.one;
        nRect.sizeDelta = Vector2.zero;

        slots[idx] = go;
    }

    private void CreatePiece(int idx, float x, float y, float w, float h)
    {
        GameObject go = new GameObject("Piece_" + idx);
        go.transform.SetParent(canvas.transform, false);
        Image bg = go.AddComponent<Image>();
        bg.color = pieceColors[idx];
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(x, y);
        rect.anchorMax = new Vector2(x + w, y + h);
        rect.sizeDelta = Vector2.zero;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        int capturedIdx = idx;
        btn.onClick.AddListener(() => OnPieceClicked(capturedIdx));

        GameObject numGO = new GameObject("Num");
        numGO.transform.SetParent(go.transform, false);
        Text numText = numGO.AddComponent<Text>();
        numText.text = (idx + 1).ToString("D2");
        numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numText.fontSize = 18;
        numText.fontStyle = FontStyle.Bold;
        numText.alignment = TextAnchor.MiddleCenter;
        numText.color = Color.white;
        Outline nOl = numGO.AddComponent<Outline>();
        nOl.effectColor = Color.black;
        nOl.effectDistance = new Vector2(1, -1);
        RectTransform nRect = numGO.GetComponent<RectTransform>();
        nRect.anchorMin = Vector2.zero;
        nRect.anchorMax = Vector2.one;
        nRect.sizeDelta = Vector2.zero;

        pieces[idx] = go;
    }

    private void OnPieceClicked(int pieceIdx)
    {
        if (puzzleCompleted || puzzleFailed) return;

        if (selectedPiece >= 0)
        {
            pieces[selectedPiece].transform.localScale = Vector3.one;
        }

        if (selectedPiece == pieceIdx)
        {
            selectedPiece = -1;
            return;
        }

        selectedPiece = pieceIdx;
        pieces[pieceIdx].transform.SetAsLastSibling();
        pieces[pieceIdx].transform.localScale = Vector3.one * 1.15f;
    }

    private void OnSlotClicked(int slotIdx)
    {
        if (puzzleCompleted || puzzleFailed) return;

        if (selectedPiece < 0)
        {
            if (slotContents[slotIdx] >= 0)
            {
                selectedPiece = slotContents[slotIdx];
                pieces[selectedPiece].transform.SetAsLastSibling();
                pieces[selectedPiece].transform.localScale = Vector3.one * 1.15f;
            }
            return;
        }

        int pieceInSlot = slotContents[slotIdx];
        int pieceSlot = GetSlotOfPiece(selectedPiece);

        if (pieceSlot >= 0) slotContents[pieceSlot] = pieceInSlot;
        if (pieceInSlot >= 0) pieces[pieceInSlot].GetComponent<RectTransform>().anchorMin = GetBankPosition(pieceInSlot);

        slotContents[slotIdx] = selectedPiece;
        RectTransform slotRect = slots[slotIdx].GetComponent<RectTransform>();
        RectTransform pieceRect = pieces[selectedPiece].GetComponent<RectTransform>();
        pieceRect.anchorMin = slotRect.anchorMin + new Vector2(0.005f, 0.005f);
        pieceRect.anchorMax = slotRect.anchorMax - new Vector2(0.005f, 0.005f);

        pieces[selectedPiece].transform.localScale = Vector3.one;
        selectedPiece = -1;

        if (pieceInSlot >= 0 && pieceSlot >= 0)
        {
            RectTransform oldSlotRect = slots[pieceSlot].GetComponent<RectTransform>();
            RectTransform otherPieceRect = pieces[pieceInSlot].GetComponent<RectTransform>();
            otherPieceRect.anchorMin = oldSlotRect.anchorMin + new Vector2(0.005f, 0.005f);
            otherPieceRect.anchorMax = oldSlotRect.anchorMax - new Vector2(0.005f, 0.005f);
        }

        int correctCount = 0;
        for (int i = 0; i < totalPieces; i++)
        {
            if (slotContents[i] == i) correctCount++;
        }

        if (correctCount == totalPieces)
        {
            Complete();
            return;
        }

        UpdateStatus(correctCount);

        if (slotContents[slotIdx] != slotIdx)
        {
            StartCoroutine(FlashSlot(slotIdx, new Color(1f, 0.3f, 0.3f)));
        }
        else
        {
            StartCoroutine(FlashSlot(slotIdx, new Color(0.3f, 1f, 0.5f)));
        }
    }

    private int GetSlotOfPiece(int pieceIdx)
    {
        for (int i = 0; i < totalPieces; i++)
        {
            if (slotContents[i] == pieceIdx) return i;
        }
        return -1;
    }

    private Vector2 GetBankPosition(int pieceIdx)
    {
        float bankPieceW = 0.9f / totalPieces;
        return new Vector2(0.05f + pieceIdx * bankPieceW, 0.2f);
    }

    private System.Collections.IEnumerator FlashSlot(int idx, Color color)
    {
        Image img = slots[idx].GetComponent<Image>();
        Color original = img.color;
        img.color = new Color(color.r, color.g, color.b, 0.6f);
        yield return new WaitForSeconds(0.36f);
        img.color = original;
    }

    private void UpdateStatus(int correct)
    {
        if (statusText != null)
            statusText.text = "PATCHCORD ROTO  |  Coloca piezas en orden  |  " + correct + "/" + totalPieces;
    }
}
