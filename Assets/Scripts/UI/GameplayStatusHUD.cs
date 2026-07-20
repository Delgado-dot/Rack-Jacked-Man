using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD de estado generado en tiempo de ejecucion para no depender de referencias
/// manuales en cada escena. Muestra vidas, tiempo restante y nivel actual.
/// </summary>
public class GameplayStatusHUD : MonoBehaviour
{
    private const int MaxVisualLives = 5;

    private static readonly Color PanelColor = new Color(0.025f, 0.055f, 0.075f, 0.94f);
    private static readonly Color CardColor = new Color(0.045f, 0.095f, 0.12f, 0.92f);
    private static readonly Color Cyan = new Color(0.18f, 0.92f, 1f, 1f);
    private static readonly Color MutedCyan = new Color(0.12f, 0.42f, 0.5f, 1f);
    private static readonly Color MutedText = new Color(0.58f, 0.76f, 0.8f, 1f);
    private static readonly Color Warning = new Color(1f, 0.72f, 0.12f, 1f);
    private static readonly Color Critical = new Color(1f, 0.22f, 0.28f, 1f);
    private static readonly Color EmptyLife = new Color(0.12f, 0.19f, 0.21f, 1f);

    private Text livesValueText;
    private Text timeValueText;
    private Text levelValueText;
    private Text levelDetailText;
    private RectTransform timeFill;
    private Image timeFillImage;
    private readonly Image[] lifeSegments = new Image[MaxVisualLives];

    private int lastLevel = -1;
    private bool criticalTime;
    private Color currentTimeColor = Cyan;

    private void Awake()
    {
        BuildHUD();
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateLives;
        GameManager.OnTimeChanged += UpdateTime;
    }

    private void Start()
    {
        UpdateLives(PlayerHealth.GetCurrentLives(), PlayerHealth.GetMaxLives());

        if (GameManager.Instance != null)
        {
            UpdateTime(GameManager.Instance.GetTimeRemaining(), GameManager.Instance.GetTimeLimit());
        }

        UpdateLevel(true);
    }

    private void Update()
    {
        UpdateLevel(false);

        if (!criticalTime)
        {
            return;
        }

        float pulse = 0.72f + Mathf.PingPong(Time.unscaledTime * 1.6f, 0.28f);
        Color pulseColor = new Color(currentTimeColor.r, currentTimeColor.g, currentTimeColor.b, pulse);
        timeValueText.color = pulseColor;
        timeFillImage.color = pulseColor;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateLives;
        GameManager.OnTimeChanged -= UpdateTime;
    }

    private void BuildHUD()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        RectTransform shadow = CreateRect("HUD Shadow", transform, new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(5f, -25f), new Vector2(814f, 120f));
        Image shadowImage = shadow.gameObject.AddComponent<Image>();
        shadowImage.color = new Color(0f, 0f, 0f, 0.48f);
        shadowImage.raycastTarget = false;

        RectTransform panel = CreateRect("Status Panel", transform, new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(814f, 120f));
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;
        panelImage.raycastTarget = false;

