using UnityEngine;

public class DoorVisualSync : MonoBehaviour
{
    [SerializeField] private Color colorCerrado = Color.red;
    [SerializeField] private Color colorAbierto = Color.green;

    private MeshRenderer rend;
    private Color colorOriginal;

    public static System.Action onDoorOpened;
    public static System.Action onDoorClosed;

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        if (rend != null)
            colorOriginal = rend.material.color;
    }

    private void OnEnable()
    {
        onDoorOpened += OnDoorOpen;
        onDoorClosed += OnDoorClose;
    }

    private void OnDisable()
    {
        onDoorOpened -= OnDoorOpen;
        onDoorClosed -= OnDoorClose;
    }

    private void Start()
    {
        if (rend != null)
            rend.material.color = colorCerrado;
    }

    private void OnDoorOpen()
    {
        if (rend != null)
            rend.material.color = colorAbierto;
    }

    private void OnDoorClose()
    {
        if (rend != null)
            rend.material.color = colorOriginal;
    }
}
