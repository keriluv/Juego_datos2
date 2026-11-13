using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioSnapPerfecto : DesafioSnap
{
    private const float AnchoPista = 372f;
    private const float AltoPista = 230f;
    private const float LimiteVisor = 126f;
    private const float LimiteSujeto = 100f;
    private const int MaxFallos = 3;

    public int objetivo = 4;
    public Sprite spriteObjetivo;

    private RectTransform visor;
    private RectTransform sujeto;
    private Image[] marcoVisor;
    private Image destello;
    private Image[] fotos;
    private TextMeshProUGUI textoSnaps;
    private TextMeshProUGUI textoFallos;
    private TextMeshProUGUI textoFeedback;

    private float posicionVisor;
    private float direccion = 1f;
    private float velocidad = 240f;
    private float objetivoX;
    private float tolerancia = 46f;
    private float cooldown;
    private int snaps;
    private int fallos;
    private int ultimoEvento;
    private Coroutine rutinaDestello;

    private static readonly Color Blanco = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color Verde = new Color(0.35f, 1f, 0.55f, 1f);
    private static readonly Color Rojo = new Color(0.95f, 0.28f, 0.3f, 1f);
    private static readonly Color Amarillo = new Color(1f, 0.86f, 0.2f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.pivot = new Vector2(0.5f, 0.5f);
        raiz.offsetMin = new Vector2(8f, 8f);
        raiz.offsetMax = new Vector2(-8f, -8f);

        if (raiz.GetComponent<RectMask2D>() == null)
            raiz.gameObject.AddComponent<RectMask2D>();

        Canvas.ForceUpdateCanvases();

        Image fondo = gameObject.AddComponent<Image>();
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.06f, 0.03f, 0.09f, 0.52f);
        fondo.raycastTarget = true;

        Button botonCaptura = gameObject.AddComponent<Button>();
        botonCaptura.targetGraphic = fondo;
        botonCaptura.transition = Selectable.Transition.None;
        botonCaptura.onClick.AddListener(CapturarPorClick);

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "SESIÓN DE FOTOS", 28f, Color.white);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(430f, 38f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Encuadra a Andy y dispara en el momento justo", 12f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -232f), new Vector2(430f, 22f));

        textoSnaps = UtilidadesDesafio.CrearTexto("Snaps", raiz, Fuente,
            "FOTOS 0/" + objetivo, 15f, Amarillo);
        UtilidadesDesafio.Colocar(textoSnaps.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -258f), new Vector2(240f, 22f));

        textoFallos = UtilidadesDesafio.CrearTexto("Fallos", raiz, Fuente,
            "FALLOS 0/" + MaxFallos, 12f, Gris);
        UtilidadesDesafio.Colocar(textoFallos.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -278f), new Vector2(240f, 18f));

        Image pista = UtilidadesDesafio.CrearPanel("Pista", raiz, SpriteRedondeado,
            new Color(0.12f, 0.13f, 0.15f, 1f));
        UtilidadesDesafio.Colocar(pista.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 62f), new Vector2(AnchoPista, AltoPista));

        Image cielo = UtilidadesDesafio.CrearPanel("Cielo", pista.transform, null,
            new Color(0.2f, 0.22f, 0.24f, 1f), false);
        UtilidadesDesafio.Colocar(cielo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(AnchoPista, 116f));

        Image suelo = UtilidadesDesafio.CrearPanel("Suelo", pista.transform, null,
            new Color(0.12f, 0.13f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(suelo.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(AnchoPista, 56f));

        CrearSujeto(pista.transform);
        CrearVisor(pista.transform);
        CrearIndicadoresCamara(pista.transform);

        textoFeedback = UtilidadesDesafio.CrearTexto("Feedback", raiz, Fuente, "", 22f, Color.white);
        UtilidadesDesafio.Colocar(textoFeedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -108f), new Vector2(400f, 30f));

        CrearFotos();

        TextMeshProUGUI pistaTexto = UtilidadesDesafio.CrearTexto("Ayuda", raiz, Fuente,
            "Click o gatillo para disparar", 12f, Gris);
        UtilidadesDesafio.Colocar(pistaTexto.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 17f), new Vector2(400f, 22f));

        CrearDestello();

        if (online && !esAnfitrion)
        {
            posicionVisor = 0f;
            objetivoX = 0f;
            visor.anchoredPosition = new Vector2(posicionVisor, 6f);
            sujeto.anchoredPosition = new Vector2(objetivoX, 6f);
        }
        else
        {
            ConfigurarRonda();
        }
    }

    private void CrearSujeto(Transform padre)
    {
        sujeto = UtilidadesDesafio.CrearRect("Sujeto", padre);
        sujeto.anchorMin = new Vector2(0.5f, 0.5f);
        sujeto.anchorMax = new Vector2(0.5f, 0.5f);
        sujeto.pivot = new Vector2(0.5f, 0.5f);
        sujeto.sizeDelta = new Vector2(96f, 110f);

        Image aro = UtilidadesDesafio.CrearPanel("Aro", sujeto, SpriteRedondeado,
            new Color(1f, 0.84f, 0.2f, 0.95f), false);
        UtilidadesDesafio.Colocar(aro.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(96f, 96f));

        if (spriteObjetivo == null)
        {
            Debug.LogError("DesafioSnapPerfecto requiere el sprite objetivo Andy.");
            return;
        }

        Image foto = UtilidadesDesafio.CrearPanel("FotoObjetivo", sujeto, spriteObjetivo, Color.white, false);
        UtilidadesDesafio.Colocar(foto.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(82f, 82f));
    }

    private void CrearIndicadoresCamara(Transform padre)
    {
        Image puntoGrabacion = UtilidadesDesafio.CrearPanel("PuntoGrabacion", padre, SpriteRedondeado,
            Rojo, false);
        UtilidadesDesafio.Colocar(puntoGrabacion.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(-158f, 99f), new Vector2(8f, 8f));

        TextMeshProUGUI grabacion = UtilidadesDesafio.CrearTexto("IndicadorREC", padre, Fuente,
            "REC", 10f, Blanco);
        UtilidadesDesafio.Colocar(grabacion.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(-136f, 99f), new Vector2(34f, 16f));

        TextMeshProUGUI zoom = UtilidadesDesafio.CrearTexto("IndicadorZoom", padre, Fuente,
            "1x", 10f, Blanco);
        UtilidadesDesafio.Colocar(zoom.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(158f, 99f), new Vector2(26f, 16f));

        TextMeshProUGUI modo = UtilidadesDesafio.CrearTexto("ModoCamara", padre, Fuente,
            "HD  |  AUTO", 9f, Gris);
        UtilidadesDesafio.Colocar(modo.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(-128f, -101f), new Vector2(84f, 16f));

        Image bateria = UtilidadesDesafio.CrearPanel("Bateria", padre, SpriteRedondeado,
            new Color(0.35f, 1f, 0.55f, 0.95f), false);
        UtilidadesDesafio.Colocar(bateria.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(151f, -101f), new Vector2(18f, 8f));
    }

    private void CrearVisor(Transform padre)
    {
        visor = UtilidadesDesafio.CrearRect("Visor", padre);
        visor.anchorMin = new Vector2(0.5f, 0.5f);
        visor.anchorMax = new Vector2(0.5f, 0.5f);
        visor.pivot = new Vector2(0.5f, 0.5f);
        visor.sizeDelta = new Vector2(120f, 150f);
        visor.anchoredPosition = new Vector2(0f, 6f);

        marcoVisor = new Image[8];

        CrearEsquina(new Vector2(-60f, 75f), new Vector2(28f, 7f), 0);
        CrearEsquina(new Vector2(-46.5f, 60.5f), new Vector2(7f, 26f), 1);
        CrearEsquina(new Vector2(60f, 75f), new Vector2(28f, 7f), 2);
        CrearEsquina(new Vector2(46.5f, 60.5f), new Vector2(7f, 26f), 3);
        CrearEsquina(new Vector2(-60f, -75f), new Vector2(28f, 7f), 4);
        CrearEsquina(new Vector2(-46.5f, -60.5f), new Vector2(7f, 26f), 5);
        CrearEsquina(new Vector2(60f, -75f), new Vector2(28f, 7f), 6);
        CrearEsquina(new Vector2(46.5f, -60.5f), new Vector2(7f, 26f), 7);

        Image punto = UtilidadesDesafio.CrearPanel("Punto", visor, SpriteRedondeado, Blanco, false);
        UtilidadesDesafio.Colocar(punto.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 8f));
    }

    private void CrearEsquina(Vector2 posicion, Vector2 tamano, int indice)
    {
        Image pieza = UtilidadesDesafio.CrearPanel("Marco_" + indice, visor, SpriteRedondeado, Blanco, false);
        UtilidadesDesafio.Colocar(pieza.rectTransform, new Vector2(0.5f, 0.5f), posicion, tamano);
        marcoVisor[indice] = pieza;
    }

    private void CrearFotos()
    {
        fotos = new Image[objetivo];

        RectTransform fila = UtilidadesDesafio.CrearRect("Fotos", raiz);
        UtilidadesDesafio.Colocar(fila, new Vector2(0.5f, 0.5f), new Vector2(0f, -164f), new Vector2(260f, 72f));

        for (int i = 0; i < objetivo; i++)
        {
            Image foto = UtilidadesDesafio.CrearPanel("Foto_" + i, fila, SpriteRedondeado,
                new Color(0.14f, 0.17f, 0.24f, 1f));
            RectTransform rt = foto.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2((i - (objetivo - 1) * 0.5f) * 58f, 0f);
            rt.sizeDelta = new Vector2(52f, 66f);

            fotos[i] = foto;
        }
    }

    private void CrearDestello()
    {
        destello = UtilidadesDesafio.CrearPanel("Destello", raiz, null,
            new Color(1f, 1f, 1f, 0f), false);
        UtilidadesDesafio.Estirar(destello.rectTransform);
        destello.rectTransform.SetAsLastSibling();
    }

    private void Update()
    {
        if (terminado) return;

        if (online && !esAnfitrion)
        {
            ActualizarRemoto();
            return;
        }

        if (cooldown > 0f) cooldown -= Time.unscaledDeltaTime;

        posicionVisor += direccion * velocidad * Time.unscaledDeltaTime;

        if (posicionVisor >= LimiteVisor)
        {
            posicionVisor = LimiteVisor;
            direccion = -1f;
        }
        else if (posicionVisor <= -LimiteVisor)
        {
            posicionVisor = -LimiteVisor;
            direccion = 1f;
        }

        visor.anchoredPosition = new Vector2(posicionVisor, 6f);

        bool dentro = Mathf.Abs(posicionVisor - objetivoX) <= tolerancia;
        Color color = dentro ? Verde : Blanco;

        for (int i = 0; i < marcoVisor.Length; i++)
            marcoVisor[i].color = color;

        bool disparo = cooldown <= 0f && UtilidadesDesafio.GolpePresionado(false);

        if (!disparo && red != null)
            disparo = red.ConsumirDisparoCliente();

        if (disparo && cooldown <= 0f)
            Evaluar(dentro);

        if (online && esAnfitrion && red != null)
            red.FijarFoto(posicionVisor, objetivoX, snaps, fallos);
    }

    private void ActualizarRemoto()
    {
        if (red == null) return;

        posicionVisor = red.FotoVisor;
        objetivoX = red.FotoObjetivo;
        snaps = red.FotoSnaps;
        fallos = red.FotoFallos;
        tolerancia = Mathf.Lerp(44f, 16f, snaps / (float)objetivo);

        visor.anchoredPosition = new Vector2(posicionVisor, 6f);
        sujeto.anchoredPosition = new Vector2(objetivoX, 6f);

        textoSnaps.text = "FOTOS " + snaps + "/" + objetivo;
        textoFallos.text = "FALLOS " + fallos + "/" + MaxFallos;

        for (int i = 0; i < fotos.Length; i++)
        {
            fotos[i].color = i < snaps
                ? new Color(0.2f, 0.62f, 0.42f, 1f)
                : new Color(0.14f, 0.17f, 0.24f, 1f);
        }

        bool dentro = Mathf.Abs(posicionVisor - objetivoX) <= tolerancia;
        Color color = dentro ? Verde : Blanco;

        for (int i = 0; i < marcoVisor.Length; i++)
            marcoVisor[i].color = color;

        if (red.FotoEvento != ultimoEvento)
        {
            ultimoEvento = red.FotoEvento;

            if (red.FotoReinicio)
            {
                textoFeedback.text = "SE ARRUINÓ LA SESIÓN";
                textoFeedback.color = Rojo;
            }
            else if (red.FotoResultado)
            {
                textoFeedback.text = "¡FOTO!";
                textoFeedback.color = Verde;
                UtilidadesDesafio.Sonido("sfx_snap");
                UtilidadesDesafio.Sacudir(raiz, 6f, 0.16f);
                DispararDestello();
            }
            else
            {
                textoFeedback.text = "FUERA DE FOCO";
                textoFeedback.color = Rojo;
                UtilidadesDesafio.Sonido("sfx_fallo");
                UtilidadesDesafio.Sacudir(raiz, 14f, 0.22f);
            }
        }

        if (UtilidadesDesafio.GolpePresionado(false))
            red.EnviarDisparo();
    }

    private void CapturarPorClick()
    {
        if (terminado || cooldown > 0f) return;

        if (online && !esAnfitrion)
        {
            if (red != null) red.EnviarDisparo();
            return;
        }

        bool dentro = Mathf.Abs(posicionVisor - objetivoX) <= tolerancia;
        Evaluar(dentro);
    }

    private void Evaluar(bool dentro)
    {
        if (dentro)
        {
            snaps++;
            fallos = 0;
            textoSnaps.text = "FOTOS " + snaps + "/" + objetivo;
            textoFallos.text = "FALLOS 0/" + MaxFallos;
            textoFeedback.text = "¡FOTO!";
            textoFeedback.color = Verde;

            fotos[Mathf.Clamp(snaps - 1, 0, fotos.Length - 1)].color = new Color(0.2f, 0.62f, 0.42f, 1f);

            UtilidadesDesafio.Sonido("sfx_snap");
            UtilidadesDesafio.Sacudir(raiz, 6f, 0.16f);
            DispararDestello();

            if (snaps >= objetivo)
            {
                if (online && esAnfitrion && red != null)
                {
                    red.FijarFoto(posicionVisor, objetivoX, snaps, fallos);
                    red.FijarFotoEvento(true, false);
                }

                terminado = true;
                StartCoroutine(Final());
                return;
            }

            ConfigurarRonda();

            if (online && esAnfitrion && red != null)
            {
                red.FijarFoto(posicionVisor, objetivoX, snaps, fallos);
                red.FijarFotoEvento(true, false);
            }

            return;
        }

        fallos++;
        textoFallos.text = "FALLOS " + fallos + "/" + MaxFallos;
        textoFeedback.text = "FUERA DE FOCO";
        textoFeedback.color = Rojo;
        UtilidadesDesafio.Sonido("sfx_fallo");
        UtilidadesDesafio.Sacudir(raiz, 14f, 0.22f);
        cooldown = 0.35f;

        if (fallos >= MaxFallos)
            StartCoroutine(ReiniciarSesion());
        else
            ConfigurarRonda();

        if (online && esAnfitrion && red != null)
        {
            red.FijarFoto(posicionVisor, objetivoX, snaps, fallos);
            red.FijarFotoEvento(false, false);
        }
    }

    private IEnumerator ReiniciarSesion()
    {
        terminado = true;
        textoFeedback.text = "SE ARRUINÓ LA SESIÓN";
        textoFeedback.color = Rojo;

        if (online && esAnfitrion && red != null)
            red.FijarFotoEvento(false, true);

        yield return new WaitForSecondsRealtime(0.85f);

        snaps = 0;
        fallos = 0;
        textoSnaps.text = "FOTOS 0/" + objetivo;
        textoFallos.text = "FALLOS 0/" + MaxFallos;
        textoFeedback.text = "OTRA TOMA";
        textoFeedback.color = Amarillo;

        for (int i = 0; i < fotos.Length; i++)
            fotos[i].color = new Color(0.14f, 0.17f, 0.24f, 1f);

        ConfigurarRonda();
        terminado = false;
    }

    private void ConfigurarRonda()
    {
        tolerancia = Mathf.Lerp(44f, 16f, snaps / (float)objetivo);
        velocidad = 260f + snaps * 125f;
        objetivoX = Random.Range(-LimiteSujeto, LimiteSujeto);
        posicionVisor = -LimiteVisor;
        direccion = 1f;

        sujeto.anchoredPosition = new Vector2(objetivoX, 6f);
        visor.anchoredPosition = new Vector2(posicionVisor, 6f);
    }

    private void DispararDestello()
    {
        if (rutinaDestello != null) StopCoroutine(rutinaDestello);
        rutinaDestello = StartCoroutine(RutinaDestello());
    }

    private IEnumerator RutinaDestello()
    {
        float t = 0f;

        while (t < 0.18f)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / 0.18f);
            destello.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, k));
            yield return null;
        }

        destello.color = new Color(1f, 1f, 1f, 0f);
        rutinaDestello = null;
    }

    private IEnumerator Final()
    {
        textoFeedback.text = "DESAFIO COMPLETADO";
        textoFeedback.color = Amarillo;
        UtilidadesDesafio.Sonido("sfx_ronda");
        yield return new WaitForSecondsRealtime(0.9f);
        Completar();
    }
}
