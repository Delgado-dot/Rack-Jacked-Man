using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FuturisticTransition : MonoBehaviour
{
    [Header("Elementos")]
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private RectTransform scanLine;

    [Header("Configuración")]
    [SerializeField] private float fadeSpeed = 0.3f;
    [SerializeField] private float scanSpeed = 0.8f;
    [SerializeField] private float buttonFadeDuration = 0.5f;

    [Header("Menu")]
    [SerializeField] private CanvasGroup menuButtons;

    [Header("Botones Futuristas")]
    [SerializeField] private RectTransform menuTransform;
    [SerializeField] private float buttonMoveDistance = 300f;
    [SerializeField] private float buttonMoveDuration = 0.4f;
    [Header("Glitch")]
    [SerializeField] private float glitchIntensity = 0.3f;
    [SerializeField] private int glitchCycles = 5;
    [SerializeField] private float glitchSpeed = 0.05f;
    [Header("Glitch Menu Completo")]
    [SerializeField] private CanvasGroup menuGlitch;
    [SerializeField] private RectTransform menuGlitchTransform;

    [SerializeField] private float glitchMoveAmount = 8f;
    [SerializeField] private float glitchDuration = 0.5f;
    [SerializeField] private int glitchCount = 6;

    [Header("Cierre del menú")]
    [SerializeField] private RectTransform menuVideoTransform;
    [SerializeField] private float closeDuration = 0.5f;


    void Start()
    {
        
    }
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }


    IEnumerator Transition(string sceneName)
    {
        yield return StartCoroutine(HideButtonsFuturistic());

        yield return StartCoroutine(HologramGlitch());

        yield return StartCoroutine(FullMenuGlitch());

        yield return StartCoroutine(Scan());

        // pantalla completa → centro
        yield return StartCoroutine(CloseMenuVideo());

        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(sceneName);
    }




    IEnumerator HideButtonsFuturistic()
    {
        Vector2 start = menuTransform.anchoredPosition;
        Vector2 end = start + new Vector2(0, -buttonMoveDistance);

        float time = 0;


        // Baja mientras pierde señal
        while (time < buttonMoveDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.SmoothStep(0, 1, time / buttonMoveDuration);


            menuTransform.anchoredPosition =
                Vector2.Lerp(start, end, t);


            menuButtons.alpha =
                Mathf.Lerp(1, 0.2f, t);


            yield return null;
        }


        // Pequeño fallo holográfico: vuelve a aparecer
        menuButtons.alpha = 1f;

        yield return new WaitForSeconds(0.15f);
    }
    IEnumerator HologramGlitch()
    {
        for (int i = 0; i < glitchCycles; i++)
        {
            // Baja brillo
            menuButtons.alpha = glitchIntensity;

            yield return new WaitForSeconds(glitchSpeed);


            // Recupera señal
            menuButtons.alpha = 1f;

            yield return new WaitForSeconds(glitchSpeed);
        }


        // Apagado final
        float time = 0;
        float duration = 0.4f;


        while (time < duration)
        {
            time += Time.deltaTime;

            menuButtons.alpha =
                Mathf.Lerp(
                    1,
                    0,
                    time / duration
                );

            yield return null;
        }


        menuButtons.alpha = 0;
    }
    IEnumerator CloseMenuVideo()
    {
        float time = 0;

        RectTransform parent =
            menuVideoTransform.parent.GetComponent<RectTransform>();

        float width = menuVideoTransform.sizeDelta.x;
        float height = menuVideoTransform.sizeDelta.y;


        Vector2 startSize = new Vector2(
            width,
            height
        );


        Vector2 endSize = new Vector2(
            width,
            20
        );


        while (time < closeDuration)
        {
            time += Time.deltaTime;


            menuVideoTransform.sizeDelta =
                Vector2.Lerp(
                    startSize,
                    endSize,
                    time / closeDuration
                );


            yield return null;
        }


        menuVideoTransform.sizeDelta = endSize;
    }
    IEnumerator FullMenuGlitch()
    {
        Vector2 originalPos = menuGlitchTransform.anchoredPosition;


        for (int i = 0; i < glitchCount; i++)
        {
            // pequeño desplazamiento tipo interferencia
            Vector2 glitchPos = originalPos + new Vector2(
                Random.Range(-glitchMoveAmount, glitchMoveAmount),
                Random.Range(-glitchMoveAmount, glitchMoveAmount)
            );


            menuGlitchTransform.anchoredPosition = glitchPos;


            // parpadeo de señal
            menuGlitch.alpha = 0.5f;

            yield return new WaitForSeconds(0.05f);


            menuGlitch.alpha = 1f;

            yield return new WaitForSeconds(0.05f);
        }


        // volver a posición original
        menuGlitchTransform.anchoredPosition = originalPos;
    }

    IEnumerator Scan()
    {
        float time = 0;

        Vector2 start = new Vector2(0, -700);
        Vector2 end = new Vector2(0, 700);

        scanLine.anchoredPosition = start;


        while (time < scanSpeed)
        {
            time += Time.deltaTime;

            scanLine.anchoredPosition =
                Vector2.Lerp(
                    start,
                    end,
                    time / scanSpeed
                );

            yield return null;
        }

        
    }
}