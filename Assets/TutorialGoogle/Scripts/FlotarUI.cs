using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FlotarUI : MonoBehaviour
{
    public float amplitud = 7f;
    public float velocidad = 1.2f;

    private RectTransform rect;
    private Vector2 posicionBase;
    private float tiempo;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        posicionBase = rect.anchoredPosition;
    }

    private void Update()
    {
        tiempo += Time.unscaledDeltaTime * velocidad;

        Vector2 posicion = posicionBase;
        posicion.y += Mathf.Sin(tiempo) * amplitud;
        rect.anchoredPosition = posicion;
    }
}
