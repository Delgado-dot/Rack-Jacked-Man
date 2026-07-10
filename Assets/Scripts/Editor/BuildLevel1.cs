using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// BuildLevel1 - Script temporal que construye el Nivel 1 directamente en la escena.
/// Ejecutar UNA VEZ desde: Rack-Jacked-Man > Build Level 1
/// Despues BORRAR este archivo.
/// TODO queda fisicamente en la escena. No hay generacion automatica.
/// </summary>
public class BuildLevel1 : EditorWindow
{
    private const float T = 2f;
    private static int objetosCreados = 0;

    private static readonly string[] MAPA = new string[]
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

    [MenuItem("Rack-Jacked-Man/Build Level 1")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Level 1",
            "Construira el Nivel 1 directamente en la escena.\n" +
            "Despues borra este script.\n\n" +
            "Seguro?",
            "Construir", "Cancelar")) return;

        objetosCreados = 0;
        CleanOldObjects();
        CreateHierarchy();
        CreatePlayerSpawn();

        // === VERIFICACION ===
        int errores = 0;
        errores += VerificarObjeto("Level");
        errores += VerificarObjeto("Level/Platforms");
        errores += VerificarObjeto("Level/Walls");
        errores += VerificarObjeto("Level/Hazards");
        errores += VerificarObjeto("Level/Enemies");
        errores += VerificarObjeto("Level/RackStart");
        errores += VerizarObjetoEn("Level", "RackGoal");
        errores += VerizarObjetoEn("Level", "LevelManager");
        errores += VerizarObjetoEn("Level", "Checkpoints");

        // === GUARDAR ESCENA ===
        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        bool guardado = EditorSceneManager.SaveScene(scene);

        if (!guardado)
        {
            Debug.LogError("[BuildLevel1] ERROR: No se pudo guardar la escena PlayerTest.unity");
        }

        // === REPORTE FINAL ===
        string msg = string.Format(
            "[BuildLevel1] OBJETOS CREADOS: {0} | ERRORES: {1} | ESCENA GUARDADA: {2}",
            objetosCreados, errores, guardado ? "SI" : "NO");
        Debug.Log(msg);

        if (errores > 0)
        {
            EditorUtility.DisplayDialog("Build Level 1",
                string.Format("Completado con {0} errores.\nRevisa la consola.", errores), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Build Level 1",
                string.Format("Nivel 1 construido.\nObjetos: {0}\nEscena guardada.\nBorra BuildLevel1.cs.", objetosCreados), "OK");
        }
    }

    static int VerificarObjeto(string ruta)
    {
        string[] partes = ruta.Split('/');
        GameObject actual = GameObject.Find(partes[0]);
        for (int i = 1; i < partes.Length && actual != null; i++)
        {
            Transform t = actual.transform.Find(partes[i]);
            actual = t != null ? t.gameObject : null;
        }
        if (actual == null)
        {
            Debug.LogError("[BuildLevel1] No se pudo crear: " + ruta);
            return 1;
        }
        return 0;
    }

    static int VerizarObjetoEn(string padre, string nombre)
    {
        GameObject p = GameObject.Find(padre);
        if (p == null)
        {
            Debug.LogError("[BuildLevel1] Padre no encontrado: " + padre);
            return 1;
        }
        Transform t = p.transform.Find(nombre);
        if (t == null)
        {
            Debug.LogError("[BuildLevel1] No se pudo crear: " + padre + "/" + nombre);
            return 1;
        }
        return 0;
    }

    static void CleanOldObjects()
    {
        string[] names = {
            "Level", "Level_01", "Mapa", "Level_Ground", "Cube", "Cube (1)", "Cube (2)",
            "Plataform", "Plataform2", "Finish",
            "EnemyBug", "EnemyBug2", "EnemyBug_1", "EnemyBug_2",
            "EnemyBug_Cylinder", "EnemyBug2_Cylinder",
            "RackStart", "Checkpoint", "RackGoal", "Checkpoint_Mid",
            "Platforms", "Walls", "Enemies", "Hazards", "Checkpoints",
            "Goal", "Spawn", "Decorations", "PlayerSpawn", "LevelManager"
        };
        foreach (string n in names)
        {
            GameObject o = GameObject.Find(n);
            if (o != null) Object.DestroyImmediate(o);
        }
    }

    static void CreateHierarchy()
    {
        GameObject root = new GameObject("Level");
        objetosCreados++;
        GameObject platforms = Child(root, "Platforms"); objetosCreados++;
        GameObject walls = Child(root, "Walls"); objetosCreados++;
        GameObject hazards = Child(root, "Hazards"); objetosCreados++;
        GameObject enemies = Child(root, "Enemies"); objetosCreados++;
        GameObject checkpoints = Child(root, "Checkpoints"); objetosCreados++;

        Vector3 spawnPos = Vector3.zero;

        for (int r = 0; r < 23; r++)
        {
            string row = MAPA[r];
            for (int c = 0; c < 21; c++)
            {
                char ch = row[c];
                Vector3 pos = G2W(c, r);

                switch (ch)
                {
                    case '$':
                        Cube(pos, "Wall", new Color(0.2f, 0.2f, 0.25f), Vector3.one * T, walls.transform);
                        objetosCreados++;
                        break;
                    case 'A':
                        spawnPos = pos;
                        Rack(pos, "RackStart", new Color(0.1f, 0.9f, 0.2f), root.transform);
                        objetosCreados++;
                        break;
                    case 'R':
                        RackGoal(pos, root.transform);
                        objetosCreados++;
                        break;
                    case 'C':
                        Checkpoint(pos, checkpoints.transform);
                        objetosCreados++;
                        break;
                    case 'B':
                        EnemyBug(pos, enemies.transform);
                        objetosCreados++;
                        break;
                    case 'E':
                        Hazard(pos, "Sierra", new Color(0.9f, 0.3f, 0.1f), hazards.transform);
                        objetosCreados++;
                        break;
                    case 'V':
                        Hazard(pos, "Sierra_Cae", new Color(0.7f, 0.1f, 0.1f), hazards.transform);
                        objetosCreados++;
                        break;
                    case 'H':
                        Hazard(pos, "Laser", new Color(1f, 0.1f, 0.1f), hazards.transform);
                        objetosCreados++;
                        break;
                    case 'J':
                        Hazard(pos, "Chaqueta", new Color(1f, 0.8f, 0f), hazards.transform);
                        objetosCreados++;
                        break;
                }
            }
        }

        // Plataformas fusionadas
        for (int r = 0; r < 23; r++)
        {
            string row = MAPA[r];
            int start = -1;
            for (int c = 0; c <= 21; c++)
            {
                char ch = c < 21 ? row[c] : ' ';
                if (ch == '#' && start == -1) start = c;
                else if (ch != '#' && start != -1)
                {
                    int len = c - start;
                    float x = start * T + (len * T) / 2f - T / 2f;
                    float y = (22 - r) * T;
                    Vector3 pos = new Vector3(x, y, 0f);
                    Vector3 scale = new Vector3(len * T, T, T);
                    Color col = r >= 14 ? new Color(0.35f, 0.35f, 0.35f)
                              : r >= 8 ? new Color(0.5f, 0.35f, 0.25f)
                              : new Color(0.4f, 0.5f, 0.35f);
                    Cube(pos, "Platform", col, scale, platforms.transform);
                    objetosCreados++;
                    start = -1;
                }
            }
        }

        // LevelManager
        GameObject gm = new GameObject("LevelManager");
        gm.transform.SetParent(root.transform);
        gm.AddComponent<GameManager>();
        objetosCreados++;
    }

    static void CreatePlayerSpawn()
    {
        GameObject rackStart = GameObject.Find("Level/RackStart");
        if (rackStart == null)
        {
            Debug.LogError("[BuildLevel1] No se encontro RackStart para crear PlayerSpawn");
            return;
        }

        GameObject spawn = new GameObject("PlayerSpawn");
        spawn.transform.position = rackStart.transform.position + Vector3.right * 2.5f;
        objetosCreados++;
    }

    static Vector3 G2W(int c, int r)
    {
        return new Vector3(c * T, (22 - r) * T, 0f);
    }

    static GameObject Child(GameObject p, string n)
    {
        GameObject c = new GameObject(n);
        c.transform.SetParent(p.transform);
        return c;
    }

    // === CUBO GENERICO ===
    static GameObject Cube(Vector3 pos, string name, Color color, Vector3 scale, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        if (parent != null) obj.transform.SetParent(parent);
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = color;
            r.sharedMaterial = m;
        }
        return obj;
    }

    // === RACK START (verde) ===
    static void Rack(Vector3 pos, string name, Color color, Transform parent)
    {
        GameObject rack = new GameObject(name);
        rack.transform.SetParent(parent);
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(2f, 3f, 2f);

        BoxCollider col = rack.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

        rack.AddComponent<RackStart>();

        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(rack.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;
        Object.DestroyImmediate(model.GetComponent<BoxCollider>());
        Renderer r = model.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = color;
            r.sharedMaterial = m;
        }
    }

    // === RACK GOAL (azul) ===
    static void RackGoal(Vector3 pos, Transform parent)
    {
        GameObject rack = new GameObject("RackGoal");
        rack.transform.SetParent(parent);
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(2f, 3f, 2f);

        BoxCollider col = rack.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

        CheckpointTrigger ct = rack.AddComponent<CheckpointTrigger>();
        SerializedObject so = new SerializedObject(ct);
        SerializedProperty p = so.FindProperty("isGoal");
        if (p != null) { p.boolValue = true; so.ApplyModifiedPropertiesWithoutUndo(); }

        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(rack.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;
        Object.DestroyImmediate(model.GetComponent<BoxCollider>());
        Renderer r = model.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = new Color(0.2f, 0.4f, 1f);
            r.sharedMaterial = m;
        }
    }

    // === CHECKPOINT (amarillo) ===
    static void Checkpoint(Vector3 pos, Transform parent)
    {
        GameObject cp = new GameObject("Checkpoint");
        cp.transform.SetParent(parent);
        cp.transform.position = pos;
        cp.transform.localScale = new Vector3(2f, 3f, 2f);

        BoxCollider col = cp.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one;

        cp.AddComponent<CheckpointTrigger>();

        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
        model.name = "Model";
        model.transform.SetParent(cp.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localScale = Vector3.one;
        Object.DestroyImmediate(model.GetComponent<BoxCollider>());
        Renderer r = model.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = new Color(1f, 0.9f, 0.1f);
            r.sharedMaterial = m;
        }
    }

    // === ENEMY BUG ===
    static int bugIndex = 0;
    static string[] bugNames = { "EnemyBug", "EnemyBug2" };

    static void EnemyBug(Vector3 pos, Transform parent)
    {
        // Intentar reutilizar EnemyBug existente
        string bugName = bugIndex < bugNames.Length ? bugNames[bugIndex] : "EnemyBug";
        GameObject existing = GameObject.Find(bugName);

        if (existing != null)
        {
            existing.transform.position = pos;
            if (parent != null) existing.transform.SetParent(parent);
            bugIndex++;
            return;
        }

        // Crear nuevo si no existe
        GameObject obj = new GameObject("EnemyBug_" + (bugIndex + 1));
        obj.transform.SetParent(parent);
        obj.transform.position = pos;

        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.name = "Collider";
        cyl.transform.SetParent(obj.transform);
        cyl.transform.localPosition = Vector3.zero;

        GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vis.name = "Modelo3D";
        vis.transform.SetParent(obj.transform);
        vis.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        vis.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Renderer r = vis.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = new Color(0.8f, 0.2f, 0.2f);
            r.sharedMaterial = m;
        }

        obj.AddComponent<EnemyBug>();
        bugIndex++;
    }

    // === HAZARD (cubo visible) ===
    static void Hazard(Vector3 pos, string name, Color color, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = Vector3.one * 1.2f;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(obj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m == null || m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = color;
            r.sharedMaterial = m;
        }
    }
}
