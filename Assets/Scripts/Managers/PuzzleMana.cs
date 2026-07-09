using UnityEngine;

public class PuzzleMana : MonoBehaviour
{
    public PlayerMode playerMode;
    public PythonPuzzleManager pythonPuzzleManager;

    private bool puzzleActive;

    // Guarda el rack que activó el puzzle
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


        int indice = Random.Range(
            0,
            puzzles.Length
        );


        return puzzles[indice];
    }
    public void StartPuzzle(RackController rack)
    {
        if (puzzleActive)
            return;


        rackActual = rack;

        puzzleActive = true;


        Debug.Log(
            "Puzzle iniciado para: " + rackActual.tipoRack
        );


        playerMode.ChangeMode(
            PlayerMode.Mode.Puzzle
        );


        // Por ahora pruebas con cables
        string puzzleElegido = ElegirPuzzleAleatorio();


        Debug.Log(
            "Puzzle elegido: " + puzzleElegido
        );


        string resultado =
            pythonPuzzleManager.EjecutarPuzzle(puzzleElegido);


        Debug.Log(
            "Resultado puzzle: " + resultado
        );


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


        // Reparar el rack que llamó al puzzle
        if (rackActual != null)
        {
            rackActual.RepararRack();
        }


        playerMode.ChangeMode(
            PlayerMode.Mode.Runner
        );


        rackActual = null;
    }



    public void PuzzleFailed()
    {
        if (!puzzleActive)
            return;


        puzzleActive = false;


        Debug.Log("Puzzle fallido");


        playerMode.ChangeMode(
            PlayerMode.Mode.Normal
        );


        rackActual = null;
    }
}