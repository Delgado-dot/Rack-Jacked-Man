using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// SetupSubcable01 - Construye la escena Subcable01 con 3 cables, spawner y jugador.
/// Ejecutar UNA VEZ desde: Rack-Jacked-Man > Setup Subcable 01
/// Despues BORRAR este archivo.
/// </summary>
public class SetupSubcable01 : EditorWindow
{
    private const string ESCENA_PATH = "Assets/Scenes/Subcable01.unity";
    private static int objetosCreados = 0;

    [MenuItem("Rack-Jacked-Man/Setup Subcable 01")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Subcable 01",
            "Creara la escena Subcable01.unity\n" +
            "con 3 cables, spawner y jugador.\n\n" +
            "Seguro?",
            "Crear", "Cancelar")) return;

        objetosCreados = 0;

        // Crear escena nueva
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // === JUGADOR ===
        CrearJugador();

        // === CABLES ===
        CrearCables();

        // === SPAWNER ===
        CrearSpawner();

        // === BOUNDARY ===
        CrearBoundary();

        // === GUARDAR ===
        EditorSceneManager.MarkSceneDirty(scene);
        bool guardado = EditorSceneManager.SaveScene(scene, ESCENA_PATH);

        // === VERIFICACION ===
        int errores = 0;
        errores += Verificar("Player");
        errores += Verificar("Cables/Cable_Izquierdo");
        errores += Verificar("Cables/Cable_Central");
        errores += Verificar("Cables/Cable_Derecho");
        errores += Verificar("EnemySpawner");

        // === REPORTE ===
        string msg = string.Format(
            "[SetupSubcable01] OBJETOS CREADOS: {0} | ERRORES: {1} | ESCENA GUARDADA: {2}",
            objetosCreados, errores, guardado ? "SI" : "NO");
        Debug.Log(msg);

        if (guardado && errores == 0)
        {
            EditorUtility.DisplayDialog("Setup Subcable 01",
                string.Format("Escena Subcable01 creada.\nObjetos: {0}\nBorra SetupSubcable01.cs.", objetosCreados), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Setup Subcable 01",
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
            Debug.LogError("[SetupSubcable01] No se pudo crear: " + ruta);
            return 1;
        }
        return 0;
    }

    static void CrearJugador()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");

        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.5f, -5f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 1f, 0f);
        }
        else
        {
            player.transform.position = new Vector3(0f, 1.5f, -5f);
        }

        objetosCreados++;

        // Asegurar que tiene PlayerMovement y PlayerHealth
        if (player.GetComponent<PlayerMovement>() == null)
        {
            player.AddComponent<PlayerMovement>();
            objetosCreados++;
        }
        if (player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
            objetosCreados++;
        }
    }

    static void CrearCables()
    {
        GameObject cablesRoot = new GameObject("Cables");
        objetosCreados++;

        float[] posicionesX = { -3f, 0f, 3f };
        string[] nombres = { "Cable_Izquierdo", "Cable_Central", "Cable_Derecho" };
        Color[] colores = { new Color(0.3f, 0.3f, 0.35f), new Color(0.4f, 0.4f, 0.42f), new Color(0.3f, 0.3f, 0.35f) };

        for (int i = 0; i < 3; i++)
        {
            // Cable visual (cubo largo)
            GameObject cable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cable.name = nombres[i];
            cable.transform.SetParent(cablesRoot.transform);
            cable.transform.position = new Vector3(posicionesX[i], 0f, 15f);
            cable.transform.localScale = new Vector3(1f, 0.3f, 60f);

            // Material normal
            Renderer rend = cable.GetComponent<Renderer>();
            Material matNormal = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (matNormal.shader == null) matNormal = new Material(Shader.Find("Standard"));
            matNormal.color = colores[i];
            rend.sharedMaterial = matNormal;

            // Material electrificado
            Material matElectrico = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (matElectrico.shader == null) matElectrico = new Material(Shader.Find("Standard"));
            matElectrico.color = Color.cyan;
            if (matElectrico.HasProperty("_EmissionColor"))
            {
                matElectrico.EnableKeyword("_EMISSION");
                matElectrico.SetColor("_EmissionColor", Color.cyan * 3f);
            }

            // Collider trigger
            BoxCollider col = cable.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 2f, 1f);

            // ParticleSystem
            GameObject partObj = new GameObject("Particulas");
            partObj.transform.SetParent(cable.transform);
            partObj.transform.localPosition = Vector3.zero;
            ParticleSystem ps = partObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = Color.cyan;
            main.maxParticles = 50;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.5f, 0.5f, 1f);
            var col2 = ps.colorOverLifetime;
            col2.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.white, 1f) },
                          new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col2.color = grad;
            ps.Stop();

            // AudioSource
            GameObject audioObj = new GameObject("AudioElectrico");
            audioObj.transform.SetParent(cable.transform);
            audioObj.transform.localPosition = Vector3.zero;
            AudioSource audioSrc = audioObj.AddComponent<AudioSource>();
            audioSrc.loop = true;
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 1f;

            // CableHazard
            CableHazard ch = cable.AddComponent<CableHazard>();

            objetosCreados++;
        }
    }

    static void CrearSpawner()
    {
        GameObject spawner = new GameObject("EnemySpawner");
        spawner.transform.position = new Vector3(0f, 2f, 25f);

        EnemySpawner es = spawner.AddComponent<EnemySpawner>();

        // Asignar cables como hijos de Cables
        GameObject cablesRoot = GameObject.Find("Cables");
        if (cablesRoot != null)
        {
            CableHazard[] cableScripts = cablesRoot.GetComponentsInChildren<CableHazard>();
            Transform[] cableTransforms = new Transform[cableScripts.Length];
            for (int i = 0; i < cableScripts.Length; i++)
            {
                cableTransforms[i] = cableScripts[i].transform;
            }

            SerializedObject so = new SerializedObject(es);
            SerializedProperty cablesProp = so.FindProperty("cables");
            if (cablesProp != null)
            {
                cablesProp.arraySize = cableTransforms.Length;
                for (int i = 0; i < cableTransforms.Length; i++)
                {
                    cablesProp.GetArrayElementAtIndex(i).objectReferenceValue = cableTransforms[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // Asignar jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player != null)
        {
            SerializedObject so = new SerializedObject(es);
            SerializedProperty playerProp = so.FindProperty("jugador");
            if (playerProp != null)
            {
                playerProp.objectReferenceValue = player.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        objetosCreados++;
    }

    static void CrearBoundary()
    {
        GameObject boundary = new GameObject("Boundary");
        boundary.transform.position = new Vector3(0f, 0f, 50f);

        BoxCollider col = boundary.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(100f, 20f, 1f);

        boundary.tag = "Finish";

        objetosCreados++;
    }
}
