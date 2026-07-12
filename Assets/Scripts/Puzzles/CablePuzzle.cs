using UnityEngine;
using UnityEngine.UI;

public class CablePuzzle : PuzzleSceneBase
{
    [SerializeField] private int totalPairs = 4;

    private int pairsCompleted = 0;
    private int selectedLeft = -1;
    private GameObject canvas;
    private Text statusText;
    private GameObject[] leftPorts;
    private GameObject[] rightPorts;
    private int[] leftCableId;
    private int[] rightCableId;
    private bool[] connected;
    private GameObject[] cableLines;
    private Color[] portColors;
    private string[] portNames;

    private struct Protocol { public string name; public Color color; }
    private Protocol[] PROTOCOLS = {
        new Protocol { name = "TCP",  color = new Color(1f, 0.39f, 0.39f) },
        new Protocol { name = "UDP",  color = new Color(0.39f, 0.71f, 1f) },
        new Protocol { name = "HTTP", color = new Color(1f, 0.78f, 0.39f) },
        new Protocol { name = "DNS",  color = new Color(0.71f, 0.51f, 1f) },
        new Protocol { name = "FTP",  color = new Color(0.51f, 0.86f, 0.71f) },
        new Protocol { name = "SSH",  color = new Color(1f, 0.59f, 0.78f) },
        new Protocol { name = "ICMP", color = new Color(0.78f, 0.78f, 0.39f) },
        new Protocol { name = "ARP",  color = new Color(0.59f, 0.86f, 1f) },
        new Protocol { name = "SMTP", color = new Color(0.86f, 0.51f, 0.39f) },
    };

    protected override void Start()
    {
        base.Start();
        puzzleName = "PATCH PANEL";
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
        int n = totalPairs;
        Protocol[] chosen = new Protocol[n];
        bool[] used = new bool[PROTOCOLS.Length];
        for (int i = 0; i < n; i++)
        {
            int idx;
            do { idx = Random.Range(0, PROTOCOLS.Length); } while (used[idx]);
            used[idx] = true;
            chosen[i] = PROTOCOLS[idx];
        }

        int[] rightOrder = new int[n];
        for (int i = 0; i < n; i++) rightOrder[i] = i;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int t = rightOrder[i]; rightOrder[i] = rightOrder[j]; rightOrder[j] = t;
        }

        leftPorts = new GameObject[n];
        rightPorts = new GameObject[n];
        leftCableId = new int[n];
        rightCableId = new int[n];
        connected = new bool[n];
        cableLines = new GameObject[n];
        portColors = new Color[n];
        portNames = new string[n];

        float startY = 0.12f;
        float endY = 0.82f;
        float step = (n > 1) ? (endY - startY) / (n - 1) : 0;

