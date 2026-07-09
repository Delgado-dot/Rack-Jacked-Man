using UnityEngine;

public class PuzzleMana : MonoBehaviour
{
    public PlayerMode playerMode;
    public PythonPuzzleManager pythonPuzzleManager;

    private bool puzzleActive;

    private string[] puzzles =
    {
        "cables",
        "dispatcher",
        "nave",
        "trafico",
        "patchcore"
    };


    public void StartPuzzle()
    {
        if (puzzleActive)
            return;


        puzzleActive = true;


        Debug.Log("Puzzle iniciado");


        playerMode.ChangeMode(
            PlayerMode.Mode.Puzzle
        );


        // Elegir puzzle aleatorio
        string puzzleElegido =
            puzzles[Random.Range(0, puzzles.Length)];


        Debug.Log("Puzzle elegido: " + puzzleElegido);


        // Ejecutar Python
        string resultado =
            pythonPuzzleManager.EjecutarPuzzle(puzzleElegido);


        Debug.Log("Resultado Python: " + resultado);



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
        puzzleActive = false;

        Debug.Log("Puzzle completado");

        playerMode.ChangeMode(
            PlayerMode.Mode.Runner
        );
    }



    public void PuzzleFailed()
    {
        puzzleActive = false;

        Debug.Log("Puzzle fallido");

        playerMode.ChangeMode(
            PlayerMode.Mode.Normal
        );
    }
}