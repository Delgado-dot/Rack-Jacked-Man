using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void Ranking()
    {
        Debug.Log("Abrir ranking");
    }

    public void Ajustes()
    {
        Debug.Log("Abrir ajustes");
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Salir");
    }
}