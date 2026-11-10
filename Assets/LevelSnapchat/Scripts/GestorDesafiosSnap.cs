using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GestorDesafiosSnap : MonoBehaviour
{
    public JugadorTutorial jugador;
    public SnapWebApp snap;
    public RectTransform ventana;
    public RectTransform mundo;
    public RectTransform panelPrincipal;
    public RectTransform[] historias = new RectTransform[3];
    public VinetaHalloween vineta;

    [Header("Zoom")]
    public float escalaZoom = 1.7f;
    public float duracionZoom = 1.05f;
    public float pausaEntreHistorias = 0.3f;
    public float duracionAviso = 0.25f;

    private RectTransform raizDesafio;
    private DesafioSnap desafioActual;
    private Image overlayHalloween;
    private GameObject panelFinal;
    private int historiaActual = -1;
    private bool ocupado;
    private bool terminado;
    private bool botonesBloqueados;

    public void IniciarEnHistoria(int indice)
    {
        if (terminado || ocupado) return;
        if (indice < 0 || indice >= historias.Length || historias[indice] == null) return;

        StartCoroutine(RutinaEntrar(indice));
    }

    private IEnumerator RutinaEntrar(int indice)
    {
        ocupado = true;
        historiaActual = indice;

        if (!botonesBloqueados)
            BloquearBotonesHistoria(true);

        if (indice == 1)
            PrepararHalloween();

        ColocarJugadorEnHistoria(indice);

        yield return ZoomAHistoria(historias[indice], escalaZoom, duracionZoom);

        Canvas.ForceUpdateCanvases();

        if (indice == 1 && overlayHalloween != null)
            StartCoroutine(FadeOverlay(overlayHalloween, 0.42f, 0.8f));

        yield return new WaitForSecondsRealtime(pausaEntreHistorias);

        CrearDesafio(indice);
        ocupado = false;
    }

    private IEnumerator CompletarDesafio(int indice)
    {
        if (ocupado) yield break;
        ocupado = true;

        yield return new WaitForSecondsRealtime(duracionAviso);

        DestruirDesafio();

        if (indice == 1)
        {
            if (vineta != null)
                vineta.Salir();

            if (overlayHalloween != null)
                yield return FadeOverlay(overlayHalloween, 0f, 0.6f);
        }

        yield return ZoomSalir(duracionZoom);

        if (indice + 1 < historias.Length)
        {
            ocupado = false;
            IniciarEnHistoria(indice + 1);
            yield break;
        }

        MostrarFinal();
        terminado = true;
        ocupado = false;
    }

    private void CrearDesafio(int indice)
    {
        DestruirDesafio();

        RectTransform historia = historias[indice];

        GameObject go = new GameObject("Desafio_" + indice, typeof(RectTransform));
        go.layer = 5;

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(historia, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, historia.rect.height * 0.08f);
        rt.sizeDelta = new Vector2(historia.rect.width - 24f, historia.rect.height * 0.5f);

        DesafioSnap desafio;

        switch (indice)
        {
            case 0:
                desafio = go.AddComponent<DesafioTiming>();
                break;
            case 1:
                desafio = go.AddComponent<DesafioFantasmas>();
                break;
            default:
                desafio = go.AddComponent<DesafioSecuencia>();
                break;
        }

        desafio.Completado = () => StartCoroutine(CompletarDesafio(indice));

        ContextoDesafio contexto = new ContextoDesafio
        {
            snap = snap,
            raiz = rt,
            historia = historia,
            mundo = mundo,
            jugador = jugador,
            jugadorRT = jugador != null ? (RectTransform)jugador.transform : null,
            vineta = vineta
        };

        raizDesafio = rt;
        desafioActual = desafio;
        desafio.Iniciar(contexto);
    }

    private void DestruirDesafio()
    {
        if (raizDesafio != null)
        {
            Destroy(raizDesafio.gameObject);
            raizDesafio = null;
        }

        desafioActual = null;
    }

    private IEnumerator ZoomAHistoria(RectTransform historia, float escala, float duracion)
    {
        if (ventana == null || historia == null || panelPrincipal == null)
            yield break;

        ventana.localScale = Vector3.one;
        ventana.anchoredPosition = Vector2.zero;
        Canvas.ForceUpdateCanvases();

        Vector2 centro = ventana.InverseTransformPoint(historia.TransformPoint(historia.rect.center));
        Vector2 objetivo = ventana.InverseTransformPoint(panelPrincipal.TransformPoint(panelPrincipal.rect.center));

        Vector3 escalaInicial = ventana.localScale;
        Vector2 posicionInicial = ventana.anchoredPosition;
        Vector3 escalaFinal = new Vector3(escala, escala, 1f);
        Vector2 posicionFinal = objetivo - centro * escala;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float suave = k * k * (3f - 2f * k);

            ventana.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, suave);
            ventana.anchoredPosition = Vector2.LerpUnclamped(posicionInicial, posicionFinal, suave);

            yield return null;
        }

        ventana.localScale = escalaFinal;
        ventana.anchoredPosition = posicionFinal;
    }

    private IEnumerator ZoomSalir(float duracion)
    {
        if (ventana == null)
            yield break;

        Vector3 escalaInicial = ventana.localScale;
        Vector2 posicionInicial = ventana.anchoredPosition;
        Vector3 escalaFinal = Vector3.one;
        Vector2 posicionFinal = Vector2.zero;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float suave = k * k * (3f - 2f * k);

            ventana.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, suave);
            ventana.anchoredPosition = Vector2.LerpUnclamped(posicionInicial, posicionFinal, suave);

            yield return null;
        }

        ventana.localScale = Vector3.one;
        ventana.anchoredPosition = Vector2.zero;
    }

    private void ColocarJugadorEnHistoria(int indice)
    {
        if (jugador == null || mundo == null) return;

        RectTransform historia = historias[indice];
        if (historia == null) return;

        if (jugador.enabled)
            jugador.enabled = false;

        RectTransform rt = (RectTransform)jugador.transform;

        Vector3 punto = historia.TransformPoint(new Vector3(
            historia.rect.width * 0.5f,
            -historia.rect.height * 0.78f,
            0f));

        Vector2 local = mundo.InverseTransformPoint(punto);
        rt.anchoredPosition = new Vector2(local.x - rt.rect.width * 0.5f, local.y);
    }

    private void PrepararHalloween()
    {
        if (vineta != null)
        {
            vineta.alphaMaximo = 0.6f;
            vineta.Entrar();
        }

        if (historias.Length < 2 || historias[1] == null) return;

        RectTransform historia = historias[1];

        CambiarTexto(historia, "Etiqueta", "HALLOWEEN", new Color(1f, 0.58f, 0.12f, 1f));
        CambiarTexto(historia, "Subtitulo", "Noche de fantasmas", new Color(1f, 0.72f, 0.35f, 1f));
        CambiarTexto(historia, "Badge/Texto", "ESPECIAL", null);

        if (overlayHalloween != null) return;

        Image overlay = UtilidadesDesafio.CrearPanel("OscuridadHalloween", historia, null,
            new Color(0.04f, 0.01f, 0.07f, 0f));

        RectTransform rt = overlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.SetAsLastSibling();

        overlayHalloween = overlay;
    }

    private void CambiarTexto(RectTransform historia, string ruta, string texto, Color? color)
    {
        Transform hijo = historia.Find(ruta);
        if (hijo == null) return;

        TextMeshProUGUI tmp = hijo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        tmp.text = texto;

        if (color.HasValue)
            tmp.color = color.Value;
    }

    private IEnumerator FadeOverlay(Image imagen, float destino, float duracion)
    {
        if (imagen == null) yield break;

        Color color = imagen.color;
        float inicio = color.a;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            color.a = Mathf.Lerp(inicio, destino, k);
            imagen.color = color;
            yield return null;
        }

        color.a = destino;
        imagen.color = color;
    }

    private void MostrarFinal()
    {
        if (ventana == null) return;

        GameObject go = new GameObject("FinalSnap", typeof(RectTransform));
        go.layer = 5;

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(ventana, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.SetAsLastSibling();

        panelFinal = go;

        Image fondo = go.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.72f);
        fondo.raycastTarget = true;

        Sprite sprite = snap != null ? snap.spriteRedondeado : null;
        TMP_FontAsset fuente = snap != null ? snap.fuente : null;

        Image tarjeta = UtilidadesDesafio.CrearPanel("Tarjeta", rt, sprite,
            new Color(0.06f, 0.07f, 0.12f, 0.98f));
        UtilidadesDesafio.Colocar(tarjeta.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 340f));

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", tarjeta.transform, fuente,
            "FELICIDADES", 42f, new Color(1f, 0.86f, 0.2f, 1f));
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 82f), new Vector2(560f, 60f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", tarjeta.transform, fuente,
            "Completaste las 3 historias de Snapchat", 22f, Color.white);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(560f, 40f));

        Button volver = UtilidadesDesafio.CrearBoton("Volver", tarjeta.transform, sprite, fuente,
            "VOLVER AL CHAT", new Color(0.13f, 0.77f, 0.37f, 1f),
            new Vector2(0f, -92f), new Vector2(300f, 64f), 22f);

        volver.onClick.AddListener(CerrarFinal);
    }

    private void CerrarFinal()
    {
        if (panelFinal != null)
        {
            Destroy(panelFinal);
            panelFinal = null;
        }

        BloquearBotonesHistoria(false);

        if (jugador != null)
            jugador.gameObject.SetActive(false);

        if (snap != null)
            snap.MostrarSeccion(0);
    }

    private void BloquearBotonesHistoria(bool bloquear)
    {
        botonesBloqueados = bloquear;

        if (snap == null || snap.botonesHistoria == null) return;

        for (int i = 0; i < snap.botonesHistoria.Length; i++)
        {
            if (snap.botonesHistoria[i] != null)
                snap.botonesHistoria[i].enabled = !bloquear;
        }
    }
}
