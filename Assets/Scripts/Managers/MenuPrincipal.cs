using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
        if (Manager_FlujoEscenas.Instancia != null)
            Manager_FlujoEscenas.Instancia.CargarSiguienteEscena();
        else
            Debug.LogError("No hay un Manager_FlujoEscenas en la escena.");
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
