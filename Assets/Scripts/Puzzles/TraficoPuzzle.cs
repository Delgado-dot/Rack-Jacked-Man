using UnityEngine;
using UnityEngine.UI;

public class TraficoPuzzle : PuzzleSceneBase
{
    private GameObject canvas;
    private Text statusText;
    private System.Collections.Generic.List<PacketData> packets = new();
    private System.Collections.Generic.List<BinData> bins = new();
    private int grabbedPacket = -1;
    private int totalPackets = 4;

    private struct BinDef { public string name; public Color color; }
    private BinDef[] BIN_DEFS = {
        new BinDef { name = "HTTP", color = new Color(1f, 0.78f, 0.39f) },
        new BinDef { name = "DNS",  color = new Color(0.71f, 0.51f, 1f) },
        new BinDef { name = "FTP",  color = new Color(0.51f, 0.86f, 0.71f) },
        new BinDef { name = "SSH",  color = new Color(1f, 0.59f, 0.78f) },
    };

    private string[] HTTP_PAYLOADS = { "GET /index.html", "POST /login", "PUT /api/v1/users", "HTTP/1.1 200 OK", "GET /styles.css" };
    private string[] DNS_PAYLOADS = { "DNS ?google.com", "DNS ?youtube.com", "DNS A example.org", "DNS PTR 8.8.8.8", "DNS MX gmail.com" };
    private string[] FTP_PAYLOADS = { "USER admin", "PASS ****", "RETR file.zip", "LIST -la", "STOR upload.txt" };
    private string[] SSH_PAYLOADS = { "SSH-2.0-OpenSSH", "RSA key exchange", "AES-256-GCM", "auth password", "client banner" };

    private struct PacketData
    {
        public string protocol;
        public string snippet;
        public Color color;
        public GameObject obj;
        public int binIdx;
    }
    private struct BinData
    {
        public string name;
        public Color color;
        public GameObject obj;
        public Image img;
    }

    protected override void Start()
    {
        base.Start();
        puzzleName = "TRAFICO DE RED";
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
        for (int i = 0; i < BIN_DEFS.Length; i++)
        {
            CreateBin(i, BIN_DEFS[i]);
        }

        string[] protocols = { "HTTP", "DNS", "FTP", "SSH" };
        string[][] allPayloads = { HTTP_PAYLOADS, DNS_PAYLOADS, FTP_PAYLOADS, SSH_PAYLOADS };

        for (int i = 0; i < totalPackets; i++)
        {
            int protIdx = i % 4;
            string proto = protocols[protIdx];
            string snippet = allPayloads[protIdx][Random.Range(0, allPayloads[protIdx].Length)];
            Color c = BIN_DEFS[protIdx].color;
            CreatePacket(i, proto, snippet, c);
        }

        GameObject statusObj = new GameObject("Status");
        statusObj.transform.SetParent(canvas.transform, false);
        statusText = statusObj.AddComponent<Text>();
        statusText.text = "TRAFICO DE RED  |  Arrastra paquetes al bin correcto  |  0/" + totalPackets;
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
    }

    private void CreateBin(int idx, BinDef def)
    {
        float[] xPositions = { 0.58f, 0.78f, 0.58f, 0.78f };
        float[] yPositions = { 0.55f, 0.55f, 0.2f, 0.2f };

        GameObject go = new GameObject("Bin_" + def.name);
        go.transform.SetParent(canvas.transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(def.color.r, def.color.g, def.color.b, 0.15f);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(xPositions[idx] - 0.1f, yPositions[idx] - 0.15f);
        rect.anchorMax = new Vector2(xPositions[idx] + 0.1f, yPositions[idx] + 0.15f);
        rect.sizeDelta = Vector2.zero;

        Image borderImg = go.AddComponent<Image>();
        borderImg.color = def.color;
        borderImg.raycastTarget = false;

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        Text t = labelGO.AddComponent<Text>();
        t.text = def.name + "\nDROP HERE";
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 14;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = def.color;
        RectTransform tRect = labelGO.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        bins.Add(new BinData { name = def.name, color = def.color, obj = go, img = img });
    }

    private void CreatePacket(int idx, string protocol, string snippet, Color color)
    {
        float yPos = 0.82f - idx * 0.13f;

        GameObject go = new GameObject("Packet_" + idx);
        go.transform.SetParent(canvas.transform, false);
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(color.r, color.g, color.b, 0.2f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        int capturedIdx = idx;
        btn.onClick.AddListener(() => OnPacketClicked(capturedIdx));

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.05f, yPos - 0.05f);
        rect.anchorMax = new Vector2(0.45f, yPos + 0.05f);
        rect.sizeDelta = Vector2.zero;

        Image accentBar = go.AddComponent<Image>();
        accentBar.color = color;
        accentBar.raycastTarget = false;
        RectTransform accentRect = accentBar.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 0);
        accentRect.anchorMax = new Vector2(0.02f, 1);
        accentRect.sizeDelta = Vector2.zero;

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        Text t = labelGO.AddComponent<Text>();
        t.text = protocol + "\n" + snippet;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 13;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleLeft;
        t.color = Color.white;
        RectTransform tRect = labelGO.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.04f, 0.1f);
        tRect.anchorMax = new Vector2(0.95f, 0.9f);
        tRect.sizeDelta = Vector2.zero;

