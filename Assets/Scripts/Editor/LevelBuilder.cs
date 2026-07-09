using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// LevelBuilder - Construye el Nivel 1 definitivo de Rack-Jacked-Man.
/// Ejecutar desde: Rack-Jacked-Man > Build Level 1.
///
/// Crea Prefabs si no existen, luego los usa para construir el nivel.
/// Organiza la jerarquia: Level/Platforms, Walls, Enemies, Checkpoints, Goal, Spawn.
/// Seguro ejecutar multiples veces.
/// </summary>
public class LevelBuilder : EditorWindow
{
    // Rutas de los Prefabs
    private const string PREFAB_PLATFORM = "Assets/Prefabs/Environment/Platform.prefab";
    private const string PREFAB_WALL = "Assets/Prefabs/Environment/Wall.prefab";
    private const string PREFAB_CHECKPOINT = "Assets/Prefabs/Environment/Checkpoint.prefab";
    private const string PREFAB_RACKSTART = "Assets/Prefabs/Environment/RackStart.prefab";
    private const string PREFAB_RACKGOAL = "Assets/Prefabs/Environment/RackGoal.prefab";
    private const string PREFAB_ENEMYBUG = "Assets/Prefabs/Enemies/EnemyBug.prefab";

    [MenuItem("Rack-Jacked-Man/Build Level 1")]
    public static void BuildLevel1()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Level 1",
            "Construira el Nivel 1 definitivo usando Prefabs.\n\n" +
            "La escena se reorganizara completamente.\n" +
            "Los Prefabs se crearan si no existen.\n\n" +
            "Seguro ejecutar multiples veces.",
            "Construir", "Cancelar"))
        {
            return;
        }

        EnsurePrefabsExist();
        CleanScene();
        BuildLevel();
        UpdateCameraBounds();

        EditorUtility.DisplayDialog(
            "Build Level 1 - Completado",
            "Nivel 1 construido!\n\n" +
            "Revisa la escena PlayerTest en el Hierarchy.",
            "OK");

        Debug.Log("[LevelBuilder] Nivel 1 construido exitosamente.");
    }

    // =========================================================
    // LIMPIAR ESCENA
    // =========================================================
    private static void CleanScene()
    {
        // Eliminar objetos de nivel anterior si existen
        string[] oldNames = {
            "Level_01", "Mapa", "Level_Ground", "Cube", "Cube (1)", "Cube (2)",
            "Plataform", "Plataform2", "Finish", "PlayerSpawn",
            "EnemyBug", "EnemyBug2", "EnemyBug_Cylinder", "EnemyBug2_Cylinder",
            "RackStart", "Checkpoint_Mid", "RackGoal",
            "Level"
        };

        foreach (string objName in oldNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
                Debug.Log("[LevelBuilder] Eliminado: " + objName);
            }
        }
    }

    // =========================================================
    // CONSTRUIR NIVEL COMPLETO
    // =========================================================
    private static void BuildLevel()
    {
        // Crear jerarquia organizada
        GameObject levelRoot = new GameObject("Level");
        GameObject platformsParent = CreateChild(levelRoot, "Platforms");
        GameObject wallsParent = CreateChild(levelRoot, "Walls");
        GameObject enemiesParent = CreateChild(levelRoot, "Enemies");
        GameObject checkpointsParent = CreateChild(levelRoot, "Checkpoints");
        GameObject goalParent = CreateChild(levelRoot, "Goal");
        GameObject spawnParent = CreateChild(levelRoot, "Spawn");

        // Cargar Prefabs
        GameObject platformPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLATFORM);
        GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_WALL);
        GameObject checkpointPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
        GameObject rackStartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_RACKSTART);
        GameObject rackGoalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_RACKGOAL);
        GameObject enemyBugPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ENEMYBUG);

        // --- SPAWN ---
        SpawnRackStart(rackStartPrefab, spawnParent.transform);

        // --- PLATAFORMAS ---
        SpawnPlatforms(platformPrefab, platformsParent.transform);

        // --- PAREDES ---
        SpawnWalls(wallPrefab, wallsParent.transform);

        // --- CHECKPOINT ---
        SpawnCheckpoint(checkpointPrefab, checkpointsParent.transform);

        // --- META ---
        SpawnRackGoal(rackGoalPrefab, goalParent.transform);

        // --- ENEMIGOS ---
        SpawnEnemies(enemyBugPrefab, enemiesParent.transform);

        // --- GAME MANAGER ---
        BuildGameManager();

        Debug.Log("[LevelBuilder] Nivel completo construido.");
    }

    // =========================================================
    // SPAWN: RackStart
    // =========================================================
    private static void SpawnRackStart(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab RackStart no encontrado."); return; }

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "RackStart";
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(0f, 0.5f, 0f);
        obj.transform.localScale = new Vector3(1.5f, 2f, 1.5f);

        // Modelo vacio para FBX futuro
        CreateEmptyModel(obj.transform, new Color(0f, 0.8f, 0f, 0.3f));

        Debug.Log("[LevelBuilder] RackStart creado.");
    }

    // =========================================================
    // PLATAFORMAS - Nivel 1 definitivo
    // =========================================================
    private static void SpawnPlatforms(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab Platform no encontrado."); return; }

        // Nivel 1: Plataformas con curva de dificultad
        // Zona inicial -> Saltos pequenos -> Saltos medianos -> Checkpoint
        // -> Saltos grandes -> Plataformas elevadas -> Meta

        PlatformData[] levelData = new PlatformData[]
        {
            // === ZONA INICIAL ===
            new PlatformData("Ground_Start",    new Vector3(0f, -0.5f, 0f),     new Vector3(12f, 1f, 5f),    new Color(0.35f, 0.35f, 0.35f)),

            // === SALTOS PEQUENOS ===
            new PlatformData("Jump_Small_1",    new Vector3(8f, 0.5f, 0f),      new Vector3(3f, 0.8f, 3f),   new Color(0.45f, 0.3f, 0.2f)),
            new PlatformData("Jump_Small_2",    new Vector3(12f, 1.2f, 0f),     new Vector3(2.5f, 0.8f, 3f), new Color(0.5f, 0.35f, 0.2f)),
            new PlatformData("Jump_Small_3",    new Vector3(15.5f, 0.8f, 0f),   new Vector3(3f, 0.8f, 3f),   new Color(0.45f, 0.3f, 0.2f)),

            // === SALTOS MEDIOS ===
            new PlatformData("Jump_Medium_1",   new Vector3(19.5f, 1.8f, 0f),   new Vector3(2.5f, 0.8f, 3f), new Color(0.4f, 0.45f, 0.3f)),
            new PlatformData("Jump_Medium_2",   new Vector3(23f, 2.8f, 0f),     new Vector3(3f, 0.8f, 3f),   new Color(0.35f, 0.5f, 0.35f)),
            new PlatformData("Jump_Medium_3",   new Vector3(27f, 2.2f, 0f),     new Vector3(2.5f, 0.8f, 3f), new Color(0.4f, 0.45f, 0.3f)),

            // === ZONA CHECKPOINT ===
            new PlatformData("Checkpoint_Ground", new Vector3(31f, 1.5f, 0f),   new Vector3(8f, 1f, 5f),     new Color(0.3f, 0.4f, 0.5f)),

            // === SALTOS GRANDES (post-checkpoint) ===
            new PlatformData("Jump_Big_1",      new Vector3(37f, 3f, 0f),       new Vector3(2.5f, 0.8f, 3f), new Color(0.5f, 0.3f, 0.3f)),
            new PlatformData("Jump_Big_2",      new Vector3(41f, 4.5f, 0f),     new Vector3(2f, 0.8f, 3f),   new Color(0.55f, 0.25f, 0.25f)),
            new PlatformData("Jump_Big_3",      new Vector3(44.5f, 3.5f, 0f),   new Vector3(2.5f, 0.8f, 3f), new Color(0.5f, 0.3f, 0.3f)),

            // === PLATAFORMAS ELEVADAS ===
            new PlatformData("Elevated_1",      new Vector3(48.5f, 5f, 0f),     new Vector3(3f, 0.8f, 3f),   new Color(0.55f, 0.4f, 0.2f)),
            new PlatformData("Elevated_2",      new Vector3(52.5f, 6.5f, 0f),   new Vector3(2.5f, 0.8f, 3f), new Color(0.6f, 0.45f, 0.2f)),
            new PlatformData("Elevated_3",      new Vector3(56f, 5.5f, 0f),     new Vector3(3f, 0.8f, 3f),   new Color(0.55f, 0.4f, 0.2f)),

            // === ZONA FINAL (descenso hacia la meta) ===
            new PlatformData("Final_Descent_1", new Vector3(60f, 4f, 0f),       new Vector3(2.5f, 0.8f, 3f), new Color(0.5f, 0.4f, 0.3f)),
            new PlatformData("Final_Descent_2", new Vector3(63.5f, 2.5f, 0f),   new Vector3(3f, 0.8f, 3f),   new Color(0.5f, 0.4f, 0.3f)),

            // === META ===
            new PlatformData("Goal_Ground",     new Vector3(68f, 1.5f, 0f),     new Vector3(10f, 1f, 5f),    new Color(0.6f, 0.55f, 0.2f)),
        };

        foreach (PlatformData data in levelData)
        {
            SpawnPlatform(prefab, data, parent);
        }
    }

    private static void SpawnPlatform(GameObject prefab, PlatformData data, Transform parent)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = data.name;
        obj.transform.SetParent(parent);
        obj.transform.position = data.position;
        obj.transform.localScale = data.scale;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = data.color;
            renderer.material = mat;
        }
    }

    // =========================================================
    // PAREDES
    // =========================================================
    private static void SpawnWalls(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab Wall no encontrado."); return; }

        // Pared izquierda (limite del jugador)
        SpawnWall(prefab, "Wall_Left", new Vector3(-8f, 4f, 0f), new Vector3(1f, 12f, 5f), parent);

        // Pared derecha despues de la meta
        SpawnWall(prefab, "Wall_Right", new Vector3(75f, 4f, 0f), new Vector3(1f, 12f, 5f), parent);
    }

    private static void SpawnWall(GameObject prefab, string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
    }

    // =========================================================
    // CHECKPOINT (mitad del nivel)
    // =========================================================
    private static void SpawnCheckpoint(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab Checkpoint no encontrado."); return; }

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "Checkpoint_Mid";
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(31f, 3f, 0f);
        obj.transform.localScale = new Vector3(1.5f, 3f, 1.5f);

        // Modelo vacio para FBX futuro
        CreateEmptyModel(obj.transform, new Color(0f, 0.5f, 1f, 0.3f));

        Debug.Log("[LevelBuilder] Checkpoint_Mid creado.");
    }

    // =========================================================
    // META (RackGoal)
    // =========================================================
    private static void SpawnRackGoal(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab RackGoal no encontrado."); return; }

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "RackGoal";
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(68f, 3.5f, 0f);
        obj.transform.localScale = new Vector3(1.5f, 3f, 1.5f);

        // Modelo vacio para FBX futuro
        CreateEmptyModel(obj.transform, new Color(1f, 0.84f, 0f, 0.3f));

        Debug.Log("[LevelBuilder] RackGoal creado.");
    }

    // =========================================================
    // ENEMIGOS (2 EnemyBug)
    // =========================================================
    private static void SpawnEnemies(GameObject prefab, Transform parent)
    {
        if (prefab == null) { Debug.LogWarning("[LevelBuilder] Prefab EnemyBug no encontrado."); return; }

        // EnemyBug 1: primera mitad (zona de saltos medios)
        SpawnEnemy(prefab, "EnemyBug_1", new Vector3(22f, 4.5f, 0f), parent);

        // EnemyBug 2: segunda mitad (zona de plataformas elevadas)
        SpawnEnemy(prefab, "EnemyBug_2", new Vector3(50f, 7.5f, 0f), parent);
    }

    private static void SpawnEnemy(GameObject prefab, string name, Vector3 position, Transform parent)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = position;

        Debug.Log("[LevelBuilder] " + name + " creado en " + position);
    }

    // =========================================================
    // GAME MANAGER
    // =========================================================
    private static void BuildGameManager()
    {
        GameObject existingManager = GameObject.Find("LevelManager");
        if (existingManager != null)
        {
            if (existingManager.GetComponent<GameManager>() != null)
            {
                Debug.Log("[LevelBuilder] GameManager ya existe.");
                return;
            }
        }

        GameObject managerObj = existingManager != null ? existingManager : new GameObject("LevelManager");
        if (managerObj.GetComponent<GameManager>() == null)
        {
            managerObj.AddComponent<GameManager>();
        }

        Debug.Log("[LevelBuilder] GameManager configurado.");
    }

    // =========================================================
    // UTILIDADES
    // =========================================================
    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        return child;
    }

    private static void CreateEmptyModel(Transform parent, Color color)
    {
        GameObject model = new GameObject("Model");
        model.transform.SetParent(parent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;

        // Placeholder visual: cubo semitransparente
        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholder.name = "Placeholder";
        placeholder.transform.SetParent(model.transform);
        placeholder.transform.localPosition = Vector3.zero;
        placeholder.transform.localScale = Vector3.one;

        // Eliminar collider del placeholder (el collider esta en el padre)
        BoxCollider placeholderCol = placeholder.GetComponent<BoxCollider>();
        if (placeholderCol != null) Object.DestroyImmediate(placeholderCol);

        Renderer renderer = placeholder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = CreateTransparentMaterial(color);
            renderer.material = mat;
        }
    }

    // =========================================================
    // CAMARA
    // =========================================================
    private static void UpdateCameraBounds()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) { Debug.LogWarning("[LevelBuilder] Main Camera no encontrada."); return; }

        CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
        if (camFollow == null) { Debug.LogWarning("[LevelBuilder] CameraFollow no encontrado."); return; }

        SerializedObject so = new SerializedObject(camFollow);

        SerializedProperty useBounds = so.FindProperty("useBounds");
        if (useBounds != null) useBounds.boolValue = true;

        SerializedProperty minX = so.FindProperty("minX");
        if (minX != null) minX.floatValue = -8f;

        SerializedProperty maxX = so.FindProperty("maxX");
        if (maxX != null) maxX.floatValue = 75f;

        SerializedProperty minY = so.FindProperty("minY");
        if (minY != null) minY.floatValue = -2f;

        SerializedProperty maxY = so.FindProperty("maxY");
        if (maxY != null) maxY.floatValue = 15f;

        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[LevelBuilder] Camara actualizada: X[-8, 75] Y[-2, 15]");
    }

    // =========================================================
    // CREACION DE PREFABS
    // =========================================================
    private static void EnsurePrefabsExist()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Environment");
        EnsureFolder("Assets/Prefabs/Enemies");
        EnsureFolder("Assets/Prefabs/Player");

        if (!AssetExists(PREFAB_PLATFORM)) CreatePlatformPrefab();
        if (!AssetExists(PREFAB_WALL)) CreateWallPrefab();
        if (!AssetExists(PREFAB_CHECKPOINT)) CreateCheckpointPrefab();
        if (!AssetExists(PREFAB_RACKSTART)) CreateRackStartPrefab();
        if (!AssetExists(PREFAB_RACKGOAL)) CreateRackGoalPrefab();
        if (!AssetExists(PREFAB_ENEMYBUG)) CreateEnemyBugPrefab();

        AssetDatabase.Refresh();
    }

    private static void CreatePlatformPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Platform";
        SetMaterial(obj, new Color(0.4f, 0.4f, 0.4f));
        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_PLATFORM);
        Object.DestroyImmediate(obj);
    }

    private static void CreateWallPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Wall";
        SetMaterial(obj, new Color(0.3f, 0.3f, 0.3f));
        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_WALL);
        Object.DestroyImmediate(obj);
    }

    private static void CreateCheckpointPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Checkpoint";

        // Hitbox como trigger
        BoxCollider col = obj.GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        // Eliminar MeshCollider
        MeshCollider meshCol = obj.GetComponent<MeshCollider>();
        if (meshCol != null) Object.DestroyImmediate(meshCol);

        // Material transparente
        SetTransparentMaterial(obj, new Color(0f, 0.5f, 1f, 0.3f));

        // Logica
        obj.AddComponent<CheckpointTrigger>();

        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_CHECKPOINT);
        Object.DestroyImmediate(obj);
    }

    private static void CreateRackStartPrefab()
    {
        GameObject obj = new GameObject("RackStart");

        // Hitbox
        BoxCollider col = obj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1f, 1f, 1f);

        // Logica
        obj.AddComponent<RackStart>();

        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_RACKSTART);
        Object.DestroyImmediate(obj);
    }

    private static void CreateRackGoalPrefab()
    {
        GameObject obj = new GameObject("RackGoal");

        // Hitbox
        BoxCollider col = obj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1f, 1f, 1f);

        // Logica
        CheckpointTrigger trigger = obj.AddComponent<CheckpointTrigger>();
        SerializedObject so = new SerializedObject(trigger);
        SerializedProperty isGoalProp = so.FindProperty("isGoal");
        if (isGoalProp != null)
        {
            isGoalProp.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_RACKGOAL);
        Object.DestroyImmediate(obj);
    }

    private static void CreateEnemyBugPrefab()
    {
        // Padre con EnemyBug.cs
        GameObject obj = new GameObject("EnemyBug");

        // Collider (cilindro hitbox)
        GameObject colliderChild = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        colliderChild.name = "Collider";
        colliderChild.transform.SetParent(obj.transform);
        colliderChild.transform.localPosition = Vector3.zero;
        colliderChild.transform.localScale = new Vector3(1f, 1f, 1f);

        // Modelo3D placeholder
        GameObject visualChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualChild.name = "Modelo3D";
        visualChild.transform.SetParent(obj.transform);
        visualChild.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        visualChild.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        SetMaterial(visualChild, new Color(0.8f, 0.2f, 0.2f));

        // Logica en el padre
        obj.AddComponent<EnemyBug>();

        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_ENEMYBUG);
        Object.DestroyImmediate(obj);
    }

    // =========================================================
    // UTILIDADES MATERIALES
    // =========================================================
    private static void SetMaterial(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null || mat.shader == null)
            mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;
    }

    private static void SetTransparentMaterial(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = CreateTransparentMaterial(color);
        renderer.material = mat;
    }

    private static Material CreateTransparentMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null || mat.shader == null)
            mat = new Material(Shader.Find("Standard"));

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = color;

        return mat;
    }

    // =========================================================
    // UTILIDADES GENERALES
    // =========================================================
    private static bool AssetExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folderName = System.IO.Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }

    // =========================================================
    // ESTRUCTURA DE DATOS
    // =========================================================
    private struct PlatformData
    {
        public string name;
        public Vector3 position;
        public Vector3 scale;
        public Color color;

        public PlatformData(string name, Vector3 position, Vector3 scale, Color color)
        {
            this.name = name;
            this.position = position;
            this.scale = scale;
            this.color = color;
        }
    }
}
