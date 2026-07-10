using UnityEngine;

public class PuzzleMana : MonoBehaviour
{
    public PlayerMode playerMode;
    public PythonPuzzleManager pythonPuzzleManager;

    private bool puzzleActive;
    private RackController rackActual;

    private string ElegirPuzzleAleatorio()
    {
        string[] puzzles =
        {
            "cables",
            "dispatcher",
            "nave",
            "trafico",
            "patchcore"
        };

        int indice = Random.Range(0, puzzles.Length);
        return puzzles[indice];
    }

    public void StartPuzzle(RackController rack)
    {
        if (puzzleActive)
            return;

        rackActual = rack;
        puzzleActive = true;

        Debug.Log("Puzzle iniciado para: " + rackActual.tipoRack);

        playerMode.ChangeMode(PlayerMode.Mode.Puzzle);

        string puzzleElegido = ElegirPuzzleAleatorio();
        Debug.Log("Puzzle elegido: " + puzzleElegido);

        string resultado = pythonPuzzleManager.EjecutarPuzzle(puzzleElegido);
        Debug.Log("Resultado puzzle: " + resultado);

        if (resultado == "resuelto")
        {
            PuzzleCompleted();
        }
        else
        {
            PuzzleFailed();
        }
    }

    public void PuzzleCompleted()
    {
        if (!puzzleActive)
            return;

        puzzleActive = false;
        Debug.Log("Puzzle completado");

        if (rackActual != null)
        {
            rackActual.RepararRack();
        }

        playerMode.ChangeMode(PlayerMode.Mode.Normal);
        rackActual = null;
    }

    public void PuzzleFailed()
    {
        if (!puzzleActive)
            return;

        puzzleActive = false;
        Debug.Log("Puzzle fallido");

        playerMode.ChangeMode(PlayerMode.Mode.Normal);
        rackActual = null;
    }
}
