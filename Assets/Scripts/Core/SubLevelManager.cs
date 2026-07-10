using UnityEngine;

/// <summary>
/// SubLevelManager - Controlador base para subniveles (interior de cables).
/// Arquitectura preparada para futura implementacion.
/// </summary>
public class SubLevelManager : MonoBehaviour
{
    [Header("Configuracion SubNivel")]
    [SerializeField] private string subLevelName = "";
    [SerializeField] private int subLevelIndex = 0;

    [Header("Referencias")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    private bool isCompleted = false;

    public void EnterSubLevel()
    {
        Debug.Log("Entrando a subnivel: " + subLevelName);
    }

    public void ExitSubLevel()
    {
        Debug.Log("Saliendo de subnivel: " + subLevelName);
        isCompleted = true;
    }

    public bool IsCompleted() { return isCompleted; }
    public string GetSubLevelName() { return subLevelName; }
}
