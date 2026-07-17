using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Nivel_1");
    }

    public void Ranking()
    {
        ConexionRanking conexion = FindObjectOfType<ConexionRanking>(true);
        if (conexion == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            conexion = (canvas != null ? canvas.gameObject : gameObject)
                .AddComponent<ConexionRanking>();
        }

        conexion.AbrirRanking();
    }

    public void Ajustes()
    {
        Debug.Log("Abrir ajustes");
    }

    public void Salir()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
        Debug.Log("Salir");
    }
}
