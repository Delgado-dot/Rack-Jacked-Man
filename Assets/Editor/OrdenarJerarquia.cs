using UnityEngine;
using UnityEditor;
using System.Linq;

public class OrdenarJerarquia : EditorWindow
{
    [MenuItem("Herramientas/Ordenar Hijos Por Nombre")]
    public static void OrdenarSeleccion()
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Selecciona un objeto en la jerarquía primero.");
            return;
        }

        OrdenarRecursivo(Selection.activeTransform);
        Debug.Log("Jerarquía ordenada por nombre dentro de: " + Selection.activeTransform.name);
    }

    private static void OrdenarRecursivo(Transform padre)
    {
        var hijos = padre.Cast<Transform>()
            .OrderBy(h => h.name)
            .ToList();

        for (int i = 0; i < hijos.Count; i++)
        {
            hijos[i].SetSiblingIndex(i);
            OrdenarRecursivo(hijos[i]);
        }
    }
}