        Outline tOl = labelGO.AddComponent<Outline>();
        tOl.effectColor = Color.black;
        tOl.effectDistance = new Vector2(1, -1);

        packets.Add(new PacketData { protocol = protocol, snippet = snippet, color = color, obj = go, binIdx = -1 });
    }

    private void OnPacketClicked(int idx)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (packets[idx].binIdx >= 0) return;

        if (grabbedPacket >= 0 && grabbedPacket != idx)
        {
            ReturnPacket(grabbedPacket);
        }

        grabbedPacket = idx;
        packets[idx].obj.transform.SetAsLastSibling();
        packets[idx].obj.transform.localScale = Vector3.one * 1.05f;
    }

    private new void Update()
    {
        base.Update();
        if (puzzleCompleted || puzzleFailed) return;

        if (grabbedPacket >= 0 && grabbedPacket < packets.Count)
        {
            RectTransform pRect = packets[grabbedPacket].obj.GetComponent<RectTransform>();
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), Input.mousePosition, null, out mousePos);

            Vector2 canvasSize = canvas.GetComponent<RectTransform>().rect.size;
            float normX = (mousePos.x + canvasSize.x / 2f) / canvasSize.x;
            float normY = (mousePos.y + canvasSize.y / 2f) / canvasSize.y;
            pRect.anchorMin = new Vector2(normX - 0.2f, normY - 0.04f);
            pRect.anchorMax = new Vector2(normX + 0.2f, normY + 0.04f);

            if (Input.GetMouseButtonDown(1))
            {
                ReturnPacket(grabbedPacket);
                grabbedPacket = -1;
                return;
            }

            for (int b = 0; b < bins.Count; b++)
            {
                RectTransform bRect = bins[b].obj.GetComponent<RectTransform>();
                Vector2 bCenter = (bRect.anchorMin + bRect.anchorMax) / 2f;
                float dist = Vector2.Distance(new Vector2(normX, normY), bCenter);

                if (dist < 0.12f && Input.GetMouseButtonDown(0))
                {
                    TryDropInBin(grabbedPacket, b);
                    return;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && grabbedPacket < 0)
        {
            for (int i = 0; i < packets.Count; i++)
            {
                if (packets[i].binIdx >= 0) continue;
                RectTransform pRect = packets[i].obj.GetComponent<RectTransform>();
                Vector2 center = (pRect.anchorMin + pRect.anchorMax) / 2f;
                Vector2 mouseNorm = ScreenToNorm(Input.mousePosition);
                float dist = Vector2.Distance(center, mouseNorm);
                if (dist < 0.1f)
                {
                    grabbedPacket = i;
                    packets[i].obj.transform.SetAsLastSibling();
                    packets[i].obj.transform.localScale = Vector3.one * 1.05f;
                    break;
                }
            }
        }
    }

    private Vector2 ScreenToNorm(Vector3 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(), screenPos, null, out localPoint);
        Vector2 canvasSize = canvas.GetComponent<RectTransform>().rect.size;
        return new Vector2(
            (localPoint.x + canvasSize.x / 2f) / canvasSize.x,
            (localPoint.y + canvasSize.y / 2f) / canvasSize.y
        );
    }

    private void TryDropInBin(int packetIdx, int binIdx)
    {
        PacketData p = packets[packetIdx];
        BinData b = bins[binIdx];

        if (p.protocol == b.name)
        {
            p.binIdx = binIdx;
            p.obj.transform.localScale = Vector3.one;
            RectTransform bRect = b.obj.GetComponent<RectTransform>();
            p.obj.GetComponent<RectTransform>().anchorMin = bRect.anchorMin + new Vector2(0.01f, 0.01f);
            p.obj.GetComponent<RectTransform>().anchorMax = bRect.anchorMax - new Vector2(0.01f, 0.01f);
            p.obj.GetComponent<Button>().interactable = false;

            StartCoroutine(FlashBin(binIdx, new Color(0.47f, 1f, 0.71f), 0.4f));

            int deposited = 0;
            for (int i = 0; i < packets.Count; i++)
            {
                if (packets[i].binIdx >= 0) deposited++;
            }
            if (deposited >= totalPackets)
            {
                Complete();
                return;
            }
            UpdateStatus(deposited);
        }
        else
        {
            ReturnPacket(packetIdx);
            StartCoroutine(FlashBin(binIdx, new Color(1f, 0.31f, 0.31f), 0.4f));
        }
        grabbedPacket = -1;
    }

    private void ReturnPacket(int idx)
    {
        PacketData p = packets[idx];
        float yPos = 0.82f - idx * 0.13f;
        p.obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, yPos - 0.05f);
        p.obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.45f, yPos + 0.05f);
        p.obj.transform.localScale = Vector3.one;
    }

    private System.Collections.IEnumerator FlashBin(int binIdx, Color flashColor, float duration)
    {
        BinData b = bins[binIdx];
        Color original = b.img.color;
        b.img.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0.4f);
        yield return new WaitForSeconds(duration);
        b.img.color = original;
    }

    private void UpdateStatus(int deposited)
    {
        if (statusText != null)
            statusText.text = "TRAFICO DE RED  |  Arrastra paquetes al bin correcto  |  " + deposited + "/" + totalPackets;
    }
}
