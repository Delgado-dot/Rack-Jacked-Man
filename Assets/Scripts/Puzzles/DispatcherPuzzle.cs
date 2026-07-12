using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DispatcherPuzzle - Ordenar paquetes por prioridad (1 a 5).
/// Click en el paquete con la prioridad correcta en secuencia.
/// </summary>
public class DispatcherPuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private int totalPackages = 5;

    private int nextPriority = 1;
    private int sortedCount = 0;
    private GameObject[] packageObjects;
    private int[] packagePriorities;
    private bool[] packageSorted;
    private Text statusText;
    private GameObject puzzleArea;

    private Color[] priorityColors = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.9f, 0.5f, 0.1f),
        new Color(0.9f, 0.9f, 0.1f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.2f, 0.5f, 0.9f)
    };

    protected override void Start()
    {
        base.Start();
        puzzleName = "Dispatcher";
        CreatePuzzleArea();
        CreatePackages();
        CreateStatusText();
    }

    private void CreatePuzzleArea()
    {
        GameObject canvas = GameObject.Find("PuzzleCanvas");
        if (canvas == null) return;

        puzzleArea = new GameObject("DispatcherArea");
        puzzleArea.transform.SetParent(canvas.transform, false);

        RectTransform areaRect = puzzleArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.05f, 0.1f);
        areaRect.anchorMax = new Vector2(0.95f, 0.85f);
        areaRect.sizeDelta = Vector2.zero;

        Image areaBg = puzzleArea.AddComponent<Image>();
        areaBg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
    }

    private void CreatePackages()
    {
        packageObjects = new GameObject[totalPackages];
        packagePriorities = new int[totalPackages];
        packageSorted = new bool[totalPackages];

        for (int i = 0; i < totalPackages; i++)
        {
            packagePriorities[i] = i + 1;
        }

        for (int i = totalPackages - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = packagePriorities[i];
            packagePriorities[i] = packagePriorities[j];
            packagePriorities[j] = temp;
        }

        for (int i = 0; i < totalPackages; i++)
        {
            CreatePackage(i);
        }

        CreateSlots();
    }

    private void CreatePackage(int index)
    {
        GameObject pkg = new GameObject("Package_" + index);
        pkg.transform.SetParent(puzzleArea.transform, false);

        RectTransform rect = pkg.AddComponent<RectTransform>();
        float x = 0.15f + (index * 0.14f);
        rect.anchorMin = new Vector2(x, 0.35f);
        rect.anchorMax = new Vector2(x + 0.11f, 0.65f);
        rect.sizeDelta = Vector2.zero;

        Image pkgBg = pkg.AddComponent<Image>();
        int colorIndex = packagePriorities[index] - 1;
        pkgBg.color = colorIndex < priorityColors.Length ? priorityColors[colorIndex] : Color.gray;

        Button btn = pkg.AddComponent<Button>();
        btn.targetGraphic = pkgBg;

        GameObject numObj = new GameObject("Number");
        numObj.transform.SetParent(pkg.transform, false);
        Text numText = numObj.AddComponent<Text>();
        numText.text = packagePriorities[index].ToString();
        numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numText.fontSize = 32;
        numText.fontStyle = FontStyle.Bold;
        numText.alignment = TextAnchor.MiddleCenter;
        numText.color = Color.white;
        RectTransform numRect = numObj.GetComponent<RectTransform>();
        numRect.anchorMin = Vector2.zero;
        numRect.anchorMax = Vector2.one;
        numRect.sizeDelta = Vector2.zero;

        Outline outline = numObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        int pkgIndex = index;
        btn.onClick.AddListener(() => OnPackageClicked(pkgIndex));

        packageObjects[index] = pkg;
    }

    private void CreateSlots()
    {
        for (int i = 0; i < totalPackages; i++)
        {
            GameObject slot = new GameObject("Slot_" + i);
            slot.transform.SetParent(puzzleArea.transform, false);

            RectTransform slotRect = slot.AddComponent<RectTransform>();
            float x = 0.15f + (i * 0.14f);
            slotRect.anchorMin = new Vector2(x, 0.12f);
            slotRect.anchorMax = new Vector2(x + 0.11f, 0.3f);
            slotRect.sizeDelta = Vector2.zero;

            Image slotBg = slot.AddComponent<Image>();
            slotBg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(slot.transform, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = (i + 1).ToString();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(0.6f, 0.6f, 0.6f);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }
    }

    private void OnPackageClicked(int index)
    {
        if (puzzleCompleted || puzzleFailed) return;
        if (packageSorted[index]) return;

        if (packagePriorities[index] == nextPriority)
        {
            packageSorted[index] = true;
            sortedCount++;

            Image pkgImg = packageObjects[index].GetComponent<Image>();
            if (pkgImg != null)
            {
                pkgImg.color = new Color(pkgImg.color.r, pkgImg.color.g, pkgImg.color.b, 0.4f);
            }

            Text numText = packageObjects[index].transform.Find("Number").GetComponent<Text>();
            if (numText != null)
            {
                numText.text = "OK";
                numText.color = Color.green;
            }

            Button btn = packageObjects[index].GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            RectTransform pkgRect = packageObjects[index].GetComponent<RectTransform>();
            float x = 0.15f + ((nextPriority - 1) * 0.14f);
            pkgRect.anchorMin = new Vector2(x, 0.12f);
            pkgRect.anchorMax = new Vector2(x + 0.11f, 0.3f);

            nextPriority++;

            Debug.Log("Dispatcher: Prioridad " + (nextPriority - 1) + " ordenada!");

            if (sortedCount >= totalPackages)
            {
                Debug.Log("Dispatcher: Todos los paquetes ordenados!");
                Complete();
                return;
            }

            UpdateStatus();
        }
        else
        {
            Debug.Log("Dispatcher: Paquete incorrecto!");
            StartCoroutine(ShakePackage(packageObjects[index]));
        }
    }

    private System.Collections.IEnumerator ShakePackage(GameObject pkg)
    {
        if (pkg == null) yield break;

        RectTransform rect = pkg.GetComponent<RectTransform>();
        Vector3 originalPos = rect.anchoredPosition3D;

        float duration = 0.3f;
        float elapsed = 0f;
        float shakeAmount = 10f;

        Image img = pkg.GetComponent<Image>();
        Color originalColor = img.color;
        img.color = Color.red;

        while (elapsed < duration)
        {
            float x = Mathf.Sin(elapsed * 30f) * shakeAmount * (1f - elapsed / duration);
            rect.anchoredPosition3D = originalPos + new Vector3(x, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition3D = originalPos;
        img.color = originalColor;
    }

    private void CreateStatusText()
    {
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(puzzleArea.transform, false);

        statusText = statusObj.AddComponent<Text>();
        statusText.text = "Ordena por prioridad: selecciona la " + nextPriority;
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

    private void UpdateStatus()
    {
        if (statusText != null)
        {
            statusText.text = "Ordena por prioridad: selecciona la " + nextPriority;
        }
    }
}
