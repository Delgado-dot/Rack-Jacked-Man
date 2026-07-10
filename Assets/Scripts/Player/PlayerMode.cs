using UnityEngine;

public class PlayerMode : MonoBehaviour
{
    public enum Mode
    {
        Normal,
        Puzzle,
        Runner
    }

    [SerializeField]
    private Mode currentMode = Mode.Normal;

    public Mode CurrentMode => currentMode;

    public bool IsRunner()
    {
        return currentMode == Mode.Runner;
    }

    public bool IsNormal()
    {
        return currentMode == Mode.Normal;
    }

    public bool IsPuzzle()
    {
        return currentMode == Mode.Puzzle;
    }

    public void ChangeMode(Mode newMode)
    {
        currentMode = newMode;
        Debug.Log("Modo cambiado a: " + currentMode);
    }
}
