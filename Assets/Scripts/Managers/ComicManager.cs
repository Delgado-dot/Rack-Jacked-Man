using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComicManager : MonoBehaviour
{
    [Header("Elementos UI")]
    [SerializeField] private Image comicImage;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private RectTransform comicMask;

    [Header("Historia")]
    [SerializeField] private Sprite[] images;

    [TextArea(3, 5)]
    [SerializeField] private string[] texts;

    [Header("Lectura automática")]
    [SerializeField] private float readingTime = 6f;

    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.04f;
    [Header("Sonido escritura")]
    [SerializeField] private AudioSource typingSound;
    

    [Header("Animación máscara")]
    [SerializeField] private float maskDuration = 1f;
    [SerializeField] private float startMaskHeight = 20f;

    [Header("Escena siguiente")]
    [SerializeField] private string nextSceneName = "Nivel_1";


    private int currentPage = 0;
    private bool typing = false;
    private bool changingPage = false;
    private Coroutine typingCoroutine;
    private RectTransform textTransform;



    void Start()
    {

        textTransform = storyText.GetComponent<RectTransform>();
        ShowPage();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            NextPage();
        }
    }


    void ShowPage()
    {
        if (currentPage >= images.Length)
        {
            LoadNextScene();
            return;
        }

        StartCoroutine(ChangePage());
    }

    IEnumerator ShowTextAnimation()
    {
        Vector2 finalPos = textTransform.anchoredPosition;

        Vector2 startPos = finalPos + new Vector2(0, -50);

        textTransform.anchoredPosition = startPos;


        float time = 0;
        float duration = 0.5f;


        while (time < duration)
        {
            time += Time.deltaTime;

            textTransform.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    finalPos,
                    time / duration
                );

            yield return null;
        }


        textTransform.anchoredPosition = finalPos;
    }
    IEnumerator ChangePage()
    {
        if (currentPage > 0)
            yield return StartCoroutine(CloseComicMask());
        else
            comicMask.sizeDelta = new Vector2(comicMask.rect.width, startMaskHeight);

        // Cambiar imagen
        comicImage.sprite = images[currentPage];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        storyText.text = "";

        // Abrir desde el centro
        yield return StartCoroutine(OpenComicMask());

        // Escribir texto
        yield return StartCoroutine(ShowTextAnimation());

        typingCoroutine = StartCoroutine(TypeText(texts[currentPage]));
    }


    IEnumerator TypeText(string text)
    {
        typing = true;
        storyText.text = "";


        foreach (char letter in text)
        {
            storyText.text += letter;

            if (typingSound != null && letter != ' ')
            {
                typingSound.PlayOneShot(typingSound.clip);
            }

            yield return new WaitForSeconds(typingSpeed); 
        }


        typing = false;

        changingPage = true;

        yield return new WaitForSeconds(readingTime);

        changingPage = false;

        NextPage();
    }


    void NextPage()
    {
        if (changingPage)
            return;


        if (typing)
        {
            StopCoroutine(typingCoroutine);

            storyText.text = texts[currentPage];

            typing = false;

            return;
        }


        currentPage++;
        ShowPage();
    }



    IEnumerator OpenComicMask()
    {
        float time = 0;

        RectTransform mask = comicMask;

        float finalHeight = mask.parent.GetComponent<RectTransform>().rect.height;

        // Empieza como una línea en el centro
        Vector2 startSize = new Vector2(
            mask.rect.width,
            startMaskHeight
        );

        Vector2 finalSize = new Vector2(
            mask.rect.width,
            finalHeight
        );


        mask.sizeDelta = startSize;


        while (time < maskDuration)
        {
            time += Time.deltaTime;

            mask.sizeDelta = Vector2.Lerp(
                startSize,
                finalSize,
                time / maskDuration
            );

            yield return null;
        }


        mask.sizeDelta = finalSize;
    }
    IEnumerator CloseComicMask()
    {
        float time = 0;

        RectTransform mask = comicMask;

        float finalHeight = mask.parent.GetComponent<RectTransform>().rect.height;

        Vector2 startSize = new Vector2(
            mask.rect.width,
            finalHeight
        );

        Vector2 endSize = new Vector2(
            mask.rect.width,
            startMaskHeight
        );


        while (time < maskDuration)
        {
            time += Time.deltaTime;

            mask.sizeDelta = Vector2.Lerp(
                startSize,
                endSize,
                time / maskDuration
            );

            yield return null;
        }

        mask.sizeDelta = endSize;
    }
    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}