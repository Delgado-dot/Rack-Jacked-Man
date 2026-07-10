using UnityEngine;

/// <summary>
/// AutoSetupRacks - Se ejecuta al inicio de la escena.
/// Crea racks, managers y conexiones automaticamente.
/// </summary>
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

        Debug.Log("[AutoSetupRacks] Iniciando setup...");

        // --- PlayerMode + Rigidbody en el Player ---
        GameObject player = GameObject.Find("Player");
        PlayerMode playerMode = null;
        if (player != null)
        {
            // Tag "Player" para que los triggers lo detecten
            try { player.tag = "Player"; } catch { }

            playerMode = player.GetComponent<PlayerMode>();
            if (playerMode == null)
            {
                playerMode = player.AddComponent<PlayerMode>();
                Debug.Log("[AutoSetupRacks] PlayerMode agregado al Player");
            }

            // Rigidbody kinematic necesario para OnTriggerEnter
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = player.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log("[AutoSetupRacks] Rigidbody kinematico agregado al Player");
            }
        }
        else
        {
            Debug.LogWarning("[AutoSetupRacks] No se encontro Player en la escena");
        }

        // --- GameManager ---
        if (GameManager.Instance == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Debug.Log("[AutoSetupRacks] GameManager creado");
        }

        // --- PuzzleMana ---
        PuzzleMana puzzleMana = null;
        GameObject puzzleManaGO = GameObject.Find("PuzzleMana");
        if (puzzleManaGO == null)
        {
            puzzleManaGO = new GameObject("PuzzleMana");
        }
        puzzleMana = puzzleManaGO.GetComponent<PuzzleMana>();
        if (puzzleMana == null)
        {
            puzzleMana = puzzleManaGO.AddComponent<PuzzleMana>();
        }

        // --- PythonPuzzleManager ---
        PythonPuzzleManager pythonMgr = null;
        GameObject pythonGO = GameObject.Find("PythonPuzzleManager");
        if (pythonGO == null)
        {
            pythonGO = new GameObject("PythonPuzzleManager");
        }
        pythonMgr = pythonGO.GetComponent<PythonPuzzleManager>();
        if (pythonMgr == null)
        {
            pythonMgr = pythonGO.AddComponent<PythonPuzzleManager>();
        }

        // Conectar referencias
        puzzleMana.playerMode = playerMode;
        puzzleMana.pythonPuzzleManager = pythonMgr;

        // --- RackCheckpoint (cubo azul) ---
        GameObject rackC = null;
        if (GameObject.Find("Rack_C") == null)
        {
            rackC = CreateRack("Rack_C", RackController.TipoRack.Checkpoint, new Vector3(0f, 1f, 20f),
                new Color(0.2f, 0.6f, 1f));
        }
        else
        {
            rackC = GameObject.Find("Rack_C");
        }

        // --- RackFinal (cubo amarillo) ---
        GameObject rackF = null;
        if (GameObject.Find("Rack_F") == null)
        {
            rackF = CreateRack("Rack_F", RackController.TipoRack.Final, new Vector3(0f, 1f, 80f),
                new Color(1f, 0.8f, 0.1f));
        }
        else
        {
            rackF = GameObject.Find("Rack_F");
        }

        // Conectar triggers
        ConnectTrigger(rackC, puzzleMana);
        ConnectTrigger(rackF, puzzleMana);

        Debug.Log("[AutoSetupRacks] Setup completo.");
        Debug.Log("[AutoSetupRacks] Rack_C (azul) en z=20 - Checkpoint");
        Debug.Log("[AutoSetupRacks] Rack_F (amarillo) en z=80 - Final");
    }

    static GameObject CreateRack(string name, RackController.TipoRack tipo, Vector3 pos, Color color)
    {
        GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rack.name = name;
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(2f, 2f, 2f);

        Renderer rend = rack.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.sharedMaterial = mat;

        BoxCollider col = rack.GetComponent<BoxCollider>();
        if (col == null) col = rack.AddComponent<BoxCollider>();
        col.isTrigger = true;

        RackController rc = rack.AddComponent<RackController>();
        rc.tipoRack = tipo;

        rack.AddComponent<RackTrigger>();

        return rack;
    }

    static void ConnectTrigger(GameObject rack, PuzzleMana puzzleMana)
    {
        if (rack == null) return;

        RackTrigger trigger = rack.GetComponent<RackTrigger>();
        RackController controller = rack.GetComponent<RackController>();

        if (trigger != null)
        {
            trigger.puzzleMana = puzzleMana;
            trigger.rackController = controller;
        }
    }
}
