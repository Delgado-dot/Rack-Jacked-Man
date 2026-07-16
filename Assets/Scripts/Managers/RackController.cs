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
        if (reparado) return;
        reparado = true;
        Debug.Log("Rack reparado: " + tipoRack);

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCheckpoint(transform);
        }
    }

    void CompletarNivel()
    {
        Debug.Log("Nivel completado. Esperando a que la puerta se abra...");
    }
}
