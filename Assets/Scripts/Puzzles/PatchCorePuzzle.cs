using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PatchCorePuzzle - Memoria: encontrar pares de colores.
/// Click en una carta para voltearla, luego en otra. Si son iguales, se quedan.
/// </summary>
public class PatchCorePuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int cols = 4;

    private int totalPairs;
    private int matchedPairs = 0;
    private int firstCard = -1;
    private int secondCard = -1;
    private bool waitingForFlip = false;
    private float flipTimer = 0f;

    private int[] cardValues;
    private bool[] cardRevealed;
    private bool[] cardMatched;
    private Button[] cardButtons;
    private Image[] cardImages;
    private Text[] cardLabels;
    private GameObject puzzleArea;
    private Text statusText;

    private Color[] pairColors = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.2f, 0.4f, 0.9f),
        new Color(0.9f, 0.9f, 0.1f),
        new Color(0.9f, 0.5f, 0.1f),
        new Color(0.7f, 0.2f, 0.8f),
        new Color(0.1f, 0.9f, 0.9f),
        new Color(0.9f, 0.3f, 0.6f)
    };

    private Color cardBackColor = new Color(0.2f, 0.2f, 0.4f);
    private Color cardMatchedColor = new Color(0.15f, 0.15f, 0.15f);

    protected override void Start()
    {
        base.Start();
        puzzleName = "PatchCore";
        totalPairs = (rows * cols) / 2;
        CreatePuzzleArea();
        CreateCards();
        CreateStatusText();
    }

    protected override void Update()
    {
        base.Update();
        if (puzzleCompleted || puzzleFailed) return;

        if (waitingForFlip)
        {
            flipTimer -= Time.deltaTime;
            if (flipTimer <= 0f)
            {
                HideCards();
                waitingForFlip = false;
            }
        }
    }

    private void CreatePuzzleArea()
    {
        GameObject canvas = GameObject.Find("PuzzleCanvas");
        if (canvas == null) return;

        puzzleArea = new GameObject("MemoryArea");
        puzzleArea.transform.SetParent(canvas.transform, false);

        RectTransform areaRect = puzzleArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.05f, 0.1f);
        areaRect.anchorMax = new Vector2(0.95f, 0.85f);
        areaRect.sizeDelta = Vector2.zero;

        Image areaBg = puzzleArea.AddComponent<Image>();
        areaBg.color = new Color(0.08f, 0.08f, 0.15f, 0.9f);
    }

    private void CreateCards()
    {
        int totalCards = rows * cols;
        cardValues = new int[totalCards];
        cardRevealed = new bool[totalCards];
        cardMatched = new bool[totalCards];
        cardButtons = new Button[totalCards];
        cardImages = new Image[totalCards];
        cardLabels = new Text[totalCards];

        for (int i = 0; i < totalPairs; i++)
        {
            cardValues[i * 2] = i;
            cardValues[i * 2 + 1] = i;
        }

        for (int i = totalCards - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = temp;
        }

        float cardWidth = 0.7f / cols;
        float cardHeight = 0.65f / rows;
        float startX = 0.15f;
        float startY = 0.78f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int index = r * cols + c;
                float x = startX + c * (cardWidth + 0.02f);
                float y = startY - r * (cardHeight + 0.02f);
                CreateCard(index, new Vector2(x, y), new Vector2(x + cardWidth, y - cardHeight));
            }
        }
    }

    private void CreateCard(int index, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject card = new GameObject("Card_" + index);
        card.transform.SetParent(puzzleArea.transform, false);

        RectTransform rect = card.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = Vector2.zero;

        Image bg = card.AddComponent<Image>();
        bg.color = cardBackColor;
        cardImages[index] = bg;

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = bg;
        cardButtons[index] = btn;

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(card.transform, false);
        Text label = labelObj.AddComponent<Text>();
        label.text = "?";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        cardLabels[index] = label;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        Outline outline = labelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.5f);
        outline.effectDistance = new Vector2(1, -1);

        int cardIndex = index;
        btn.onClick.AddListener(() => OnCardClicked(cardIndex));
    }

    private void OnCardClicked(int index)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (waitingForFlip) return;
        if (cardRevealed[index] || cardMatched[index]) return;

        RevealCard(index);

        if (firstCard == -1)
        {
            firstCard = index;
        }
        else
        {
            secondCard = index;
            CheckMatch();
        }
    }

    private void RevealCard(int index)
    {
        cardRevealed[index] = true;
        int colorIndex = cardValues[index];
        if (colorIndex < pairColors.Length)
        {
            cardImages[index].color = pairColors[colorIndex];
        }
        cardLabels[index].text = (cardValues[index] + 1).ToString();
    }

    private void HideCards()
    {
        for (int i = 0; i < cardRevealed.Length; i++)
        {
            if (cardRevealed[i] && !cardMatched[i])
            {
                cardRevealed[i] = false;
                cardImages[i].color = cardBackColor;
                cardLabels[i].text = "?";
            }
        }

        firstCard = -1;
        secondCard = -1;
    }

    private void CheckMatch()
    {
        if (cardValues[firstCard] == cardValues[secondCard])
        {
            cardMatched[firstCard] = true;
            cardMatched[secondCard] = true;
            matchedPairs++;

            cardImages[firstCard].color = cardMatchedColor;
            cardImages[secondCard].color = cardMatchedColor;
            cardLabels[firstCard].text = "OK";
            cardLabels[secondCard].text = "OK";
            cardLabels[firstCard].color = Color.green;
            cardLabels[secondCard].color = Color.green;

            cardButtons[firstCard].interactable = false;
            cardButtons[secondCard].interactable = false;

            Debug.Log("PatchCore: Par encontrado! (" + matchedPairs + "/" + totalPairs + ")");

            if (matchedPairs >= totalPairs)
            {
                Debug.Log("PatchCore: Todos los pares encontrados!");
                Complete();
                return;
            }

            UpdateStatus("Pares: " + matchedPairs + "/" + totalPairs);
            firstCard = -1;
            secondCard = -1;
        }
        else
        {
            waitingForFlip = true;
            flipTimer = 0.8f;
        }
    }

    private void CreateStatusText()
    {
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(puzzleArea.transform, false);

        statusText = statusObj.AddComponent<Text>();
        statusText.text = "Encuentra los pares: 0/" + totalPairs;
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 20;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.yellow;

        Outline outline = statusObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        RectTransform rect = statusObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.88f);
        rect.anchorMax = new Vector2(0.9f, 0.97f);
        rect.sizeDelta = Vector2.zero;
    }

    private void UpdateStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}
