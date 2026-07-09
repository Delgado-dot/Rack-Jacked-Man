using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelBuilder : EditorWindow
{
    
    private const string PREFAB_PLATFORM = "Assets/Prefabs/Environment/Platform.prefab";
    private const string PREFAB_WALL = "Assets/Prefabs/Environment/Wall.prefab";
    private const string PREFAB_ENEMYBUG = "Assets/Prefabs/Enemies/EnemyBug.prefab";

    
    private static readonly string[] MAPA_NIVEL_1 = new string[]
    {
        "$##################$",
        "$     VR           $",
        "$      # V         $",
        "$                  $",
        "$     #       #    $",
        "$                  $",
        "$        ##  ##    $",
        "$     #            $",
        "$        B         $",
        "$    #             $",
        "$      E           $",
        "$                  $",
        "$   #              $",
        "$       B          $",
        "$A                 $",
        "$###               $",
        "$                  $",
        "$                  $",
        "$                  $",
        "$                  $",
        "$                  $",
        "$                  $",
        "$  H    H   H   H  $"
    };

    private const int TOTAL_FILAS = 23;
    private const int TOTAL_COLUMNAS = 21;
    private const float TILE_SIZE = 2f;
    private const float RACK_SCALE = 2f;

    [MenuItem("Rack-Jacked-Man/Build Level 1")]
    public static void BuildLevel1()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Level 1",
            "Construira el Nivel 1 desde el mapa ASCII original.\n\n" +
            "La escena se reorganizara completamente.\n" +
            "Los Prefabs se crearan si no existen.\n\n" +
            "Seguro ejecutar multiples veces.",
            "Construir", "Cancelar"))
        {
            return;
        }

        EnsurePrefabsExist();
        CleanScene();
        BuildLevelFromMap();

        EditorUtility.DisplayDialog(
            "Build Level 1 - Completado",
            "Nivel 1 construido desde mapa ASCII!\n\n" +
            "Revisa la escena PlayerTest.",
            "OK");

        Debug.Log("[LevelBuilder] Nivel 1 construido desde mapa ASCII.");
    }

    private static void CleanScene()
    {
        string[] oldNames = {
            "Level", "Level_01", "Mapa", "Level_Ground",
            "Cube", "Cube (1)", "Cube (2)",
            "Plataform", "Plataform2", "Finish", "PlayerSpawn",
            "EnemyBug", "EnemyBug2", "EnemyBug_1", "EnemyBug_2",
            "EnemyBug_Cylinder", "EnemyBug2_Cylinder",
            "RackStart", "Checkpoint_Mid", "RackGoal",
            "Platforms", "Walls", "Enemies", "Checkpoints", "Goal", "Spawn"
        };

        foreach (string objName in oldNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    private static void BuildLevelFromMap()
    {
        // Jerarquia organizada
        GameObject levelRoot = new GameObject("Level");
        GameObject platformsParent = CreateChild(levelRoot, "Platforms");
        GameObject wallsParent = CreateChild(levelRoot, "Walls");
        GameObject enemiesParent = CreateChild(levelRoot, "Enemies");

        GameObject platformPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLATFORM);
        GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_WALL);
        GameObject enemyBugPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ENEMYBUG);


        List<PlatformRow> platformRows = new List<PlatformRow>();
        Vector3 spawnPos = Vector3.zero;
        Vector3 goalPos = Vector3.zero;
        Vector3 checkpointPos = Vector3.zero;
        bool hasCheckpoint = false;

        for (int row = 0; row < TOTAL_FILAS; row++)
        {
            string fila = MAPA_NIVEL_1[row];
            PlatformRow currentRow = new PlatformRow(row);

            for (int col = 0; col < TOTAL_COLUMNAS; col++)
            {
                char tile = fila[col];
                Vector3 worldPos = GridToWorld(col, row);

                switch (tile)
                {
                    case '#':
                        currentRow.AddSegment(col);
                        break;

                    case '$':
                        SpawnWall(wallPrefab, worldPos, wallsParent.transform);
                        break;

                    case 'A':
                        spawnPos = worldPos;
                        SpawnRack(spawnPos, "RackStart", new Color(0f, 0.8f, 0f), levelRoot.transform);
                        SpawnPlayerSpawn(spawnPos, levelRoot.transform);
                        break;

                    case 'R':
                        goalPos = worldPos;
                        SpawnRackGoal(goalPos, levelRoot.transform);
                        break;

                    case 'C':
                        checkpointPos = worldPos;
                        hasCheckpoint = true;
                        SpawnCheckpoint(checkpointPos, levelRoot.transform);
                        break;

                    case 'B':
                        SpawnEnemyBug(enemyBugPrefab, worldPos, enemiesParent.transform);
                        break;

                    case 'E':
                        SpawnHazard(worldPos, "Sierra", new Color(0.8f, 0.2f, 0.2f), levelRoot.transform);
                        break;

                    case 'V':
                        SpawnHazard(worldPos, "Sierra_Cae", new Color(0.6f, 0.1f, 0.1f), levelRoot.transform);
                        break;

                    case 'H':
                        SpawnHazard(worldPos, "Laser", new Color(1f, 0f, 0f, 0.5f), levelRoot.transform);
                        break;

                    case 'J':
                        SpawnHazard(worldPos, "Chaqueta", new Color(0.8f, 0.6f, 0f), levelRoot.transform);
                        break;
                }
            }

            platformRows.Add(currentRow);
        }

        foreach (PlatformRow pr in platformRows)
        {
            foreach (var segment in pr.GetSegments())
            {
                SpawnPlatformFused(platformPrefab, segment, platformsParent.transform);
            }
        }


        BuildGameManager();


        UpdateCameraBounds();

        Debug.Log("[LevelBuilder] Spawn: " + spawnPos + " | Meta: " + goalPos +
                  (hasCheckpoint ? " | Checkpoint: " + checkpointPos : " | Sin checkpoint"));
    }

    private static void SpawnRack(Vector3 position, string name, Color color, Transform parent)
    {
        GameObject rack = new GameObject(name);
        rack.transform.SetParent(parent);
        rack.transform.position = position;
        rack.transform.localScale = Vector3.one * RACK_SCALE;

        
        BoxCollider col = rack.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

       
        rack.AddComponent<RackStart>();

        
        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(rack.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;


        BoxCollider modelCol = model.GetComponent<BoxCollider>();
        if (modelCol != null) Object.DestroyImmediate(modelCol);

        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }

    private static void SpawnRackGoal(Vector3 position, Transform parent)
    {
        GameObject rack = new GameObject("RackGoal");
        rack.transform.SetParent(parent);
        rack.transform.position = position;
        rack.transform.localScale = Vector3.one * RACK_SCALE;

        
        BoxCollider col = rack.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

       
        CheckpointTrigger trigger = rack.AddComponent<CheckpointTrigger>();
        SerializedObject so = new SerializedObject(trigger);
        SerializedProperty isGoalProp = so.FindProperty("isGoal");
        if (isGoalProp != null)
        {
            isGoalProp.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(rack.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;

        BoxCollider modelCol = model.GetComponent<BoxCollider>();
        if (modelCol != null) Object.DestroyImmediate(modelCol);

        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.4f, 1f);
            renderer.material = mat;
        }
    }

    private static void SpawnCheckpoint(Vector3 position, Transform parent)
    {
        GameObject checkpoint = new GameObject("Checkpoint");
        checkpoint.transform.SetParent(parent);
        checkpoint.transform.position = position;
        checkpoint.transform.localScale = Vector3.one * RACK_SCALE;

        
        BoxCollider col = checkpoint.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

       
        checkpoint.AddComponent<CheckpointTrigger>();

        
        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(checkpoint.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;

        BoxCollider modelCol = model.GetComponent<BoxCollider>();
        if (modelCol != null) Object.DestroyImmediate(modelCol);

        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.9f, 0.2f);
            renderer.material = mat;
        }
    }

    
    private static void SpawnPlayerSpawn(Vector3 rackPosition, Transform parent)
    {
        GameObject spawn = new GameObject("PlayerSpawn");
        spawn.transform.SetParent(parent);
        // Posicionar justo delante del RackStart (a la derecha)
        spawn.transform.position = rackPosition + Vector3.right * 2.5f;
    }

   
    private static void SpawnPlatformFused(GameObject prefab, PlatformSegment segment, Transform parent)
    {
        if (prefab == null) return;

        float worldX = segment.startCol * TILE_SIZE + (segment.length * TILE_SIZE) / 2f - TILE_SIZE / 2f;
        float worldY = (TOTAL_FILAS - 1 - segment.row) * TILE_SIZE;

        Vector3 position = new Vector3(worldX, worldY, 0f);
        Vector3 scale = new Vector3(segment.length * TILE_SIZE, TILE_SIZE, TILE_SIZE);

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "Platform_R" + segment.row + "_C" + segment.startCol;
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;

       
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = GetPlatformColor(segment.row);
            renderer.material = mat;
        }
    }

    private static Color GetPlatformColor(int row)
    {
        
        if (row >= 14) return new Color(0.35f, 0.35f, 0.35f);   
        if (row >= 8) return new Color(0.45f, 0.3f, 0.2f);      
        return new Color(0.4f, 0.5f, 0.3f);                      
    }

    private static void SpawnWall(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "Wall_R" + Mathf.RoundToInt((TOTAL_FILAS - 1 - position.y / TILE_SIZE));
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * TILE_SIZE;
    }


    private static void SpawnEnemyBug(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.name = "EnemyBug";
        obj.transform.SetParent(parent);
        obj.transform.position = position;

        Debug.Log("[LevelBuilder] EnemyBug en " + position);
    }

 
    private static void SpawnHazard(Vector3 position, string name, Color color, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 1.5f;

     
        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Visual";
        model.transform.SetParent(obj.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;

        BoxCollider modelCol = model.GetComponent<BoxCollider>();
        if (modelCol != null) Object.DestroyImmediate(modelCol);

        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null)
                mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }

      private static void BuildGameManager()
    {
        GameObject existingManager = GameObject.Find("LevelManager");
        if (existingManager != null && existingManager.GetComponent<GameManager>() != null)
            return;

        GameObject managerObj = existingManager != null ? existingManager : new GameObject("LevelManager");
        if (managerObj.GetComponent<GameManager>() == null)
            managerObj.AddComponent<GameManager>();
    }


    private static void UpdateCameraBounds()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
        if (camFollow == null) return;

        SerializedObject so = new SerializedObject(camFollow);

        SerializedProperty useBounds = so.FindProperty("useBounds");
        if (useBounds != null) useBounds.boolValue = true;

      
        float worldWidth = TOTAL_COLUMNAS * TILE_SIZE;
        float worldHeight = TOTAL_FILAS * TILE_SIZE;

        SerializedProperty minX = so.FindProperty("minX");
        if (minX != null) minX.floatValue = -5f;

        SerializedProperty maxX = so.FindProperty("maxX");
        if (maxX != null) maxX.floatValue = worldWidth + 5f;

        SerializedProperty minY = so.FindProperty("minY");
        if (minY != null) minY.floatValue = -2f;

        SerializedProperty maxY = so.FindProperty("maxY");
        if (maxY != null) maxY.floatValue = worldHeight + 2f;

        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[LevelBuilder] Camara: X[-5, " + (worldWidth + 5) + "] Y[-2, " + (worldHeight + 2) + "]");
    }

    private static Vector3 GridToWorld(int col, int row)
    {
        float x = col * TILE_SIZE;
        float y = (TOTAL_FILAS - 1 - row) * TILE_SIZE;
        return new Vector3(x, y, 0f);
    }

    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        return child;
    }

    private struct PlatformSegment
    {
        public int row;
        public int startCol;
        public int length;

        public PlatformSegment(int row, int startCol, int length)
        {
            this.row = row;
            this.startCol = startCol;
            this.length = length;
        }
    }

    private class PlatformRow
    {
        public int row;
        private List<int> columns = new List<int>();

        public PlatformRow(int row) { this.row = row; }

        public void AddSegment(int col) { columns.Add(col); }

        public List<PlatformSegment> GetSegments()
        {
            List<PlatformSegment> segments = new List<PlatformSegment>();
            if (columns.Count == 0) return segments;

            columns.Sort();
            int start = columns[0];
            int prev = columns[0];

            for (int i = 1; i < columns.Count; i++)
            {
                if (columns[i] == prev + 1)
                {
                    prev = columns[i];
                }
                else
                {
                    segments.Add(new PlatformSegment(row, start, prev - start + 1));
                    start = columns[i];
                    prev = columns[i];
                }
            }
            segments.Add(new PlatformSegment(row, start, prev - start + 1));

            return segments;
        }
    }


    private static void EnsurePrefabsExist()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Environment");
        EnsureFolder("Assets/Prefabs/Enemies");

        if (!AssetExists(PREFAB_PLATFORM)) CreatePlatformPrefab();
        if (!AssetExists(PREFAB_WALL)) CreateWallPrefab();
        if (!AssetExists(PREFAB_ENEMYBUG)) CreateEnemyBugPrefab();

        AssetDatabase.Refresh();
    }

    private static void CreatePlatformPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Platform";
        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_PLATFORM);
        Object.DestroyImmediate(obj);
    }

    private static void CreateWallPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Wall";
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null) mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.3f, 0.3f);
            renderer.material = mat;
        }
        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_WALL);
        Object.DestroyImmediate(obj);
    }

    private static void CreateEnemyBugPrefab()
    {
        GameObject obj = new GameObject("EnemyBug");

        GameObject colliderChild = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        colliderChild.name = "Collider";
        colliderChild.transform.SetParent(obj.transform);
        colliderChild.transform.localPosition = Vector3.zero;

        GameObject visualChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualChild.name = "Modelo3D";
        visualChild.transform.SetParent(obj.transform);
        visualChild.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        visualChild.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Renderer vr = visualChild.GetComponent<Renderer>();
        if (vr != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null || mat.shader == null) mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.2f, 0.2f);
            vr.material = mat;
        }

        obj.AddComponent<EnemyBug>();

        PrefabUtility.SaveAsPrefabAsset(obj, PREFAB_ENEMYBUG);
        Object.DestroyImmediate(obj);
    }

    private static bool AssetExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folderName = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