        Outline panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.48f);
        panelOutline.effectDistance = new Vector2(1f, -1f);
        panelOutline.useGraphicAlpha = false;

        RectTransform topAccent = CreateRect("Top Accent", panel, new Vector2(0f, 1f),
            new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 4f));
        Image topAccentImage = topAccent.gameObject.AddComponent<Image>();
        topAccentImage.color = Cyan;
        topAccentImage.raycastTarget = false;

        CreateText("System Label", panel, "RACK-JACKED  //  ESTADO DE MISION", 11,
            FontStyle.Bold, MutedText, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -7f), new Vector2(430f, 18f), false);

        RectTransform livesCard = CreateCard(panel, "Lives Module", new Vector2(12f, -28f), new Vector2(224f, 80f));
        BuildLivesModule(livesCard);

        RectTransform timeCard = CreateCard(panel, "Time Module", new Vector2(247f, -28f), new Vector2(320f, 80f));
        BuildTimeModule(timeCard);

        RectTransform levelCard = CreateCard(panel, "Level Module", new Vector2(578f, -28f), new Vector2(224f, 80f));
        BuildLevelModule(levelCard);
    }

    private void BuildLivesModule(RectTransform card)
    {
        CreateModuleTitle(card, "VIDAS", "INTEGRIDAD");

        livesValueText = CreateText("Lives Value", card, "03", 32, FontStyle.Bold, Cyan,
            TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -25f), new Vector2(72f, 37f), true);

        CreateText("Lives Unit", card, "/ 03", 14, FontStyle.Bold, MutedText,
            TextAnchor.LowerLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(74f, -31f), new Vector2(55f, 27f), false).name = "Lives Maximum";

        for (int i = 0; i < MaxVisualLives; i++)
        {
            RectTransform segment = CreateRect("Life Segment " + (i + 1), card,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(14f + (i * 39f), 10f), new Vector2(33f, 6f));
            Image image = segment.gameObject.AddComponent<Image>();
            image.color = Cyan;
            image.raycastTarget = false;
            lifeSegments[i] = image;
        }
    }

    private void BuildTimeModule(RectTransform card)
    {
        CreateModuleTitle(card, "TIEMPO", "CUENTA REGRESIVA");

        timeValueText = CreateText("Time Value", card, "05:00", 34, FontStyle.Bold, Cyan,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -25f), new Vector2(220f, 38f), true);

        RectTransform track = CreateRect("Time Track", card, new Vector2(0f, 0f),
            new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(-28f, 6f));
        Image trackImage = track.gameObject.AddComponent<Image>();
        trackImage.color = new Color(0.08f, 0.18f, 0.2f, 1f);
        trackImage.raycastTarget = false;

        timeFill = CreateRect("Time Fill", track, new Vector2(0f, 0f),
            new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        timeFillImage = timeFill.gameObject.AddComponent<Image>();
        timeFillImage.color = Cyan;
        timeFillImage.raycastTarget = false;
    }

    private void BuildLevelModule(RectTransform card)
    {
        CreateModuleTitle(card, "NIVEL", "PROGRESO");

        levelValueText = CreateText("Level Value", card, "01", 32, FontStyle.Bold, Cyan,
            TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -25f), new Vector2(72f, 37f), true);

        levelDetailText = CreateText("Level Detail", card, "SECTOR ACTIVO", 11, FontStyle.Bold, MutedText,
            TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(78f, -31f), new Vector2(132f, 25f), false);

        RectTransform signal = CreateRect("Level Signal", card, new Vector2(0f, 0f),
            new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(-28f, 6f));
        Image signalImage = signal.gameObject.AddComponent<Image>();
        signalImage.color = MutedCyan;
        signalImage.raycastTarget = false;
    }

    private static RectTransform CreateCard(RectTransform parent, string name, Vector2 position, Vector2 size)
    {
        RectTransform card = CreateRect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, 1f), position, size);
        Image image = card.gameObject.AddComponent<Image>();
        image.color = CardColor;
        image.raycastTarget = false;

        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(MutedCyan.r, MutedCyan.g, MutedCyan.b, 0.65f);
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = false;
        return card;
    }

    private static void CreateModuleTitle(RectTransform card, string title, string descriptor)
    {
        CreateText(title + " Title", card, title, 12, FontStyle.Bold, Cyan,
            TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(14f, -5f), new Vector2(70f, 18f), false);

        CreateText(title + " Descriptor", card, descriptor, 9, FontStyle.Normal, MutedText,
            TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-14f, -5f), new Vector2(125f, 18f), false);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return rect;
    }

    private static Text CreateText(string name, Transform parent, string value, int fontSize,
        FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, bool addOutline)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        if (addOutline)
        {
            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.88f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        return text;
    }

    private void UpdateLives(int currentLives, int maxLives)
    {
        int safeMax = Mathf.Max(1, maxLives);
        int safeCurrent = Mathf.Clamp(currentLives, 0, safeMax);
        bool lowLives = safeCurrent <= 1;
        Color activeColor = lowLives ? Critical : Cyan;

        livesValueText.text = safeCurrent.ToString("00");
        livesValueText.color = activeColor;

        Transform maximum = livesValueText.transform.parent.Find("Lives Maximum");
        if (maximum != null)
        {
            Text maximumText = maximum.GetComponent<Text>();
            maximumText.text = "/ " + safeMax.ToString("00");
        }

        int visibleCount = Mathf.Min(safeMax, MaxVisualLives);
        for (int i = 0; i < lifeSegments.Length; i++)
        {
            bool visible = i < visibleCount;
            lifeSegments[i].gameObject.SetActive(visible);
            if (visible)
            {
                lifeSegments[i].color = i < safeCurrent ? activeColor : EmptyLife;
            }
        }
    }

    private void UpdateTime(float remainingSeconds, float totalSeconds)
    {
        float safeRemaining = Mathf.Max(0f, remainingSeconds);
        int displayedSeconds = Mathf.CeilToInt(safeRemaining);
        int minutes = displayedSeconds / 60;
        int seconds = displayedSeconds % 60;

        timeValueText.text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (safeRemaining <= 15f)
        {
            currentTimeColor = Critical;
            criticalTime = true;
        }
        else if (safeRemaining <= 60f)
        {
            currentTimeColor = Warning;
            criticalTime = false;
        }
        else
        {
            currentTimeColor = Cyan;
            criticalTime = false;
        }

        timeValueText.color = currentTimeColor;
        timeFillImage.color = currentTimeColor;

        float progress = totalSeconds > 0f ? Mathf.Clamp01(safeRemaining / totalSeconds) : 0f;
        timeFill.anchorMax = new Vector2(progress, 1f);
        timeFill.offsetMin = Vector2.zero;
        timeFill.offsetMax = Vector2.zero;
    }

    private void UpdateLevel(bool force)
    {
        int currentLevel = GameManager.Instance != null
            ? Mathf.Max(1, GameManager.Instance.GetNivelActual())
            : 1;

        if (!force && currentLevel == lastLevel)
        {
            return;
        }

        lastLevel = currentLevel;
        levelValueText.text = currentLevel.ToString("00");
        levelDetailText.text = "SECTOR " + currentLevel.ToString("00");
    }
}
