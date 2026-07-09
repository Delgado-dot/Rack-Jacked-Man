using UnityEngine;
using UnityEngine.InputSystem;


public class DamageTest : MonoBehaviour
{
    private PlayerHealth playerHealth;


    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }


    private void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            playerHealth.TakeDamage(1);
        }
    }
}