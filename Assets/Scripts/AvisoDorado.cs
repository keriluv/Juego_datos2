using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AvisoDorado : MonoBehaviour
{
    [Header("Opcional: franja oscura detras del texto")]
    public GameObject fondo;

    [Header("Entrada")]
    public float duracionEntrada = 0.6f;
    public float rebote = 1.70158f;

    [Header("Pulso")]
    public float velocidadPulso = 3f;
    public float intensidadPulso = 0.05f;

    [Header("Flotar")]
    public float alturaFlote = 6f;
    public float velocidadFlote = 2f;

    [Header("Salida")]
    public float duracionSalida = 0.5f;
    public float alturaSalida = 80f;

    CanvasGroup grupo;
    RectTransform rt;
    Vector2 posInicial;
    Vector3 escalaBase;
    bool listo;

    void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        rt = GetComponent<RectTransform>();
        posInicial = rt.anchoredPosition;
        escalaBase = rt.localScale;
        listo = true;
    }

    void OnEnable()
    {
        if (!listo) return;
        if (fondo != null) fondo.SetActive(true);
        rt.anchoredPosition = posInicial;
        StopAllCoroutines();
        StartCoroutine(Entrada());
    }

    void OnDisable()
    {
        if (fondo != null) fondo.SetActive(false);
    }

    public void Ocultar()
    {
        StopAllCoroutines();
        StartCoroutine(Salida());
    }

    float EaseOutBack(float t)
    {
        float c3 = rebote + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3) + rebote * Mathf.Pow(t - 1f, 2);
    }

    IEnumerator Entrada()
    {
        grupo.alpha = 0f;
        rt.localScale = Vector3.zero;

        float t = 0f;
        while (t < duracionEntrada)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracionEntrada);
            rt.localScale = escalaBase * EaseOutBack(k);
            grupo.alpha = Mathf.Clamp01(k * 3f);
            yield return null;
        }

        grupo.alpha = 1f;

        float tiempo = 0f;
        while (true)
        {
            tiempo += Time.unscaledDeltaTime;
            float pulso = 1f + Mathf.Sin(tiempo * velocidadPulso) * intensidadPulso;
            rt.localScale = escalaBase * pulso;
            rt.anchoredPosition = posInicial + Vector2.up * Mathf.Sin(tiempo * velocidadFlote) * alturaFlote;
            yield return null;
        }
    }

    IEnumerator Salida()
    {
        Vector2 inicio = rt.anchoredPosition;
        float t = 0f;
        while (t < duracionSalida)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracionSalida);
            grupo.alpha = 1f - k;
            rt.anchoredPosition = inicio + Vector2.up * (alturaSalida * k);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}