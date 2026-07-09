using UnityEngine;


public class RackTrigger : MonoBehaviour
{
    private bool activated = false;


    public PuzzleMana puzzleMana;


    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;


        if (other.CompareTag("Player"))
        {
            activated = true;


            puzzleMana.StartPuzzle();


            Debug.Log("Rack alcanzado");
        }
    }
}