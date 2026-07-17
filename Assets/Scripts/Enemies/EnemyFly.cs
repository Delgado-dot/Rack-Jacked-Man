using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Puntos de patrulla")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float distanciaCambio = 0.2f;

    [Header("Detección")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float distanciaDeteccion = 3f;

    [Header("Disparo")]
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float tiempoEntreDisparos = 2f;

    private float timerDisparo;
    private Transform destino;

    private void Start()
    {
        destino = puntoB;

        if (jugador == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                jugador = player.transform;
        }
    }

    private void Update()
    {
        if (puntoA == null || puntoB == null || jugador == null)
            return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaDeteccion)
        {
            // Mirar al jugador
            Vector3 dir = (jugador.position - transform.position).normalized;
            dir.y = 0;

            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180, 0);
            }

            timerDisparo -= Time.deltaTime;

            if (timerDisparo <= 0f)
            {
                Disparar();
                timerDisparo = tiempoEntreDisparos;
            }
        }
        else
        {
            // Patrullar
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino.position,
                velocidad * Time.deltaTime);

            Vector3 direccion = (destino.position - transform.position).normalized;

            if (direccion != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direccion) * Quaternion.Euler(0, 180, 0);
            }

            if (Vector3.Distance(transform.position, destino.position) < distanciaCambio)
            {
                destino = (destino == puntoA) ? puntoB : puntoA;
            }
        }
    }

    private void Disparar()
    {
        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        bala.transform.position = puntoDisparo.position;
        bala.transform.localScale = Vector3.one * 0.3f;

        Renderer r = bala.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = Color.red;
        r.material.SetColor("_EmissionColor", Color.red * 3f);
        r.material.EnableKeyword("_EMISSION");

        SphereCollider col = bala.GetComponent<SphereCollider>();
        col.isTrigger = true;

        Rigidbody rb = bala.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        EnemyProjectile proyectil = bala.AddComponent<EnemyProjectile>();
        proyectil.Inicializar((jugador.position - puntoDisparo.position).normalized);
    }
}
