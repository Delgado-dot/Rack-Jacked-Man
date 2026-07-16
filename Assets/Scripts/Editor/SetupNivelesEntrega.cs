using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class SetupNivelesEntrega : EditorWindow
{
    private const string SCENE_SOURCE = "Assets/Scenes/Nivel_1.unity";
    private const string SCENE_NIVEL2 = "Assets/Scenes/Nivel_2.unity";
    private const string SCENE_NIVEL3 = "Assets/Scenes/Nivel_3.unity";

    // FileIDs del Player en Nivel_1
    private static readonly HashSet<long> PlayerFileIDs = new HashSet<long>
    {
        500000100, 500000101, 500000103, 500000104, 500000105,
        500000110, 500000111,
        500000120, 500000121,
        500000130, 500000131,
        1899228408, 1899228409,
        960945012,
        563622406, 563622407, 563622408
    };

    // FileIDs de la Main Camera en Nivel_1
    private static readonly HashSet<long> CameraFileIDs = new HashSet<long>
    {
        500000200, 500000201, 500000202, 500000203,
        500000204, 500000205, 500000206, 500000207
    };

    // Todos los fileIDs que vamos a clonar
    private static readonly HashSet<long> AllCloneIDs = new HashSet<long>();
    static SetupNivelesEntrega()
    {
        foreach (long id in PlayerFileIDs) AllCloneIDs.Add(id);
        foreach (long id in CameraFileIDs) AllCloneIDs.Add(id);
    }

    [MenuItem("Rack-Jacked-Man/Setup Niveles Entrega")]
    public static void ShowWindow()
    {
        GetWindow<SetupNivelesEntrega>("Setup Niveles Entrega");
    }

    private void OnGUI()
    {
        GUILayout.Label("Setup de Niveles para Entrega Final", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Corregir Nivel_2", GUILayout.Height(40)))
        {
            FixNivel2();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("2. Corregir Nivel_3", GUILayout.Height(40)))
        {
            FixNivel3();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("3. Configurar Menu Victoria", GUILayout.Height(40)))
        {
            SetupMenuScene("Assets/Scenes/Menu Victoria.unity", "Victoria");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("4. Configurar Menu GameOver", GUILayout.Height(40)))
        {
            SetupMenuScene("Assets/Scenes/Menu GameOver.unity", "GameOver");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("5. CONFIGURAR TODO", GUILayout.Height(50)))
        {
            FixNivel2();
            FixNivel3();
            SetupMenuScene("Assets/Scenes/Menu Victoria.unity", "Victoria");
            SetupMenuScene("Assets/Scenes/Menu GameOver.unity", "GameOver");
            EditorUtility.DisplayDialog("Setup Completo",
                "Nivel_2, Nivel_3, Victoria y GameOver configurados.", "OK");
        }

        GUILayout.Space(10);
        GUILayout.Label("MenuPrincipal -> Nivel_1 -> SubCable -> Nivel_2 -> SubCable -> Nivel_3 -> Victoria", EditorStyles.wordWrappedMiniLabel);
    }

    // ═════════════════════════════════════════════════════════════════
    // NIVEL_2
    // ═════════════════════════════════════════════════════════════════

    private static void FixNivel2()
    {
        Scene scene = EditorSceneManager.OpenScene(SCENE_NIVEL2, OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel2 ===");

        // 1. Eliminar Player y Camera de Samuel
        DeleteIfExists("Player");
        DeleteIfExists("Main Camera");

        // 2. Clonar Player de Nivel_1 (base fileID 700000000)
        ClonePlayerFromNivel1(scene, 700000000, new Vector3(-3.75f, 3.5f, 8.1f));

        // 3. Clonar Camera de Nivel_1 (base fileID 900000000)
        //    CameraTarget del Player clonado tendra fileID 700000111
        CloneCameraFromNivel1(scene, 900000000, 700000111);

        // 4. Fix Ground Layer
        FixGroundLayer("Piso_N2_");

        // 5. Verificar
        VerifyScene("Nivel_2");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_2 listo.");
    }

    // ═════════════════════════════════════════════════════════════════
    // NIVEL_3
    // ═════════════════════════════════════════════════════════════════

    private static void FixNivel3()
    {
        Scene scene = EditorSceneManager.OpenScene(SCENE_NIVEL3, OpenSceneMode.Single);
        Debug.Log("[Setup] === FixNivel3 ===");

        // 1. Eliminar Player y Camera de Samuel
        DeleteIfExists("Player");
        DeleteIfExists("Main Camera");

        // 2. Clonar Player de Nivel_1 (base fileID 800000000)
        ClonePlayerFromNivel1(scene, 800000000, new Vector3(4.285f, 2.017f, 30.808f));

        // 3. Clonar Camera de Nivel_1 (base fileID 910000000)
        //    CameraTarget del Player clonado tendra fileID 800000111
        CloneCameraFromNivel1(scene, 910000000, 800000111);

        // 4. Fix Ground Layer
        FixGroundLayer("20m Epoxy");

        // 5. Deshabilitar UI de Samuel (no eliminar)
        DisableSamuelUI();

        // 6. Crear trigger de salida
        CreateExitTriggerNivel3();

        // 7. Verificar
        VerifyScene("Nivel_3");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Nivel_3 listo.");
    }

    // ═════════════════════════════════════════════════════════════════
    // CLONAR PLAYER DE NIVEL_1
    // ═════════════════════════════════════════════════════════════════

    private static void ClonePlayerFromNivel1(Scene targetScene, long baseFileID, Vector3 spawnPosition)
    {
        string[] sourceLines = ReadSourceScene();
        if (sourceLines == null) return;

        Dictionary<long, List<string>> blocks = ExtractBlocks(sourceLines, PlayerFileIDs);
        if (blocks.Count == 0)
        {
            Debug.LogError("[Setup] No se encontro Player en Nivel_1");
            return;
        }

        Dictionary<long, long> idMap = BuildIDMap(blocks.Keys, baseFileID);
        List<string> targetContent = ReadTargetScene(targetScene);

        // Eliminar Player viejo
        targetContent = RemoveBlocks(targetContent, PlayerFileIDs);

        // Inyectar Player clonado
        int insertIdx = FindSceneRootsLine(targetContent);
        if (insertIdx < 0) return;

        List<string> injectLines = BuildInjectLines(blocks, idMap);
        targetContent.InsertRange(insertIdx, injectLines);

        // Agregar a SceneRoots
        long newRootID = idMap[500000101];
        targetContent = AddToSceneRoots(targetContent, newRootID);

        // Resetear spawnPoint
        targetContent = ResetSpawnPoint(targetContent, idMap);

        WriteTargetScene(targetScene, targetContent);
        Debug.Log("[Setup] Player clonado (base " + baseFileID + ")");
    }

    // ═════════════════════════════════════════════════════════════════
    // CLONAR CAMERA DE NIVEL_1
    // ═════════════════════════════════════════════════════════════════

    private static void CloneCameraFromNivel1(Scene targetScene, long baseFileID, long cameraTargetFileID)
    {
        string[] sourceLines = ReadSourceScene();
        if (sourceLines == null) return;

        Dictionary<long, List<string>> blocks = ExtractBlocks(sourceLines, CameraFileIDs);
        if (blocks.Count == 0)
        {
            Debug.LogError("[Setup] No se encontro Main Camera en Nivel_1");
            return;
        }

        Dictionary<long, long> idMap = BuildIDMap(blocks.Keys, baseFileID);
        List<string> targetContent = ReadTargetScene(targetScene);

        // Eliminar Camera vieja
        targetContent = RemoveBlocks(targetContent, CameraFileIDs);

        // Inyectar Camera clonada
        int insertIdx = FindSceneRootsLine(targetContent);
        if (insertIdx < 0) return;

        List<string> injectLines = BuildInjectLines(blocks, idMap);
        targetContent.InsertRange(insertIdx, injectLines);

        // Agregar a SceneRoots
        long newCameraRootID = idMap[500000201];
        targetContent = AddToSceneRoots(targetContent, newCameraRootID);

        // Actualizar CameraFollow target → CameraTarget del Player clonado
        long newFollowID = idMap[500000205];
        targetContent = FixCameraFollowTarget(targetContent, newFollowID, cameraTargetFileID);

        WriteTargetScene(targetScene, targetContent);
        Debug.Log("[Setup] Camera clonada (base " + baseFileID + ", target→" + cameraTargetFileID + ")");
    }

    // ═════════════════════════════════════════════════════════════════
    // UTILIDADES DE EXTRACCION/INYECCION YAML
    // ═════════════════════════════════════════════════════════════════

    private static string[] ReadSourceScene()
    {
        string path = Application.dataPath + "/../" + SCENE_SOURCE;
        if (!File.Exists(path))
        {
            Debug.LogError("[Setup] No se encontro: " + path);
            return null;
        }
        return File.ReadAllLines(path);
    }

    private static string[] ReadSourceSceneArray()
    {
        return ReadSourceScene();
    }

    private static List<string> ReadTargetScene(Scene scene)
    {
        string path = Application.dataPath + "/../" + scene.path;
        return new List<string>(File.ReadAllLines(path));
    }

    private static void WriteTargetScene(Scene scene, List<string> content)
    {
        string path = Application.dataPath + "/../" + scene.path;
        File.WriteAllLines(path, content.ToArray());
    }

    private static Dictionary<long, List<string>> ExtractBlocks(string[] lines, HashSet<long> targetIDs)
    {
        Dictionary<long, List<string>> blocks = new Dictionary<long, List<string>>();
        bool inSection = false;
        long currentID = 0;
        List<string> currentBlock = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.StartsWith("--- !u!"))
            {
                if (inSection && currentBlock.Count > 0)
                {
                    blocks[currentID] = new List<string>(currentBlock);
                }

                long fileID = ExtractFileID(line);

                if (targetIDs.Contains(fileID))
                {
                    inSection = true;
                    currentID = fileID;
                    currentBlock = new List<string> { line };
                }
                else
                {
                    inSection = false;
                    currentID = 0;
                    currentBlock = new List<string>();
                }
            }
            else if (inSection)
            {
                currentBlock.Add(line);
            }
        }

        if (inSection && currentBlock.Count > 0)
        {
            blocks[currentID] = new List<string>(currentBlock);
        }

        return blocks;
    }

    private static long ExtractFileID(string yamlLine)
    {
        var match = Regex.Match(yamlLine, @"&(\d+)");
        if (match.Success)
            return long.Parse(match.Groups[1].Value);
        return 0;
    }

    private static Dictionary<long, long> BuildIDMap(ICollection<long> oldIDs, long baseFileID)
    {
        Dictionary<long, long> map = new Dictionary<long, long>();
        long next = baseFileID;
        foreach (long oldID in oldIDs)
        {
            map[oldID] = next;
            next++;
        }
        return map;
    }

    private static List<string> BuildInjectLines(Dictionary<long, List<string>> blocks, Dictionary<long, long> idMap)
    {
        List<string> lines = new List<string>();
        foreach (long oldID in blocks.Keys)
        {
            foreach (string line in blocks[oldID])
            {
                lines.Add(RemapLine(line, idMap));
            }
            lines.Add("");
        }
        return lines;
    }

    private static string RemapLine(string line, Dictionary<long, long> idMap)
    {
        foreach (var kvp in idMap)
        {
            string oldStr = kvp.Key.ToString();
            string newStr = kvp.Value.ToString();
            if (line.Contains(oldStr))
            {
                line = line.Replace(oldStr, newStr);
            }
        }
        return line;
    }

    private static List<string> RemoveBlocks(List<string> content, HashSet<long> idsToRemove)
    {
        List<string> result = new List<string>();
        bool skipping = false;

        for (int i = 0; i < content.Count; i++)
        {
            string line = content[i];

            if (line.StartsWith("--- !u!"))
            {
                long fileID = ExtractFileID(line);
                if (idsToRemove.Contains(fileID))
                {
                    skipping = true;
                    continue;
                }
                else
                {
                    skipping = false;
                }
            }

            if (!skipping)
            {
                result.Add(line);
            }
        }

        return result;
    }

    private static int FindSceneRootsLine(List<string> content)
    {
        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].Trim() == "SceneRoots:")
                return i;
        }
        return -1;
    }

    private static List<string> AddToSceneRoots(List<string> content, long transformFileID)
    {
        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].Trim() == "SceneRoots:")
            {
                for (int j = i + 1; j < Mathf.Min(i + 10, content.Count); j++)
                {
                    if (content[j].Trim() == "m_Roots:")
                    {
                        content.Insert(j + 1, "    - {fileID: " + transformFileID + "}");
                        return content;
                    }
                }
            }
        }
        return content;
    }

    private static List<string> ResetSpawnPoint(List<string> content, Dictionary<long, long> idMap)
    {
        long oldPHID = 500000104;
        if (!idMap.ContainsKey(oldPHID)) return content;

        string marker = "&" + idMap[oldPHID].ToString();
        bool found = false;
        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].Contains(marker))
                found = true;

            if (found && content[i].Trim().StartsWith("spawnPoint:"))
            {
                content[i] = "  spawnPoint: {fileID: 0}";
                return content;
            }
        }
        return content;
    }

    private static List<string> FixCameraFollowTarget(List<string> content, long cameraFollowFileID, long newTargetFileID)
    {
        string marker = "&" + cameraFollowFileID.ToString();
        bool found = false;
        for (int i = 0; i < content.Count; i++)
        {
            if (content[i].Contains(marker))
                found = true;

            if (found && content[i].Trim().StartsWith("target:"))
            {
                content[i] = "  target: {fileID: " + newTargetFileID + "}";
                Debug.Log("[Setup] CameraFollow target actualizado a fileID: " + newTargetFileID);
                return content;
            }
        }
        return content;
    }

    // ═════════════════════════════════════════════════════════════════
    // DELETE / FIX / LAYER / UI / TRIGGER
    // ═════════════════════════════════════════════════════════════════

    private static void DeleteIfExists(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
            Debug.Log("[Setup] Eliminado: " + name);
        }
    }

    private static void FixGroundLayer(string floorPrefix)
    {
        int groundLayer = 3;
        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (GameObject obj in all)
        {
            if (obj.name.StartsWith(floorPrefix) && obj.layer != groundLayer)
            {
                obj.layer = groundLayer;
                count++;
            }
        }
        Debug.Log("[Setup] Layer Ground aplicado a " + count + " objetos con prefijo '" + floorPrefix + "'");
    }

    private static void DisableSamuelUI()
    {
        string[] names = { "HUDCanvas", "EventSystem", "MobileControls" };
        foreach (string n in names)
        {
            GameObject obj = GameObject.Find(n);
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log("[Setup] Deshabilitado: " + n);
            }
        }
    }

    private static void CreateExitTriggerNivel3()
    {
        if (GameObject.Find("Trigger_SalidaNivel3") != null)
        {
            Debug.Log("[Setup] Trigger_SalidaNivel3 ya existe");
            return;
        }

        GameObject trigger = new GameObject("Trigger_SalidaNivel3");
        trigger.transform.position = new Vector3(2.3f, 1.0f, 48.5f);

        BoxCollider col = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3.0f, 3.0f, 2.0f);

        trigger.AddComponent<DestinoNivel>();
        Debug.Log("[Setup] Trigger_SalidaNivel3 creado en (2.3, 1.0, 48.5)");
    }

    private static void VerifyScene(string nivelName)
    {
        int players = 0, cameras = 0, audioListeners = 0;

        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in all)
        {
            if (obj.CompareTag("Player")) players++;
            if (obj.CompareTag("MainCamera")) cameras++;
            if (obj.GetComponent<AudioListener>() != null) audioListeners++;
        }

        // Verificar Camera.main
        Camera mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogError("[Setup] " + nivelName + ": Camera.main == NULL");

        // Verificar CameraFollow.target
        if (mainCam != null)
        {
            CameraFollow cf = mainCam.GetComponent<CameraFollow>();
            if (cf == null)
                Debug.LogError("[Setup] " + nivelName + ": CameraFollow no existe en Main Camera");
        }

        // Verificar Missing Scripts
        if (mainCam != null)
        {
            Component[] components = mainCam.GetComponents<Component>();
            foreach (Component c in components)
            {
                if (c == null)
                    Debug.LogError("[Setup] " + nivelName + ": MISSING SCRIPT en Main Camera");
            }
        }

        // Conteos
        if (players != 1)
            Debug.LogWarning("[Setup] " + nivelName + ": Players = " + players + " (deberia ser 1)");
        if (cameras != 1)
            Debug.LogWarning("[Setup] " + nivelName + ": MainCameras = " + cameras + " (deberia ser 1)");
        if (audioListeners != 1)
            Debug.LogWarning("[Setup] " + nivelName + ": AudioListeners = " + audioListeners + " (deberia ser 1)");

        if (players == 1 && cameras == 1 && audioListeners == 1 && mainCam != null)
            Debug.Log("[Setup] " + nivelName + ": Verificacion OK");
    }

    // ═════════════════════════════════════════════════════════════════
    // MENU SCENES
    // ═════════════════════════════════════════════════════════════════

    private static void SetupMenuScene(string scenePath, string menuName)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject menuObj = GameObject.Find("MenuManager");
        if (menuObj == null)
            menuObj = new GameObject("MenuManager");

        MenuButtonHandler handler = menuObj.GetComponent<MenuButtonHandler>();
        if (handler == null)
            handler = menuObj.AddComponent<MenuButtonHandler>();

        UnityEngine.UI.Button[] buttons = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (UnityEngine.UI.Button btn in buttons)
        {
            string btnName = btn.gameObject.name.ToLower();
            btn.onClick.RemoveAllListeners();

            if (btnName.Contains("reintentar") || btnName.Contains("retry"))
                btn.onClick.AddListener(handler.Reintentar);
            else if (btnName.Contains("menu") || btnName.Contains("principal") || btnName.Contains("main"))
                btn.onClick.AddListener(handler.MenuPrincipal);
            else if (btnName.Contains("salir") || btnName.Contains("quit"))
                btn.onClick.AddListener(handler.Salir);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Setup] Menu " + menuName + " configurado.");
    }
}
