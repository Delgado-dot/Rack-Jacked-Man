using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 50f;
    private float lifetime = 3f;
    private int damage = 1;
    private Vector3 direction;

    private void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyCable enemy = other.GetComponent<EnemyCable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    public static void Spawn(Vector3 position, Vector3 forward)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.transform.position = position;
        obj.transform.forward = forward;
        obj.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);

        Renderer r = obj.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Standard"));
        Color celeste = new Color(0f, 0.8f, 1f, 1f);
        r.material.color = celeste;
        r.material.SetColor("_EmissionColor", celeste * 2f);
        r.material.EnableKeyword("_EMISSION");

        Collider col = obj.GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        obj.AddComponent<Projectile>();
    }
}
