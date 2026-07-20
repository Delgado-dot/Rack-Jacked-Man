using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_FlujoEscenas : MonoBehaviour
{
    public static Manager_FlujoEscenas Instancia { get; private set; }

    [Header("Inicio del juego")]
    [Tooltip("Solo fuerza MenuPrincipal al iniciar el juego, no en cada carga de escena.")]
    [SerializeField] private bool comenzarDesdeLaPrimeraEscena = true;

    [Header("Orden de las escenas")]
    [Tooltip("Las escenas deben estar agregadas también en File > Build Settings.")]
    [SerializeField] private string[] escenasEnOrden =
    {
        "MenuPrincipal",
        "ComicIntro",
        "Nivel_1",
        "SubCable01_Copy",
        "Nivel_2",
        "Nivel_3",
        "Menu Victoria",
        "Menu GameOver"
    };

    private static bool juegoIniciado = false;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (comenzarDesdeLaPrimeraEscena && !juegoIniciado && escenasEnOrden.Length > 0 &&
            SceneManager.GetActiveScene().name != escenasEnOrden[0])
        {
            juegoIniciado = true;
            CargarEscena(escenasEnOrden[0]);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearAutomaticamente()
    {
        if (Instancia == null)
        {
            GameObject objetoManager = new GameObject("Manager_FlujoEscenas");
            objetoManager.AddComponent<Manager_FlujoEscenas>();
        }
    }

    public void CargarSiguienteEscena()
    {
        int indiceActual = ObtenerIndice(SceneManager.GetActiveScene().name);

        if (indiceActual < 0)
        {
            Debug.LogError("La escena actual no está configurada en Manager_FlujoEscenas.");
            return;
        }

        int siguienteIndice = indiceActual + 1;
        if (siguienteIndice >= escenasEnOrden.Length)
        {
            Debug.Log("No hay una escena siguiente configurada.");
            return;
        }

        CargarEscena(escenasEnOrden[siguienteIndice]);
    }

    public void CargarMenuPrincipal()
    {
        if (escenasEnOrden.Length > 0)
            CargarEscena(escenasEnOrden[0]);
    }

    public void ReiniciarEscena()
    {
        CargarEscena(SceneManager.GetActiveScene().name);
    }

    public void CargarEscena(string nombreEscena)
    {
        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogError("El nombre de la escena está vacío.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError($"La escena '{nombreEscena}' no existe o no está agregada en Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    private int ObtenerIndice(string nombreEscena)
    {
        for (int i = 0; i < escenasEnOrden.Length; i++)
        {
            if (escenasEnOrden[i] == nombreEscena)
                return i;
        }

        return -1;
    }
}
