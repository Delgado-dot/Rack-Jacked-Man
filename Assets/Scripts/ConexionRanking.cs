using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class DatoRanking
{
    public string jugador;
    public int puntos;
    public int nivel;
    public int chaquetas;
}

[Serializable]
public class ListaRanking
{
    public DatoRanking[] elementos;
}

public class ConexionRanking : MonoBehaviour
{
    private const string ClaveRankingLocal = "ranking_cache";
    private const int MaximoPuntajes = 10;

    [Header("API")]
    [SerializeField]
    private string urlRanking =
        "http://localhost:3000/api/ranking";

    [Header("Interfaz")]
    public GameObject panelRanking;
    public TMP_Text textoRanking;

    private Coroutine cargaActual;

    public void AbrirRanking()
    {
        AsegurarInterfaz();
        panelRanking.SetActive(true);
        textoRanking.text = "CARGANDO RANKING...";

        if (cargaActual != null)
            StopCoroutine(cargaActual);

        cargaActual = StartCoroutine(CargarRanking());
    }

    public void CerrarRanking()
    {
        if (panelRanking != null)
            panelRanking.SetActive(false);
    }

    private IEnumerator CargarRanking()
    {
        using UnityWebRequest peticion =
            UnityWebRequest.Get(urlRanking);
        peticion.timeout = 5;

        yield return peticion.SendWebRequest();

        if (peticion.result != UnityWebRequest.Result.Success)
        {
            // La API puede no estar iniciada durante el desarrollo. El menú debe
            // seguir funcionando y mostrar los puntajes guardados localmente.
            Debug.LogWarning("API de ranking no disponible: " + peticion.error);
            MostrarRanking(CargarRankingLocal());
            cargaActual = null;
            yield break;
        }

        string json = peticion.downloadHandler.text?.Trim();

        if (string.IsNullOrEmpty(json) || json == "null" || json == "[]")
        {
            MostrarRanking(CargarRankingLocal());
            cargaActual = null;
            yield break;
        }

        ListaRanking lista = null;
        try
        {
            // Acepta tanto un array: [...] como { "elementos": [...] }.
            string jsonAdaptado = json.StartsWith("[")
                ? "{\"elementos\":" + json + "}"
                : json;
            lista = JsonUtility.FromJson<ListaRanking>(jsonAdaptado);
        }
        catch (Exception excepcion)
        {
            Debug.LogError("Respuesta inválida del ranking: " + excepcion.Message);
        }

        if (lista == null || lista.elementos == null || lista.elementos.Length == 0)
        {
            MostrarRanking(CargarRankingLocal());
            cargaActual = null;
            yield break;
        }

        GuardarRankingLocal(lista.elementos);
        MostrarRanking(lista.elementos);
        cargaActual = null;
    }

    private void MostrarRanking(DatoRanking[] elementos)
    {
        if (elementos == null || elementos.Length == 0)
        {
            textoRanking.text = "TODAVÍA NO HAY PUNTAJES";
            return;
        }

        string contenido = "MEJORES JUGADORES\n\n";

        for (int i = 0; i < elementos.Length; i++)
        {
            contenido +=
                (i + 1) + ". " +
                NombreSeguro(elementos[i].jugador) +
                "  -  " +
                elementos[i].puntos +
                " PUNTOS" +
                (elementos[i].nivel > 0 ? "  |  NIVEL " + elementos[i].nivel : string.Empty) +
                (elementos[i].chaquetas > 0 ? "  |  CHAQUETAS " + elementos[i].chaquetas : string.Empty) +
                "\n";
        }

        textoRanking.text = contenido;
    }

    private void GuardarRankingLocal(DatoRanking[] elementos)
    {
        ListaRanking lista = new ListaRanking { elementos = elementos };
        PlayerPrefs.SetString(ClaveRankingLocal, JsonUtility.ToJson(lista));
        PlayerPrefs.Save();
    }

    public void GuardarPuntaje(string jugador, int puntos, int nivel = 0, int chaquetas = 0)
    {
        DatoRanking nuevoPuntaje = new DatoRanking
        {
            jugador = string.IsNullOrWhiteSpace(jugador) ? "JUGADOR" : jugador.Trim(),
            puntos = Mathf.Max(0, puntos),
            nivel = Mathf.Max(0, nivel),
            chaquetas = Mathf.Max(0, chaquetas)
        };

        DatoRanking[] anteriores = CargarRankingLocal();
        DatoRanking[] actualizados = new DatoRanking[anteriores.Length + 1];
        Array.Copy(anteriores, actualizados, anteriores.Length);
        actualizados[actualizados.Length - 1] = nuevoPuntaje;

        Array.Sort(actualizados, (a, b) => b.puntos.CompareTo(a.puntos));
        if (actualizados.Length > MaximoPuntajes)
            Array.Resize(ref actualizados, MaximoPuntajes);

        GuardarRankingLocal(actualizados);
        StartCoroutine(SubirPuntaje(nuevoPuntaje));
    }

