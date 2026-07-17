using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Configura los racks funcionales en Nivel_2.
/// Agrega RackInteractable, RackState, PuzzleMana, PlayerInteract, ObjectiveDoorController.
/// Ejecutar: Rack-Jacked-Man > Setup Nivel 2 Racks
/// </summary>
[InitializeOnLoad]
public class SetupNivel2Racks
{
    private const string PREFKEY = "SetupNivel2Racks_v1";

    static SetupNivel2Racks()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    [MenuItem("Rack-Jacked-Man/Setup Nivel 2 Racks")]
    public static void ShowWindow()
    {
        Run();
        EditorUtility.DisplayDialog("Listo", "Nivel_2 racks configurados.", "OK");
    }

    private static void AutoSetup()
    {
        if (SessionState.GetBool(PREFKEY, false)) return;
        if (!NeedsSetup())
        {
            SessionState.SetBool(PREFKEY, true);
            return;
        }
        Run();
        SessionState.SetBool(PREFKEY, true);
    }

    private static bool NeedsSetup()
    {
        string full = Application.dataPath + "/../Assets/Scenes/Nivel_2.unity";
        if (!System.IO.File.Exists(full)) return false;
        string[] lines = System.IO.File.ReadAllLines(full);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("m_Name: Rack_N2_05"))
            {
                for (int j = i; j < Mathf.Min(i + 20, lines.Length); j++)
                {
                    if (lines[j].Contains("9a409283bd2894a4db81a7c39c950e6d"))
                        return false;
                }
                return true;
            }
        }
        return false;
    }

    private static void Run()
    {
        Debug.Log("[SetupNivel2Racks] Iniciando...");
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_2.unity", OpenSceneMode.Single);

        SetupRack("Rack_N2_05", 0);
        SetupRack("Rack_N2_12", 1);
        GameObject pmGO = SetupPuzzleManager();
        SetupPlayer();
        SetupDoor();
        WireReferences(pmGO);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupNivel2Racks] Listo.");
    }

    private static void SetupRack(string rackName, int index)
    {
        GameObject rack = GameObject.Find(rackName);
        if (rack == null)
        {
            Debug.LogWarning("[SetupNivel2Racks] No encontrado: " + rackName);
            return;
        }

        RackInteractable interactable = rack.GetComponent<RackInteractable>();
        if (interactable == null)
            interactable = rack.AddComponent<RackInteractable>();

        SerializedObject so = new SerializedObject(interactable);
        so.FindProperty("rackType").enumValueIndex = 0;
        so.FindProperty("rackIndex").intValue = index;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (rack.GetComponent<RackState>() == null)
            rack.AddComponent<RackState>();

        Debug.Log("[SetupNivel2Racks] " + rackName + " configurado (index=" + index + ")");
    }

    private static GameObject SetupPuzzleManager()
    {
        GameObject go = GameObject.Find("PuzzleManager_N2");
        if (go == null)
        {
            go = new GameObject("PuzzleManager_N2");
            go.transform.position = Vector3.zero;
        }

        if (go.GetComponent<PuzzleMana>() == null)
            go.AddComponent<PuzzleMana>();

        if (go.GetComponent<PythonPuzzleManager>() == null)
            go.AddComponent<PythonPuzzleManager>();

        Debug.Log("[SetupNivel2Racks] PuzzleManager_N2 creado");
        return go;
    }

    private static void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SetupNivel2Racks] Player no encontrado");
            return;
        }

        if (player.GetComponent<PlayerMode>() == null)
            player.AddComponent<PlayerMode>();

        if (player.GetComponent<PlayerInteract>() == null)
            player.AddComponent<PlayerInteract>();

        Debug.Log("[SetupNivel2Racks] Player configurado");
    }

    private static void SetupDoor()
    {
        GameObject door = GameObject.Find("Trigger_SalidaNivel2");
        if (door == null)
        {
            Debug.LogWarning("[SetupNivel2Racks] Trigger_SalidaNivel2 no encontrado");
            return;
        }

        if (door.GetComponent<ObjectiveDoorController>() == null)
            door.AddComponent<ObjectiveDoorController>();

        Debug.Log("[SetupNivel2Racks] Trigger_SalidaNivel2 configurado");
    }

    private static void WireReferences(GameObject pmGO)
    {
        RackInteractable rack05 = GetRack("Rack_N2_05");
        RackInteractable rack12 = GetRack("Rack_N2_12");
        ObjectiveDoorController door = GetDoor();
        PuzzleMana puzzleMana = pmGO != null ? pmGO.GetComponent<PuzzleMana>() : null;
        PythonPuzzleManager pythonPM = pmGO != null ? pmGO.GetComponent<PythonPuzzleManager>() : null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMode playerMode = player != null ? player.GetComponent<PlayerMode>() : null;

        if (rack05 != null) WireRack(rack05, door, puzzleMana);
        if (rack12 != null) WireRack(rack12, door, puzzleMana);
        if (puzzleMana != null) WirePuzzleMana(puzzleMana, pythonPM, playerMode);

        Debug.Log("[SetupNivel2Racks] Referencias conectadas");
    }

    private static void WireRack(RackInteractable rack, ObjectiveDoorController door, PuzzleMana puzzleMana)
    {
        SerializedObject so = new SerializedObject(rack);
        if (door != null)
            so.FindProperty("doorController").objectReferenceValue = door;
        if (puzzleMana != null)
            so.FindProperty("puzzleMana").objectReferenceValue = puzzleMana;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WirePuzzleMana(PuzzleMana pm, PythonPuzzleManager pythonPM, PlayerMode playerMode)
    {
        SerializedObject so = new SerializedObject(pm);
        if (pythonPM != null)
            so.FindProperty("pythonPuzzleManager").objectReferenceValue = pythonPM;
        if (playerMode != null)
            so.FindProperty("playerMode").objectReferenceValue = playerMode;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RackInteractable GetRack(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<RackInteractable>() : null;
    }

    private static ObjectiveDoorController GetDoor()
    {
        GameObject go = GameObject.Find("Trigger_SalidaNivel2");
        return go != null ? go.GetComponent<ObjectiveDoorController>() : null;
    }
}
