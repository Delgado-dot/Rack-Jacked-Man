using UnityEngine;

public class RackTrigger : MonoBehaviour
{
    private bool activated = false;

    public PuzzleMana puzzleMana;
    public RackController rackController;

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            puzzleMana.StartPuzzle(rackController);
            Debug.Log("Rack alcanzado: " + rackController.tipoRack);
        }
    }
}
