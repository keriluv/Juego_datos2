using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioSecuencia : DesafioSnap
{
    private const int TotalRondas = 3;
    private const int TotalColores = 4;

    private readonly Image[] paneles = new Image[TotalColores];
    private readonly Color[] colores =
    {
        new Color(0.1f, 0.72f, 0.95f, 1f),
        new Color(0.92f, 0.22f, 0.58f, 1f),
        new Color(1f, 0.78f, 0.12f, 1f),
        new Color(0.2f, 0.82f, 0.42f, 1f)
    };

    private TextMeshProUGUI textoRonda;
    private TextMeshProUGUI textoEstado;
    private TextMeshProUGUI textoProgreso;

    private int[] secuencia = new int[4];
    private int ronda;
    private int indiceEntrada;
    private int indiceSeleccionado;
    private bool mostrando;
    private bool entrada;
    private bool castigo;

    private static readonly Color Verde = new Color(0.13f, 0.77f, 0.37f, 1f);
    private static readonly Color Rojo = new Color(0.91f, 0.15f, 0.17f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        Image fondo = gameObject.AddComponent<Image>();
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.04f, 0.07f, 0.14f, 0.96f);
        fondo.raycastTarget = false;

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "SECUENCIA DE LA CIUDAD", 27f, Color.white);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 38f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Memoriza el orden. A/D te mueve, click o gatillo elige", 13f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(420f, 24f));

        textoRonda = UtilidadesDesafio.CrearTexto("Ronda", raiz, Fuente,
            "RONDA 1/" + TotalRondas, 17f, Color.white, TextAlignmentOptions.Left);
        UtilidadesDesafio.Colocar(textoRonda.rectTransform, new Vector2(0f, 1f), new Vector2(110f, -94f), new Vector2(200f, 24f));

        textoProgreso = UtilidadesDesafio.CrearTexto("Progreso", raiz, Fuente,
            "0 / 4", 17f, Color.white, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoProgreso.rectTransform, new Vector2(1f, 1f), new Vector2(-110f, -94f), new Vector2(200f, 24f));

        RectTransform fila = UtilidadesDesafio.CrearRect("Colores", raiz);
        UtilidadesDesafio.Colocar(fila, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(382f, 130f));

        for (int i = 0; i < TotalColores; i++)
        {
            Image panel = UtilidadesDesafio.CrearPanel("Color_" + i, fila, SpriteRedondeado, colores[i]);
            RectTransform rt = panel.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-147f + i * 98f, 0f);
            rt.sizeDelta = new Vector2(88f, 120f);

            TextMeshProUGUI numero = UtilidadesDesafio.CrearTexto("Numero", rt, Fuente,
                (i + 1).ToString(), 30f, new Color(1f, 1f, 1f, 0.85f));
            UtilidadesDesafio.Estirar(numero.rectTransform, 4f);

            paneles[i] = panel;
        }

        textoEstado = UtilidadesDesafio.CrearTexto("Estado", raiz, Fuente, "PREPARATE", 22f, Color.white);
        UtilidadesDesafio.Colocar(textoEstado.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(420f, 34f));

        TextMeshProUGUI pista = UtilidadesDesafio.CrearTexto("Pista", raiz, Fuente,
            "El jugador se mueve con A/D. Golpea para confirmar.", 12f, Gris);
        UtilidadesDesafio.Colocar(pista.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(420f, 22f));

        ronda = 0;
        IniciarRonda();
    }

    private void IniciarRonda()
    {
        mostrando = true;
        entrada = false;
        castigo = false;
        indiceEntrada = 0;
        indiceSeleccionado = 0;

        int longitud = 4 + ronda;
        GenerarSecuencia(longitud);

        textoRonda.text = "RONDA " + (ronda + 1) + "/" + TotalRondas;
        textoProgreso.text = "0 / " + longitud;
        textoEstado.text = "OBSERVA";
        textoEstado.color = Color.white;

        StartCoroutine(MostrarSecuencia(longitud));
    }

    private void GenerarSecuencia(int longitud)
    {
        secuencia = new int[longitud];

        for (int i = 0; i < longitud; i++)
        {
            int valor = Random.Range(0, TotalColores);

            if (i >= 2 && valor == secuencia[i - 1] && valor == secuencia[i - 2])
                valor = (valor + 1 + Random.Range(0, TotalColores - 1)) % TotalColores;

            secuencia[i] = valor;
        }
    }

    private IEnumerator MostrarSecuencia(int longitud)
    {
        RestaurarPaneles();
        MoverJugadorA(indiceSeleccionado);

        yield return new WaitForSecondsRealtime(0.55f);

        for (int i = 0; i < longitud; i++)
        {
            yield return Destacar(secuencia[i], 0.38f);
            yield return new WaitForSecondsRealtime(0.12f);
        }

        RestaurarPaneles();
        indiceEntrada = 0;
        indiceSeleccionado = 0;
        ActualizarSeleccion();
        textoProgreso.text = "0 / " + longitud;
        textoEstado.text = "TU TURNO";
        textoEstado.color = Verde;

        mostrando = false;
        entrada = true;
    }

    private IEnumerator Destacar(int indice, float duracion)
    {
        Image panel = paneles[indice];
        Color baseColor = colores[indice];

        panel.color = Color.Lerp(baseColor, Color.white, 0.8f);
        panel.rectTransform.localScale = Vector3.one * 1.12f;

        yield return new WaitForSecondsRealtime(duracion);

        panel.color = baseColor;
        panel.rectTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (terminado || mostrando || castigo) return;

        if (!entrada)
        {
            ActualizarJugador();
            return;
        }

        int direccion = UtilidadesDesafio.DireccionPresionada();
        if (direccion != 0)
        {
            indiceSeleccionado = (indiceSeleccionado + direccion + TotalColores) % TotalColores;
            ActualizarSeleccion();
        }

        ActualizarJugador();

        if (UtilidadesDesafio.GolpePresionado())
            Elegir();
    }

    private void Elegir()
    {
        entrada = false;

        if (indiceSeleccionado == secuencia[indiceEntrada])
        {
            indiceEntrada++;
            textoProgreso.text = indiceEntrada + " / " + secuencia.Length;

            if (indiceEntrada >= secuencia.Length)
            {
                StartCoroutine(TerminarRonda());
                return;
            }

            StartCoroutine(ConfirmarPanel(indiceSeleccionado));
        }
        else
        {
            StartCoroutine(Fallar(indiceSeleccionado));
        }
    }

    private IEnumerator ConfirmarPanel(int indice)
    {
        Image panel = paneles[indice];
        Color baseColor = colores[indice];

        panel.color = Verde;
        panel.rectTransform.localScale = Vector3.one * 1.1f;

        yield return new WaitForSecondsRealtime(0.22f);

        panel.color = baseColor;
        panel.rectTransform.localScale = Vector3.one;
        entrada = true;
    }

    private IEnumerator Fallar(int indice)
    {
        castigo = true;
        textoEstado.text = "FALLASTE";
        textoEstado.color = Rojo;

        Image panel = paneles[indice];
        Color baseColor = colores[indice];

        panel.color = Rojo;
        panel.rectTransform.localScale = Vector3.one * 1.12f;

        yield return new WaitForSecondsRealtime(0.45f);

        panel.color = baseColor;
        panel.rectTransform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(0.65f);

        castigo = false;
        IniciarRonda();
    }

    private IEnumerator TerminarRonda()
    {
        textoEstado.text = "RONDA SUPERADA";
        textoEstado.color = Verde;

        yield return new WaitForSecondsRealtime(0.9f);

        ronda++;

        if (ronda >= TotalRondas)
        {
            terminado = true;
            textoEstado.text = "DESAFIO COMPLETADO";
            textoEstado.color = Color.white;
            yield return new WaitForSecondsRealtime(1f);
            Completar();
            yield break;
        }

        IniciarRonda();
    }

    private void ActualizarSeleccion()
    {
        for (int i = 0; i < TotalColores; i++)
        {
            bool seleccionado = i == indiceSeleccionado;
            paneles[i].color = seleccionado ? Color.Lerp(colores[i], Color.white, 0.35f) : colores[i];
            paneles[i].rectTransform.localScale = Vector3.one * (seleccionado ? 1.08f : 1f);
        }
    }

    private void RestaurarPaneles()
    {
        for (int i = 0; i < TotalColores; i++)
        {
            paneles[i].color = colores[i];
            paneles[i].rectTransform.localScale = Vector3.one;
        }
    }

    private void MoverJugadorA(int indice)
    {
        if (jugadorRT == null || mundo == null || historia == null) return;

        Vector2 destino = CalcularDestino(indice);
        jugadorRT.anchoredPosition = new Vector2(destino.x, jugadorRT.anchoredPosition.y);
    }

    private void ActualizarJugador()
    {
        if (jugadorRT == null || mundo == null || historia == null) return;

        Vector2 destino = CalcularDestino(indiceSeleccionado);
        Vector2 posicion = jugadorRT.anchoredPosition;
        posicion.x = Mathf.MoveTowards(posicion.x, destino.x, 950f * Time.unscaledDeltaTime);
        jugadorRT.anchoredPosition = posicion;
    }

    private Vector2 CalcularDestino(int indice)
    {
        Image panel = paneles[indice];
        Vector3 centro = mundo.InverseTransformPoint(panel.rectTransform.TransformPoint(panel.rectTransform.rect.center));

        float ancho = jugadorRT != null ? jugadorRT.rect.width : 72f;
        Rect limites = PlataformaUI.RectDe(mundo, historia);

        float x = centro.x - ancho * 0.5f;
        x = Mathf.Clamp(x, limites.xMin, Mathf.Max(limites.xMin, limites.xMax - ancho));

        return new Vector2(x, jugadorRT != null ? jugadorRT.anchoredPosition.y : 0f);
    }
}
