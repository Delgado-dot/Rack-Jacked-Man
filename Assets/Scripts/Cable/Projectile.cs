using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 50f;
    private float lifetime = 3f;
    private int damage = 1;
    private Vector3 direction;
    private Light pointLight;
    private float lightTimer;

    private void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);

        pointLight = GetComponentInChildren<Light>();
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (pointLight != null)
        {
            lightTimer += Time.deltaTime * 8f;
            pointLight.intensity = 1.5f + Mathf.Sin(lightTimer) * 0.5f;
        }
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
            SpawnImpact(transform.position);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            SpawnImpact(transform.position);
            Destroy(gameObject);
        }
    }

    private void SpawnImpact(Vector3 pos)
    {
        GameObject impact = new GameObject("Impact");
        impact.transform.position = pos;

        ParticleSystem ps = impact.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = new Color(0f, 0.8f, 1f, 1f);
        main.maxParticles = 15;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0f, 0.8f, 1f, 1f);
        renderer.material.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f, 1f) * 3f);
        renderer.material.EnableKeyword("_EMISSION");

        Destroy(impact, 0.5f);
    }

    public static void Spawn(Vector3 position, Vector3 forward)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.position = position;
        obj.transform.forward = forward;
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        Renderer r = obj.GetComponent<Renderer>();
        Color celeste = new Color(0f, 0.85f, 1f, 1f);
        r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        r.material.color = celeste;
        r.material.SetColor("_EmissionColor", celeste * 3f);
        r.material.EnableKeyword("_EMISSION");
        r.material.SetFloat("_Metallic", 0.8f);
        r.material.SetFloat("_Glossiness", 0.9f);

        Collider col = obj.GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        GameObject lightObj = new GameObject("ProjectileLight");
        lightObj.transform.SetParent(obj.transform, false);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = celeste;
        light.intensity = 1.5f;
        light.range = 5f;

        Projectile proj = obj.AddComponent<Projectile>();
    }
}
