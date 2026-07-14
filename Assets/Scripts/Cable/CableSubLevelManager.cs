using UnityEngine;

public class CableSubLevelManager : MonoBehaviour
{
    [Header("Cable Groups")]
    [SerializeField] private CableGroup[] cableGroups;

    [Header("Electrificacion")]
    [SerializeField] private int maxCablesElectrificados = 2;

    [Header("Dificultad")]
    [SerializeField] private float intervaloAumentoDificultad = 30f;
    [SerializeField] private float reduccionIntervaloSpawner = 0.1f;
    [SerializeField] private int maxCablesPorNivel = 4;
    private float timerDificultad;

    [Header("Referencias")]
    [SerializeField] private EnemySpawnerCable spawner;

    private void Start()
    {
        if (cableGroups == null || cableGroups.Length == 0)
        {
            cableGroups = FindObjectsByType<CableGroup>(FindObjectsInactive.Exclude);
        }

        if (spawner == null)
        {
            spawner = FindAnyObjectByType<EnemySpawnerCable>();
        }

        maxCablesElectrificados = Mathf.Max(1, maxCablesElectrificados);
        ElectrifiedCable.SetMaxActiveCables(maxCablesElectrificados);
        timerDificultad = intervaloAumentoDificultad;

        Debug.Log("[CableSubLevelManager] Start: " + cableGroups.Length + " CableGroups, " +
            ElectrifiedCable.GetElectrifiedCount() + "/" + maxCablesElectrificados + " cables activos, " +
            "spawner=" + (spawner != null ? "OK" : "null"));

        int totalElectrifiedCables = FindObjectsByType<ElectrifiedCable>(FindObjectsInactive.Exclude).Length;
        Debug.Log("[CableSubLevelManager] ElectrifiedCables en escena: " + totalElectrifiedCables);
    }

    private void Update()
    {
        timerDificultad -= Time.deltaTime;
        if (timerDificultad <= 0f)
        {
            AumentarDificultad();
            timerDificultad = intervaloAumentoDificultad;
        }
    }

    private void AumentarDificultad()
    {
        if (spawner != null)
        {
            spawner.ReducirIntervalo(reduccionIntervaloSpawner);
        }

        if (maxCablesElectrificados < maxCablesPorNivel)
        {
            maxCablesElectrificados++;
            ElectrifiedCable.SetMaxActiveCables(maxCablesElectrificados);
        }

        Debug.Log("[CableSubLevelManager] Dificultad aumentada. Max cables activos: " + maxCablesElectrificados);
    }

    public CableGroup[] GetCableGroups() => cableGroups;
    public int GetMaxCablesElectrificados() => maxCablesElectrificados;
}
