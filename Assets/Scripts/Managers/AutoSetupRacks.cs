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

        GameObject puzzleManagerGO = GameObject.Find("PuzzleManager");
        if (puzzleManagerGO == null)
        {
            puzzleManagerGO = new GameObject("PuzzleManager");
        }
        PuzzleManager puzzleManager = puzzleManagerGO.GetComponent<PuzzleManager>();
        if (puzzleManager == null)
        {
            puzzleManager = puzzleManagerGO.AddComponent<PuzzleManager>();
        }

        GameObject puzzleUIGO = GameObject.Find("PuzzleUI");
        if (puzzleUIGO == null)
        {
            puzzleUIGO = new GameObject("PuzzleUI");
        }
        PuzzleUI puzzleUI = puzzleUIGO.GetComponent<PuzzleUI>();
        if (puzzleUI == null)
        {
            puzzleUI = puzzleUIGO.AddComponent<PuzzleUI>();
        }

        SetupCanvas(puzzleUI);
    }

    static void SetupCanvas(PuzzleUI puzzleUI)
    {
        Canvas existingCanvas = FindAnyObjectByType<Canvas>();
        if (existingCanvas == null)
        {
            GameObject canvasGO = new GameObject("PuzzleCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            UnityEngine.EventSystems.EventSystem eventSystem = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }

    static void SetupSceneRacks()
    {
        string[] rackNames = { "ServerRack_0", "ServerRack_1", "ServerRack_2", "ServerRack_3", "ServerRack_4", "ServerRack_5" };
        string[] puzzleTypes = { "cables", "dispatcher", "nave", "trafico", "patchcore" };

        PuzzleManager puzzleManager = FindAnyObjectByType<PuzzleManager>();

        int[] shuffledPuzzleIndices = new int[puzzleTypes.Length];
        for (int i = 0; i < puzzleTypes.Length; i++)
            shuffledPuzzleIndices[i] = i;
        for (int i = puzzleTypes.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffledPuzzleIndices[i];
            shuffledPuzzleIndices[i] = shuffledPuzzleIndices[j];
            shuffledPuzzleIndices[j] = temp;
        }

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

            string assignedPuzzle = puzzleTypes[shuffledPuzzleIndices[i % shuffledPuzzleIndices.Length]];
            rackInteractable.SetPuzzleName(assignedPuzzle);

            if (puzzleManager != null)
            {
                rackInteractable.SetPuzzleManager(puzzleManager);
            }

            BoxCollider col = rackGO.GetComponent<BoxCollider>();
            if (col == null)
            {
                col = rackGO.AddComponent<BoxCollider>();
            }

            Debug.Log("[AutoSetupRacks] Rack configurado: " + rackNames[i] + " -> puzzle: " + assignedPuzzle);
        }

        int totalRacks = GameObject.FindObjectsByType<RackInteractable>(FindObjectsSortMode.None).Length;
        Debug.Log("[AutoSetupRacks] Total racks interactivos: " + totalRacks);

        FixDuplicateAudioListeners();
    }

    static void FixDuplicateAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
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
