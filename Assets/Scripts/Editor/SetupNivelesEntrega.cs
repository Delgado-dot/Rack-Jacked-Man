using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Editor script que adapta Nivel_2 y Nivel_3 de Samuel a la arquitectura de Cambio.
/// Clona Player y Camera de Nivel_1. Corrige flujo automaticamente.
/// </summary>
[InitializeOnLoad]
public class SetupNivelesEntrega : EditorWindow
{
    private const string SRC = "Assets/Scenes/Nivel_1.unity";
    private const string PREFKEY_FLOW_FIXED = "SetupNiveles_FlowFixed_v2";

    // ═══════════════════════════════════════════════════════════════
    // EJECUCION AUTOMATICA AL ABRIR UNITY
    // ═══════════════════════════════════════════════════════════════
    static SetupNivelesEntrega()
    {
        EditorApplication.delayCall += AutoFixFlow;
    }

    private static void AutoFixFlow()
    {
        if (SessionState.GetBool(PREFKEY_FLOW_FIXED, false)) return;

        bool needsNivel1 = NeedsFlowFixNivel1();
        bool needsNivel2 = NeedsFlowFixNivel2();

        if (!needsNivel1 && !needsNivel2)
        {
            SessionState.SetBool(PREFKEY_FLOW_FIXED, true);
            return;
        }

        Debug.Log("[Setup] Auto-fix: aplicando correccion de flujo...");
        if (needsNivel1) FixNivel1Flow();
        if (needsNivel2) FixNivel2Flow();
        SessionState.SetBool(PREFKEY_FLOW_FIXED, true);
        Debug.Log("[Setup] Auto-fix: flujo corregido.");
    }

