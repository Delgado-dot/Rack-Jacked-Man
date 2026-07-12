using UnityEngine;
using UnityEngine.UI;

public class InteractHUD : MonoBehaviour
{
    private GameObject canvas;
    private GameObject promptGroup;
    private Text keyText;
    private Text labelText;
    private PlayerInteract playerInteract;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvas = FindOrCreateCanvas();
        CreatePrompt();
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            playerInteract = player.GetComponent<PlayerInteract>();
        }
    }

    private void Update()
    {
        if (playerInteract == null)
        {
            FindPlayer();
            return;
        }

        bool nearRack = playerInteract.IsNearRack();
        if (promptGroup != null)
        {
            promptGroup.SetActive(nearRack);
        }
    }

    private GameObject FindOrCreateCanvas()
    {
        GameObject existing = GameObject.Find("InteractHUDCanvas");
        if (existing != null) return existing;

        GameObject go = new GameObject("InteractHUDCanvas");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 50;
        CanvasScaler sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        go.AddComponent<GraphicRaycaster>();
        canvasGroup = go.AddComponent<CanvasGroup>();
        return go;
    }

    private void CreatePrompt()
    {
        promptGroup = new GameObject("InteractPrompt");
        promptGroup.transform.SetParent(canvas.transform, false);

        RectTransform groupRect = promptGroup.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.35f);
        groupRect.anchorMax = new Vector2(0.5f, 0.35f);
        groupRect.anchoredPosition = Vector2.zero;
        groupRect.sizeDelta = new Vector2(250, 80);

        GameObject keyBg = new GameObject("KeyBg");
        keyBg.transform.SetParent(promptGroup.transform, false);
        Image keyImg = keyBg.AddComponent<Image>();
        keyImg.color = new Color(0, 0, 0, 0.75f);
        RectTransform keyRect = keyBg.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.3f, 0.45f);
        keyRect.anchorMax = new Vector2(0.7f, 1f);
        keyRect.sizeDelta = Vector2.zero;

        GameObject keyLabel = new GameObject("KeyLabel");
        keyLabel.transform.SetParent(keyBg.transform, false);
        keyText = keyLabel.AddComponent<Text>();
        keyText.text = "E";
        keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        keyText.fontSize = 36;
        keyText.fontStyle = FontStyle.Bold;
        keyText.alignment = TextAnchor.MiddleCenter;
        keyText.color = Color.white;
        RectTransform kRect = keyLabel.GetComponent<RectTransform>();
        kRect.anchorMin = Vector2.zero;
        kRect.anchorMax = Vector2.one;
        kRect.sizeDelta = Vector2.zero;

        Outline kOl = keyLabel.AddComponent<Outline>();
        kOl.effectColor = Color.black;
        kOl.effectDistance = new Vector2(1, -1);

        GameObject interactBg = new GameObject("InteractBg");
        interactBg.transform.SetParent(promptGroup.transform, false);
        Image interImg = interactBg.AddComponent<Image>();
        interImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform interRect = interactBg.GetComponent<RectTransform>();
        interRect.anchorMin = new Vector2(0.05f, 0f);
        interRect.anchorMax = new Vector2(0.95f, 0.42f);
        interRect.sizeDelta = Vector2.zero;

        GameObject interLabel = new GameObject("InteractLabel");
        interLabel.transform.SetParent(interactBg.transform, false);
        labelText = interLabel.AddComponent<Text>();
        labelText.text = "Interactuar";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 16;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        RectTransform iRect = interLabel.GetComponent<RectTransform>();
        iRect.anchorMin = Vector2.zero;
        iRect.anchorMax = Vector2.one;
        iRect.sizeDelta = Vector2.zero;

        Outline iOl = interLabel.AddComponent<Outline>();
        iOl.effectColor = Color.black;
        iOl.effectDistance = new Vector2(1, -1);

        promptGroup.SetActive(false);
    }
}
