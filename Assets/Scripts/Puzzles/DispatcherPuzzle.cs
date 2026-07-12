using UnityEngine;

public class DispatcherPuzzle : PuzzleSceneBase
{
    protected override void Start()
    {
        base.Start();
        puzzleName = "DISPATCHER";
        timeLimit = 10f;
        timer = timeLimit;
        Debug.Log("DispatcherPuzzle: Redirigiendo a puzzle aleatorio...");
    }

    public override void Complete()
    {
        base.Complete();
    }
}
