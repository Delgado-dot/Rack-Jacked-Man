using UnityEngine;

[CreateAssetMenu(fileName = "CableSceneData", menuName = "Cable/Scene Data")]
public class CableSceneData : ScriptableObject
{
    [Header("Escena")]
    public string sceneName = "";
    public string displayName = "";

    [Header("Jugador")]
    public int startHealth = 3;
    public int maxHealth = 5;
    public float forwardSpeed = 20f;

    [Header("Cables")]
    public int maxCablesElectrificados = 2;
    public float warningDuration = 5f;
    public float activeDuration = 2f;

    [Header("Enemigos")]
    public int maxEnemigosActivos = 5;
    public float tiempoEntreEnemigos = 2f;

    [Header("Puntuacion")]
    public int pointsPerEnemy = 100;
    public int pointsPerLevel = 500;

    [Header("Dificultad")]
    public float intervaloAumentoDificultad = 30f;

    private int currentScore = 0;
    private int currentHealth = 3;

    public void Initialize()
    {
        currentScore = 0;
        currentHealth = startHealth;
    }

    public int GetCurrentScore() => currentScore;
    public int GetCurrentHealth() => currentHealth;
    public void SetScore(int score) => currentScore = score;
    public void SetHealth(int health) => currentHealth = Mathf.Clamp(health, 0, maxHealth);
    public void AddScore(int points) => currentScore += points;
    public void TakeDamage(int amount) => currentHealth = Mathf.Max(0, currentHealth - amount);
}
