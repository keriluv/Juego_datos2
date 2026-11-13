using System.Collections.Generic;
using UnityEngine;

public class PlataformaUI : MonoBehaviour
{
    [Tooltip("Nombre que se muestra cuando el jugador aterriza aquí.")]
    public string etiqueta = "plataforma";

    [Tooltip("Si está activo, cuenta como una de las 5 extensiones del tutorial.")]
    public bool esExtension = false;

    [Tooltip("Número de orden dentro de la fila de extensiones (1-5).")]
    public int indiceExtension = 0;

    public static readonly List<PlataformaUI> Todas = new List<PlataformaUI>();

    private static readonly Vector3[] Esquinas = new Vector3[4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void LimpiarLista()
    {
        Todas.Clear();
    }

    public RectTransform RectPlataforma => (RectTransform)transform;

    private void OnEnable()
    {
        if (!Todas.Contains(this)) Todas.Add(this);
    }

    private void OnDisable()
    {
        Todas.Remove(this);
    }

    public Rect RectLocal(RectTransform mundo) => RectDe(mundo, RectPlataforma);

    public static Rect RectDe(RectTransform mundo, RectTransform objetivo)
    {
        if (objetivo == null) return new Rect();

        objetivo.GetWorldCorners(Esquinas);
        Vector3 min = mundo != null ? mundo.InverseTransformPoint(Esquinas[0]) : Esquinas[0];
        Vector3 max = mundo != null ? mundo.InverseTransformPoint(Esquinas[2]) : Esquinas[2];

        return Rect.MinMaxRect(
            Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }
}
