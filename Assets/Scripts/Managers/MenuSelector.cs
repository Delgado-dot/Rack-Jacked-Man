using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuSelector : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        DisableChildButtonsOnText();
        ConnectButton("Guardar y Reintentar", SaveAndRestart);
        ConnectButton("Reintentar", RestartLevel);
        ConnectButton("Volver al Menu", LoadMainMenu);
        ConnectButton("Salir", QuitGame);
        MostrarResultadoFinal();
    }

    private void DisableChildButtonsOnText()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn.GetComponent<TextMeshProUGUI>() != null)
            {
                btn.enabled = false;
                Debug.Log("[MenuSelector] Button desactivado en TextMeshPro: " + btn.gameObject.name);
            }
        }
    }

    private void ConnectButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = GameObject.Find(buttonName);
        if (obj == null) return;

        Button button = obj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(action);
            Debug.Log("[MenuSelector] Boton conectado: " + buttonName);
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetState();
            GameManager.Instance.SetNivelActual(1);
        }

        PlayerHealth.ResetForNewScene();
        SceneManager.LoadScene("Nivel_1");
    }

    private void SaveAndRestart()
    {
        GuardarResultado();
        RestartLevel();
    }

    private void GuardarResultado()
    {
        int puntos = GameManager.Instance != null ? GameManager.Instance.GetPuntos() : 0;
        int nivel = GameManager.Instance != null ? GameManager.Instance.GetNivelActual() : 1;
        int chaquetas = GameManager.Instance != null ? GameManager.Instance.GetChaquetasUsadas() : 0;
        string jugador = PlayerPrefs.GetString("jugador_nombre", "JUGADOR");

        ConexionRanking conexion = FindObjectOfType<ConexionRanking>(true);
        if (conexion == null)
            conexion = gameObject.AddComponent<ConexionRanking>();

        conexion.GuardarPuntaje(jugador, puntos, nivel, chaquetas);
    }

    private void MostrarResultadoFinal()
    {
        int puntos = GameManager.Instance != null ? GameManager.Instance.GetPuntos() : 0;
        int chaquetas = GameManager.Instance != null ? GameManager.Instance.GetChaquetasUsadas() : 0;

        TMP_Text puntajeTexto = BuscarTexto("Puntaje Final");
        if (puntajeTexto != null)
            puntajeTexto.text = "Puntaje Final: " + puntos;

        TMP_Text chaquetasTexto = BuscarTexto("Chaquetas");
        if (chaquetasTexto != null)
            chaquetasTexto.text = "Chaquetas: " + chaquetas;

        TMP_Text nivelTexto = BuscarTexto("Nivel Alcanzado");
        if (nivelTexto != null)
            nivelTexto.text = "Nivel Alcanzado: " +
                (GameManager.Instance != null ? GameManager.Instance.GetNivelActual() : 1);
    }

    private TMP_Text BuscarTexto(string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);
        return objeto != null ? objeto.GetComponent<TMP_Text>() : null;
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
