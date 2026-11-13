using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VinetaHalloween : MonoBehaviour
{
    public static VinetaHalloween Instancia;

    [Header("Referencias")]
    [Tooltip("Imagen de la vineta a pantalla completa.")]
    public Image vineta;

    [Tooltip("Opcional: audio de ambiente que baja de volumen al entrar.")]
    public AudioSource ambiente;

    [Header("Ajustes")]
    [Range(0f, 1f)] public float alphaMaximo = 0.85f;
    public float duracionEntrada = 1.4f;
    public float duracionSalida = 0.9f;

    [Tooltip("Cantidad de pulso sobre la oscuridad. 0 lo desactiva.")]
    [Range(0f, 0.2f)] public float pulso = 0.035f;
    public float velocidadPulso = 1.2f;

    [Tooltip("Si esta activado, la vineta empieza ya oscura.")]
    public bool empezarOscuro;

    [Tooltip("Volumen del ambiente mientras la vineta esta activa.")]
    [Range(0f, 1f)] public float volumenAmbienteOscuro = 0.25f;

    private float alphaBase;
    private float volumenBase = 1f;
    private bool activa;
    private Coroutine transicion;

    private void Awake()
    {
        Instancia = this;

        if (ambiente != null) volumenBase = ambiente.volume;

        alphaBase = empezarOscuro ? alphaMaximo : 0f;
        activa = empezarOscuro;
        Aplicar(alphaBase);
    }

    private void Update()
    {
        if (vineta == null) return;

        float extra = (activa && pulso > 0f)
            ? Mathf.Sin(Time.unscaledTime * velocidadPulso) * pulso
            : 0f;

        Aplicar(alphaBase + extra);
    }

    public void Entrar()
    {
        activa = true;
        IniciarTransicion(alphaMaximo, duracionEntrada);

        if (ambiente != null)
        {
            StopCoroutine(nameof(BajarAmbiente));
            StartCoroutine(BajarAmbiente(volumenAmbienteOscuro, duracionEntrada));
        }
    }

    public void Salir()
    {
        activa = false;
        IniciarTransicion(0f, duracionSalida);

        if (ambiente != null)
        {
            StopCoroutine(nameof(BajarAmbiente));
            StartCoroutine(BajarAmbiente(volumenBase, duracionSalida));
        }
    }

    public void Fijar(bool oscuro)
    {
        if (transicion != null) StopCoroutine(transicion);

        activa = oscuro;
        alphaBase = oscuro ? alphaMaximo : 0f;
        Aplicar(alphaBase);
    }

    private void IniciarTransicion(float destino, float duracion)
    {
        if (transicion != null) StopCoroutine(transicion);
        transicion = StartCoroutine(Transicion(destino, duracion));
    }

    private IEnumerator Transicion(float destino, float duracion)
    {
        float desde = alphaBase;
        float t = 0f;

        if (duracion <= 0f)
        {
            alphaBase = destino;
            yield break;
        }

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(t / duracion);
            alphaBase = Mathf.SmoothStep(desde, destino, progreso);
            yield return null;
        }

        alphaBase = destino;
    }

    private IEnumerator BajarAmbiente(float destino, float duracion)
    {
        float desde = ambiente.volume;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            ambiente.volume = Mathf.Lerp(desde, destino, Mathf.Clamp01(t / duracion));
            yield return null;
        }

        ambiente.volume = destino;
    }

    private void Aplicar(float alpha)
    {
        if (vineta == null) return;
        Color color = vineta.color;
        color.a = Mathf.Clamp01(alpha);
        vineta.color = color;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }
}
