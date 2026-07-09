using UnityEngine;
using UnityEngine.SceneManagement;
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
        originalScale = transform.localScale;
        btnRenderer = GetComponent<Renderer>();
        if (btnRenderer != null)
        {
            // Note: Since we will set the default non-hover color to neon cyberpunk colors,
            // we should dynamically read the color in Start so that OnMouseExit reverts back to it.
            originalColor = btnRenderer.material.color;
        }
    }

    // Callbacks from EventSystem (IPointer*Handler) to ensure compatibility with New Input System
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

    // Standard Unity physics pointer callbacks
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
                SceneManager.LoadScene("PlayerTest");
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
