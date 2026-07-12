using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupRacksAndPuzzles : EditorWindow
{
    [MenuItem("Rack-Jacked-Man/Setup Racks & Puzzles")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Racks & Puzzles",
            "Agregara racks y managers de puzzles\na la escena activa.\n\nSeguro?",
            "Crear", "Cancelar")) return;

        SceneSetup();
    }

    static void SceneSetup()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        int count = 0;

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

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerMode pm = player.GetComponent<PlayerMode>();
            if (pm == null)
            {
                pm = player.AddComponent<PlayerMode>();
                count++;
            }
            puzzleMana.playerMode = pm;
        }

        puzzleMana.pythonPuzzleManager = pythonMgr;

        GameObject interactables = FindOrCreateParent("Interactables");

        GameObject rackC = CreateRack("Rack_C", RackInteractable.RackType.Checkpoint,
            new Vector3(0f, 1f, 20f), interactables.transform);
        count++;

        GameObject rackF = CreateRack("Rack_F", RackInteractable.RackType.Final,
            new Vector3(0f, 1f, 80f), interactables.transform);
        count++;

        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[SetupRacks] Listo. Objetos creados: " + count);

        EditorUtility.DisplayDialog("Setup Racks & Puzzles",
            "Racks y managers agregados a la escena.\nObjetos: " + count, "OK");
    }

    static GameObject CreateRack(string name, RackInteractable.RackType tipo, Vector3 pos, Transform parent)
    {
        GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rack.name = name;
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(2f, 2f, 2f);

        if (parent != null)
            rack.transform.SetParent(parent);

        Renderer rend = rack.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));

        if (tipo == RackInteractable.RackType.Checkpoint)
            mat.color = new Color(0.2f, 0.6f, 1f);
        else
            mat.color = new Color(1f, 0.8f, 0.1f);

        rend.sharedMaterial = mat;

        rack.AddComponent<BoxCollider>();
        rack.AddComponent<RackInteractable>();

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
