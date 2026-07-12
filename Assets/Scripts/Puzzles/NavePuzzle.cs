using UnityEngine;
using UnityEngine.UI;

public class NavePuzzle : PuzzleSceneBase
{
    [Header("Configuracion")]
    [SerializeField] private float shipSpeed = 5f;
    [SerializeField] private float bulletSpeed = 9f;
    [SerializeField] private float fireCooldown = 0.18f;

    private RectTransform playArea;
    private RectTransform shipRect;
    private Image shipImage;
    private Text hudText;
    private GameObject canvas;

    private Vector2 shipPos;
    private float shipAngle = -90f;
    private float shootCooldown = 0f;

    private int delivered = 0;
    private int needed = 5;
    private int lives = 4;
    private int lost = 0;
    private int virusesKilled = 0;
    private int maxLost = 3;

    private float packageTimer = 0f;
    private float virusTimer = 0f;
    private float packageInterval = 1.4f;
    private float virusInterval = 2.2f;
    private float virusSpeed = 1.4f;

    private System.Collections.Generic.List<PackageData> packages = new();
    private System.Collections.Generic.List<VirusData> viruses = new();
    private System.Collections.Generic.List<BulletData> bullets = new();
    private System.Collections.Generic.List<ParticleData> particles = new();

    private struct PackageData { public RectTransform rect; public float x; public float vx; public Color color; public bool alive; }
    private struct VirusData { public RectTransform rect; public float x; public float y; public int hp; public bool alive; public Image img; }
    private struct BulletData { public RectTransform rect; public float x; public float y; public float angle; public bool alive; }
    private struct ParticleData { public RectTransform rect; public float vx; public float vy; public float life; public Image img; }

    protected override void Start()
    {
        base.Start();
        puzzleName = "ENTREGA DE PAQUETES";
        timeLimit = 60f;
        timer = timeLimit;
        canvas = FindOrCreateCanvas();
        CreatePlayArea();
        CreateHUD();
        CreateShip();
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

    private void CreatePlayArea()
    {
        GameObject areaGO = new GameObject("PlayArea");
        areaGO.transform.SetParent(canvas.transform, false);
        playArea = areaGO.AddComponent<RectTransform>();
        playArea.anchorMin = new Vector2(0.02f, 0.08f);
        playArea.anchorMax = new Vector2(0.98f, 0.95f);
        playArea.sizeDelta = Vector2.zero;
        Image bg = areaGO.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.05f, 0.11f, 0.95f);

        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(areaGO.transform, false);
        Image borderImg = borderGO.AddComponent<Image>();
        borderImg.color = new Color(0.24f, 0.35f, 0.55f, 0.8f);
        RectTransform borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderGO.transform.SetAsFirstSibling();

        CreateTower("Origin", 0.08f, new Color(0.16f, 0.31f, 0.2f), new Color(0.47f, 0.78f, 0.55f));
        CreateTower("Destino", 0.92f, new Color(0.12f, 0.24f, 0.35f), new Color(0.39f, 0.78f, 0.94f));
    }

    private void CreateTower(string name, float x, Color bg, Color accent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(playArea, false);
        Image img = go.AddComponent<Image>();
        img.color = bg;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(x - 0.025f, 0.4f);
        rect.anchorMax = new Vector2(x + 0.025f, 0.6f);
        rect.sizeDelta = Vector2.zero;

        GameObject borderGO = new GameObject("B");
        borderGO.transform.SetParent(go.transform, false);
        Image bImg = borderGO.AddComponent<Image>();
        bImg.color = accent;
        RectTransform bRect = borderGO.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.sizeDelta = Vector2.zero;

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        Text t = labelGO.AddComponent<Text>();
        t.text = name.ToUpper();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 10;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = accent;
        RectTransform tRect = labelGO.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(-0.5f, -0.3f);
        tRect.anchorMax = new Vector2(1.5f, -0.05f);
        tRect.sizeDelta = Vector2.zero;
    }

    private void CreateHUD()
    {
        GameObject hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(canvas.transform, false);
        Image hudBg = hudGO.AddComponent<Image>();
        hudBg.color = new Color(0.08f, 0.12f, 0.2f, 0.9f);
        RectTransform hudRect = hudGO.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.02f, 0.95f);
        hudRect.anchorMax = new Vector2(0.98f, 1f);
        hudRect.sizeDelta = Vector2.zero;

