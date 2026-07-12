using System;
using System.Collections;
using System.Runtime.InteropServices;
using Diagnostics = System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

public class PythonPuzzleManager : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

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

    [Header("Timeout maximo (segundos)")]
    public float timeout = 60f;

    public string ElegirPuzzleAleatorio()
    {
        return puzzles[UnityEngine.Random.Range(0, puzzles.Length)];
    }

    public void EjecutarPuzzleAsync(string nombrePuzzle, Action<string> callback)
    {
        StartCoroutine(EjecutarPuzzleCoroutine(nombrePuzzle, callback));
    }

    private IEnumerator EjecutarPuzzleCoroutine(string nombrePuzzle, Action<string> callback)
    {
        string rutaCarpeta = Path.Combine(Application.streamingAssetsPath, carpetaPuzzles);

        if (!Directory.Exists(rutaCarpeta))
        {
            UnityEngine.Debug.LogError("No existe la carpeta:\n" + rutaCarpeta);
            callback?.Invoke("error");
            yield break;
        }

        // Borrar resultado.txt anterior para evitar datos stale
        string rutaResultado = Path.Combine(rutaCarpeta, "resultado.txt");
        try
        {
            if (File.Exists(rutaResultado))
                File.Delete(rutaResultado);
        }
        catch { }

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

        Diagnostics.Process proceso = null;

        try
        {
            proceso = Diagnostics.Process.Start(info);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
            callback?.Invoke("error");
            yield break;
        }

        if (proceso != null && !proceso.HasExited)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            try
            {
                if (proceso.MainWindowHandle != IntPtr.Zero)
                    SetForegroundWindow(proceso.MainWindowHandle);
            }
            catch { }
        }

        if (proceso == null)
        {
            UnityEngine.Debug.LogError("No se pudo iniciar el proceso.");
            callback?.Invoke("error");
            yield break;
        }

        string salida = "";
        string errores = "";

        Thread stdoutThread = new Thread(() =>
        {
            try { salida = proceso.StandardOutput.ReadToEnd(); }
            catch { }
        });
        stdoutThread.IsBackground = true;
        stdoutThread.Start();

        Thread stderrThread = new Thread(() =>
        {
            try { errores = proceso.StandardError.ReadToEnd(); }
            catch { }
        });
        stderrThread.IsBackground = true;
        stderrThread.Start();

        float elapsed = 0f;
        while (!proceso.HasExited && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!proceso.HasExited)
        {
            UnityEngine.Debug.LogWarning("Proceso excedio timeout, matiendo...");
            try { proceso.Kill(); } catch { }
            try { proceso.Dispose(); } catch { }
            callback?.Invoke("timeout");
            yield break;
        }

        stdoutThread.Join(2000);
        stderrThread.Join(2000);

        if (!string.IsNullOrWhiteSpace(salida))
            UnityEngine.Debug.Log(salida);

        if (!string.IsNullOrWhiteSpace(errores))
            UnityEngine.Debug.LogError(errores);

        try { proceso.Dispose(); } catch { }

        if (!File.Exists(rutaResultado))
        {
            UnityEngine.Debug.LogError("No se encontro resultado.txt");
            callback?.Invoke("error");
            yield break;
        }

        string resultado = File.ReadAllText(rutaResultado).Trim();
        callback?.Invoke(resultado);
    }
}
