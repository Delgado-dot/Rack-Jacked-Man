using UnityEngine;


public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public int maxLives = 3;

    private int currentLives;

    private bool isDead;


    private void Awake()
    {
        currentLives = maxLives;
    }


    public void TakeDamage(int damage)
    {
        if (isDead)
            return;


        currentLives -= damage;

        currentLives = Mathf.Max(currentLives, 0);


        Debug.Log("Vida actual: " + currentLives);


        if (currentLives <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        isDead = true;

        Debug.Log("Jugador muerto");
    }


    public int GetCurrentLives()
    {
        return currentLives;
    }
}