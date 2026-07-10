using UnityEngine;

/// <summary>
/// CableSubLevelManager - Coordinador del subnivel de cables.
/// Controla los segmentos electrificados, la dificultad y el estado del subnivel.
/// </summary>
public class CableSubLevelManager : MonoBehaviour
{
    [Header("Segmentos")]
    [SerializeField] private CableSegment[] segmentos;

    [Header("Electrificacion")]
    [SerializeField] private float tiempoMinElectrificado = 2f;
    [SerializeField] private float tiempoMaxElectrificado = 5f;
    [SerializeField] private float tiempoMinNormal = 3f;
    [SerializeField] private float tiempoMaxNormal = 8f;
    [SerializeField] private int maxSegmentosElectrificados = 2;

    [Header("Dificultad")]
    [SerializeField] private float intervaloAumentoDificultad = 30f;
    [SerializeField] private float reduccionIntervaloSpawner = 0.1f;
    private float timerDificultad;

    [Header("Referencias")]
    [SerializeField] private EnemySpawnerCable spawner;

    private float timerElectrificacion;
    private bool esperandoCambio = false;

    private void Start()
    {
        // Buscar todos los segmentos si no estan asignados
        if (segmentos == null || segmentos.Length == 0)
        {
            segmentos = FindObjectsOfType<CableSegment>();
        }

        // Buscar spawner si no esta asignado
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawnerCable>();
        }

        timerElectrificacion = Random.Range(tiempoMinNormal, tiempoMaxNormal);
        timerDificultad = intervaloAumentoDificultad;
    }

    private void Update()
    {
        // Sistema de electrificacion
        timerElectrificacion -= Time.deltaTime;
        if (timerElectrificacion <= 0f)
        {
            CambiarElectrificacion();
        }

        // Aumento de dificultad
        timerDificultad -= Time.deltaTime;
        if (timerDificultad <= 0f)
        {
            AumentarDificultad();
            timerDificultad = intervaloAumentoDificultad;
        }
    }

    private void CambiarElectrificacion()
    {
        if (!esperandoCambio)
        {
            // Activar electrificados aleatorios
            int segmentosActivados = 0;
            foreach (CableSegment segmento in segmentos)
            {
                if (segmentosActivados >= maxSegmentosElectrificados) break;
                if (segmento.GetEstado() == CableSegment.Estado.NORMAL)
                {
                    if (Random.value > 0.5f)
                    {
                        segmento.SetEstado(CableSegment.Estado.ELECTRIFICADO);
                        segmentosActivados++;
                    }
                }
            }

            esperandoCambio = true;
            timerElectrificacion = Random.Range(tiempoMinElectrificado, tiempoMaxElectrificado);
        }
        else
        {
            // Desactivar todos los electrificados
            foreach (CableSegment segmento in segmentos)
            {
                segmento.SetEstado(CableSegment.Estado.NORMAL);
            }

            esperandoCambio = false;
            timerElectrificacion = Random.Range(tiempoMinNormal, tiempoMaxNormal);
        }
    }

    private void AumentarDificultad()
    {
        // Reducir intervalo de spawn (mas enemigos)
        if (spawner != null)
        {
            spawner.ReducirIntervalo(reduccionIntervaloSpawner);
        }

        // Aumentar segmentos electrificados
        if (maxSegmentosElectrificados < segmentos.Length)
        {
            maxSegmentosElectrificados++;
        }

        Debug.Log("Dificultad aumentada.");
    }
}
