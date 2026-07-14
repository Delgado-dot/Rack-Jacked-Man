using UnityEngine;

public class CableSubLevelManager : MonoBehaviour
{
    [Header("Cable Groups")]
    [SerializeField] private CableGroup[] cableGroups;

    [Header("Electrificacion")]
    [SerializeField] private int maxActiveGroups = 2;

    private void Start()
    {
        if (cableGroups == null || cableGroups.Length == 0)
        {
            cableGroups = FindObjectsByType<CableGroup>(FindObjectsInactive.Exclude);
        }

        CableGroup.SetMaxActiveGroups(maxActiveGroups);

        Debug.Log("[CableSubLevelManager] Start: " + cableGroups.Length + " CableGroups, max activos=" + maxActiveGroups);
    }
}