    private IEnumerator SubirPuntaje(DatoRanking puntaje)
    {
        string json = JsonUtility.ToJson(puntaje);
        byte[] contenido = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest peticion = new UnityWebRequest(urlRanking, UnityWebRequest.kHttpVerbPOST);
        peticion.uploadHandler = new UploadHandlerRaw(contenido);
        peticion.downloadHandler = new DownloadHandlerBuffer();
        peticion.SetRequestHeader("Content-Type", "application/json");
        peticion.SetRequestHeader("Accept", "application/json");
        peticion.timeout = 8;

        yield return peticion.SendWebRequest();

        if (peticion.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Puntaje subido a la base de datos: {puntaje.jugador} - {puntaje.puntos}");
        }
        else
        {
            Debug.LogWarning(
                "No se pudo subir el puntaje a la base de datos. " +
                "Se conservo localmente. Error: " + peticion.error);
        }
    }

    private DatoRanking[] CargarRankingLocal()
    {
        string json = PlayerPrefs.GetString(ClaveRankingLocal, string.Empty);
        if (string.IsNullOrEmpty(json))
            return Array.Empty<DatoRanking>();

        try
        {
            ListaRanking lista = JsonUtility.FromJson<ListaRanking>(json);
            return lista?.elementos ?? Array.Empty<DatoRanking>();
        }
        catch (Exception excepcion)
        {
            Debug.LogWarning("No se pudo leer el ranking local: " + excepcion.Message);
            return Array.Empty<DatoRanking>();
        }
    }

    private static string NombreSeguro(string nombre)
    {
        return string.IsNullOrWhiteSpace(nombre) ? "ANÓNIMO" : nombre.Trim().ToUpperInvariant();
    }

    private void AsegurarInterfaz()
    {
        if (panelRanking != null && textoRanking != null)
            return;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas Ranking", typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        panelRanking = new GameObject("Panel Ranking", typeof(RectTransform), typeof(Image));
        panelRanking.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelRanking.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.15f);
        panelRect.anchorMax = new Vector2(0.8f, 0.85f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRanking.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.09f, 0.96f);

        GameObject textoObject = new GameObject("Texto Ranking", typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textoObject.transform.SetParent(panelRanking.transform, false);
        textoRanking = textoObject.GetComponent<TextMeshProUGUI>();
        textoRanking.alignment = TextAlignmentOptions.Center;
        textoRanking.fontSize = 30;
        textoRanking.color = Color.cyan;
        RectTransform textoRect = textoObject.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0.05f, 0.12f);
        textoRect.anchorMax = new Vector2(0.95f, 0.92f);
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        GameObject cerrarObject = new GameObject("Cerrar Ranking", typeof(RectTransform),
            typeof(Image), typeof(Button));
        cerrarObject.transform.SetParent(panelRanking.transform, false);
        RectTransform cerrarRect = cerrarObject.GetComponent<RectTransform>();
        cerrarRect.anchorMin = new Vector2(0.4f, 0.02f);
        cerrarRect.anchorMax = new Vector2(0.6f, 0.12f);
        cerrarRect.offsetMin = Vector2.zero;
        cerrarRect.offsetMax = Vector2.zero;
        cerrarObject.GetComponent<Image>().color = new Color(0.05f, 0.45f, 0.55f, 1f);
        cerrarObject.GetComponent<Button>().onClick.AddListener(CerrarRanking);

        GameObject etiqueta = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        etiqueta.transform.SetParent(cerrarObject.transform, false);
        TMP_Text textoCerrar = etiqueta.GetComponent<TextMeshProUGUI>();
        textoCerrar.text = "CERRAR";
        textoCerrar.alignment = TextAlignmentOptions.Center;
        textoCerrar.color = Color.white;
        RectTransform etiquetaRect = etiqueta.GetComponent<RectTransform>();
        etiquetaRect.anchorMin = Vector2.zero;
        etiquetaRect.anchorMax = Vector2.one;
        etiquetaRect.offsetMin = Vector2.zero;
        etiquetaRect.offsetMax = Vector2.zero;
    }
}
