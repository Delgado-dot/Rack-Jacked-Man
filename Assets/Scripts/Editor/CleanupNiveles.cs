using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Limpieza de duplicados y configuración de Nivel_2 y Nivel_3.
/// Ejecutar: Rack-Jacked-Man > Cleanup Nivel 2 / Cleanup Nivel 3
/// </summary>
public class CleanupNiveles : EditorWindow
{
    [MenuItem("Rack-Jacked-Man/Cleanup Nivel 2")]
    static void CleanupNivel2()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_2.unity", OpenSceneMode.Single);
        Debug.Log("[Cleanup] === Nivel_2 ===");

        int deleted = 0;

        // ─── 1. Eliminar Rack_N2_12 duplicado ──────────────────────
        // Buscar todos los objetos名为 Rack_N2_12, quedarnos con el primero
        List<GameObject> rack12List = FindByName("Rack_N2_12");
        if (rack12List.Count > 1)
        {
            // Mantener el primero, eliminar el resto
            for (int i = 1; i < rack12List.Count; i++)
            {
                Debug.Log("[Cleanup] Nivel_2: Eliminando Rack_N2_12 duplicado: " + rack12List[i].name +
                    " (entityId: " + rack12List[i].GetEntityId() + ")");
                Object.DestroyImmediate(rack12List[i]);
                deleted++;
            }
        }
        else if (rack12List.Count == 0)
        {
            Debug.LogWarning("[Cleanup] Nivel_2: No se encontro Rack_N2_12");
        }
        else
        {
            Debug.Log("[Cleanup] Nivel_2: Rack_N2_12 unico, OK");
        }

        // ─── 2. Fix Rack_N2_06 trailing space ──────────────────────
        List<GameObject> rack06List = FindByName("Rack_N2_06 ");
        if (rack06List.Count > 0)
        {
            rack06List[0].name = "Rack_N2_06";
            Debug.Log("[Cleanup] Nivel_2: Rack_N2_06 nombre corregido (eliminado espacio trailing)");
        }

        // Verificar también el correcto
        List<GameObject> rack06Correct = FindByName("Rack_N2_06");
        if (rack06Correct.Count > 1)
        {
            Debug.LogWarning("[Cleanup] Nivel_2: Multiples Rack_N2_06 encontrados: " + rack06Correct.Count);
        }

        // ─── 3. Verificar PuertaPared_N2_01 ────────────────────────
        List<GameObject> puertas = FindByName("PuertaPared_N2_01");
        if (puertas.Count > 0)
        {
            GameObject puerta = puertas[0];
            MeshRenderer renderer = puerta.GetComponent<MeshRenderer>();
            BoxCollider col = puerta.GetComponent<BoxCollider>();
            Debug.Log("[Cleanup] Nivel_2: PuertaPared_N2_01 - Renderer: " +
                (renderer != null ? "SI" : "NO") + " | Collider: " +
                (col != null ? "SI (isTrigger=" + col.isTrigger + ")" : "NO"));
        }

        // ─── 4. Verificar Trigger_SalidaNivel2 ─────────────────────
        List<GameObject> triggers = FindByName("Trigger_SalidaNivel2");
        if (triggers.Count > 0)
        {
            GameObject trigger = triggers[0];
            ObjectiveDoorController door = trigger.GetComponent<ObjectiveDoorController>();
            PuertaCambioNivel puertaCambio = trigger.GetComponent<PuertaCambioNivel>();
            BoxCollider col = trigger.GetComponent<BoxCollider>();
            Debug.Log("[Cleanup] Nivel_2: Trigger_SalidaNivel2 - " +
                "ObjectiveDoorController: " + (door != null ? "SI" : "NO") +
                " | PuertaCambioNivel: " + (puertaCambio != null ? "SI (→ " + puertaCambio.nombreEscena + ")" : "NO") +
                " | BoxCollider: " + (col != null ? "SI (isTrigger=" + col.isTrigger + ")" : "NO"));
        }

        // ─── 5. Verificar racks con scripts ────────────────────────
        string[] rackNames = { "Rack_N2_05", "Rack_N2_12" };
        foreach (string name in rackNames)
        {
            List<GameObject> racks = FindByName(name);
            foreach (GameObject rack in racks)
            {
                RackInteractable ri = rack.GetComponent<RackInteractable>();
                RackState rs = rack.GetComponent<RackState>();
                Debug.Log("[Cleanup] Nivel_2: " + rack.name + " - RackInteractable: " +
                    (ri != null ? "SI (index=" + ri.GetRackIndex() + ")" : "NO") +
                    " | RackState: " + (rs != null ? "SI" : "NO"));
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Cleanup] Nivel_2 completado. Eliminados: " + deleted);
        EditorUtility.DisplayDialog("Cleanup Nivel 2",
            "Duplicados eliminados: " + deleted +
            "\nRack_N2_06: nombre corregido" +
            "\nVerificar consola para detalles.", "OK");
    }