    private static bool NeedsFlowFixNivel1()
    {
        string full = Application.dataPath + "/../" + "Assets/Scenes/Nivel_1.unity";
        if (!File.Exists(full)) return false;
        string[] lines = File.ReadAllLines(full);
        bool insidePuntoFinal = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("m_Name: PuntoFinal")) insidePuntoFinal = true;
            if (insidePuntoFinal && lines[i].Trim() == "m_IsActive: 1") return true;
            if (insidePuntoFinal && lines[i].Trim() == "m_IsActive: 0") return false;
            if (insidePuntoFinal && lines[i].StartsWith("--- !u!")) return false;
        }
        return false;
    }

    private static bool NeedsFlowFixNivel2()
    {
        string full = Application.dataPath + "/../" + "Assets/Scenes/Nivel_2.unity";
        if (!File.Exists(full)) return false;
        string[] lines = File.ReadAllLines(full);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("escenaDestino: Nivel_3")) return true;
            if (lines[i].Contains("nombreEscena: Nivel_3")) return true;
        }
        return false;
    }

    // FileIDs del Player en Nivel_1
    private static readonly HashSet<long> PlayerIDs = new HashSet<long>
    {
        500000100, 500000101, 500000103, 500000104, 500000105,
        500000110, 500000111,
        500000120, 500000121,
        500000130, 500000131,
        1899228408, 1899228409,
        960945012,
        563622406, 563622407, 563622408
    };

    // FileIDs de la Camera en Nivel_1
    private static readonly HashSet<long> CameraIDs = new HashSet<long>
    {
        500000200, 500000201, 500000202, 500000203,
        500000204, 500000205, 500000206, 500000207
    };

    [MenuItem("Rack-Jacked-Man/Setup Niveles Entrega")]
    public static void ShowWindow()
    {
        GetWindow<SetupNivelesEntrega>("Setup Niveles Entrega");
    }

    private void OnGUI()
    {
        GUILayout.Label("Adaptar Nivel_2 y Nivel_3 de Samuel", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("Clona Player y Camera de Nivel_1.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Fix Nivel_2", GUILayout.Height(35)))
            FixNivel2();

        GUILayout.Space(5);

        if (GUILayout.Button("2. Fix Nivel_3", GUILayout.Height(35)))
            FixNivel3();

        GUILayout.Space(10);

        if (GUILayout.Button("CONFIGURAR TODO", GUILayout.Height(45)))
        {
            FixNivel2();
            FixNivel3();
            EditorUtility.DisplayDialog("Listo", "Nivel_2 y Nivel_3 configurados.", "OK");
        }

        GUILayout.Space(15);
        GUILayout.Label("CORRECCION DE FLUJO", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("3. Fix Flujo (Nivel_1 + Nivel_2)", GUILayout.Height(35)))
        {
            FixNivel1Flow();
            FixNivel2Flow();
            EditorUtility.DisplayDialog("Listo", "Flujo corregido.\nNivel_1: PuntoFinal deshabilitado.\nNivel_2: Trigger_SalidaNivel2 -> SubCable01_Copy.", "OK");
        }

        GUILayout.Space(10);
        GUILayout.Label("Flujo: Nivel_1 -> SubCable -> Nivel_2 -> SubCable -> Nivel_3 -> Victoria", EditorStyles.wordWrappedMiniLabel);
    }

    // ═══════════════════════════════════════════════════════════════
    // NIVEL_2
    // ═══════════════════════════════════════════════════════════════

    private static void FixNivel2()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_2.unity", OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel2 ===");

        // Eliminar Player y Camera de Samuel
        DestroyByName("Player");
        DestroyByName("Main Camera");

        // Clonar Player (base 700000000)
        long playerCameraTargetID = CloneObject("Player", PlayerIDs, 700000000,
            new Vector3(-3.75f, 3.5f, 8.1f), "Player");

        // Clonar Camera (base 900000000)
        // CameraFollow.target apunta a CameraTarget del Player clonado
        CloneCamera(900000000, playerCameraTargetID);

        // Fix Ground Layer (Layer 3 = Ground)
        FixLayer("Piso_N2_", 3);

        // Verificar
        Verify("Nivel_2");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_2 listo.");
    }

    // ═══════════════════════════════════════════════════════════════
    // NIVEL_3
    // ═══════════════════════════════════════════════════════════════

    private static void FixNivel3()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_3.unity", OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel3 ===");

        // Eliminar Player y Camera de Samuel
        DestroyByName("Player");
        DestroyByName("Main Camera");

        // Clonar Player (base 800000000)
        long playerCameraTargetID = CloneObject("Player", PlayerIDs, 800000000,
            new Vector3(4.285f, 2.017f, 30.808f), "Player");

        // Clonar Camera (base 910000000)
        CloneCamera(910000000, playerCameraTargetID);

        // Fix Ground Layer
        FixLayer("20m Epoxy", 3);

        // Deshabilitar UI de Samuel (no eliminar)
        SetActive("HUDCanvas", false);
        SetActive("EventSystem", false);
        SetActive("MobileControls", false);

        // Crear trigger de salida -> Victoria
        CreateTrigger("Trigger_SalidaNivel3", new Vector3(2.3f, 1.0f, 48.5f), new Vector3(3f, 3f, 2f), "");

        // Verificar
        Verify("Nivel_3");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_3 listo.");
    }

    // ═══════════════════════════════════════════════════════════════
    // CORRECCION DE FLUJO
    // ═══════════════════════════════════════════════════════════════

    private static void FixNivel1Flow()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_1.unity", OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel1Flow ===");

        // PuntoFinal tiene DestinoNivel que carga Nivel_2 directamente.
        // Debe estar deshabilitado para que ObjectiveDoor/PuertaSubLevel controle la salida.
        SetActive("PuntoFinal", false);

        // Verificar que ObjectiveDoor sigue activo
        GameObject door = GameObject.Find("ObjectiveDoor");
        if (door != null && door.activeSelf)
            Debug.Log("[Setup] Nivel_1: ObjectiveDoor activo OK");
        else
            Debug.LogWarning("[Setup] Nivel_1: ObjectiveDoor NO encontrado o inactivo");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_1 flujo corregido.");
    }

    private static void FixNivel2Flow()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_2.unity", OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel2Flow ===");

        // Trigger_SalidaNivel2 tiene PuertaCambioNivel.escenaDestino = "Nivel_3"
        // Debe apuntar a "SubCable01_Copy" para mantener el flujo correcto.
        GameObject trigger = GameObject.Find("Trigger_SalidaNivel2");
        if (trigger != null)
        {
            PuertaCambioNivel puerta = trigger.GetComponent<PuertaCambioNivel>();
            if (puerta != null)
            {
                string anterior = puerta.nombreEscena;
                puerta.nombreEscena = "SubCable01_Copy";
                Debug.Log("[Setup] Trigger_SalidaNivel2: nombreEscena " + anterior + " -> SubCable01_Copy");
            }
            else
            {
                Debug.LogWarning("[Setup] Trigger_SalidaNivel2: no tiene PuertaCambioNivel");
            }
        }
        else
        {
            Debug.LogWarning("[Setup] Nivel_2: Trigger_SalidaNivel2 no encontrado");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_2 flujo corregido.");
    }

    // ═══════════════════════════════════════════════════════════════
    // CLONAR OBJETOS DESDE NIVEL_1 (extraccion YAML textual)
    // ═══════════════════════════════════════════════════════════════

    private static long CloneObject(string objectName, HashSet<long> sourceIDs,
        long baseFileID, Vector3 position, string tagName)
    {
        string[] srcLines = ReadScene(SRC);
        if (srcLines == null) return 0;

        // Extraer bloques YAML
        Dictionary<long, List<string>> blocks = ExtractBlocks(srcLines, sourceIDs);
        if (blocks.Count == 0)
        {
            Debug.LogError("[Setup] No se encontro " + objectName + " en Nivel_1");
            return 0;
        }

        // Remapear IDs
        Dictionary<long, long> map = BuildMap(blocks.Keys, baseFileID);

        // Leer escena destino
        List<string> dst = ReadSceneList(EditorSceneManager.GetActiveScene());

        // Eliminar bloques viejos
        dst = RemoveBlocks(dst, sourceIDs);

        // Inyectar antes de SceneRoots
        int idx = FindLine(dst, "SceneRoots:");
        if (idx < 0) return 0;

        List<string> inject = new List<string>();
        foreach (long oldID in blocks.Keys)
        {
            foreach (string line in blocks[oldID])
                inject.Add(Remap(line, map));
            inject.Add("");
        }
        dst.InsertRange(idx, inject);

        // Agregar a SceneRoots
        AddToRoots(dst, map[sourceIDs.First()]); // Transform root

        // Fix spawnPoint en PlayerHealth
        ResetSpawnPoint(dst, map);

        // Guardar
        WriteScene(EditorSceneManager.GetActiveScene(), dst);

        long cameraTargetId = map.ContainsKey(500000111) ? map[500000111] : 0;
        Debug.Log("[Setup] " + objectName + " clonado. CameraTarget=" + cameraTargetId);
        return cameraTargetId;
    }

    private static void CloneCamera(long baseFileID, long cameraTargetFileID)
    {
        string[] srcLines = ReadScene(SRC);
        if (srcLines == null) return;

        Dictionary<long, List<string>> blocks = ExtractBlocks(srcLines, CameraIDs);
        if (blocks.Count == 0)
        {
            Debug.LogError("[Setup] No se encontro Main Camera en Nivel_1");
            return;
        }

        Dictionary<long, long> map = BuildMap(blocks.Keys, baseFileID);
        List<string> dst = ReadSceneList(EditorSceneManager.GetActiveScene());

        dst = RemoveBlocks(dst, CameraIDs);

        int idx = FindLine(dst, "SceneRoots:");
        if (idx < 0) return;

        List<string> inject = new List<string>();
        foreach (long oldID in blocks.Keys)
        {
            foreach (string line in blocks[oldID])
                inject.Add(Remap(line, map));
            inject.Add("");
        }
        dst.InsertRange(idx, inject);

        AddToRoots(dst, map[500000201]); // Camera Transform

        // Fix CameraFollow.target -> CameraTarget del Player clonado
        long followId = map[500000205];
        FixFollowTarget(dst, followId, cameraTargetFileID);

        WriteScene(EditorSceneManager.GetActiveScene(), dst);
        Debug.Log("[Setup] Camera clonada. Follow target=" + cameraTargetFileID);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILIDADES YAML
    // ═══════════════════════════════════════════════════════════════

    private static string[] ReadScene(string path)
    {
        string full = Application.dataPath + "/../" + path;
        if (!File.Exists(full)) { Debug.LogError("[Setup] No existe: " + full); return null; }
        return File.ReadAllLines(full);
    }

    private static List<string> ReadSceneList(Scene s)
    {
        return new List<string>(File.ReadAllLines(Application.dataPath + "/../" + s.path));
    }

    private static void WriteScene(Scene s, List<string> content)
    {
        File.WriteAllLines(Application.dataPath + "/../" + s.path, content.ToArray());
    }

    private static Dictionary<long, List<string>> ExtractBlocks(string[] lines, HashSet<long> ids)
    {
        var blocks = new Dictionary<long, List<string>>();
        bool inside = false;
        long curID = 0;
        var cur = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines[i];
            if (l.StartsWith("--- !u!"))
            {
                if (inside && cur.Count > 0) blocks[curID] = new List<string>(cur);
                long fid = ParseFID(l);
                if (ids.Contains(fid)) { inside = true; curID = fid; cur = new List<string> { l }; }
                else { inside = false; curID = 0; cur = new List<string>(); }
            }
            else if (inside) cur.Add(l);
        }
        if (inside && cur.Count > 0) blocks[curID] = new List<string>(cur);
        return blocks;
    }

    private static long ParseFID(string line)
    {
        var m = Regex.Match(line, @"&(\d+)");
        return m.Success ? long.Parse(m.Groups[1].Value) : 0;
    }

    private static Dictionary<long, long> BuildMap(ICollection<long> olds, long baseID)
    {
        var map = new Dictionary<long, long>();
        long n = baseID;
        foreach (long o in olds) { map[o] = n++; }
        return map;
    }

    private static string Remap(string line, Dictionary<long, long> map)
    {
        foreach (var kv in map)
        {
            string olds = kv.Key.ToString();
            if (line.Contains(olds)) line = line.Replace(olds, kv.Value.ToString());
        }
        return line;
    }

    private static List<string> RemoveBlocks(List<string> content, HashSet<long> ids)
    {
        var result = new List<string>();
        bool skip = false;
        for (int i = 0; i < content.Count; i++)
        {
            string l = content[i];
            if (l.StartsWith("--- !u!"))
            {
                long fid = ParseFID(l);
                if (ids.Contains(fid)) { skip = true; continue; }
                skip = false;
            }
            if (!skip) result.Add(l);
        }
        return result;
    }

    private static int FindLine(List<string> c, string text)
    {
        for (int i = 0; i < c.Count; i++)
            if (c[i].Trim() == text) return i;
        return -1;
    }

    private static void AddToRoots(List<string> c, long transformFID)
    {
        int roots = FindLine(c, "m_Roots:");
        if (roots >= 0)
            c.Insert(roots + 1, "    - {fileID: " + transformFID + "}");
    }

    private static void ResetSpawnPoint(List<string> c, Dictionary<long, long> map)
    {
        if (!map.ContainsKey(500000104)) return;
        string marker = "&" + map[500000104];
        bool found = false;
        for (int i = 0; i < c.Count; i++)
        {
            if (c[i].Contains(marker)) found = true;
            if (found && c[i].Trim().StartsWith("spawnPoint:"))
            { c[i] = "  spawnPoint: {fileID: 0}"; return; }
        }
    }

    private static void FixFollowTarget(List<string> c, long followFID, long targetFID)
    {
        string marker = "&" + followFID;
        bool found = false;
        for (int i = 0; i < c.Count; i++)
        {
            if (c[i].Contains(marker)) found = true;
            if (found && c[i].Trim().StartsWith("target:"))
            { c[i] = "  target: {fileID: " + targetFID + "}"; return; }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILIDADES DE ESCENA (Unity API)
    // ═══════════════════════════════════════════════════════════════

    private static void DestroyByName(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) { Object.DestroyImmediate(go); Debug.Log("[Setup] Eliminado: " + name); }
    }

    private static void SetActive(string name, bool active)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) { go.SetActive(active); Debug.Log("[Setup] " + name + " -> " + (active ? "ON" : "OFF")); }
    }

    private static void FixLayer(string prefix, int layer)
    {
        int count = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>())
        {
            if (go.name.StartsWith(prefix) && go.layer != layer)
            { go.layer = layer; count++; }
        }
        Debug.Log("[Setup] Layer " + layer + " aplicado a " + count + " objetos (" + prefix + ")");
    }

    private static void CreateTrigger(string name, Vector3 pos, Vector3 size, string siguienteNivel)
    {
        if (GameObject.Find(name) != null) { Debug.Log("[Setup] " + name + " ya existe"); return; }
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        go.AddComponent<DestinoNivel>();
        Debug.Log("[Setup] " + name + " creado en " + pos);
    }

    private static void Verify(string nivel)
    {
        int players = 0, cams = 0, audio = 0;
        foreach (GameObject go in Object.FindObjectsByType<GameObject>())
        {
            if (go.CompareTag("Player")) players++;
            if (go.CompareTag("MainCamera")) cams++;
            if (go.GetComponent<AudioListener>() != null) audio++;
        }

        Camera main = Camera.main;
        if (main == null)
            Debug.LogError("[Setup] " + nivel + ": Camera.main == NULL");

        if (main != null)
        {
            CameraFollow cf = main.GetComponent<CameraFollow>();
            if (cf == null)
                Debug.LogError("[Setup] " + nivel + ": CameraFollow no existe");

            Component[] comps = main.GetComponents<Component>();
            foreach (Component c in comps)
                if (c == null) Debug.LogError("[Setup] " + nivel + ": MISSING SCRIPT en Main Camera");
        }

        if (players != 1) Debug.LogWarning("[Setup] " + nivel + ": Players=" + players);
        if (cams != 1) Debug.LogWarning("[Setup] " + nivel + ": MainCameras=" + cams);
        if (audio != 1) Debug.LogWarning("[Setup] " + nivel + ": AudioListeners=" + audio);

        if (players == 1 && cams == 1 && audio == 1 && main != null)
            Debug.Log("[Setup] " + nivel + ": OK");
    }
}
