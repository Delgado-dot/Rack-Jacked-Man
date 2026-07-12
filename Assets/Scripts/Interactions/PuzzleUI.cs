using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PuzzleUI - Maneja la UI de interaccion con racks.
/// Crea dinamicamente el prompt "E" + "Interactuar" y el panel de puzzle.
/// </summary>
public class PuzzleUI : MonoBehaviour
{
    private GameObject canvas;
    private GameObject interactPromptGroup;
    private GameObject puzzlePanel;
    private Text puzzleTitleText;
    private Text puzzleTimerText;
    private PlayerInteract playerInteract;

    private void Start()
    {
        canvas = FindOrCreateCanvas();
        CreateInteractPrompt();
        CreatePuzzlePanel();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            playerInteract = player.GetComponent<PlayerInteract>();
        }

        if (interactPromptGroup != null)
            interactPromptGroup.SetActive(false);
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
    }

    private void Update()
    {
        if (playerInteract == null) return;

        RackInteractable currentRack = playerInteract.GetCurrentRack();

        if (currentRack != null && currentRack.IsInteractable())
        {
            ShowInteractPrompt();
        }
        else
        {
            HideInteractPrompt();
        }
    }

    private GameObject FindOrCreateCanvas()
    {
        Canvas existing = FindAnyObjectByType<Canvas>();
        if (existing != null) return existing.gameObject;

        GameObject canvasGO = new GameObject("PuzzleCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        UnityEngine.EventSystems.EventSystem es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasGO;
    }

    private void CreateInteractPrompt()
    {
        interactPromptGroup = new GameObject("InteractPrompt");
        interactPromptGroup.transform.SetParent(canvas.transform, false);

        RectTransform groupRect = interactPromptGroup.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.3f);
        groupRect.anchorMax = new Vector2(0.5f, 0.3f);
        groupRect.anchoredPosition = Vector2.zero;
        groupRect.sizeDelta = new Vector2(200, 100);

        GameObject keyObj = new GameObject("KeyE");
        keyObj.transform.SetParent(interactPromptGroup.transform, false);
        Image keyBg = keyObj.AddComponent<Image>();
        keyBg.color = new Color(0, 0, 0, 0.7f);
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.3f, 0.5f);
        keyRect.anchorMax = new Vector2(0.7f, 1f);
        keyRect.sizeDelta = Vector2.zero;

        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(keyObj.transform, false);
        Text keyText = keyTextObj.AddComponent<Text>();
        keyText.text = "E";
        keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        keyText.fontSize = 36;
        keyText.fontStyle = FontStyle.Bold;
        keyText.alignment = TextAnchor.MiddleCenter;
        keyText.color = Color.white;
        RectTransform keyTextRect = keyTextObj.GetComponent<RectTransform>();
        keyTextRect.anchorMin = Vector2.zero;
        keyTextRect.anchorMax = Vector2.one;
        keyTextRect.sizeDelta = Vector2.zero;

        GameObject labelObj = new GameObject("InteractLabel");
        labelObj.transform.SetParent(interactPromptGroup.transform, false);
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Interactuar";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 18;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0.5f);
        labelRect.sizeDelta = Vector2.zero;

        Outline outline = keyTextObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        Outline labelOutline = labelObj.AddComponent<Outline>();
        labelOutline.effectColor = Color.black;
        labelOutline.effectDistance = new Vector2(1, -1);
    }

    private void CreatePuzzlePanel()
    {
        puzzlePanel = new GameObject("PuzzlePanel");
        puzzlePanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = puzzlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelBg = puzzlePanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.85f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(puzzlePanel.transform, false);
        puzzleTitleText = titleObj.AddComponent<Text>();
        puzzleTitleText.text = "PUZZLE";
        puzzleTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        puzzleTitleText.fontSize = 28;
        puzzleTitleText.fontStyle = FontStyle.Bold;
        puzzleTitleText.alignment = TextAnchor.MiddleCenter;
        puzzleTitleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.9f);
        titleRect.anchorMax = new Vector2(0.9f, 0.98f);
        titleRect.sizeDelta = Vector2.zero;

        GameObject timerObj = new GameObject("Timer");
        timerObj.transform.SetParent(puzzlePanel.transform, false);
        puzzleTimerText = timerObj.AddComponent<Text>();
        puzzleTimerText.text = "Tiempo: 30";
        puzzleTimerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        puzzleTimerText.fontSize = 20;
        puzzleTimerText.alignment = TextAnchor.MiddleCenter;
        puzzleTimerText.color = Color.yellow;
        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.1f, 0.82f);
        timerRect.anchorMax = new Vector2(0.9f, 0.9f);
        timerRect.sizeDelta = Vector2.zero;

        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(puzzlePanel.transform, false);
        Button closeBtn = closeObj.AddComponent<Button>();
        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        closeBtn.targetGraphic = closeImg;
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.92f, 0.92f);
        closeRect.anchorMax = new Vector2(0.98f, 0.98f);
        closeRect.sizeDelta = Vector2.zero;

        GameObject closeLabel = new GameObject("X");
        closeLabel.transform.SetParent(closeObj.transform, false);
        Text closeText = closeLabel.AddComponent<Text>();
        closeText.text = "X";
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 16;
        closeText.fontStyle = FontStyle.Bold;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.white;
        RectTransform closeTextRect = closeLabel.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        closeBtn.onClick.AddListener(() =>
        {
            PuzzleManager pm = FindAnyObjectByType<PuzzleManager>();
            if (pm != null) pm.PuzzleFailed();
        });
    }

    public void ShowInteractPrompt()
    {
        if (interactPromptGroup != null)
            interactPromptGroup.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        if (interactPromptGroup != null)
            interactPromptGroup.SetActive(false);
    }

    public void ShowPuzzlePanel(string title)
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(true);
            if (puzzleTitleText != null)
                puzzleTitleText.text = title;
        }
        HideInteractPrompt();
    }

    public void HidePuzzlePanel()
    {
        if (puzzlePanel != null)
            puzzlePanel.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (puzzleTimerText != null)
            puzzleTimerText.text = "Tiempo: " + Mathf.CeilToInt(timeRemaining);
    }

    public GameObject GetPuzzlePanel() { return puzzlePanel; }
}
