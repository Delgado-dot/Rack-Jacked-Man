using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyVirus : MonoBehaviour, IPausable
{
    [Header("Jugador")]
    public Transform player;

    [Header("Movimiento")]
    public float detectionRange = 10f;
    public float wanderRadius = 8f;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        GoToRandomPoint();
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Si el jugador está cerca, perseguirlo
        if (distance <= detectionRange)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Si llegó a su destino, buscar otro punto aleatorio
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                GoToRandomPoint();
            }
        }
    }

    private void GoToRandomPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * wanderRadius;
        randomPoint += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    public void Pausar()
    {
        agent.isStopped = true;
    }

    public void Reanudar()
    {
        agent.isStopped = false;
    }
}