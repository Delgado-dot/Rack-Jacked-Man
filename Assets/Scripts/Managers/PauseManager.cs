using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject panelPausa;

    [Header("Botones (se encuentran automaticamente si no se asignan)")]
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonAjustes;
    [SerializeField] private Button botonMenu;
    [SerializeField] private GameObject panelAjustes;

    private bool pausado = false;

    private void Awake()
    {
        ConectarBotones();
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        pausado = true;

        panelPausa.SetActive(true);

        PausarObjetos();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        pausado = false;

        panelPausa.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ReanudarObjetos();
    }


    public void Reiniciar()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetState();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MostrarAjustes()
    {
        if (panelAjustes != null)
            panelAjustes.SetActive(!panelAjustes.activeSelf);
        else
            Debug.LogWarning("[PauseManager] No hay un panel de ajustes asignado.");
    }


    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }


    private void PausarObjetos()
    {
        MonoBehaviour[] objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour objeto in objetos)
        {
            if (objeto is IPausable pausable)
            {
                pausable.Pausar();
            }
        }
    }


    private void ReanudarObjetos()
    {
        MonoBehaviour[] objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour objeto in objetos)
        {
            if (objeto is IPausable pausable)
            {
                pausable.Reanudar();
            }
        }
    }

    private void ConectarBotones()
    {
        if (panelPausa == null)
        {
            Debug.LogError("[PauseManager] Falta asignar PanelPause.");
            return;
        }

        Button[] botones = panelPausa.GetComponentsInChildren<Button>(true);
        botonReanudar ??= BuscarBoton(botones, "BtnReanudar");
        botonReiniciar ??= BuscarBoton(botones, "BtnReiniciar");
        botonAjustes ??= BuscarBoton(botones, "Btnajustes");
        botonMenu ??= BuscarBoton(botones, "BtnMenu");

        Conectar(botonReanudar, Reanudar);
        Conectar(botonReiniciar, Reiniciar);
        Conectar(botonAjustes, MostrarAjustes);
        Conectar(botonMenu, MenuPrincipal);
    }

    private static Button BuscarBoton(Button[] botones, string nombre)
    {
        foreach (Button boton in botones)
            if (boton.name == nombre)
                return boton;

        Debug.LogError($"[PauseManager] No se encontro el boton {nombre}.");
        return null;
    }

    private static void Conectar(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null) return;
        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
    }
}
