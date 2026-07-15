using UnityEngine;
using UnityEngine.EventSystems;

public class BotonMenu3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public enum ActionType
    {
        Jugar,
        Ranking,
        Ajustes,
        Salir
    }

    public ActionType action;
    public float hoverScaleMultiplier = 1.1f;
    public Color hoverColor = Color.cyan;

    private Vector3 originalScale;
    private Color originalColor;
    private Renderer btnRenderer;
    private bool isHovered = false;

    void Start()
    {
        hoverScaleMultiplier = Mathf.Max(0f, hoverScaleMultiplier);
        originalScale = transform.localScale;
        btnRenderer = GetComponent<Renderer>();

        if (btnRenderer != null)
        {
            originalColor = btnRenderer.material.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnMouseDown();
    }

    public void OnMouseEnter()
    {
        if (isHovered) return;

        isHovered = true;
        transform.localScale = originalScale * hoverScaleMultiplier;

        if (btnRenderer != null)
        {
            btnRenderer.material.color = hoverColor;
        }
    }

    public void OnMouseExit()
    {
        if (!isHovered) return;

        isHovered = false;
        transform.localScale = originalScale;

        if (btnRenderer != null)
        {
            btnRenderer.material.color = originalColor;
        }
    }

    public void OnMouseDown()
    {
        ExecuteAction();
    }

    private void ExecuteAction()
    {
        switch (action)
        {
            case ActionType.Jugar:
                if (Manager_FlujoEscenas.Instancia != null)
                    Manager_FlujoEscenas.Instancia.CargarSiguienteEscena();
                else
                    Debug.LogError("No hay un Manager_FlujoEscenas en la escena.");
                break;

            case ActionType.Ranking:
                Debug.Log("Abrir ranking");
                break;

            case ActionType.Ajustes:
                Debug.Log("Abrir ajustes");
                break;

            case ActionType.Salir:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}
