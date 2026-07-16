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
        Debug.Log("Guardar: funcionalidad pendiente");
    }
}
