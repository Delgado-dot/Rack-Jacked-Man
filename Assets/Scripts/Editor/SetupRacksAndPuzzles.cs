using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Agrega los racks (cubos) y managers de puzzles a la escena PlayerTest.
/// Ejecutar desde: Rack-Jacked-Man > Setup Racks & Puzzles
/// </summary>
public class SetupRacksAndPuzzles : EditorWindow
{
    [MenuItem("Rack-Jacked-Man/Setup Racks & Puzzles")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Racks & Puzzles",
            "Agregara racks (Checkpoint + Final) y managers de puzzles\n" +
            "a la escena activa.\n\nSeguro?",
            "Crear", "Cancelar")) return;

        SceneSetup();
    }

    static void SceneSetup()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        int count = 0;

        // --- Managers (si no existen) ---
        GameObject managers = FindOrCreateParent("Managers");

        GameObject puzzleManaGO = GameObject.Find("PuzzleMana");
        if (puzzleManaGO == null)
        {
            puzzleManaGO = new GameObject("PuzzleMana");
            puzzleManaGO.transform.SetParent(managers.transform);
            count++;
        }
        PuzzleMana puzzleMana = puzzleManaGO.GetComponent<PuzzleMana>();
        if (puzzleMana == null) puzzleMana = puzzleManaGO.AddComponent<PuzzleMana>();

        GameObject pythonGO = GameObject.Find("PythonPuzzleManager");
        if (pythonGO == null)
        {
            pythonGO = new GameObject("PythonPuzzleManager");
            pythonGO.transform.SetParent(managers.transform);
            count++;
        }
        PythonPuzzleManager pythonMgr = pythonGO.GetComponent<PythonPuzzleManager>();
        if (pythonMgr == null) pythonMgr = pythonGO.AddComponent<PythonPuzzleManager>();

        // --- PlayerMode en el Player ---
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerMode pm = player.GetComponent<PlayerMode>();
            if (pm == null)
            {
                pm = player.AddComponent<PlayerMode>();
                count++;
                Debug.Log("[SetupRacks] PlayerMode agregado al Player");
            }
            puzzleMana.playerMode = pm;
        }
        else
        {
            Debug.LogWarning("[SetupRacks] No se encontro 'Player' en la escena. Asigna PlayerMode manualmente.");
        }

        puzzleMana.pythonPuzzleManager = pythonMgr;

        // --- RackCheckpoint (cubo) ---
        GameObject interactables = FindOrCreateParent("Interactables");

        GameObject rackC = CreateRackCube("Rack_C", RackController.TipoRack.Checkpoint,
            new Vector3(0f, 1f, 20f), interactables.transform);
        rackC.GetComponent<BoxCollider>().isTrigger = true;
        count++;

        // --- RackFinal (cubo) ---
        GameObject rackF = CreateRackCube("Rack_F", RackController.TipoRack.Final,
            new Vector3(0f, 1f, 80f), interactables.transform);
        rackF.GetComponent<BoxCollider>().isTrigger = true;
        count++;

        // --- Conectar triggers ---
        RackTrigger triggerC = rackC.GetComponent<RackTrigger>();
        if (triggerC != null)
        {
            triggerC.puzzleMana = puzzleMana;
            triggerC.rackController = rackC.GetComponent<RackController>();
        }

        RackTrigger triggerF = rackF.GetComponent<RackTrigger>();
        if (triggerF != null)
        {
            triggerF.puzzleMana = puzzleMana;
            triggerF.rackController = rackF.GetComponent<RackController>();
        }

        // --- Marcar escena como modificada ---
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log(string.Format(
            "[SetupRacks] Listo. Objetos creados/reutilizados: {0}\n" +
            "Rack_C (Checkpoint) en z=20\n" +
            "Rack_F (Final) en z=80\n" +
            "PuzzleMana + PythonPuzzleManager en Managers",
            count));

        EditorUtility.DisplayDialog("Setup Racks & Puzzles",
            string.Format("Racks y managers agregados a la escena.\nObjetos: {0}\n\n" +
            "Recuerda:\n- Ajusta posiciones Z de los racks\n" +
            "- Asegurate de que el Player tenga la etiqueta 'Player'",
            count), "OK");
    }

    static GameObject CreateRackCube(string name, RackController.TipoRack tipo, Vector3 pos, Transform parent)
    {
        GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rack.name = name;
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(2f, 2f, 2f);
        rack.tag = "Untagged";

        if (parent != null)
            rack.transform.SetParent(parent);

        Renderer rend = rack.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));

        if (tipo == RackController.TipoRack.Checkpoint)
            mat.color = new Color(0.2f, 0.6f, 1f);
        else
            mat.color = new Color(1f, 0.8f, 0.1f);

        rend.sharedMaterial = mat;

        rack.AddComponent<BoxCollider>();

        RackController rc = rack.AddComponent<RackController>();
        rc.tipoRack = tipo;

        rack.AddComponent<RackTrigger>();

        return rack;
    }

    static GameObject FindOrCreateParent(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
        }
        return go;
    }
}
