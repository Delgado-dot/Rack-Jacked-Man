using UnityEngine;

public class PuzzleMana : MonoBehaviour
{
    public PlayerMode playerMode;
    public PythonPuzzleManager pythonPuzzleManager;

    private bool puzzleActive;
    private RackInteractable rackActual;

    public void StartPuzzle(RackInteractable rack)
    {
        if (puzzleActive) return;

        rackActual = rack;
        puzzleActive = true;

        Debug.Log("Puzzle iniciado para rack " + rack.GetRackIndex());

        if (playerMode != null)
            playerMode.ChangeMode(PlayerMode.Mode.Puzzle);

        string puzzleElegido = pythonPuzzleManager.ElegirPuzzleAleatorio();
        Debug.Log("Puzzle elegido: " + puzzleElegido);

        pythonPuzzleManager.EjecutarPuzzleAsync(puzzleElegido, OnPuzzleFinished);
    }

    private void OnPuzzleFinished(string resultado)
    {
        Debug.Log("Resultado puzzle: " + resultado);

        if (resultado == "resuelto")
            PuzzleCompleted();
        else
            PuzzleFailed();
    }

    public void PuzzleCompleted()
    {
        if (!puzzleActive) return;

        puzzleActive = false;
        Debug.Log("Puzzle completado");

        if (rackActual != null)
            rackActual.Repair();

        if (playerMode != null)
            playerMode.ChangeMode(PlayerMode.Mode.Normal);

        rackActual = null;
    }

    public void PuzzleFailed()
    {
        if (!puzzleActive) return;

        puzzleActive = false;
        Debug.Log("Puzzle fallido");

        if (rackActual != null)
            rackActual.OnPuzzleFailed();

        if (playerMode != null)
            playerMode.ChangeMode(PlayerMode.Mode.Normal);

        rackActual = null;
    }
}
