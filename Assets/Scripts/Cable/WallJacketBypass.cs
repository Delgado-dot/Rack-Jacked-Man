using UnityEngine;

public class WallJacketBypass : MonoBehaviour
{
    [SerializeField] private float teleportDistance = 5f;

    private SubLevelPlayerController slpc;

    private void Start()
    {
        slpc = GetComponent<SubLevelPlayerController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (slpc == null || !slpc.TienePoder()) return;
        if (!hit.gameObject.name.Contains("Pared")) return;

        Vector3 pos = transform.position;
        pos.z += teleportDistance;
        transform.position = pos;

        Debug.Log("[WallJacketBypass] Teletransportado " + teleportDistance + "m en Z.");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttach()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<WallJacketBypass>() == null)
            player.AddComponent<WallJacketBypass>();
    }
}