        GameObject txtGO = new GameObject("HUDText");
        txtGO.transform.SetParent(hudGO.transform, false);
        hudText = txtGO.AddComponent<Text>();
        hudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hudText.fontSize = 16;
        hudText.fontStyle = FontStyle.Bold;
        hudText.alignment = TextAnchor.MiddleCenter;
        hudText.color = Color.white;
        RectTransform tRect = txtGO.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;
        Outline ol = txtGO.AddComponent<Outline>();
        ol.effectColor = Color.black;
    }

    private void CreateShip()
    {
        GameObject shipGO = new GameObject("Ship");
        shipGO.transform.SetParent(playArea, false);
        shipImage = shipGO.AddComponent<Image>();
        shipImage.color = new Color(0.71f, 0.86f, 1f);
        shipRect = shipGO.GetComponent<RectTransform>();
        shipRect.anchorMin = new Vector2(0.45f, 0.48f);
        shipRect.anchorMax = new Vector2(0.49f, 0.52f);
        shipRect.sizeDelta = Vector2.zero;
        shipPos = new Vector2(0.47f, 0.5f);
    }

    private new void Update()
    {
        if (puzzleCompleted || puzzleFailed) return;

        timer -= Time.deltaTime;
        HandleInput();
        SpawnEntities();
        UpdateEntities();
        UpdateHUD();
        CheckDefeat();

        if (delivered >= needed) Complete();
    }

    private void HandleInput()
    {
        float dx = 0, dy = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dy += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dy -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dx -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dx += 1;

        if (dx != 0 && dy != 0) { dx *= 0.707f; dy *= 0.707f; }

        float speed = shipSpeed * 0.001f;
        shipPos.x += dx * speed;
        shipPos.y += dy * speed;
        shipPos.x = Mathf.Clamp(shipPos.x, 0.06f, 0.94f);
        shipPos.y = Mathf.Clamp(shipPos.y, 0.1f, 0.9f);

        shipRect.anchorMin = new Vector2(shipPos.x - 0.02f, shipPos.y - 0.02f);
        shipRect.anchorMax = new Vector2(shipPos.x + 0.02f, shipPos.y + 0.02f);

        Vector3 mouseScreen = Input.mousePosition;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouseScreen, null, out localPoint);
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 mouseNorm = new Vector2(
            (localPoint.x + canvasSize.x / 2f) / canvasSize.x,
            (localPoint.y + canvasSize.y / 2f) / canvasSize.y
        );

        Vector2 shipCenter = shipPos;
        Vector2 dir = mouseNorm - shipCenter;
        shipAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        shipRect.localRotation = Quaternion.Euler(0, 0, shipAngle - 90f);

        shootCooldown -= Time.deltaTime;
        if (Input.GetMouseButton(0) && shootCooldown <= 0f)
        {
            ShootBullet();
            shootCooldown = fireCooldown;
        }
    }

    private void ShootBullet()
    {
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.transform.SetParent(playArea, false);
        Image bulletImg = bulletGO.AddComponent<Image>();
        bulletImg.color = new Color(0.86f, 0.96f, 1f);
        RectTransform bulletRect = bulletGO.GetComponent<RectTransform>();
        bulletRect.anchorMin = new Vector2(shipPos.x - 0.005f, shipPos.y - 0.01f);
        bulletRect.anchorMax = new Vector2(shipPos.x + 0.005f, shipPos.y + 0.01f);
        bulletRect.sizeDelta = Vector2.zero;

        float rad = shipAngle * Mathf.Deg2Rad;
        bullets.Add(new BulletData
        {
            rect = bulletRect,
            x = shipPos.x,
            y = shipPos.y,
            angle = rad,
            alive = true
        });
    }

    private void SpawnEntities()
    {
        packageTimer += Time.deltaTime;
        if (packageTimer >= packageInterval)
        {
            packageTimer = 0f;
            SpawnPackage();
        }

        virusTimer += Time.deltaTime;
        if (virusTimer >= virusInterval)
        {
            virusTimer = 0f;
            SpawnVirus();
        }
    }

    private void SpawnPackage()
    {
        float y = Random.Range(0.15f, 0.85f);
        Color[] cols = { new Color(0.47f, 0.9f, 0.63f), new Color(0.51f, 0.86f, 1f), new Color(1f, 0.86f, 0.47f) };
        Color c = cols[Random.Range(0, cols.Length)];

        GameObject go = new GameObject("Package");
        go.transform.SetParent(playArea, false);
        Image img = go.AddComponent<Image>();
        img.color = c;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.11f, y - 0.02f);
        rect.anchorMax = new Vector2(0.14f, y + 0.02f);
        rect.sizeDelta = Vector2.zero;

        packages.Add(new PackageData { rect = rect, x = 0.11f, vx = 0.015f + Random.Range(0f, 0.005f), color = c, alive = true });
    }

    private void SpawnVirus()
    {
        float y = Random.Range(0.15f, 0.85f);
        int hp = 2;

        GameObject go = new GameObject("Virus");
        go.transform.SetParent(playArea, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.78f, 0.16f, 0.24f);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.88f, y - 0.025f);
        rect.anchorMax = new Vector2(0.91f, y + 0.025f);
        rect.sizeDelta = Vector2.zero;

        viruses.Add(new VirusData { rect = rect, x = 0.88f, y = y, hp = hp, alive = true, img = img });
    }

    private void UpdateEntities()
    {
        float dt = Time.deltaTime;

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            BulletData b = bullets[i];
            b.x += Mathf.Cos(b.angle) * bulletSpeed * dt * 0.05f;
            b.y += Mathf.Sin(b.angle) * bulletSpeed * dt * 0.05f;
            b.rect.anchorMin = new Vector2(b.x - 0.005f, b.y - 0.01f);
            b.rect.anchorMax = new Vector2(b.x + 0.005f, b.y + 0.01f);

            if (b.x > 1f || b.x < 0f || b.y > 1f || b.y < 0f)
            {
                b.alive = false;
                Destroy(b.rect.gameObject);
            }
            bullets[i] = b;
        }

        for (int i = packages.Count - 1; i >= 0; i--)
        {
            PackageData p = packages[i];
            p.x += p.vx * dt;
            p.rect.anchorMin = new Vector2(p.x, p.rect.anchorMin.y);
            p.rect.anchorMax = new Vector2(p.x + 0.03f, p.rect.anchorMax.y);

            if (p.x >= 0.87f)
            {
                delivered++;
                SpawnParticles(p.x, p.rect.anchorMin.y + 0.02f, p.color, 8);
                p.alive = false;
                Destroy(p.rect.gameObject);
            }
            else if (p.x > 1.05f)
            {
                p.alive = false;
                Destroy(p.rect.gameObject);
            }
            packages[i] = p;
        }

        for (int i = viruses.Count - 1; i >= 0; i--)
        {
            VirusData v = viruses[i];
            v.x -= virusSpeed * dt * 0.03f;
            v.rect.anchorMin = new Vector2(v.x - 0.025f, v.y - 0.025f);
            v.rect.anchorMax = new Vector2(v.x + 0.025f, v.y + 0.025f);

            if (v.x < -0.05f)
            {
                v.alive = false;
                Destroy(v.rect.gameObject);
            }
            viruses[i] = v;
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            BulletData b = bullets[i];
            if (!b.alive) continue;

            for (int j = viruses.Count - 1; j >= 0; j--)
            {
                VirusData v = viruses[j];
                if (!v.alive) continue;

                float dist = Vector2.Distance(new Vector2(b.x, b.y), new Vector2(v.x, v.y));
                if (dist < 0.05f)
                {
                    v.hp--;
                    b.alive = false;
                    Destroy(b.rect.gameObject);

                    if (v.hp <= 0)
                    {
                        v.alive = false;
                        virusesKilled++;
                        SpawnParticles(v.x, v.y, new Color(1f, 0.47f, 0.47f), 12);
                        Destroy(v.rect.gameObject);
                    }
                    else
                    {
                        v.img.color = new Color(1f, 0.6f, 0.6f);
                    }
                    viruses[j] = v;
                    break;
                }
            }
            bullets[i] = b;
        }

        for (int i = packages.Count - 1; i >= 0; i--)
        {
            PackageData p = packages[i];
            if (!p.alive) continue;

            for (int j = viruses.Count - 1; j >= 0; j--)
            {
                VirusData v = viruses[j];
                if (!v.alive) continue;

                float dist = Vector2.Distance(new Vector2(p.x + 0.015f, p.rect.anchorMin.y + 0.02f), new Vector2(v.x, v.y));
                if (dist < 0.05f)
                {
                    p.alive = false;
                    v.alive = false;
                    lost++;
                    SpawnParticles(p.x, p.rect.anchorMin.y, new Color(1f, 0.39f, 0.39f), 10);
                    Destroy(p.rect.gameObject);
                    Destroy(v.rect.gameObject);
                    viruses[j] = v;
                    break;
                }
            }
            packages[i] = p;
        }

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            ParticleData pt = particles[i];
            pt.vx *= 0.95f;
            pt.vy *= 0.95f;
            pt.rect.anchorMin += new Vector2(pt.vx * dt, pt.vy * dt);
            pt.rect.anchorMax = pt.rect.anchorMin + new Vector2(0.008f, 0.008f);
            pt.life -= dt;
            if (pt.life <= 0f)
            {
                Destroy(pt.rect.gameObject);
                particles.RemoveAt(i);
            }
        }
    }

    private void SpawnParticles(float x, float y, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Particle");
            go.transform.SetParent(playArea, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(x - 0.004f, y - 0.004f);
            rect.anchorMax = new Vector2(x + 0.004f, y + 0.004f);
            rect.sizeDelta = Vector2.zero;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float spd = Random.Range(0.02f, 0.06f);
            particles.Add(new ParticleData
            {
                rect = rect,
                vx = Mathf.Cos(angle) * spd,
                vy = Mathf.Sin(angle) * spd,
                life = Random.Range(0.3f, 0.7f),
                img = img
            });
        }
    }

    private void CheckDefeat()
    {
        if (lives <= 0 || lost > maxLost)
        {
            Fail();
        }
    }

    private void UpdateHUD()
    {
        if (hudText != null)
        {
            hudText.text = string.Format("ENTREGADOS {0}/{1}   |   PERDIDOS {2}/{3}   |   VIRUS {4}   |   VIDAS {5}",
                delivered, needed, lost, maxLost, virusesKilled, lives);
        }
    }

    private void OnDestroy()
    {
        if (playArea != null) Destroy(playArea.gameObject);
    }
}
