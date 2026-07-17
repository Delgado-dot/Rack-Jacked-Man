using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSetupRacks : MonoBehaviour
{
    private static bool alreadySetup = false;
    private static int totalRacksNeeded = 6;
    private static int lastSceneIndex = -1;

    private void Awake()
    {
        if (!alreadySetup)
        {
            Setup();
        }
    }

    private void OnDestroy()
    {
        alreadySetup = false;
    }

    public static void Setup()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex != lastSceneIndex)
        {
            alreadySetup = false;
            lastSceneIndex = currentSceneIndex;
        }

        if (alreadySetup) return;
        alreadySetup = true;

        string sceneName = SceneManager.GetActiveScene().name;
        bool isSubLevel = sceneName.StartsWith("SubCable") || sceneName.StartsWith("Sub");

        Debug.Log("[AutoSetupRacks] Iniciando setup en " + sceneName + "...");

        SetupPlayer();
        SetupManagers();

        bool isNivel3 = sceneName == "Nivel_3";
        bool isNivel2 = sceneName == "Nivel_2";

        if (!isSubLevel)
        {
            if (isNivel3)
            {
                SetupNivel3();
            }
            else if (isNivel2)
            {
                Debug.Log("[AutoSetupRacks] Nivel_2: usando configuracion propia. Saltando EnsureObjectiveDoor/SetupSceneRacks.");
            }
            else
            {
                EnsureObjectiveDoor();
                SetupSceneRacks();
            }
        }

        Debug.Log("[AutoSetupRacks] Setup completo.");
    }

    static void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogWarning("[AutoSetupRacks] No se encontro Player en la escena");
            return;
        }

        try { player.tag = "Player"; } catch { }

        PlayerMode playerMode = player.GetComponent<PlayerMode>();
        if (playerMode == null)
            playerMode = player.AddComponent<PlayerMode>();

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = player.AddComponent<PlayerHealth>();

        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        if (playerInteract == null)
            playerInteract = player.AddComponent<PlayerInteract>();
    }

    static void SetupManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        PuzzleMana puzzleMana = FindAnyObjectByType<PuzzleMana>();
        if (puzzleMana == null)
        {
            GameObject pmGO = new GameObject("PuzzleMana");
            puzzleMana = pmGO.AddComponent<PuzzleMana>();
        }

        PythonPuzzleManager pythonMgr = FindAnyObjectByType<PythonPuzzleManager>();
        if (pythonMgr == null)
        {
            GameObject pyGO = new GameObject("PythonPuzzleManager");
            pythonMgr = pyGO.AddComponent<PythonPuzzleManager>();
        }

        puzzleMana.pythonPuzzleManager = pythonMgr;

        PlayerMode playerMode = FindAnyObjectByType<PlayerMode>();
        if (playerMode == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerMode = player.GetComponent<PlayerMode>();
        }
        puzzleMana.playerMode = playerMode;

        if (FindAnyObjectByType<InteractHUD>() == null)
        {
            GameObject hudGO = new GameObject("InteractHUD");
            hudGO.AddComponent<InteractHUD>();
        }
    }

    static void EnsureObjectiveDoor()
    {
        GameObject door = GameObject.Find("ObjectiveDoor");

        if (door == null)
        {
            door = new GameObject("ObjectiveDoor");
            door.transform.position = new Vector3(0f, 2f, 50f);
            Debug.Log("[AutoSetupRacks] ObjectiveDoor creado en escena");
        }

        if (door.GetComponent<ObjectiveDoorController>() == null)
            door.AddComponent<ObjectiveDoorController>();

        if (door.GetComponent<PuertaSubLevel>() == null)
            door.AddComponent<PuertaSubLevel>();
    }

    static void SetupSceneRacks()
    {
        PuzzleMana puzzleMana = FindAnyObjectByType<PuzzleMana>();
        ObjectiveDoorController doorCtrl = FindAnyObjectByType<ObjectiveDoorController>();

        GameObject[] existingRacks = FindObjectsByType<GameObject>();

        System.Collections.Generic.List<GameObject> rackList =
            new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < existingRacks.Length; i++)
        {
            if (existingRacks[i].GetComponent<RackInteractable>() != null ||
                existingRacks[i].GetComponent<RackState>() != null)
            {
                if (!rackList.Contains(existingRacks[i]))
                    rackList.Add(existingRacks[i]);
            }
        }

        string[] rackNames = { "ServerRack_0", "ServerRack_1", "ServerRack_2", "ServerRack_3", "ServerRack_4", "ServerRack_5" };

        for (int i = 0; i < rackNames.Length; i++)
        {
            GameObject rackGO = GameObject.Find(rackNames[i]);
            if (rackGO != null && !rackList.Contains(rackGO))
                rackList.Add(rackGO);
        }

        int extraNeeded = totalRacksNeeded - rackList.Count;
        if (extraNeeded > 0)
        {
            Debug.Log("[AutoSetupRacks] Faltan " + extraNeeded + " racks. Creando placeholders...");

            Transform doorTf = null;
            if (doorCtrl != null)
                doorTf = doorCtrl.transform;

            Vector3 centerPos = doorTf != null ? doorTf.position + Vector3.left * 6f : new Vector3(-6f, 1.5f, 40f);

            for (int i = 0; i < extraNeeded; i++)
            {
                GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.name = "ServerRack_" + (rackList.Count);
                placeholder.transform.position = centerPos + Vector3.right * (i * 2f);
                placeholder.transform.localScale = new Vector3(0.8f, 1.5f, 0.6f);
                rackList.Add(placeholder);
                Debug.Log("[AutoSetupRacks] Rack placeholder creado: " + placeholder.name);
            }
        }

        for (int i = 0; i < rackList.Count; i++)
        {
            GameObject rackGO = rackList[i];
            if (rackGO == null) continue;

            RackInteractable rackInteractable = rackGO.GetComponent<RackInteractable>();
            if (rackInteractable == null)
                rackInteractable = rackGO.AddComponent<RackInteractable>();

            bool isLast = (i == rackList.Count - 1);
            RackInteractable.RackType rackType = isLast
                ? RackInteractable.RackType.Final
                : RackInteractable.RackType.Checkpoint;
            rackInteractable.Initialize(i, rackType);

            RackState rackState = rackGO.GetComponent<RackState>();
            if (rackState == null)
                rackGO.AddComponent<RackState>();

            BoxCollider col = rackGO.GetComponent<BoxCollider>();
            if (col == null)
                col = rackGO.AddComponent<BoxCollider>();

            Debug.Log("[AutoSetupRacks] Rack configurado: " + rackGO.name + " (index=" + i + ", type=" + rackType + ")");
        }

        int totalRacks = GameObject.FindObjectsByType<RackInteractable>().Length;
        Debug.Log("[AutoSetupRacks] Total racks configurados: " + totalRacks);

        FixDuplicateAudioListeners();
    }

    static void FixDuplicateAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>();
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++)
            {
                Debug.Log("[AutoSetupRacks] AudioListener duplicado desactivado: " + listeners[i].gameObject.name);
                listeners[i].enabled = false;
            }
        }
    }

    static void SetupNivel3()
    {
        PuzzleMana puzzleMana = FindAnyObjectByType<PuzzleMana>();

        string[] rackNames = { "Server Rack (3)", "Server Rack (4)", "Server Rack (5)", "Server Rack (6)" };

        for (int i = 0; i < rackNames.Length; i++)
        {
            GameObject rackGO = GameObject.Find(rackNames[i]);
            if (rackGO == null)
            {
                Debug.LogWarning("[AutoSetupRacks] Nivel_3: Rack no encontrado: " + rackNames[i]);
                continue;
            }

            RackInteractable rackInteractable = rackGO.GetComponent<RackInteractable>();
            if (rackInteractable == null)
                rackInteractable = rackGO.AddComponent<RackInteractable>();

            bool isLast = (i == rackNames.Length - 1);
            RackInteractable.RackType rackType = isLast
                ? RackInteractable.RackType.Final
                : RackInteractable.RackType.Checkpoint;
            rackInteractable.Initialize(i, rackType);

            RackState rackState = rackGO.GetComponent<RackState>();
            if (rackState == null)
                rackGO.AddComponent<RackState>();

            BoxCollider col = rackGO.GetComponent<BoxCollider>();
            if (col == null)
                col = rackGO.AddComponent<BoxCollider>();

            Debug.Log("[AutoSetupRacks] Nivel_3 Rack configurado: " + rackGO.name + " (index=" + i + ", type=" + rackType + ")");
        }

        GameObject tv = GameObject.Find("TV 32 inch 2");
        if (tv != null && tv.GetComponent<RackExitTrigger>() == null)
        {
            tv.AddComponent<RackExitTrigger>();
            Debug.Log("[AutoSetupRacks] Nivel_3: RackExitTrigger agregado a " + tv.name);
        }

        int totalRacks = GameObject.FindObjectsByType<RackInteractable>().Length;
        Debug.Log("[AutoSetupRacks] Nivel_3 Total racks: " + totalRacks);

        FixDuplicateAudioListeners();
    }
}
