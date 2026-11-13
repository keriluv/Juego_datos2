using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LatidoColorUI : MonoBehaviour
{
    public Color colorA = new Color(0.91f, 0.945f, 0.996f, 1f);
    public Color colorB = new Color(0.82f, 0.89f, 0.996f, 1f);
    public float velocidad = 1.1f;

    private Image imagen;
    private float tiempo;

    private void Awake()
    {
        imagen = GetComponent<Image>();
        if (imagen != null)
            imagen.color = colorA;
    }

    private void Update()
    {
        if (imagen == null) return;

        tiempo += Time.unscaledDeltaTime * velocidad;
        float k = (Mathf.Sin(tiempo) + 1f) * 0.5f;
        imagen.color = Color.Lerp(colorA, colorB, k);
    }
}
