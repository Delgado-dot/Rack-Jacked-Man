using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuSelector : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        DisableChildButtonsOnText();
        ConnectButton("Guardar y Reintentar", RestartLevel);
        ConnectButton("Reintentar", RestartLevel);
        ConnectButton("Volver al Menu", LoadMainMenu);
        ConnectButton("Salir", QuitGame);
    }

    private void DisableChildButtonsOnText()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        Button[] allButtons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            if (btn.GetComponent<TextMeshProUGUI>() != null)
            {
                btn.enabled = false;
                Debug.Log("[MenuSelector] Button desactivado en TextMeshPro: " + btn.gameObject.name);
            }
        }
    }

    private void ConnectButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = GameObject.Find(buttonName);
        if (obj == null) return;

        Button button = obj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(action);
            Debug.Log("[MenuSelector] Boton conectado: " + buttonName);
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetState();
            GameManager.Instance.SetNivelActual(1);
        }

        PlayerHealth.ResetForNewScene();
        SceneManager.LoadScene("Nivel_1");
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
