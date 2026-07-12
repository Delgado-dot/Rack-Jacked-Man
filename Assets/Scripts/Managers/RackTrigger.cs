using UnityEngine;

public class RackTrigger : MonoBehaviour
{
    private bool activated = false;

    public PuzzleMana puzzleMana;
    public RackInteractable rackInteractable;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            if (puzzleMana != null && rackInteractable != null)
            {
                puzzleMana.StartPuzzle(rackInteractable);
                Debug.Log("RackTrigger: Puzzle iniciado para rack " + rackInteractable.GetRackIndex());
            }
        }
    }

    public void ResetTrigger()
    {
        activated = false;
    }
}
