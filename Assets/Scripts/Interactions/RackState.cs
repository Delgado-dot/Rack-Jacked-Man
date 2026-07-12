using UnityEngine;

public class RackState : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool repaired = false;

    [Header("Colores")]
    private Color offColor = new Color(0.3f, 0.3f, 0.35f);
    private Color onColor = new Color(0.2f, 0.9f, 0.4f);

    private Renderer rackRenderer;
    private Light[] leds;
    private Material blockMat;

    private void Start()
    {
        rackRenderer = GetComponent<Renderer>();
        if (rackRenderer != null)
        {
            blockMat = rackRenderer.material;
            blockMat.color = offColor;
        }

        leds = GetComponentsInChildren<Light>();
        SetLEDs(false);

        Transform panel = transform.Find("FrontPanel");
        if (panel != null)
        {
            Renderer panelRenderer = panel.GetComponent<Renderer>();
            if (panelRenderer != null)
            {
                blockMat = panelRenderer.material;
                blockMat.color = offColor;
            }
        }
    }

    public void SetRepaired(bool state)
    {
        repaired = state;

        if (repaired)
        {
            if (blockMat != null) blockMat.color = onColor;
            SetLEDs(true);
            Debug.Log("RackState: " + gameObject.name + " ENCENDIDO");
        }
        else
        {
            if (blockMat != null) blockMat.color = offColor;
            SetLEDs(false);
            Debug.Log("RackState: " + gameObject.name + " APAGADO");
        }
    }

    private void SetLEDs(bool on)
    {
        if (leds == null) return;
        foreach (Light led in leds)
        {
            if (led != null)
            {
                led.enabled = on;
                if (on)
                {
                    led.color = Color.green;
                    led.intensity = 2f;
                }
            }
        }
    }

    public bool IsRepaired() { return repaired; }
}
