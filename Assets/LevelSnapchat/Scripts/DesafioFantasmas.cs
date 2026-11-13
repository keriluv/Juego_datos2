using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioFantasmas : DesafioSnap
{
    private const int Objetivo = 6;
    private const float TiempoInicial = 30f;
    private const int Columnas = 3;
    private const float LadoCelda = 92f;
    private const float Separacion = 12f;

    private readonly Image[] celdas = new Image[9];
    private RectTransform iconoContenedor;
    private GameObject fantasmaGO;
    private GameObject calabazaGO;

    private TextMeshProUGUI textoFantasmas;
    private TextMeshProUGUI textoTiempo;
    private TextMeshProUGUI textoFeedback;

    private int fantasmas;
    private int celdaActiva = -1;
    private int ultimaCelda = -1;
    private bool esFantasma;
    private bool esperando = true;
    private bool resolviendo;
    private bool reiniciando;
    private float tiempoRestante = TiempoInicial;
    private float temporizadorAparicion = 0.5f;
    private float duracionVisible = 1.5f;
    private float duracionMaxima = 1.5f;

    private static readonly Color FondoCelda = new Color(0.1f, 0.07f, 0.16f, 0.96f);
    private static readonly Color Verde = new Color(0.13f, 0.77f, 0.37f, 1f);
    private static readonly Color Rojo = new Color(0.91f, 0.15f, 0.17f, 1f);
    private static readonly Color Naranja = new Color(1f, 0.58f, 0.12f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        Image fondo = gameObject.AddComponent<Image>();
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.05f, 0.02f, 0.09f, 0.96f);
        fondo.raycastTarget = false;

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "SNAPS DE HALLOWEEN", 28f, Naranja);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 38f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Abre el snap antes de que desaparezca", 14f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(420f, 24f));

        textoFantasmas = UtilidadesDesafio.CrearTexto("Fantasmas", raiz, Fuente,
            "SNAPS 0/" + Objetivo, 16f, Color.white, TextAlignmentOptions.Left);
        UtilidadesDesafio.Colocar(textoFantasmas.rectTransform, new Vector2(0f, 1f), new Vector2(110f, -98f), new Vector2(220f, 26f));

        textoTiempo = UtilidadesDesafio.CrearTexto("Tiempo", raiz, Fuente,
            "TIEMPO " + Mathf.CeilToInt(TiempoInicial), 16f, Color.white, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoTiempo.rectTransform, new Vector2(1f, 1f), new Vector2(-110f, -98f), new Vector2(220f, 26f));

        RectTransform rejilla = UtilidadesDesafio.CrearRect("Rejilla", raiz);
        UtilidadesDesafio.Colocar(rejilla, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(300f, 300f));

        for (int i = 0; i < celdas.Length; i++)
        {
            int columna = i % Columnas;
            int fila = i / Columnas;

            Image celda = UtilidadesDesafio.CrearPanel("Celda_" + i, rejilla, SpriteRedondeado, FondoCelda);
            RectTransform rt = celda.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(columna * (LadoCelda + Separacion), -fila * (LadoCelda + Separacion));
            rt.sizeDelta = new Vector2(LadoCelda, LadoCelda);
            celdas[i] = celda;
        }

        CrearIconos();

        textoFeedback = UtilidadesDesafio.CrearTexto("Feedback", raiz, Fuente, "", 20f, Color.white);
        UtilidadesDesafio.Colocar(textoFeedback.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(420f, 30f));
    }

    private void CrearIconos()
    {
        iconoContenedor = UtilidadesDesafio.CrearRect("Icono", raiz);
        iconoContenedor.anchorMin = new Vector2(0.5f, 0.5f);
        iconoContenedor.anchorMax = new Vector2(0.5f, 0.5f);
        iconoContenedor.pivot = new Vector2(0.5f, 0.5f);
        iconoContenedor.sizeDelta = new Vector2(70f, 70f);

        fantasmaGO = CrearFantasma(iconoContenedor);
        calabazaGO = CrearCalabaza(iconoContenedor);
        iconoContenedor.gameObject.SetActive(false);
    }

    private GameObject CrearFantasma(Transform padre)
    {
        RectTransform raizFantasma = UtilidadesDesafio.CrearRect("Fantasma", padre);
        UtilidadesDesafio.Estirar(raizFantasma);

        Image cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", raizFantasma, SpriteRedondeado,
            new Color(0.86f, 0.94f, 1f, 1f), false);
        UtilidadesDesafio.Estirar(cuerpo.rectTransform, 2f);

        CrearOjo(raizFantasma, new Vector2(-15f, 8f));
        CrearOjo(raizFantasma, new Vector2(15f, 8f));

        Image boca = UtilidadesDesafio.CrearPanel("Boca", raizFantasma, SpriteRedondeado,
            new Color(0.08f, 0.08f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(boca.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(22f, 7f));

        return raizFantasma.gameObject;
    }

    private GameObject CrearCalabaza(Transform padre)
    {
        RectTransform raizCalabaza = UtilidadesDesafio.CrearRect("Calabaza", padre);
        UtilidadesDesafio.Estirar(raizCalabaza);

        Image cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", raizCalabaza, SpriteRedondeado,
            new Color(1f, 0.52f, 0.08f, 1f), false);
        UtilidadesDesafio.Estirar(cuerpo.rectTransform, 4f);

        Image tallo = UtilidadesDesafio.CrearPanel("Tallo", raizCalabaza, SpriteRedondeado,
            new Color(0.2f, 0.55f, 0.18f, 1f), false);
        UtilidadesDesafio.Colocar(tallo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 38f), new Vector2(12f, 16f));

        CrearOjo(raizCalabaza, new Vector2(-15f, 6f));
        CrearOjo(raizCalabaza, new Vector2(15f, 6f));

        Image boca = UtilidadesDesafio.CrearPanel("Boca", raizCalabaza, SpriteRedondeado,
            new Color(0.16f, 0.05f, 0.02f, 1f), false);
        UtilidadesDesafio.Colocar(boca.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(26f, 7f));

        return raizCalabaza.gameObject;
    }

    private void CrearOjo(Transform padre, Vector2 posicion)
    {
        Image ojo = UtilidadesDesafio.CrearPanel("Ojo", padre, SpriteRedondeado,
            new Color(0.08f, 0.08f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(ojo.rectTransform, new Vector2(0.5f, 0.5f), posicion, new Vector2(11f, 16f));
    }

    private void Update()
    {
        if (terminado || reiniciando) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        textoTiempo.text = "TIEMPO " + Mathf.Max(0, Mathf.CeilToInt(tiempoRestante));
        textoTiempo.color = tiempoRestante <= 8f ? Rojo : Color.white;

        if (tiempoRestante <= 0f)
        {
            StartCoroutine(Reiniciar());
            return;
        }

        if (!resolviendo)
        {
            if (esperando)
            {
                temporizadorAparicion -= Time.unscaledDeltaTime;
                if (temporizadorAparicion <= 0f) MostrarSiguiente();
            }
            else
            {
                duracionVisible -= Time.unscaledDeltaTime;
                float restante = Mathf.Clamp01(duracionVisible / Mathf.Max(0.01f, duracionMaxima));
                iconoContenedor.localScale = Vector3.one * Mathf.Lerp(0.45f, 1f, restante);
                if (duracionVisible <= 0f) OcultarSinGolpe();
            }
        }

        if (!resolviendo && UtilidadesDesafio.GolpePresionado())
            Golpear();
    }

    private void MostrarSiguiente()
    {
        esperando = false;

        int indice = Random.Range(0, celdas.Length);
        if (indice == ultimaCelda)
            indice = (indice + 1 + Random.Range(0, celdas.Length - 1)) % celdas.Length;

        ultimaCelda = indice;
        celdaActiva = indice;
        esFantasma = Random.value > ProbabilidadCalabaza();
        duracionVisible = Mathf.Lerp(1.6f, 0.8f, fantasmas / (float)Objetivo);
        duracionMaxima = duracionVisible;

        celdas[indice].color = esFantasma
            ? new Color(0.16f, 0.3f, 0.42f, 1f)
            : new Color(0.42f, 0.18f, 0.05f, 1f);

        iconoContenedor.SetParent(celdas[indice].transform, false);
        iconoContenedor.anchorMin = new Vector2(0.5f, 0.5f);
        iconoContenedor.anchorMax = new Vector2(0.5f, 0.5f);
        iconoContenedor.pivot = new Vector2(0.5f, 0.5f);
        iconoContenedor.anchoredPosition = Vector2.zero;
        iconoContenedor.sizeDelta = new Vector2(70f, 70f);
        iconoContenedor.localScale = Vector3.one;
        iconoContenedor.gameObject.SetActive(true);

        fantasmaGO.SetActive(esFantasma);
        calabazaGO.SetActive(!esFantasma);

        UtilidadesDesafio.Sonido("sfx_lente", 0.2f);
    }

    private void OcultarSinGolpe()
    {
        if (celdaActiva >= 0)
            celdas[celdaActiva].color = FondoCelda;

        celdaActiva = -1;
        iconoContenedor.gameObject.SetActive(false);
        esperando = true;
        temporizadorAparicion = Random.Range(0.22f, 0.42f);
    }

    private void Golpear()
    {
        if (celdaActiva < 0)
        {
            textoFeedback.text = "SE ESCAPO";
            textoFeedback.color = Gris;
            return;
        }

        int indice = celdaActiva;
        bool fantasma = esFantasma;
        celdaActiva = -1;
        resolviendo = true;
        iconoContenedor.gameObject.SetActive(false);

        if (fantasma)
        {
            fantasmas++;
            textoFantasmas.text = "SNAPS " + fantasmas + "/" + Objetivo;
            textoFeedback.text = "ABIERTO!";
            textoFeedback.color = Verde;
            celdas[indice].color = Verde;

            UtilidadesDesafio.Sonido("sfx_acierto");
            UtilidadesDesafio.Sacudir(raiz, 5f, 0.14f);

            if (fantasmas >= Objetivo)
            {
                terminado = true;
                StartCoroutine(Final());
                return;
            }
        }
        else
        {
            tiempoRestante = Mathf.Max(0f, tiempoRestante - 2f);
            textoFeedback.text = "SNAP CADUCADO -2s";
            textoFeedback.color = Rojo;
            celdas[indice].color = Rojo;

            UtilidadesDesafio.Sonido("sfx_fallo");
            UtilidadesDesafio.Sacudir(raiz, 14f, 0.22f);
        }

        StartCoroutine(Resolver(indice));
    }

    private IEnumerator Resolver(int indice)
    {
        yield return new WaitForSecondsRealtime(0.2f);

        celdas[indice].color = FondoCelda;
        resolviendo = false;
        esperando = true;
        temporizadorAparicion = Random.Range(0.22f, 0.42f);
    }

    private IEnumerator Reiniciar()
    {
        reiniciando = true;
        resolviendo = true;

        if (celdaActiva >= 0)
        {
            celdas[celdaActiva].color = FondoCelda;
            celdaActiva = -1;
        }

        iconoContenedor.gameObject.SetActive(false);
        textoFeedback.text = "SE ACABO EL TIEMPO";
        textoFeedback.color = Rojo;
        UtilidadesDesafio.Sonido("sfx_fallo");

        yield return new WaitForSecondsRealtime(1.2f);

        fantasmas = 0;
        tiempoRestante = TiempoInicial;
        textoFantasmas.text = "SNAPS 0/" + Objetivo;
        textoFeedback.text = "";
        reiniciando = false;
        resolviendo = false;
        esperando = true;
        temporizadorAparicion = 0.4f;
    }

    private IEnumerator Final()
    {
        textoFeedback.text = "DESAFIO COMPLETADO";
        textoFeedback.color = Naranja;
        UtilidadesDesafio.Sonido("sfx_ronda");
        yield return new WaitForSecondsRealtime(0.9f);
        Completar();
    }

    private float ProbabilidadCalabaza()
    {
        return 0.28f + 0.27f * (fantasmas / (float)Objetivo);
    }
}
