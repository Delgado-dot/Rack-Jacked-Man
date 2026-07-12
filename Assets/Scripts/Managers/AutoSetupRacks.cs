using UnityEngine;

public class AutoSetupRacks : MonoBehaviour
{
    private static bool alreadySetup = false;

    private void Awake()
    {
        if (!alreadySetup)
        {
            Setup();
        }
    }

    public static void Setup()
    {
        if (alreadySetup) return;
        alreadySetup = true;

        Debug.Log("[AutoSetupRacks] Iniciando setup con racks de escena...");

        SetupPlayer();
        SetupManagers();
        SetupSceneRacks();

        Debug.Log("[AutoSetupRacks] Setup completo.");
    }

    static void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            Debug.LogWarning("[AutoSetupRacks] No se encontro Player en la escena");
            return;
        }

        try { player.tag = "Player"; } catch { }

        PlayerMode playerMode = player.GetComponent<PlayerMode>();
        if (playerMode == null)
        {
            playerMode = player.AddComponent<PlayerMode>();
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        if (playerInteract == null)
        {
            playerInteract = player.AddComponent<PlayerInteract>();
        }
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

    static void SetupSceneRacks()
    {
        string[] rackNames = { "ServerRack_0", "ServerRack_1", "ServerRack_2", "ServerRack_3", "ServerRack_4", "ServerRack_5" };

        PuzzleMana puzzleMana = FindAnyObjectByType<PuzzleMana>();

        for (int i = 0; i < rackNames.Length; i++)
        {
            GameObject rackGO = GameObject.Find(rackNames[i]);
            if (rackGO == null)
            {
                Debug.LogWarning("[AutoSetupRacks] No se encontro: " + rackNames[i]);
                continue;
            }

            RackInteractable rackInteractable = rackGO.GetComponent<RackInteractable>();
            if (rackInteractable == null)
            {
                rackInteractable = rackGO.AddComponent<RackInteractable>();
            }

            BoxCollider col = rackGO.GetComponent<BoxCollider>();
            if (col == null)
            {
                col = rackGO.AddComponent<BoxCollider>();
            }

            Debug.Log("[AutoSetupRacks] Rack configurado: " + rackNames[i]);
        }

        int totalRacks = GameObject.FindObjectsByType<RackInteractable>().Length;
        Debug.Log("[AutoSetupRacks] Total racks interactivos: " + totalRacks);

        FixDuplicateAudioListeners();
    }

    static void FixDuplicateAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>();
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++)
            {
                Debug.Log("[AutoSetupRacks] AudioListener duplicado desactivado de: " + listeners[i].gameObject.name);
                listeners[i].enabled = false;
            }
        }
    }
}
