using System;
using System.IO;
using UnityEngine;
using Diagnostics = System.Diagnostics;

public class PythonPuzzleManager : MonoBehaviour
{
    [Header("Carpeta dentro de StreamingAssets")]
    public string carpetaPuzzles = "Puzzles_Python";

    [Header("Launcher")]
    public string launcher = "launcher.py";

    [Header("Puzzles disponibles")]
    public string[] puzzles =
    {
        "cables",
        "dispatcher",
        "nave",
        "trafico",
        "patchcore"
    };

    [Header("Usar ejecutable")]
    public bool usarExe = false;

    [Header("Nombre del ejecutable")]
    public string ejecutable = "launcher.exe";

    public string EjecutarPuzzleAleatorio()
    {
        string puzzleElegido = puzzles[UnityEngine.Random.Range(0, puzzles.Length)];
        Debug.Log("Puzzle seleccionado: " + puzzleElegido);
        return EjecutarPuzzle(puzzleElegido);
    }

    public string EjecutarPuzzle(string nombrePuzzle)
    {
        string rutaCarpeta = Path.Combine(Application.streamingAssetsPath, carpetaPuzzles);

        if (!Directory.Exists(rutaCarpeta))
        {
            Debug.LogError("No existe la carpeta:\n" + rutaCarpeta);
            return "error";
        }

        try
        {
            Diagnostics.ProcessStartInfo info = new Diagnostics.ProcessStartInfo();

            if (usarExe)
            {
                info.FileName = Path.Combine(rutaCarpeta, ejecutable);
                info.Arguments = nombrePuzzle;
            }
            else
            {
                info.FileName = "python";
                info.Arguments = $"{launcher} {nombrePuzzle}";
            }

            info.WorkingDirectory = rutaCarpeta;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            Diagnostics.Process proceso = Diagnostics.Process.Start(info);

            if (proceso == null)
            {
                Debug.LogError("No se pudo iniciar el proceso.");
                return "error";
            }

            string salida = proceso.StandardOutput.ReadToEnd();
            string errores = proceso.StandardError.ReadToEnd();

            proceso.WaitForExit();

            if (!string.IsNullOrWhiteSpace(salida))
                Debug.Log(salida);

            if (!string.IsNullOrWhiteSpace(errores))
                Debug.LogError(errores);

            string rutaResultado = Path.Combine(rutaCarpeta, "resultado.txt");

            if (!File.Exists(rutaResultado))
            {
                Debug.LogError("No se encontró resultado.txt");
                return "error";
            }

            return File.ReadAllText(rutaResultado).Trim();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return "error";
        }
    }
}
