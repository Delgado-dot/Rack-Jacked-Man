using UnityEngine;
using UnityEngine.UI;

public class CablePuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private int totalPairs = 4;
    [SerializeField] private float circleRadius = 40f;

    private int pairsCompleted = 0;
    private int selectedLeft = -1;
    private GameObject[] leftCircles;
    private GameObject[] rightCircles;
    private int[] leftColorIndex;
    private int[] rightColorIndex;
    private bool[] connected;
    private GameObject[] lineObjs;
    private GameObject puzzleArea;
    private Text statusText;

    private Color[] colors = {
        new Color(1f, 0.2f, 0.2f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.2f, 0.4f, 1f),
        new Color(1f, 1f, 0.2f),
        new Color(1f, 0.5f, 0f),
        new Color(0.8f, 0.2f, 0.8f),
        new Color(0f, 0.9f, 0.9f),
        new Color(1f, 0.7f, 0.8f)
    };

    private string[] colorNames = {
        "Rojo", "Verde", "Azul", "Amarillo", "Naranja", "Morado", "Cyan", "Rosa"
    };

    protected override void Start()
    {
        base.Start();
        puzzleName = "Conectar Cables";
        timeLimit = 45f;
        timer = timeLimit;
        CreatePuzzleArea();
        CreateCircles();
        CreateStatusText();
    }

    private void CreatePuzzleArea()
    {
        GameObject canvas = GameObject.Find("PuzzleCanvas");
        if (canvas == null)
        {
            canvas = new GameObject("PuzzleCanvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 200;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
        }

        puzzleArea = new GameObject("CablePuzzleArea");
        puzzleArea.transform.SetParent(canvas.transform, false);

        RectTransform areaRect = puzzleArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.02f, 0.02f);
        areaRect.anchorMax = new Vector2(0.98f, 0.98f);
        areaRect.sizeDelta = Vector2.zero;

        Image areaBg = puzzleArea.AddComponent<Image>();
        areaBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
    }

    private void CreateCircles()
    {
        leftCircles = new GameObject[totalPairs];
        rightCircles = new GameObject[totalPairs];
        connected = new bool[totalPairs];
        lineObjs = new GameObject[totalPairs];
        leftColorIndex = new int[totalPairs];
        rightColorIndex = new int[totalPairs];

        for (int i = 0; i < totalPairs; i++)
        {
            leftColorIndex[i] = i;
            rightColorIndex[i] = i;
        }

        for (int i = totalPairs - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = rightColorIndex[i];
            rightColorIndex[i] = rightColorIndex[j];
            rightColorIndex[j] = temp;
        }

        float spacing = 1f / (totalPairs + 1);

        for (int i = 0; i < totalPairs; i++)
        {
            float yPos = spacing * (i + 1);

            leftCircles[i] = CreateCircle(
                "Left_" + i,
                new Vector2(0.18f, yPos),
                colors[leftColorIndex[i]],
                i,
                true
            );

            rightCircles[i] = CreateCircle(
                "Right_" + i,
                new Vector2(0.82f, yPos),
                colors[rightColorIndex[i]],
                i,
                false
            );
        }
    }

    private GameObject CreateCircle(string name, Vector2 anchorPos, Color color, int index, bool isLeft)
    {
        GameObject circleObj = new GameObject(name);
        circleObj.transform.SetParent(puzzleArea.transform, false);

        RectTransform rect = circleObj.AddComponent<RectTransform>();
        float size = 0.08f;
        rect.anchorMin = anchorPos - new Vector2(size, size);
        rect.anchorMax = anchorPos + new Vector2(size, size);
        rect.sizeDelta = Vector2.zero;

        Image img = circleObj.AddComponent<Image>();
        img.color = color;

        Button btn = circleObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;

        int circleIndex = index;
        bool leftSide = isLeft;
        btn.onClick.AddListener(() => OnCircleClicked(circleIndex, leftSide));

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(circleObj.transform, false);
        Text label = labelObj.AddComponent<Text>();
        int colorIdx = isLeft ? leftColorIndex[index] : rightColorIndex[index];
        label.text = colorNames[colorIdx].Substring(0, 2).ToUpper();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        Outline ol = labelObj.AddComponent<Outline>();
        ol.effectColor = Color.black;
        ol.effectDistance = new Vector2(1, -1);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(circleObj.transform, false);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(1f, 1f, 1f, 0.8f);
        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(-0.08f, -0.08f);
        borderRect.anchorMax = new Vector2(1.08f, 1.08f);
        borderRect.sizeDelta = Vector2.zero;
        borderObj.transform.SetAsFirstSibling();

        return circleObj;
    }

    private void CreateStatusText()
    {
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(puzzleArea.transform, false);

        statusText = statusObj.AddComponent<Text>();
        statusText.text = "Conecta los colores: 0/" + totalPairs;
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 24;
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

    private void OnCircleClicked(int index, bool isLeft)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (connected[index]) return;

        if (isLeft)
        {
            if (selectedLeft >= 0 && selectedLeft != index)
            {
                HighlightCircle(leftCircles[selectedLeft], false);
            }
            selectedLeft = index;
            HighlightCircle(leftCircles[index], true);
            Debug.Log("CablePuzzle: Izquierdo " + index + " seleccionado (" + colorNames[leftColorIndex[index]] + ")");
        }
        else
        {
            if (selectedLeft == -1)
            {
                Debug.Log("CablePuzzle: Selecciona primero un circulo IZQUIERDO");
                return;
            }

            if (leftColorIndex[selectedLeft] == rightColorIndex[index])
            {
                connected[selectedLeft] = true;
                pairsCompleted++;
                DrawUILine(selectedLeft, index);
                HighlightCircle(leftCircles[selectedLeft], false);

                Image lImg = leftCircles[selectedLeft].GetComponent<Image>();
                if (lImg != null) lImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                Image rImg = rightCircles[index].GetComponent<Image>();
                if (rImg != null) rImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

                selectedLeft = -1;
                Debug.Log("CablePuzzle: Conectado! (" + pairsCompleted + "/" + totalPairs + ")");

                if (pairsCompleted >= totalPairs)
                {
                    Complete();
                    return;
                }
                UpdateStatus();
            }
            else
            {
                Debug.Log("CablePuzzle: Color incorrecto!");
                HighlightCircle(leftCircles[selectedLeft], false);
                selectedLeft = -1;
            }
        }
    }

    private void DrawUILine(int leftIndex, int rightIndex)
    {
        GameObject lineObj = new GameObject("Line_" + leftIndex);
        lineObj.transform.SetParent(puzzleArea.transform, false);

        RectTransform lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.one;
        lineRect.sizeDelta = Vector2.zero;

        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = colors[leftColorIndex[leftIndex]];
        lineImg.raycastTarget = false;

        RectTransform leftRect = leftCircles[leftIndex].GetComponent<RectTransform>();
        RectTransform rightRect = rightCircles[rightIndex].GetComponent<RectTransform>();

        Vector2 leftCenter = (leftRect.anchorMin + leftRect.anchorMax) / 2f;
        Vector2 rightCenter = (rightRect.anchorMin + rightRect.anchorMax) / 2f;

        float y = leftCenter.y;
        float xMin = Mathf.Min(leftCenter.x, rightCenter.x);
        float xMax = Mathf.Max(leftCenter.x, rightCenter.x);

        lineRect.anchorMin = new Vector2(xMin, y - 0.005f);
        lineRect.anchorMax = new Vector2(xMax, y + 0.005f);
        lineRect.sizeDelta = Vector2.zero;

        lineObjs[leftIndex] = lineObj;
    }

    private void HighlightCircle(GameObject circle, bool highlight)
    {
        if (circle == null) return;
        if (highlight)
        {
            circle.transform.localScale = Vector3.one * 1.3f;
        }
        else
        {
            circle.transform.localScale = Vector3.one;
        }
    }

    private void UpdateStatus()
    {
        if (statusText != null)
        {
            statusText.text = "Conecta los colores: " + pairsCompleted + "/" + totalPairs;
        }
    }
}
