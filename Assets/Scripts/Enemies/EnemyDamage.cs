using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float timer;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null || PlayerHealth.IsDead())
            return;

        timer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && timer <= 0f)
        {
            PlayerHealth.TakeDamage(damage);
            timer = attackCooldown;
        }
    }
}