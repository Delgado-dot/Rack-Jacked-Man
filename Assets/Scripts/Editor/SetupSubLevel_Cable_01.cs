using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// SetupSubLevel_Cable_01 - Construye la escena SubLevel_Cable_01.
/// Ejecutar UNA VEZ desde: Rack-Jacked-Man > Setup SubLevel Cable 01
/// Despues BORRAR este archivo.
/// </summary>
public class SetupSubLevel_Cable_01 : EditorWindow
{
    private const string ESCENA_PATH = "Assets/Scenes/SubLevel_Cable_01.unity";
    private const float DISTANCIA_CABLES = 3f;
    private const int NUM_SEGMENTOS = 20;
    private const float LARGO_SEGMENTO = 10f;
    private static int objetosCreados = 0;

    [MenuItem("Rack-Jacked-Man/Setup SubLevel Cable 01")]
    public static void Execute()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup SubLevel Cable 01",
            "Creara la escena SubLevel_Cable_01.unity\n" +
            "con 3 cables, spawner, chaqueta y jugador.\n\n" +
            "Seguro?",
            "Crear", "Cancelar")) return;

        objetosCreados = 0;

        // Crear escena nueva
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Jugador
        CrearJugador();

        // 2. Tres cables con segmentos
        CrearCables();

        // 3. Spawner de enemigos
        CrearSpawner();

        // 4. Chaqueta
        CrearChaqueta();

        // 5. Manager del subnivel
        CrearManager();

        // 6. Limite final
        CrearLimiteFinal();

        // GUARDAR
        EditorSceneManager.MarkSceneDirty(scene);
        bool guardado = EditorSceneManager.SaveScene(scene, ESCENA_PATH);

        // VERIFICACION
        int errores = 0;
        errores += Verificar("Player");
        errores += Verificar("Cables");
        errores += Verificar("EnemySpawner");
        errores += Verificar("JacketPickup");
        errores += Verificar("SubLevelManager");

        // REPORTE
        string msg = string.Format(
            "[SetupSubLevel_Cable_01] OBJETOS CREADOS: {0} | ERRORES: {1} | ESCENA GUARDADA: {2}",
            objetosCreados, errores, guardado ? "SI" : "NO");
        Debug.Log(msg);

        if (guardado && errores == 0)
        {
            EditorUtility.DisplayDialog("Setup SubLevel Cable 01",
                string.Format("Escena SubLevel_Cable_01 creada.\nObjetos: {0}\nBorra SetupSubLevel_Cable_01.cs.", objetosCreados), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Setup SubLevel Cable 01",
                string.Format("Completado con {0} errores.\nRevisa la consola.", errores), "OK");
        }
    }

    static int Verificar(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj == null)
        {
            Debug.LogError("[SetupSubLevel_Cable_01] No se pudo crear: " + nombre);
            return 1;
        }
        return 0;
    }

    // === JUGADOR ===
    static void CrearJugador()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.5f, 0f);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 1f, 0f);

        player.AddComponent<PlayerCableMovement>();
        player.AddComponent<PlayerHealth>();

        // Camera dentro del jugador
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

        objetosCreados += 4; // Player, PlayerCamera, CharacterController, Camera
    }

    // === CABLES ===
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

            // Crear segmentos a lo largo del cable
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
                objetosCreados++;

                // Material normal
                Renderer rend = segmento.GetComponent<Renderer>();
                Material matNormal = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (matNormal.shader == null) matNormal = new Material(Shader.Find("Standard"));
                matNormal.color = cableColores[i];

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
                BoxCollider col = segmento.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(1f, 3f, 1f);

                // ParticleSystem
                GameObject partObj = new GameObject("Particulas");
                partObj.transform.SetParent(segmento.transform);
                partObj.transform.localPosition = Vector3.zero;
                ParticleSystem ps = partObj.AddComponent<ParticleSystem>();
                ConfigurarParticulas(ps);
                objetosCreados++;

                // AudioSource
                GameObject audioObj = new GameObject("AudioElectrico");
                audioObj.transform.SetParent(segmento.transform);
                audioObj.transform.localPosition = Vector3.zero;
                AudioSource audioSrc = audioObj.AddComponent<AudioSource>();
                audioSrc.loop = true;
                audioSrc.playOnAwake = false;
                audioSrc.spatialBlend = 1f;
                objetosCreados++;

                // CableSegment
                CableSegment cableSegment = segmento.AddComponent<CableSegment>();

                // Asignar materiales por SerializedObject
                SerializedObject so = new SerializedObject(cableSegment);
                AsignarPropiedad(so, "materialNormal", matNormal);
                AsignarPropiedad(so, "materialElectrificado", matElectrico);
                AsignarPropiedad(so, "particulas", ps);
                AsignarPropiedad(so, "audioElectrico", audioSrc);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    static void ConfigurarParticulas(ParticleSystem ps)
    {
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = Color.cyan;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 0.5f, 0.5f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        ps.Stop();
    }

    static void AsignarPropiedad(SerializedObject so, string nombre, object valor)
    {
        SerializedProperty prop = so.FindProperty(nombre);
        if (prop == null) return;

        if (valor is Material m) prop.objectReferenceValue = m;
        else if (valor is ParticleSystem p) prop.objectReferenceValue = p;
        else if (valor is AudioSource a) prop.objectReferenceValue = a;
        else if (valor is Transform t) prop.objectReferenceValue = t;
    }

    // === SPAWNER ===
    static void CrearSpawner()
    {
        GameObject spawner = new GameObject("EnemySpawner");
        spawner.transform.position = new Vector3(0f, 1f, 30f);

        EnemySpawnerCable es = spawner.AddComponent<EnemySpawnerCable>();

        // Crear prefab de enemigo temporal
        GameObject enemyPrefab = CrearPrefabEnemigo();

        // Asignar prefab al spawner
        SerializedObject so = new SerializedObject(es);
        SerializedProperty prefabProp = so.FindProperty("enemyPrefab");
        if (prefabProp != null)
        {
            prefabProp.objectReferenceValue = enemyPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Asignar jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            SerializedProperty playerProp = so.FindProperty("jugador");
            if (playerProp != null)
            {
                playerProp.objectReferenceValue = player.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        objetosCreados++;
    }

    static GameObject CrearPrefabEnemigo()
    {
        // Crear enemigo como objeto temporal (se destruira al spawner instanciar)
        GameObject enemy = new GameObject("EnemyBug_Cable");
        enemy.transform.position = new Vector3(0f, 1000f, 0f);

        // Cuerpo (cilindro)
        GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cuerpo.name = "Cuerpo";
        cuerpo.transform.SetParent(enemy.transform);
        cuerpo.transform.localPosition = Vector3.zero;
        cuerpo.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        Renderer rendCuerpo = cuerpo.GetComponent<Renderer>();
        Material matCuerpo = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (matCuerpo.shader == null) matCuerpo = new Material(Shader.Find("Standard"));
        matCuerpo.color = new Color(0.8f, 0.2f, 0.2f);
        rendCuerpo.sharedMaterial = matCuerpo;
        objetosCreados++;

        // Collider
        BoxCollider col = enemy.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1f, 2f, 1f);
        col.center = new Vector3(0f, 1f, 0f);

        // Script de movimiento
        enemy.AddComponent<EnemyCable>();

        return enemy;
    }

    // === CHAQUETA ===
    static void CrearChaqueta()
    {
        GameObject jacket = GameObject.CreatePrimitive(PrimitiveType.Cube);
        jacket.name = "JacketPickup";
        jacket.transform.position = new Vector3(0f, 1.5f, 50f);
        jacket.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        // Material amarillo/dorado
        Renderer rend = jacket.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.85f, 0f);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0f) * 2f);
        }
        rend.sharedMaterial = mat;

        // Collider trigger
        BoxCollider col = jacket.GetComponent<BoxCollider>();
        if (col == null) col = jacket.AddComponent<BoxCollider>();
        col.isTrigger = true;

        // Script
        JacketPickup jp = jacket.AddComponent<JacketPickup>();

        // Asignar jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            SerializedObject so = new SerializedObject(jp);
            SerializedProperty playerProp = so.FindProperty("jugador");
            if (playerProp != null)
            {
                playerProp.objectReferenceValue = player.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        objetosCreados++;
    }

    // === MANAGER ===
    static void CrearManager()
    {
        GameObject manager = new GameObject("SubLevelManager");
        CableSubLevelManager csm = manager.AddComponent<CableSubLevelManager>();

        // Asignar spawner
        EnemySpawnerCable spawner = FindObjectOfType<EnemySpawnerCable>();
        if (spawner != null)
        {
            SerializedObject so = new SerializedObject(csm);
            SerializedProperty spawnerProp = so.FindProperty("spawner");
            if (spawnerProp != null)
            {
                spawnerProp.objectReferenceValue = spawner;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        objetosCreados++;
    }

    // === LIMITE FINAL ===
    static void CrearLimiteFinal()
    {
        GameObject limite = new GameObject("LimiteFinal");
        limite.transform.position = new Vector3(0f, 5f, NUM_SEGMENTOS * LARGO_SEGMENTO);

        BoxCollider col = limite.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(20f, 20f, 1f);

        objetosCreados++;
    }
}
