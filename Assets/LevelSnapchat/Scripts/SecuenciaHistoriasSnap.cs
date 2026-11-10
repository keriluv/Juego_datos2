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

    private bool activada;

    private void OnEnable()
    {
        if (jugador != null) jugador.alGolpear += AlGolpear;
    }

    private void OnDisable()
    {
        if (jugador != null) jugador.alGolpear -= AlGolpear;
    }

    private IEnumerator Start()
    {
        if (jugador == null) yield break;

        yield return null;
        Canvas.ForceUpdateCanvases();

        if (contactoInicial != null)
            jugador.IrA(contactoInicial);
    }

    private void AlGolpear(PlataformaUI plataforma)
    {
        if (activada) return;
        if (plataformaBoton == null) return;

        bool golpeoBoton = plataforma == plataformaBoton;
        bool encimaDelBoton = jugador != null && jugador.PlataformaActual == plataformaBoton;

        if (!golpeoBoton && !encimaDelBoton) return;

        activada = true;
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        RectTransform jugadorRT = jugador != null ? (RectTransform)jugador.transform : null;

        if (jugador != null)
        {
            jugador.usarClick = false;
            jugador.usarGatillo = false;
            jugador.enabled = false;
        }

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
