using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroNivel : MonoBehaviour
{
    [Header("Pantalla de intro")]
    public CanvasGroup pantallaIntro;
    public Image logo;
    public TMP_Text textoNivel;
    public TMP_Text textoSubtitulo;

    [Header("Elementos que se animan al revelar el nivel (opcionales)")]
    public RectTransform menuLateral;
    public RectTransform mascaraVideo;

    [Header("Jugador y video")]
    public MovimientoTiktok jugador;
    public ControladorFeed feed;

    [Header("Tiempos (segundos)")]
    public float esperaInicial = 0.4f;
    public float duracionLogo = 0.7f;
    public float duracionTextos = 0.5f;
    public float tiempoMostrando = 1.6f;
    public float duracionRevelado = 0.9f;
    public float desplazamientoMenu = 450f;

    private Vector3 escalaLogo;
    private Vector2 posMenu;
    private Vector3 escalaVideo;
    private Vector3 escalaJugador;
    private Renderer rendererJugador;
    private bool terminada = false;
    private float tiempoInicio;

    void Awake()
    {
        // Evita que el video arranque solo
        if (feed != null) feed.iniciarAutomaticamente = false;

        // Congela y esconde a la capsula mientras dura la intro
        if (jugador != null)
        {
            jugador.congelado = true;
            escalaJugador = jugador.transform.localScale;
            rendererJugador = jugador.GetComponent<Renderer>();
            if (rendererJugador != null) rendererJugador.enabled = false;
        }

        pantallaIntro.gameObject.SetActive(true);
        pantallaIntro.alpha = 1f;
    }

    void Start()
    {
        escalaLogo = logo.rectTransform.localScale;
        if (menuLateral != null) posMenu = menuLateral.anchoredPosition;
        if (mascaraVideo != null) escalaVideo = mascaraVideo.localScale;

        // Estado inicial: todo escondido detras de la pantalla negra
        logo.color = ConAlfa(logo.color, 0f);
        logo.rectTransform.localScale = escalaLogo * 0.3f;
        textoNivel.alpha = 0f;
        textoSubtitulo.alpha = 0f;

        if (menuLateral != null)
            menuLateral.anchoredPosition = posMenu + Vector2.left * desplazamientoMenu;
        if (mascaraVideo != null)
            mascaraVideo.localScale = escalaVideo * 0.9f;

        tiempoInicio = Time.time;
        StartCoroutine(Secuencia());
    }

    void Update()
    {
        // Cualquier tecla (o clic) salta la intro
        if (!terminada && Time.time - tiempoInicio > 0.5f && Input.anyKeyDown)
        {
            StopAllCoroutines();
            Finalizar();
        }
    }

    private IEnumerator Secuencia()
    {
        yield return new WaitForSeconds(esperaInicial);

        // 1. El logo aparece con un pequeno rebote
        yield return Animar(duracionLogo, k =>
        {
            logo.rectTransform.localScale =
                Vector3.LerpUnclamped(escalaLogo * 0.3f, escalaLogo, EaseOutBack(k));
            logo.color = ConAlfa(logo.color, Mathf.Clamp01(k * 2f));
        });

        // 2. Aparecen los textos
        yield return Animar(duracionTextos, k =>
        {
            textoNivel.alpha = k;
            textoSubtitulo.alpha = k;
        });

        // 3. Se mantiene un momento con el logo "latiendo"
        float t = 0f;
        while (t < tiempoMostrando)
        {
            t += Time.deltaTime;
            float pulso = 1f + Mathf.Sin(t * 6f) * 0.04f;
            logo.rectTransform.localScale = escalaLogo * pulso;
            yield return null;
        }
        logo.rectTransform.localScale = escalaLogo;

        // 4. Se revela el nivel: el video arranca y la interfaz entra
        if (feed != null) feed.ComenzarFeed();

        if (menuLateral != null)
        {
            StartCoroutine(Animar(duracionRevelado, k =>
                menuLateral.anchoredPosition = Vector2.Lerp(
                    posMenu + Vector2.left * desplazamientoMenu, posMenu, EaseOutCubic(k))));
        }

        if (mascaraVideo != null)
        {
            StartCoroutine(Animar(duracionRevelado, k =>
                mascaraVideo.localScale = Vector3.Lerp(
                    escalaVideo * 0.9f, escalaVideo, EaseOutCubic(k))));
        }

        yield return Animar(duracionRevelado, k => pantallaIntro.alpha = 1f - k);
        pantallaIntro.gameObject.SetActive(false);

        // 5. Aparece la capsula y se libera el control
        if (jugador != null)
        {
            if (rendererJugador != null) rendererJugador.enabled = true;

            yield return Animar(0.4f, k =>
                jugador.transform.localScale =
                    Vector3.LerpUnclamped(escalaJugador * 0.3f, escalaJugador, EaseOutBack(k)));

            jugador.congelado = false;
        }

        terminada = true;
    }

    // Deja todo en su estado final (se usa al saltar la intro)
    private void Finalizar()
    {
        terminada = true;

        pantallaIntro.alpha = 0f;
        pantallaIntro.gameObject.SetActive(false);

        if (menuLateral != null) menuLateral.anchoredPosition = posMenu;
        if (mascaraVideo != null) mascaraVideo.localScale = escalaVideo;

        if (jugador != null)
        {
            if (rendererJugador != null) rendererJugador.enabled = true;
            jugador.transform.localScale = escalaJugador;
            jugador.congelado = false;
        }

        if (feed != null) feed.ComenzarFeed();
    }

    // Ejecuta una animacion de 0 a 1 durante "duracion" segundos
    private IEnumerator Animar(float duracion, Action<float> alActualizar)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            alActualizar(Mathf.Clamp01(t / duracion));
            yield return null;
        }
        alActualizar(1f);
    }

    private float EaseOutBack(float k)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
    }

    private float EaseOutCubic(float k)
    {
        return 1f - Mathf.Pow(1f - k, 3f);
    }

    private Color ConAlfa(Color c, float a)
    {
        c.a = a;
        return c;
    }
}