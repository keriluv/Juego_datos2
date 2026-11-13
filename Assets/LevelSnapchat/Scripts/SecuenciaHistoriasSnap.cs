using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class SecuenciaHistoriasSnap : MonoBehaviour
{
    public JugadorTutorial jugador;
    public PlataformaUI plataformaBoton;
    public Button botonHistorias;
    public SnapWebApp snap;
    public RectTransform mundo;
    public RectTransform primeraHistoria;
    public RectTransform contactoInicial;
    public GestorDesafiosSnap gestor;

    [Header("Animacion")]
    public float duracionCaida = 0.9f;
    public float pausaAntesZoom = 0.2f;
    public float margenHistoria = 24f;

    private bool historiaActivada;

    private void OnEnable()
    {
        if (jugador != null) jugador.alAterrizar += AlAterrizar;
    }

    private void OnDisable()
    {
        if (jugador != null) jugador.alAterrizar -= AlAterrizar;
    }

    private IEnumerator Start()
    {
        if (jugador == null || mundo == null)
        {
            Debug.LogWarning("SNAP_SECUENCIA: faltan el jugador o el mundo.");
            yield break;
        }

        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform jugadorRT = (RectTransform)jugador.transform;
        float ancho = jugadorRT.rect.width;
        float xInicial = contactoInicial != null
            ? PlataformaUI.RectDe(mundo, contactoInicial).center.x - ancho * 0.5f
            : jugador.posicionInicial.x;
        float maxX = Mathf.Max(0f, mundo.rect.width - ancho);
        jugador.ColocarEn(new Vector2(
            Mathf.Clamp(xInicial, 0f, maxX),
            Mathf.Max(0f, jugador.alturaSuelo)));
    }

    private void AlAterrizar(PlataformaUI plataforma)
    {
        if (historiaActivada || plataforma != plataformaBoton) return;

        historiaActivada = true;
        StartCoroutine(Secuencia());
    }

    private IEnumerator AnimarToque(RectTransform spriteRT, Button boton)
    {
        Vector3 escalaSpriteInicial = spriteRT != null ? spriteRT.localScale : Vector3.one;
        Quaternion rotacionSpriteInicial = spriteRT != null ? spriteRT.localRotation : Quaternion.identity;
        Vector3 escalaBotonInicial = boton != null
            ? boton.transform.localScale
            : Vector3.one;
        float duracion = 0.24f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float golpe = Mathf.Sin(t * Mathf.PI);

            if (spriteRT != null)
            {
                spriteRT.localScale = Vector3.Scale(escalaSpriteInicial,
                    new Vector3(1f + 0.18f * golpe, 1f - 0.2f * golpe, 1f));
                spriteRT.localRotation = rotacionSpriteInicial * Quaternion.Euler(0f, 0f, -18f * golpe);
            }

            if (boton != null)
                boton.transform.localScale = escalaBotonInicial * (1f - 0.08f * golpe);

            yield return null;
        }

        if (spriteRT != null)
        {
            spriteRT.localScale = escalaSpriteInicial;
            spriteRT.localRotation = rotacionSpriteInicial;
        }

        if (boton != null)
            boton.transform.localScale = escalaBotonInicial;
    }

    private IEnumerator Secuencia()
    {
        RectTransform jugadorRT = jugador != null ? (RectTransform)jugador.transform : null;
        RectTransform spriteRT = jugador != null && jugador.sprite != null
            ? jugador.sprite.rectTransform
            : null;

        if (jugador != null)
        {
            jugador.usarClick = false;
            jugador.usarGatillo = false;
            jugador.enabled = false;
        }

        yield return AnimarToque(spriteRT, botonHistorias);

        if (snap != null)
            snap.MostrarSeccion(2);
        else if (botonHistorias != null)
            botonHistorias.onClick.Invoke();

        yield return null;
        Canvas.ForceUpdateCanvases();

        if (jugadorRT != null)
            yield return MoverJugador(jugadorRT, CalcularDestinoHistoria(), duracionCaida, true);

        yield return new WaitForSecondsRealtime(pausaAntesZoom);

        if (gestor != null)
            gestor.IniciarEnHistoria(0);
        else
            Debug.LogWarning("SNAP_SECUENCIA: falta el GestorDesafiosSnap en la escena.");
    }

    private IEnumerator MoverJugador(RectTransform jugadorRT, Vector2 destino, float duracion, bool caida)
    {
        Vector2 inicio = jugadorRT.anchoredPosition;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float suave = caida ? k * k : k * k * (3f - 2f * k);
            jugadorRT.anchoredPosition = Vector2.LerpUnclamped(inicio, destino, suave);
            yield return null;
        }

        jugadorRT.anchoredPosition = destino;
    }

    private Vector2 CalcularDestinoHistoria()
    {
        if (primeraHistoria == null || mundo == null) return Vector2.zero;

        Rect r = primeraHistoria.rect;
        Vector3 esquina = primeraHistoria.TransformPoint(new Vector3(r.xMin, r.yMin, 0f));
        Vector3 local = mundo.InverseTransformPoint(esquina);

        float ancho = 72f;
        float alto = 96f;

        if (jugador != null)
        {
            RectTransform rt = (RectTransform)jugador.transform;
            ancho = rt.rect.width;
            alto = rt.rect.height;
        }

        Vector2 destino = new Vector2(local.x + margenHistoria, local.y + margenHistoria * 0.5f);
        destino.x = Mathf.Clamp(destino.x, 0f, Mathf.Max(0f, mundo.rect.width - ancho));
        destino.y = Mathf.Clamp(destino.y, 0f, Mathf.Max(0f, mundo.rect.height - alto));

        return destino;
    }
}
