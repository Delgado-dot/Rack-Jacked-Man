using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// SetupBuildScenes - Agrega todas las escenas al Build Settings.
/// Ejecutar desde: Rack-Jacked-Man > Setup Build Scenes
/// </summary>
public class SetupBuildScenes
{
    [MenuItem("Rack-Jacked-Man/Setup Build Scenes")]
    public static void Execute()
    {
        string scenesPath = "Assets/Scenes";
        string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity");
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        // Orden deseado
        string[] orden =
        {
            "MenuPrincipal",
            "Nivel_1",
            "SubCable01_Copy",
            "Nivel_2",
            "Nivel_3",
            "Menu Victoria",
            "Menu GameOver",
            "Puzzle_Cables",
            "Puzzle_Dispatcher",
            "Puzzle_Nave",
            "Puzzle_Trafico",
            "Puzzle_PatchCore"
        };

        HashSet<string> added = new HashSet<string>();

        // Agregar en orden primero
        foreach (string nombre in orden)
        {
            string path = scenesPath + "/" + nombre + ".unity";
            if (File.Exists(path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                added.Add(nombre);
                Debug.Log("[SetupBuildScenes] Agregada: " + nombre);
            }
        }

        // Agregar las que queden (si alguna no estaba en la lista)
        foreach (string file in sceneFiles)
        {
            string nombre = Path.GetFileNameWithoutExtension(file);
            if (!added.Contains(nombre))
            {
                scenes.Add(new EditorBuildSettingsScene(file, true));
                added.Add(nombre);
                Debug.Log("[SetupBuildScenes] Agregada (extra): " + nombre);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[SetupBuildScenes] Total escenas en Build Settings: " + scenes.Count);
        EditorUtility.DisplayDialog("Setup Build Scenes",
            scenes.Count + " escenas agregadas al Build Settings.", "OK");
    }
}
