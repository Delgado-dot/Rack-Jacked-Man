using UnityEngine;

/// <summary>
/// RackStart - Punto de aparicion inicial del jugador.
/// Se busca automaticamente por GameManager al inicio.
/// Estructura: BoxCollider (hitbox) + RackStart.cs + Model (vacio para FBX futuro).
/// </summary>
public class RackStart : MonoBehaviour
{
    private void Start()
    {
        // RackStart es un marcador de posicion.
        // GameManager lo busca por nombre "RackStart" para establecer
        // el primer checkpoint y la posicion de spawn inicial.
    }
}
