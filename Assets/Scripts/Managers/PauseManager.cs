using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject panelPausa;

    private bool pausado = false;

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
    }

    public void Reanudar()
    {
        pausado = false;

        panelPausa.SetActive(false);

        Time.timeScale = 1f;

        ReanudarObjetos();
    }


    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}