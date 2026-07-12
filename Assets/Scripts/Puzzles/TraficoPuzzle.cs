using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TraficoPuzzle - Dirigir carros al carril correcto por color.
/// Hay 3 carriles de colores y carros que aparecen abajo.
/// Click en un carro, luego click en el carril correcto.
/// </summary>
public class TraficoPuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private int totalCars = 6;
    [SerializeField] private int laneCount = 3;

    private int carsDirected = 0;
    private int selectedCar = -1;
    private GameObject[] carObjects;
    private int[] carLaneAssignment;
    private bool[] carDirected;
    private GameObject[] laneObjects;
    private GameObject puzzleArea;
    private Text statusText;

    private Color[] laneColors = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.2f, 0.6f, 0.9f),
        new Color(0.2f, 0.8f, 0.2f)
    };

    protected override void Start()
    {
        base.Start();
        puzzleName = "Trafico";
        CreatePuzzleArea();
        CreateLanes();
        CreateCars();
        CreateStatusText();
    }

    private void CreatePuzzleArea()
    {
        GameObject canvas = GameObject.Find("PuzzleCanvas");
        if (canvas == null) return;

        puzzleArea = new GameObject("TrafficArea");
        puzzleArea.transform.SetParent(canvas.transform, false);

        RectTransform areaRect = puzzleArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.05f, 0.1f);
        areaRect.anchorMax = new Vector2(0.95f, 0.85f);
        areaRect.sizeDelta = Vector2.zero;

        Image areaBg = puzzleArea.AddComponent<Image>();
        areaBg.color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
    }

    private void CreateLanes()
    {
        laneObjects = new GameObject[laneCount];

        for (int i = 0; i < laneCount; i++)
        {
            GameObject lane = new GameObject("Lane_" + i);
            lane.transform.SetParent(puzzleArea.transform, false);

            RectTransform laneRect = lane.AddComponent<RectTransform>();
            float x = 0.1f + (i * 0.28f);
            laneRect.anchorMin = new Vector2(x, 0.15f);
            laneRect.anchorMax = new Vector2(x + 0.24f, 0.82f);
            laneRect.sizeDelta = Vector2.zero;

            Image laneBg = lane.AddComponent<Image>();
            laneBg.color = new Color(laneColors[i].r, laneColors[i].g, laneColors[i].b, 0.25f);

            Button laneBtn = lane.AddComponent<Button>();
            laneBtn.targetGraphic = laneBg;
            laneBtn.transition = Selectable.Transition.None;

            int laneIndex = i;
            laneBtn.onClick.AddListener(() => OnLaneClicked(laneIndex));

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(lane.transform, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "Carril " + (i + 1);
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = laneColors[i];
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.9f);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.sizeDelta = Vector2.zero;

            Outline outline = labelObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);

            laneObjects[i] = lane;
        }
    }

    private void CreateCars()
    {
        carObjects = new GameObject[totalCars];
        carLaneAssignment = new int[totalCars];
        carDirected = new bool[totalCars];

        for (int i = 0; i < totalCars; i++)
        {
            carLaneAssignment[i] = Random.Range(0, laneCount);
            CreateCar(i);
        }
    }

    private void CreateCar(int index)
    {
        GameObject car = new GameObject("Car_" + index);
        car.transform.SetParent(puzzleArea.transform, false);

        RectTransform carRect = car.AddComponent<RectTransform>();
        float x = 0.1f + (index * 0.13f);
        carRect.anchorMin = new Vector2(x, 0.02f);
        carRect.anchorMax = new Vector2(x + 0.1f, 0.13f);
        carRect.sizeDelta = Vector2.zero;

        Image carBg = car.AddComponent<Image>();
        carBg.color = laneColors[carLaneAssignment[index]];

        Button btn = car.AddComponent<Button>();
        btn.targetGraphic = carBg;

        GameObject carLabel = new GameObject("Label");
        carLabel.transform.SetParent(car.transform, false);
        Text carText = carLabel.AddComponent<Text>();
        carText.text = "Car";
        carText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        carText.fontSize = 12;
        carText.fontStyle = FontStyle.Bold;
        carText.alignment = TextAnchor.MiddleCenter;
        carText.color = Color.white;
        RectTransform carLabelRect = carLabel.GetComponent<RectTransform>();
        carLabelRect.anchorMin = Vector2.zero;
        carLabelRect.anchorMax = Vector2.one;
        carLabelRect.sizeDelta = Vector2.zero;

        Outline outline = carLabel.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        int carIndex = index;
        btn.onClick.AddListener(() => OnCarClicked(carIndex));

        carObjects[index] = car;
    }

    private void OnCarClicked(int index)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (carDirected[index]) return;

        if (selectedCar >= 0)
        {
            HighlightCar(selectedCar, false);
        }

        selectedCar = index;
        HighlightCar(index, true);

        UpdateStatus("Selecciona el carril " + GetColorName(carLaneAssignment[index]));
    }

    private void OnLaneClicked(int laneIndex)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (selectedCar < 0) return;

        if (carLaneAssignment[selectedCar] == laneIndex)
        {
            carDirected[selectedCar] = true;
            carsDirected++;

            Image carImg = carObjects[selectedCar].GetComponent<Image>();
            carImg.color = new Color(carImg.color.r, carImg.color.g, carImg.color.b, 0.3f);

            Button carBtn = carObjects[selectedCar].GetComponent<Button>();
            carBtn.interactable = false;

            Text carLabel = carObjects[selectedCar].transform.Find("Label").GetComponent<Text>();
            carLabel.text = "OK";

            RectTransform carRect = carObjects[selectedCar].GetComponent<RectTransform>();
            float laneX = 0.1f + (laneIndex * 0.28f) + 0.08f;
            carRect.anchorMin = new Vector2(laneX, 0.4f);
            carRect.anchorMax = new Vector2(laneX + 0.08f, 0.55f);

            selectedCar = -1;

            Debug.Log("Trafico: Carro dirigido al carril " + (laneIndex + 1));

            if (carsDirected >= totalCars)
            {
                Debug.Log("Trafico: Todos los carros dirigidos!");
                Complete();
                return;
            }

            UpdateStatus("Carros restantes: " + (totalCars - carsDirected));
        }
        else
        {
            Debug.Log("Trafico: Carril incorrecto!");
            StartCoroutine(FlashWrong());
        }
    }

    private System.Collections.IEnumerator FlashWrong()
    {
        if (selectedCar >= 0)
        {
            Image img = carObjects[selectedCar].GetComponent<Image>();
            Color orig = img.color;
            img.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            img.color = orig;
            HighlightCar(selectedCar, false);
            selectedCar = -1;
        }
        UpdateStatus("Carril incorrecto! Intenta de nuevo");
    }

    private void HighlightCar(int index, bool highlight)
    {
        if (carObjects[index] == null) return;
        carObjects[index].transform.localScale = highlight ? Vector3.one * 1.15f : Vector3.one;
    }

    private string GetColorName(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return "ROJO";
            case 1: return "AZUL";
            case 2: return "VERDE";
            default: return "?";
        }
    }

    private void CreateStatusText()
    {
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(puzzleArea.transform, false);

        statusText = statusObj.AddComponent<Text>();
        statusText.text = "Click en un carro, luego en su carril";
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 18;
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
