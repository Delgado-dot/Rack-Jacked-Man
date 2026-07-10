using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// SetupSubLevel_Test - Construye la escena SubLevel_Test.unity
/// con tres cables y el jugador usando SubLevelPlayerController.
/// Ejecutar UNA VEZ desde: Rack-Jacked-Man > Setup SubLevel Test
/// Despues BORRAR este archivo.
/// </summary>
public class SetupSubLevel_Test : EditorWindow
{
    private const string ESCENA_PATH = "Assets/Scenes/SubLevel_Test.unity";
    private const float DISTANCIA_CABLES = 2f;
    private const int NUM_SEGMENTOS = 30;
    private const float LARGO_SEGMENTO = 10f;
    private static int objetosCreados = 0;

    [MenuItem("Rack-Jacked-Man/Setup SubLevel Test")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup SubLevel Test",
            "Creara la escena SubLevel_Test.unity\n" +
            "con 3 cables y jugador (solo movimiento).\n\n" +
            "Seguro?",
            "Crear", "Cancelar")) return;

        objetosCreados = 0;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CrearJugador();
        CrearCables();
        CrearSueloVisual();
        CrearParedesLaterales();

        EditorSceneManager.MarkSceneDirty(scene);
        bool guardado = EditorSceneManager.SaveScene(scene, ESCENA_PATH);

        int errores = 0;
        errores += Verificar("Player");
        errores += Verificar("Cables");
        errores += Verificar("Cables/Cable_Izquierdo");
        errores += Verificar("Cables/Cable_Central");
        errores += Verificar("Cables/Cable_Derecho");

        string msg = string.Format(
            "[SetupSubLevel_Test] OBJETOS CREADOS: {0} | ERRORES: {1} | ESCENA GUARDADA: {2}",
            objetosCreados, errores, guardado ? "SI" : "NO");
        Debug.Log(msg);

        if (guardado && errores == 0)
        {
            EditorUtility.DisplayDialog("Setup SubLevel Test",
                string.Format("Escena SubLevel_Test creada.\nObjetos: {0}\nBorra SetupSubLevel_Test.cs.", objetosCreados), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Setup SubLevel Test",
                string.Format("Completado con {0} errores.\nRevisa la consola.", errores), "OK");
        }
    }

    static int Verificar(string ruta)
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
            Debug.LogError("[SetupSubLevel_Test] No se pudo crear: " + ruta);
            return 1;
        }
        return 0;
    }

    static void CrearJugador()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.5f, 0f);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 1f, 0f);

        player.AddComponent<SubLevelPlayerController>();

        // Camera como hija del jugador
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0f, 1.6f, 0.3f);
        camObj.transform.localRotation = Quaternion.identity;

        Camera cam = camObj.GetComponent<Camera>();
        if (cam == null) cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;

        AudioListener listener = camObj.GetComponent<AudioListener>();
        if (listener == null) camObj.AddComponent<AudioListener>();

        objetosCreados += 3; // Player, Camera, SubLevelPlayerController
    }

    static void CrearCables()
    {
        GameObject cablesRoot = new GameObject("Cables");
        objetosCreados++;

        float[] cableX = { -DISTANCIA_CABLES, 0f, DISTANCIA_CABLES };
        string[] cableNombres = { "Cable_Izquierdo", "Cable_Central", "Cable_Derecho" };
        Color[] cableColores = {
            new Color(0.35f, 0.35f, 0.4f),
            new Color(0.45f, 0.45f, 0.48f),
            new Color(0.35f, 0.35f, 0.4f)
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject cableRoot = new GameObject(cableNombres[i]);
            cableRoot.transform.SetParent(cablesRoot.transform);
            objetosCreados++;

            for (int s = 0; s < NUM_SEGMENTOS; s++)
            {
                float z = s * LARGO_SEGMENTO + LARGO_SEGMENTO / 2f;
                Vector3 pos = new Vector3(cableX[i], 0f, z);
                Vector3 scale = new Vector3(1f, 0.3f, LARGO_SEGMENTO);

                GameObject segmento = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segmento.name = "Segmento_" + s;
                segmento.transform.SetParent(cableRoot.transform);
                segmento.transform.position = pos;
                segmento.transform.localScale = scale;

                Renderer rend = segmento.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
                mat.color = cableColores[i];
                rend.sharedMaterial = mat;

                objetosCreados++;
            }
        }
    }

    static void CrearSueloVisual()
    {
        GameObject suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        suelo.name = "SueloVisual";
        suelo.transform.position = new Vector3(0f, -0.5f, NUM_SEGMENTOS * LARGO_SEGMENTO / 2f);
        suelo.transform.localScale = new Vector3(10f, 0.1f, NUM_SEGMENTOS * LARGO_SEGMENTO);

        Renderer rend = suelo.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.15f, 0.15f, 0.2f);
        rend.sharedMaterial = mat;

        objetosCreados++;
    }

    static void CrearParedesLaterales()
    {
        for (int lado = -1; lado <= 1; lado += 2)
        {
            GameObject pared = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pared.name = lado == -1 ? "ParedIzquierda" : "ParedDerecha";
            pared.transform.position = new Vector3(lado * 5f, 2f, NUM_SEGMENTOS * LARGO_SEGMENTO / 2f);
            pared.transform.localScale = new Vector3(0.3f, 4f, NUM_SEGMENTOS * LARGO_SEGMENTO);

            Renderer rend = pared.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.25f, 0.25f, 0.3f);
            rend.sharedMaterial = mat;

            objetosCreados++;
        }
    }
}