    [MenuItem("Rack-Jacked-Man/Cleanup Nivel 3")]
    static void CleanupNivel3()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Nivel_3.unity", OpenSceneMode.Single);
        Debug.Log("[Cleanup] === Nivel_3 ===");

        int deleted = 0;

        // ─── 1. Eliminar Server Rack (8) duplicados ────────────────
        // El prefab se instanció 4 veces. Mantener solo la primera.
        List<GameObject> rack8List = FindByName("Server Rack (8)");
        Debug.Log("[Cleanup] Nivel_3: Server Rack (8) encontrados: " + rack8List.Count);
        if (rack8List.Count > 1)
        {
            for (int i = 1; i < rack8List.Count; i++)
            {
                Debug.Log("[Cleanup] Nivel_3: Eliminando Server Rack (8) duplicado #" + (i + 1));
                Object.DestroyImmediate(rack8List[i]);
                deleted++;
            }
        }

        // ─── 2. Eliminar TV 32 inch 2 duplicado ────────────────────
        List<GameObject> tv2List = FindByName("TV 32 inch 2");
        Debug.Log("[Cleanup] Nivel_3: TV 32 inch 2 encontrados: " + tv2List.Count);
        if (tv2List.Count > 1)
        {
            // Mantener el primero, eliminar el resto
            for (int i = 1; i < tv2List.Count; i++)
            {
                Debug.Log("[Cleanup] Nivel_3: Eliminando TV 32 inch 2 duplicado #" + (i + 1));
                Object.DestroyImmediate(tv2List[i]);
                deleted++;
            }
        }

        // ─── 3. Habilitar HUDCanvas ────────────────────────────────
        List<GameObject> hudList = FindByName("HUDCanvas");
        if (hudList.Count > 0)
        {
            GameObject hud = hudList[0];
            if (!hud.activeSelf)
            {
                hud.SetActive(true);
                Debug.Log("[Cleanup] Nivel_3: HUDCanvas habilitado");
            }
            else
            {
                Debug.Log("[Cleanup] Nivel_3: HUDCanvas ya esta habilitado");
            }
        }

        // ─── 4. Verificar Player ───────────────────────────────────
        List<GameObject> players = FindByName("Player");
        if (players.Count > 0)
        {
            GameObject player = players[0];
            Debug.Log("[Cleanup] Nivel_3: Player components - " +
                "CharacterController: " + (player.GetComponent<CharacterController>() != null ? "SI" : "NO") +
                " | PlayerMovement: " + (player.GetComponent<PlayerMovement>() != null ? "SI" : "NO") +
                " | PlayerHealth: " + (player.GetComponent<PlayerHealth>() != null ? "SI" : "NO") +
                " | PlayerInteract: " + (player.GetComponent<PlayerInteract>() != null ? "SI" : "NO") +
                " | PlayerMode: " + (player.GetComponent<PlayerMode>() != null ? "SI" : "NO") +
                " | Tag: " + player.tag);
        }

        // ─── 5. Verificar Server Racks ─────────────────────────────
        string[] rackNames = { "Server Rack (3)", "Server Rack (5)", "Server Rack (6)", "Server Rack (8)" };
        foreach (string name in rackNames)
        {
            List<GameObject> racks = FindByName(name);
            foreach (GameObject rack in racks)
            {
                RackInteractable ri = rack.GetComponent<RackInteractable>();
                RackState rs = rack.GetComponent<RackState>();
                BoxCollider col = rack.GetComponent<BoxCollider>();
                Debug.Log("[Cleanup] Nivel_3: " + rack.name + " - " +
                    "RackInteractable: " + (ri != null ? "SI" : "NO") +
                    " | RackState: " + (rs != null ? "SI" : "NO") +
                    " | BoxCollider: " + (col != null ? "SI" : "NO"));
            }
        }

        // ─── 6. Verificar TV 32 inch 2 ─────────────────────────────
        List<GameObject> tv2 = FindByName("TV 32 inch 2");
        if (tv2.Count > 0)
        {
            GameObject tv = tv2[0];
            Debug.Log("[Cleanup] Nivel_3: TV 32 inch 2 - " +
                "ObjectiveDoorController: " + (tv.GetComponent<ObjectiveDoorController>() != null ? "SI" : "NO") +
                " | PuertaCambioNivel: " + (tv.GetComponent<PuertaCambioNivel>() != null ? "SI" : "NO") +
                " | BoxCollider: " + (tv.GetComponent<BoxCollider>() != null ? "SI" : "NO"));
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Cleanup] Nivel_3 completado. Eliminados: " + deleted);
        EditorUtility.DisplayDialog("Cleanup Nivel 3",
            "Duplicados eliminados: " + deleted +
            "\nHUDCanvas habilitado" +
            "\nVerificar consola para detalles.", "OK");
    }

    static List<GameObject> FindByName(string name)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == name && go.scene.isLoaded)
                result.Add(go);
        }
        return result;
    }
}
