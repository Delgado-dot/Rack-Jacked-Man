using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonHandler : MonoBehaviour
{
    public void Reintentar()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetState();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuPrincipal()
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

    public void Salir()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void Guardar()
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
}