        for (int i = 0; i < n; i++)
        {
            float y = (n > 1) ? startY + step * i : 0.47f;
            leftCableId[i] = i;
            rightCableId[i] = i;
            portColors[i] = chosen[i].color;
            portNames[i] = chosen[i].name;

            leftPorts[i] = CreatePort(canvas.transform, "L_" + chosen[i].name, 0.15f, y, chosen[i].color, chosen[i].name, i, true);
            rightPorts[i] = CreatePort(canvas.transform, "R_" + chosen[i].name, 0.85f, y + (Random.Range(-0.02f, 0.02f)), chosen[i].color, chosen[i].name, rightOrder[i], false);
        }

        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(canvas.transform, false);
        statusText = statusObj.AddComponent<Text>();
        statusText.text = "PATCH PANEL  |  Click 2 puertos del mismo protocolo  |  0/" + n;
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 20;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(0.9f, 0.95f, 1f);
        Outline ol = statusObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.05f, 0.05f, 0.15f);
        ol.effectDistance = new Vector2(1, -1);
        RectTransform sRect = statusObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.05f, 0.91f);
        sRect.anchorMax = new Vector2(0.95f, 0.98f);
        sRect.sizeDelta = Vector2.zero;
    }

    private GameObject CreatePort(Transform parent, string name, float x, float y, Color color, string label, int cableIdx, bool isLeft)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        float portSize = 0.05f;
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(x - portSize, y - portSize);
        rect.anchorMax = new Vector2(x + portSize, y + portSize);
        rect.sizeDelta = Vector2.zero;

        Image bg = go.AddComponent<Image>();
        bg.color = color;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        int capturedCable = cableIdx;
        bool capturedIsLeft = isLeft;
        btn.onClick.AddListener(() => OnPortClick(capturedCable, capturedIsLeft, go));

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        Text t = labelGO.AddComponent<Text>();
        t.text = label;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 11;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        Outline tOl = labelGO.AddComponent<Outline>();
        tOl.effectColor = Color.black;
        tOl.effectDistance = new Vector2(1, -1);
        RectTransform tRect = labelGO.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        return go;
    }

    private void OnPortClick(int cableIdx, bool isLeft, GameObject clickedPort)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (connected[cableIdx] && isLeft) return;

        if (isLeft)
        {
            if (selectedLeft >= 0 && selectedLeft != cableIdx)
            {
                SetScale(leftPorts[selectedLeft], 1f);
            }
            selectedLeft = cableIdx;
            SetScale(clickedPort, 1.4f);
        }
        else
        {
            if (selectedLeft < 0) return;

            if (selectedLeft == cableIdx)
            {
                connected[cableIdx] = true;
                pairsCompleted++;
                SetScale(leftPorts[selectedLeft], 1f);
                DimPort(leftPorts[selectedLeft]);
                DimPort(clickedPort);
                DrawCableLine(selectedLeft, clickedPort);
                selectedLeft = -1;
                Debug.Log("PATCH PANEL: " + portNames[cableIdx] + " conectado! " + pairsCompleted + "/" + totalPairs);

                if (pairsCompleted >= totalPairs)
                {
                    Complete();
                    return;
                }
                UpdateStatus();
            }
            else
            {
                Debug.Log("PATCH PANEL: Protocolo incorrecto!");
                StartCoroutine(FlashPorts(leftPorts[selectedLeft], clickedPort, portColors[selectedLeft], portColors[cableIdx]));
                SetScale(leftPorts[selectedLeft], 1f);
                selectedLeft = -1;
            }
        }
    }

    private System.Collections.IEnumerator FlashPorts(GameObject a, GameObject b, Color ca, Color cb)
    {
        if (a != null) a.GetComponent<Image>().color = Color.red;
        if (b != null) b.GetComponent<Image>().color = Color.red;
        yield return new WaitForSeconds(0.32f);
        if (a != null && !connected[System.Array.IndexOf(leftPorts, a)]) a.GetComponent<Image>().color = ca;
        if (b != null) b.GetComponent<Image>().color = cb;
    }

    private void DimPort(GameObject port)
    {
        if (port == null) return;
        Image img = port.GetComponent<Image>();
        if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, 0.4f);
    }

    private void DrawCableLine(int cableIdx, GameObject rightPort)
    {
        GameObject lineGO = new GameObject("Cable_" + cableIdx);
        lineGO.transform.SetParent(canvas.transform, false);
        lineGO.transform.SetAsFirstSibling();

        Image lineImg = lineGO.AddComponent<Image>();
        lineImg.color = portColors[cableIdx];
        lineImg.raycastTarget = false;

        RectTransform lineRect = lineGO.GetComponent<RectTransform>();
        RectTransform leftRect = leftPorts[cableIdx].GetComponent<RectTransform>();
        RectTransform rightRect = rightPort.GetComponent<RectTransform>();

        Vector2 lAnchor = (leftRect.anchorMin + leftRect.anchorMax) / 2f;
        Vector2 rAnchor = (rightRect.anchorMin + rightRect.anchorMax) / 2f;

        float y = (lAnchor.y + rAnchor.y) / 2f;
        float xMin = Mathf.Min(lAnchor.x, rAnchor.x);
        float xMax = Mathf.Max(lAnchor.x, rAnchor.x);

        lineRect.anchorMin = new Vector2(xMin, y - 0.003f);
        lineRect.anchorMax = new Vector2(xMax, y + 0.003f);
        lineRect.sizeDelta = Vector2.zero;

        cableLines[cableIdx] = lineGO;
    }

    private void SetScale(GameObject go, float s)
    {
        if (go != null) go.transform.localScale = Vector3.one * s;
    }

    private void UpdateStatus()
    {
        if (statusText != null)
            statusText.text = "PATCH PANEL  |  Click 2 puertos del mismo protocolo  |  " + pairsCompleted + "/" + totalPairs;
    }
}
