using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCinematico3D : MonoBehaviour
{
    private readonly List<Transform> botones = new List<Transform>();
    private readonly List<Vector3> escalasFinales = new List<Vector3>();
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private float startTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Instalar()
    {
        if (SceneManager.GetActiveScene().name != "MenuPrincipal" || Camera.main == null)
            return;

        if (Camera.main.GetComponent<MenuCinematico3D>() == null)
            Camera.main.gameObject.AddComponent<MenuCinematico3D>();
    }

    private void Awake()
    {
        cameraPosition = transform.localPosition;
        cameraRotation = transform.localRotation;
        startTime = Time.unscaledTime;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RegistrarBoton("BotonJugar3D");
        RegistrarBoton("BotonRanking3D");
        RegistrarBoton("BotonAjustes3D");
        RegistrarBoton("BotonSalir3D");
    }

    private void RegistrarBoton(string nombre)
    {
        GameObject boton = GameObject.Find(nombre);
        if (boton == null)
            return;

        botones.Add(boton.transform);
        escalasFinales.Add(boton.transform.localScale);
        boton.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        float mouseX = Mathf.Clamp01(Input.mousePosition.x / Mathf.Max(1f, Screen.width)) - 0.5f;
        float mouseY = Mathf.Clamp01(Input.mousePosition.y / Mathf.Max(1f, Screen.height)) - 0.5f;
        Vector3 targetPosition = cameraPosition + new Vector3(mouseX * 0.08f, mouseY * 0.05f, 0f);
        Quaternion targetRotation = cameraRotation * Quaternion.Euler(-mouseY * 0.35f, mouseX * 0.55f, 0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 2.5f * Time.unscaledDeltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 2.5f * Time.unscaledDeltaTime);

        float elapsed = Time.unscaledTime - startTime;
        for (int i = 0; i < botones.Count; i++)
        {
            float progress = Mathf.Clamp01((elapsed - 0.12f * i) / 0.38f);
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            botones[i].localScale = Vector3.LerpUnclamped(Vector3.zero, escalasFinales[i], progress);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < botones.Count; i++)
        {
            if (botones[i] != null)
                botones[i].localScale = escalasFinales[i];
        }
    }
}
