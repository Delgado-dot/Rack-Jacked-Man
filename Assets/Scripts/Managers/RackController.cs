using UnityEngine;

public class RackController : MonoBehaviour
{
    public enum TipoRack
    {
        Checkpoint,
        Final
    }


    public TipoRack tipoRack;

    public bool reparado = false;


    public void RepararRack()
    {
        reparado = true;


        Debug.Log(
            "Rack reparado: " + tipoRack
        );


        if (tipoRack == TipoRack.Checkpoint)
        {
            ActivarCheckpoint();
        }
        else if (tipoRack == TipoRack.Final)
        {
            CompletarNivel();
        }
    }



    void ActivarCheckpoint()
    {
        Debug.Log("Checkpoint activado");

        // Guardar partida
        // Cambiar estado del mapa
        // Activar respawn
    }



    void CompletarNivel()
    {
        Debug.Log("Nivel completado");

        // Pantalla victoria
        // Siguiente nivel
    }
}