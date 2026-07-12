using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NavePuzzle - Navegar una nave hasta el objetivo sin chocar.
/// Control con flechas/WASD. Obstaculos que se mueven.
/// </summary>
public class NavePuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private float shipSpeed = 300f;
    [SerializeField] private int obstacleCount = 6;
    [SerializeField] private float obstacleSpeed = 80f;

    private RectTransform shipRect;
    private Image shipImage;
    private Vector2 shipPos;
    private GameObject target;
    private Vector2 targetPos;
    private GameObject[] obstacles;
    private RectTransform[] obstacleRects;
    private Vector2[] obstacleDirections;
    private GameObject puzzleArea;
    private Text statusText;
    private bool gameStarted = false;

    protected override void Start()
    {
        base.Start();
        puzzleName = "Navegar";
        CreatePuzzleArea();
        CreateShip();
        CreateTarget();
        CreateObstacles();
        CreateStatusText();
        gameStarted = true;
    }

    protected override void Update()
    {
        base.Update();
        if (puzzleCompleted || puzzleFailed || !gameStarted) return;

        MoveShip();
        MoveObstacles();
        CheckCollisions();
        CheckWin();
    }

    private void CreatePuzzleArea()
    {
        GameObject canvas = GameObject.Find("PuzzleCanvas");
        if (canvas == null) return;

        puzzleArea = new GameObject("NaveArea");
        puzzleArea.transform.SetParent(canvas.transform, false);

        RectTransform areaRect = puzzleArea.AddComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.05f, 0.1f);
        areaRect.anchorMax = new Vector2(0.95f, 0.85f);
        areaRect.sizeDelta = Vector2.zero;

        Image areaBg = puzzleArea.AddComponent<Image>();
        areaBg.color = new Color(0.02f, 0.02f, 0.08f, 0.95f);

        for (int i = 0; i < 20; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            star.transform.SetParent(puzzleArea.transform, false);
            Image starImg = star.AddComponent<Image>();
            starImg.color = new Color(1, 1, 1, Random.Range(0.3f, 0.8f));
            RectTransform starRect = star.GetComponent<RectTransform>();
            starRect.anchorMin = new Vector2(Random.Range(0.02f, 0.98f), Random.Range(0.02f, 0.98f));
            starRect.anchorMax = starRect.anchorMin + new Vector2(0.008f, 0.008f);
            starRect.sizeDelta = Vector2.zero;
        }
    }

    private void CreateShip()
    {
        GameObject shipObj = new GameObject("Ship");
        shipObj.transform.SetParent(puzzleArea.transform, false);

        shipRect = shipObj.AddComponent<RectTransform>();
        shipImage = shipObj.AddComponent<Image>();
        shipImage.color = Color.cyan;

        shipPos = new Vector2(0.1f, 0.5f);
        shipRect.anchorMin = shipPos - new Vector2(0.025f, 0.02f);
        shipRect.anchorMax = shipPos + new Vector2(0.025f, 0.02f);
        shipRect.sizeDelta = Vector2.zero;

        GameObject shipLabel = new GameObject("ShipLabel");
        shipLabel.transform.SetParent(shipObj.transform, false);
        Text shipText = shipLabel.AddComponent<Text>();
        shipText.text = ">";
        shipText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shipText.fontSize = 20;
        shipText.fontStyle = FontStyle.Bold;
        shipText.alignment = TextAnchor.MiddleCenter;
        shipText.color = Color.white;
        RectTransform shipLabelRect = shipLabel.GetComponent<RectTransform>();
        shipLabelRect.anchorMin = Vector2.zero;
        shipLabelRect.anchorMax = Vector2.one;
        shipLabelRect.sizeDelta = Vector2.zero;
    }

    private void CreateTarget()
    {
        target = new GameObject("Target");
        target.transform.SetParent(puzzleArea.transform, false);

        RectTransform targetRect = target.AddComponent<RectTransform>();
        Image targetImg = target.AddComponent<Image>();
        targetImg.color = Color.green;

        targetPos = new Vector2(0.9f, Random.Range(0.2f, 0.8f));
        targetRect.anchorMin = targetPos - new Vector2(0.025f, 0.02f);
        targetRect.anchorMax = targetPos + new Vector2(0.025f, 0.02f);
        targetRect.sizeDelta = Vector2.zero;

        GameObject targetLabel = new GameObject("TargetLabel");
        targetLabel.transform.SetParent(target.transform, false);
        Text targetText = targetLabel.AddComponent<Text>();
        targetText.text = "GOAL";
        targetText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        targetText.fontSize = 10;
        targetText.alignment = TextAnchor.MiddleCenter;
        targetText.color = Color.white;
        RectTransform targetLabelRect = targetLabel.GetComponent<RectTransform>();
        targetLabelRect.anchorMin = Vector2.zero;
        targetLabelRect.anchorMax = Vector2.one;
        targetLabelRect.sizeDelta = Vector2.zero;
    }

    private void CreateObstacles()
    {
        obstacles = new GameObject[obstacleCount];
        obstacleRects = new RectTransform[obstacleCount];
        obstacleDirections = new Vector2[obstacleCount];

        for (int i = 0; i < obstacleCount; i++)
        {
            obstacles[i] = new GameObject("Obstacle_" + i);
            obstacles[i].transform.SetParent(puzzleArea.transform, false);

            obstacleRects[i] = obstacles[i].AddComponent<RectTransform>();
            Image obsImg = obstacles[i].AddComponent<Image>();
            obsImg.color = new Color(1f, 0.3f, 0.3f, 0.9f);

            float x = Random.Range(0.25f, 0.8f);
            float y = Random.Range(0.15f, 0.85f);
            obstacleRects[i].anchorMin = new Vector2(x, y);
            obstacleRects[i].anchorMax = new Vector2(x + 0.05f, y + 0.05f);
            obstacleRects[i].sizeDelta = Vector2.zero;

            obstacleDirections[i] = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
        }
    }

    private void MoveShip()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(h, v).normalized * shipSpeed * Time.deltaTime;

        float newX = shipPos.x + movement.x / Screen.width * 2f;
        float newY = shipPos.y + movement.y / Screen.height * 2f;

        newX = Mathf.Clamp(newX, 0.03f, 0.97f);
        newY = Mathf.Clamp(newY, 0.05f, 0.95f);

        shipPos = new Vector2(newX, newY);

        if (shipRect != null)
        {
            shipRect.anchorMin = shipPos - new Vector2(0.025f, 0.02f);
            shipRect.anchorMax = shipPos + new Vector2(0.025f, 0.02f);
        }
    }

    private void MoveObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            if (obstacleRects[i] == null) continue;

            Vector3[] corners = new Vector3[4];
            obstacleRects[i].GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) / 2f;

            Vector2 moveDir = obstacleDirections[i] * obstacleSpeed * Time.deltaTime;
            Vector2 newPos = (Vector2)obstacleRects[i].anchoredPosition + moveDir;

            float halfWidth = 0.025f;
            float halfHeight = 0.025f;

            if (newPos.x < halfWidth || newPos.x > 1f - halfWidth)
            {
                obstacleDirections[i].x *= -1;
                newPos.x = Mathf.Clamp(newPos.x, halfWidth, 1f - halfWidth);
            }
            if (newPos.y < halfHeight || newPos.y > 1f - halfHeight)
            {
                obstacleDirections[i].y *= -1;
                newPos.y = Mathf.Clamp(newPos.y, halfHeight, 1f - halfHeight);
            }

            obstacleRects[i].anchoredPosition = newPos;
        }
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            if (obstacleRects[i] == null) continue;

            Vector3[] obsCorners = new Vector3[4];
            obstacleRects[i].GetWorldCorners(obsCorners);
            Vector2 obsCenter = (obsCorners[0] + obsCorners[2]) / 2f;

            Vector3[] shipCorners = new Vector3[4];
            shipRect.GetWorldCorners(shipCorners);
            Vector2 shipCenter = (shipCorners[0] + shipCorners[2]) / 2f;

            float dist = Vector2.Distance(shipCenter, obsCenter);
            if (dist < 0.04f)
            {
                Debug.Log("NavePuzzle: Chocado!");
                StartCoroutine(FlashAndFail());
                return;
            }
        }
    }

    private System.Collections.IEnumerator FlashAndFail()
    {
        if (shipImage != null) shipImage.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        Fail();
    }

    private void CheckWin()
    {
        if (target == null) return;

        Vector3[] targetCorners = new Vector3[4];
        target.GetComponent<RectTransform>().GetWorldCorners(targetCorners);
        Vector2 targetCenter = (targetCorners[0] + targetCorners[2]) / 2f;

        Vector3[] shipCorners = new Vector3[4];
        shipRect.GetWorldCorners(shipCorners);
        Vector2 shipCenter = (shipCorners[0] + shipCorners[2]) / 2f;

        float dist = Vector2.Distance(shipCenter, targetCenter);
        if (dist < 0.05f)
        {
            Debug.Log("NavePuzzle: Objetivo alcanzado!");
            if (shipImage != null) shipImage.color = Color.green;
            Complete();
        }
    }

    private void CreateStatusText()
    {
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(puzzleArea.transform, false);

        statusText = statusObj.AddComponent<Text>();
        statusText.text = "Usa WASD/Flechas para llegar al objetivo verde";
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
}
