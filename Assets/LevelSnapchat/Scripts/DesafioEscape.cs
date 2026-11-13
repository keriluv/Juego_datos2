using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DesafioEscape : DesafioSnap
{
    private const float VelocidadPatrullaFantasmas = 90f;
    private const float VelocidadCazaFantasmas = 78f;
    private const float SeparacionMinimaFantasmas = 96f;
    private const float DistanciaDetencionCaza = 44f;
    private const float DuracionCazaFantasmas = 2.2f;
    private const float DuracionPatrullaFantasmas = 4.5f;

    public int nivel;
    public Sprite spriteFantasma;
    public Sprite spriteGlobosCumpleanos;
    public Sprite spriteGloboRojo;
    public Sprite spriteGloboVerde;
    public Sprite spriteGloboMorado;
    public Sprite spritePastel;

    private class PlataformaEscape
    {
        public RectTransform rt;
        public Graphic imagen;
        public PlataformaUI colision;
        public float baseX;
        public float baseY;
        public float ancho;
        public float alto;
        public bool movil;
        public float amplitud;
        public float velocidad;
        public float fase;
        public bool rompible;
        public float pisada;
        public float oculta;
        public Color colorBase;
    }

    private class PeligroEscape
    {
        public RectTransform rt;
        public Image cuerpo;
        public Color colorCuerpo;
        public float baseX;
        public float baseY;
        public float amplitud;
        public float velocidad;
        public float fase;
        public float ancho;
        public float alto;
        public bool persigue;
        public bool esDron;
        public bool esFantasma;
        public Vector2[] rutaPatrulla;
        public int puntoPatrulla;
        public int direccionPatrulla;
        public bool persigueJugador;
        public float tiempoModo;
        public float velocidadPersecucion;
        public float aturdido;
    }

    private class LlaveEscape
    {
        public RectTransform rt;
        public Image imagen;
        public float baseY;
        public float fase;
        public bool recogida;
    }

    private class EscombroEscape
    {
        public RectTransform rt;
        public float velocidad;
        public float giro;
    }

    private class ImagenAmbiente
    {
        public RectTransform rt;
        public Graphic imagen;
        public float baseX;
        public float amplitud;
        public float velocidad;
        public float fase;
        public float deriva;
        public float alfa;
    }

    private readonly List<PlataformaEscape> plataformas = new List<PlataformaEscape>();
    private readonly List<PeligroEscape> peligros = new List<PeligroEscape>();
    private readonly List<LlaveEscape> llaves = new List<LlaveEscape>();
    private readonly List<EscombroEscape> escombros = new List<EscombroEscape>();
    private readonly List<ImagenAmbiente> ambiente = new List<ImagenAmbiente>();
    private readonly List<PlataformaUI> plataformasPrevias = new List<PlataformaUI>();
    private readonly List<bool> estadosPrevios = new List<bool>();

    private RectTransform capaJuego;
    private RectTransform salida;
    private Image marcoSalida;
    private Image brilloSalida;
    private TextMeshProUGUI textoSalida;
    private RawImage marea;
    private Image destello;
    private Image[] corazones;
    private TextMeshProUGUI textoEstado;
    private TextMeshProUGUI textoProgreso;
    private TextMeshProUGUI textoAyuda;
    private Coroutine rutinaEstado;

    private float anchoCarta;
    private float altoCarta;
    private float ySuelo;
    private float yTope;
    private float subidaInicial;
    private float subidaY;
    private float subidaVelocidad;
    private float lineaMuerteMundo;
    private float inicioX;
    private float inicioX2;
    private float alturaSueloMundo;
    private float sueloMundo;
    private float xMinMundo;
    private float xMaxMundo;
    private float anchoJugador;
    private float tiempo;
    private float invulnerable;
    private float invulnerable2;
    private float siguienteEscombro;
    private float velocidadJugador;
    private float fuerzaSalto;
    private float alfaVelo;
    private int vidas;
    private int vidasIniciales;
    private int totalLlaves;
    private int llavesRecogidas;
    private bool activo;
    private bool salidaAbierta;
    private bool reiniciando;
    private bool avisandoEspera;
    private int ultimaVersionRemota;
    private bool usaMarea;
    private bool usaEscombros;
    private string tituloNivel;
    private string subtituloNivel;
    private Color colorFondo;
    private Color colorPlataforma;
    private Color colorAcento;
    private Color colorPeligro;

    private float velOriginal;
    private float saltoOriginal;
    private float gravedadOriginal;
    private float alturaOriginal;
    private float esperaOriginal;
    private float limiteIzqOriginal;
    private float limiteDerOriginal;
    private bool limitesOriginal;
    private bool clickOriginal;
    private bool gatilloOriginal;
    private bool ignorarFocoDeTextoOriginal;

    private float velOriginal2;
    private float saltoOriginal2;
    private float gravedadOriginal2;
    private float alturaOriginal2;
    private float esperaOriginal2;
    private float limiteIzqOriginal2;
    private float limiteDerOriginal2;
    private bool limitesOriginal2;
    private bool clickOriginal2;
    private bool gatilloOriginal2;
    private bool ignorarFocoDeTextoOriginal2;
    private bool remotoOriginal;
    private bool remotoOriginal2;
    private System.Action<PlataformaUI> golpe1;
    private System.Action<PlataformaUI> golpe2;

    private static readonly Color Verde = new Color(0.2f, 0.85f, 0.45f, 1f);
    private static readonly Color Rojo = new Color(0.95f, 0.25f, 0.28f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);
    private static readonly Color Amarillo = new Color(1f, 0.84f, 0.2f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();
        DesactivarPlataformasPrevias();
        GuardarEstadoJugador();

        EventSystem sistemaEventos = EventSystem.current;
        if (sistemaEventos != null)
            sistemaEventos.SetSelectedGameObject(null);

        Image bloqueadorEntrada = raiz.GetComponent<Image>();
        if (bloqueadorEntrada == null)
            bloqueadorEntrada = raiz.gameObject.AddComponent<Image>();
        bloqueadorEntrada.color = new Color(0f, 0f, 0f, 0.01f);
        bloqueadorEntrada.raycastTarget = true;

        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.pivot = new Vector2(0.5f, 0.5f);
        raiz.offsetMin = new Vector2(8f, 8f);
        raiz.offsetMax = new Vector2(-8f, -8f);

        if (raiz.GetComponent<RectMask2D>() == null)
            raiz.gameObject.AddComponent<RectMask2D>();

        Canvas.ForceUpdateCanvases();

        anchoCarta = raiz.rect.width > 1f ? raiz.rect.width : 452f;
        altoCarta = raiz.rect.height > 1f ? raiz.rect.height : 956f;

        ySuelo = -altoCarta * 0.5f + altoCarta * 0.225f;
        yTope = -altoCarta * 0.5f + altoCarta * 0.65f;
        subidaInicial = ySuelo - 240f;

        ConfigurarNivel();

        CrearFondo();
        CrearInterfaz();
        CrearPlataformas();
        CrearPeligrosNivel();
        CrearLlavesNivel();
        CrearSalida();
        CrearDestello();

        PrepararJugador();

        subidaY = subidaInicial;
        ActualizarMareaVisual();
        ActualizarCorazones();
        ActualizarSalida();
        ActualizarProgreso();

        activo = true;

        if (online && esAnfitrion && red != null)
            SincronizarRed();
    }

    private void ConfigurarNivel()
    {
        switch (nivel)
        {
            case 0:
                vidasIniciales = 3;
                velocidadJugador = 470f;
                fuerzaSalto = 1250f;
                usaMarea = true;
                subidaVelocidad = 16f;
                totalLlaves = 0;
                colorPeligro = new Color(0.16f, 0.58f, 0.9f, 0.62f);
                colorFondo = new Color(0.02f, 0.09f, 0.16f, 0.92f);
                colorPlataforma = Color.white;
                colorAcento = new Color(1f, 0.84f, 0.2f, 1f);
                alfaVelo = 0.3f;
                tituloNivel = "SAL DEL SNAP";
                subtituloNivel = "Sube hasta la salida antes de que te alcance la marea";
                break;
            case 1:
                vidasIniciales = 3;
                velocidadJugador = 490f;
                fuerzaSalto = 1250f;
                usaMarea = false;
                subidaVelocidad = 0f;
                totalLlaves = 3;
                colorPeligro = new Color(0.62f, 0.28f, 0.95f, 0.55f);
                colorFondo = new Color(0.04f, 0.01f, 0.08f, 0.95f);
                colorPlataforma = Color.white;
                colorAcento = new Color(1f, 0.58f, 0.12f, 1f);
                alfaVelo = 0.48f;
                tituloNivel = "TRICK OR TREAT";
                subtituloNivel = "Junta 3 dulces y esquiva a los fantasmas";
                break;
            default:
                vidasIniciales = 2;
                velocidadJugador = 540f;
                fuerzaSalto = 1320f;
                usaMarea = true;
                subidaVelocidad = 44f;
                totalLlaves = 3;
                colorPeligro = new Color(1f, 0.36f, 0.64f, 0.68f);
                colorFondo = new Color(0.14f, 0.04f, 0.18f, 0.93f);
                colorPlataforma = Color.white;
                colorAcento = new Color(1f, 0.82f, 0.28f, 1f);
                alfaVelo = 1f;
                tituloNivel = "HBD: MISION PASTEL";
                subtituloNivel = "Junta las 3 velas y llega al pastel";
                break;
        }

        usaEscombros = nivel >= 2;
        vidas = vidasIniciales;
        llavesRecogidas = 0;
        salidaAbierta = totalLlaves == 0;
    }

    private void CrearFondo()
    {
        Color tinteVelo = colorFondo;
        tinteVelo.a = alfaVelo;
        Image velo = UtilidadesDesafio.CrearPanel("Velo", raiz, null, tinteVelo, false);
        UtilidadesDesafio.Estirar(velo.rectTransform);

        Image sombraBase = UtilidadesDesafio.CrearPanel("SombraBase", raiz,
            TemasEscape.Degradado(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.42f)), Color.white, false);
        RectTransform srt = sombraBase.rectTransform;
        srt.anchorMin = new Vector2(0f, 0f);
        srt.anchorMax = new Vector2(1f, 0f);
        srt.pivot = new Vector2(0.5f, 0f);
        srt.sizeDelta = new Vector2(0f, altoCarta * 0.55f);
        srt.anchoredPosition = Vector2.zero;

        CrearDecorado();
        CrearAmbiente();
        CrearMarea();

        capaJuego = UtilidadesDesafio.CrearRect("Juego", raiz);
        UtilidadesDesafio.Estirar(capaJuego);
    }

    private void CrearDecorado()
    {
        switch (nivel)
        {
            case 0:
                Prop("prop_palmera", -anchoCarta * 0.5f + 30f, -altoCarta * 0.5f, 260f, new Color(0.82f, 0.94f, 1f, 1f));
                Prop("prop_palmera", anchoCarta * 0.5f - 34f, -altoCarta * 0.5f, 200f, new Color(0.7f, 0.88f, 1f, 1f));
                Prop("prop_arbusto", -150f, ySuelo - 6f, 54f, new Color(0.78f, 1f, 0.86f, 1f));
                Prop("prop_arbusto", 168f, ySuelo - 6f, 42f, new Color(0.68f, 0.95f, 0.8f, 1f));
                Prop("prop_roca", 108f, ySuelo - 4f, 26f, new Color(0.95f, 0.95f, 1f, 1f));
                Brillo("Sol", new Vector2(1f, 1f), new Vector2(-96f, -150f), 210f,
                    new Color(1f, 0.82f, 0.4f, 0.5f));
                break;
            case 1:
                Prop("prop_pino", -180f, -altoCarta * 0.5f, 340f, new Color(0.4f, 0.3f, 0.62f, 1f));
                Prop("prop_pino", -84f, -altoCarta * 0.5f, 250f, new Color(0.3f, 0.22f, 0.5f, 1f));
                Prop("prop_pino", 178f, -altoCarta * 0.5f, 370f, new Color(0.34f, 0.25f, 0.55f, 1f));
                Prop("prop_calavera", -62f, ySuelo - 4f, 26f, new Color(0.9f, 0.86f, 1f, 0.95f));
                Prop("prop_calavera", 128f, ySuelo - 4f, 22f, new Color(0.85f, 0.8f, 1f, 0.85f));
                Prop("prop_arbusto", 44f, ySuelo - 6f, 46f, new Color(0.38f, 0.28f, 0.58f, 1f));
                Brillo("Luna", new Vector2(1f, 1f), new Vector2(-100f, -160f), 200f,
                    new Color(0.82f, 0.78f, 1f, 0.5f));
                break;
            default:
                CrearDecoracionCumpleanos();
                break;
        }
    }

    private void CrearDecoracionCumpleanos()
    {
        Image banner = UtilidadesDesafio.CrearPanel("BannerHBD", raiz, SpriteRedondeado,
            new Color(0.42f, 0.16f, 0.58f, 0.92f), false);
        UtilidadesDesafio.Colocar(banner.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -82f), new Vector2(190f, 48f));

        TextMeshProUGUI textoHbd = UtilidadesDesafio.CrearTexto("TextoHBD", raiz, Fuente,
            "HBD!", 25f, colorAcento);
        UtilidadesDesafio.Colocar(textoHbd.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -82f), new Vector2(180f, 40f));

        CrearGloboDecorativo(spriteGlobosCumpleanos, -150f, 150f, new Vector2(105f, 145f));
        CrearGloboDecorativo(spriteGloboMorado, 158f, 145f, new Vector2(70f, 95f));
    }

    private void CrearGloboDecorativo(Sprite sprite, float x, float y, Vector2 tamano)
    {
        if (sprite == null)
        {
            Debug.LogError("DesafioEscape necesita los sprites de globos para la historia HBD.", this);
            return;
        }

        Image globo = UtilidadesDesafio.CrearPanel("GloboDecorativo", raiz, sprite, Color.white, false);
        globo.preserveAspect = true;
        UtilidadesDesafio.Colocar(globo.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(x, y), tamano);
    }

    private Image Prop(string archivo, float x, float y, float alto, Color tinte)
    {
        Sprite sprite = TemasEscape.Obtener(nivel, archivo);
        if (sprite == null) return null;

        Image img = UtilidadesDesafio.CrearPanel("Prop_" + archivo, raiz, sprite, tinte, false);
        img.preserveAspect = true;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(alto * sprite.rect.width / sprite.rect.height, alto);
        rt.anchoredPosition = new Vector2(x, y);
        return img;
    }

    private Image Brillo(string nombre, Vector2 ancla, Vector2 pos, float tam, Color color)
    {
        Sprite sprite = TemasEscape.ObtenerSuave(nivel, "luz");
        if (sprite == null) return null;

        Image img = UtilidadesDesafio.CrearPanel(nombre, raiz, sprite, color, false);
        UtilidadesDesafio.Colocar(img.rectTransform, ancla, pos, new Vector2(tam, tam));
        return img;
    }

    private void CrearAmbiente()
    {
        string archivo = nivel == 2 ? "chispa" : "luz";
        Sprite sprite = TemasEscape.ObtenerSuave(nivel, archivo);
        if (sprite == null) return;

        int cantidad = nivel == 1 ? 14 : 10;

        for (int i = 0; i < cantidad; i++)
        {
            Image img = UtilidadesDesafio.CrearPanel("Mota_" + i, raiz, sprite, Color.white, false);
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float tam = Random.Range(8f, nivel == 1 ? 22f : 16f);
            rt.sizeDelta = new Vector2(tam, tam);

            float x = Random.Range(-anchoCarta * 0.5f, anchoCarta * 0.5f);
            float y = Random.Range(-altoCarta * 0.5f, altoCarta * 0.5f);
            rt.anchoredPosition = new Vector2(x, y);

            Color color;
            if (nivel == 2)
            {
                Color[] fiesta =
                {
                    new Color(1f, 0.38f, 0.62f),
                    new Color(0.35f, 0.78f, 1f),
                    new Color(1f, 0.8f, 0.25f),
                    new Color(0.4f, 0.95f, 0.62f)
                };
                color = fiesta[i % fiesta.Length];
                color.a = Random.Range(0.12f, 0.3f);
            }
            else
            {
                color = nivel == 0
                    ? new Color(0.8f, 0.95f, 1f, Random.Range(0.12f, 0.3f))
                    : new Color(0.75f, 0.6f, 1f, Random.Range(0.1f, 0.28f));
            }
            img.color = color;

            ambiente.Add(new ImagenAmbiente
            {
                rt = rt,
                imagen = img,
                baseX = x,
                amplitud = Random.Range(4f, 16f),
                velocidad = Random.Range(0.4f, 1.2f),
                fase = Random.Range(0f, 6.28f),
                deriva = Random.Range(8f, nivel == 2 ? 34f : 20f),
                alfa = color.a
            });
        }
    }

    private void ActualizarAmbiente(float dt)
    {
        if (ambiente.Count == 0) return;

        float limite = altoCarta * 0.5f + 30f;
        float fondo = -altoCarta * 0.5f - 30f;

        for (int i = 0; i < ambiente.Count; i++)
        {
            ImagenAmbiente m = ambiente[i];
            if (m == null || m.rt == null) continue;

            float y = m.rt.anchoredPosition.y + m.deriva * dt;
            if (y > limite) y = fondo;

            float x = m.baseX + Mathf.Sin(tiempo * m.velocidad + m.fase) * m.amplitud;
            m.rt.anchoredPosition = new Vector2(x, y);

            Color c = m.imagen.color;
            c.a = m.alfa * (0.55f + Mathf.Sin(tiempo * m.velocidad * 1.7f + m.fase) * 0.45f);
            m.imagen.color = c;
        }
    }

    private RawImage CrearRaw(string nombre, Transform padre, Texture2D textura, Color color)
    {
        RectTransform rt = UtilidadesDesafio.CrearRect(nombre, padre);
        RawImage raw = rt.gameObject.AddComponent<RawImage>();
        raw.texture = textura;
        raw.color = color;
        raw.raycastTarget = false;
        return raw;
    }

    private void CrearMarea()
    {
        if (!usaMarea) return;

        Texture2D textura = TemasEscape.TexturaSuave(nivel, "particula", true);

        marea = CrearRaw("Marea", raiz, textura, colorPeligro);
        marea.color = new Color(colorPeligro.r, colorPeligro.g, colorPeligro.b, 0.85f);

        RectTransform rt = marea.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(anchoCarta, 200f);
        rt.anchoredPosition = new Vector2(0f, subidaInicial);

        Sprite luz = TemasEscape.ObtenerSuave(nivel, "luz");

        if (luz != null)
        {
            Image resplandor = UtilidadesDesafio.CrearPanel("Resplandor", marea.transform, luz,
                new Color(1f, 1f, 1f, 0.55f), false);
            RectTransform rrt = resplandor.rectTransform;
            rrt.anchorMin = new Vector2(0f, 1f);
            rrt.anchorMax = new Vector2(1f, 1f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = new Vector2(0f, -8f);
            rrt.sizeDelta = new Vector2(0f, 46f);
        }

        Image borde = UtilidadesDesafio.CrearPanel("Borde", marea.transform, null,
            new Color(1f, 1f, 1f, 0.5f), false);
        RectTransform brt = borde.rectTransform;
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(1f, 1f);
        brt.pivot = new Vector2(0.5f, 1f);
        brt.anchoredPosition = Vector2.zero;
        brt.sizeDelta = new Vector2(0f, 4f);
    }

    private void CrearInterfaz()
    {
        Image sombra = UtilidadesDesafio.CrearPanel("SombraTexto", raiz,
            TemasEscape.Degradado(new Color(0f, 0f, 0f, 0.62f), new Color(0f, 0f, 0f, 0f)), Color.white, false);
        RectTransform srt = sombra.rectTransform;
        srt.anchorMin = new Vector2(0f, 1f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.sizeDelta = new Vector2(0f, 270f);
        srt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            tituloNivel, 28f, colorAcento);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -205f), new Vector2(430f, 38f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            subtituloNivel, 12f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -232f), new Vector2(430f, 22f));

        corazones = new Image[vidasIniciales];

        for (int i = 0; i < vidasIniciales; i++)
        {
            Image corazon = UtilidadesDesafio.CrearPanel("Vida_" + i, raiz, TemasEscape.Corazon(), Rojo, false);
            UtilidadesDesafio.Colocar(corazon.rectTransform, new Vector2(0f, 1f),
                new Vector2(42f + i * 28f, -262f), new Vector2(22f, 20f));
            corazones[i] = corazon;
        }

        textoProgreso = UtilidadesDesafio.CrearTexto("Progreso", raiz, Fuente, "", 14f,
            colorAcento, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoProgreso.rectTransform, new Vector2(1f, 1f),
            new Vector2(-102f, -262f), new Vector2(200f, 24f));

        string textoAyudaNivel = nivel == 2
            ? "A/D mover   Espacio saltar   Click / Gatillo reventar globos"
            : "A/D mover   Espacio saltar   Click / Gatillo golpear";
        textoAyuda = UtilidadesDesafio.CrearTexto("Ayuda", raiz, Fuente,
            textoAyudaNivel, 10f, Gris);
        UtilidadesDesafio.Colocar(textoAyuda.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -178f), new Vector2(430f, 16f));

        textoEstado = UtilidadesDesafio.CrearTexto("Estado", raiz, Fuente, "", 15f, Color.white);
        UtilidadesDesafio.Colocar(textoEstado.rectTransform, new Vector2(0.5f, 1f),
            new Vector2(0f, -178f), new Vector2(430f, 18f));
        textoEstado.gameObject.SetActive(false);
    }

    private void CrearPlataformas()
    {
        float paso = (yTope - ySuelo) / 3f;
        float ancho = anchoCarta - 22f;

        CrearPlataforma(0f, ySuelo, ancho, colorPlataforma, false, 0f, 0f, false);

        switch (nivel)
        {
            case 0:
                CrearPlataforma(-140f, ySuelo + paso, 140f, colorPlataforma, false, 0f, 0f, false);
                CrearPlataforma(140f, ySuelo + paso * 2f, 140f, colorPlataforma, false, 0f, 0f, false);
                break;
            case 1:
                CrearPlataforma(-140f, ySuelo + paso, 140f, colorPlataforma, false, 0f, 0f, false);
                CrearPlataforma(130f, ySuelo + paso * 2f, 120f, new Color(0.55f, 0.9f, 1f, 1f), true, 25f, 1.1f, false);
                break;
            default:
                CrearPlataforma(-140f, ySuelo + paso, 140f, new Color(1f, 0.55f, 0.72f, 1f), false, 0f, 0f, true);
                CrearPlataforma(130f, ySuelo + paso * 2f, 120f, new Color(0.48f, 0.82f, 1f, 1f), true, 30f, 1.2f, false);
                break;
        }

        CrearPlataforma(0f, yTope, 200f, new Color(0.6f, 1f, 0.72f, 1f), false, 0f, 0f, false);
    }

    private void CrearPlataforma(float x, float y, float ancho, Color color, bool movil,
        float amplitud, float velocidad, bool rompible)
    {
        const float alto = 16f;

        RectTransform rt = UtilidadesDesafio.CrearRect("Plataforma", capaJuego);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(ancho, alto);
        rt.anchoredPosition = new Vector2(x, y - alto * 0.5f);

        Texture2D textura = TemasEscape.Textura(nivel, "plataforma", true);
        Graphic grafico;

        if (textura != null)
        {
            RawImage raw = rt.gameObject.AddComponent<RawImage>();
            raw.texture = textura;
            raw.uvRect = new Rect(0f, 0f, ancho / textura.width, alto / textura.height);
            raw.color = color;
            raw.raycastTarget = false;
            grafico = raw;
        }
        else
        {
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteRedondeado;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;

            Image borde = UtilidadesDesafio.CrearPanel("Borde", rt, SpriteRedondeado,
                Color.Lerp(color, Color.white, 0.35f), false);
            RectTransform brt = borde.rectTransform;
            brt.anchorMin = new Vector2(0f, 1f);
            brt.anchorMax = new Vector2(1f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(0f, 4f);
            grafico = img;
        }

        PlataformaUI colision = rt.gameObject.AddComponent<PlataformaUI>();
        colision.etiqueta = "escape";

        plataformas.Add(new PlataformaEscape
        {
            rt = rt,
            imagen = grafico,
            colision = colision,
            baseX = x,
            baseY = y - alto * 0.5f,
            ancho = ancho,
            alto = alto,
            movil = movil,
            amplitud = amplitud,
            velocidad = velocidad,
            fase = plataformas.Count * 1.37f,
            rompible = rompible,
            pisada = 0f,
            oculta = 0f,
            colorBase = color
        });
    }

    private void CrearPeligrosNivel()
    {
        float paso = (yTope - ySuelo) / 3f;

        if (nivel == 0)
        {
            CrearPeligro(0f, ySuelo + paso * 1.5f, 150f, 1.1f, 0f,
                new Color(1f, 0.86f, 0.4f, 1f));
        }
        else if (nivel == 1)
        {
            Vector2[] ruta = CrearRutaFantasmas();
            CrearFantasma(ruta, 0, 1);
            CrearFantasma(ruta, 1, -1);
            CrearFantasma(ruta, 2, -1);
        }
        else
        {
            CrearGloboPeligro(-150f, ySuelo + paso * 2.4f, 0.4f, 96f);
            CrearGloboPeligro(150f, ySuelo + paso * 3.4f, 2.2f, 108f);
        }
    }

    private Vector2[] CrearRutaFantasmas()
    {
        float minX = -anchoCarta * 0.5f + 42f;
        float maxX = anchoCarta * 0.5f - 42f;
        float minY = -altoCarta * 0.5f + 48f;
        float maxY = altoCarta * 0.5f - 48f;

        return new[]
        {
            new Vector2(minX, maxY),
            new Vector2(maxX, maxY),
            new Vector2(maxX, minY),
            new Vector2(minX, minY)
        };
    }

    private void CrearFantasma(Vector2[] ruta, int esquinaInicial, int direccionPatrulla)
    {
        if (spriteFantasma == null)
        {
            Debug.LogError("DesafioEscape necesita el sprite fantasma.png para crear los fantasmas de la historia 2.");
            return;
        }

        RectTransform cont = UtilidadesDesafio.CrearRect("Fantasma", capaJuego);
        cont.anchorMin = new Vector2(0.5f, 0.5f);
        cont.anchorMax = new Vector2(0.5f, 0.5f);
        cont.pivot = new Vector2(0.5f, 0.5f);
        cont.sizeDelta = new Vector2(64f, 72f);
        cont.anchoredPosition = ruta[esquinaInicial];

        Sprite luz = TemasEscape.ObtenerSuave(nivel, "luz");

        if (luz != null)
        {
            Image aura = UtilidadesDesafio.CrearPanel("Aura", cont, luz,
                new Color(0.75f, 0.55f, 1f, 0.28f), false);
            UtilidadesDesafio.Colocar(aura.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(104f, 104f));
        }

        Image cuerpo = UtilidadesDesafio.CrearPanel("ImagenFantasma", cont, spriteFantasma,
            Color.white, false);
        cuerpo.preserveAspect = true;
        UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(64f, 72f));

        peligros.Add(new PeligroEscape
        {
            rt = cont,
            cuerpo = cuerpo,
            colorCuerpo = cuerpo.color,
            baseX = ruta[esquinaInicial].x,
            baseY = ruta[esquinaInicial].y,
            amplitud = 0f,
            velocidad = 0f,
            fase = 0f,
            ancho = 46f,
            alto = 52f,
            persigue = false,
            esDron = false,
            esFantasma = true,
            rutaPatrulla = ruta,
            puntoPatrulla = (esquinaInicial + direccionPatrulla + ruta.Length) % ruta.Length,
            direccionPatrulla = direccionPatrulla,
            persigueJugador = false,
            tiempoModo = 0.7f + esquinaInicial * 0.55f,
            velocidadPersecucion = VelocidadCazaFantasmas,
            aturdido = 0f
        });
    }

    private void CrearGloboPeligro(float x, float y, float fase, float velocidadPersecucion)
    {
        RectTransform cont = UtilidadesDesafio.CrearRect(nivel == 2 ? "Globo" : "Dron", capaJuego);
        cont.anchorMin = new Vector2(0.5f, 0.5f);
        cont.anchorMax = new Vector2(0.5f, 0.5f);
        cont.pivot = new Vector2(0.5f, 0.5f);
        cont.sizeDelta = new Vector2(72f, 72f);
        cont.anchoredPosition = new Vector2(x, y);

        Sprite spriteGlobo = fase < 1f ? spriteGloboRojo : spriteGloboVerde;
        if (spriteGlobo == null)
        {
            Debug.LogError("DesafioEscape necesita los sprites individuales de globos para la historia HBD.", this);
            return;
        }

        Image cuerpo = UtilidadesDesafio.CrearPanel("ImagenGlobo", cont, spriteGlobo, Color.white, false);
        cuerpo.preserveAspect = true;
        UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(72f, 72f));

        peligros.Add(new PeligroEscape
        {
            rt = cont,
            cuerpo = cuerpo,
            colorCuerpo = cuerpo.color,
            baseX = x,
            baseY = y,
            amplitud = 0f,
            velocidad = 0f,
            fase = fase,
            ancho = 46f,
            alto = 32f,
            persigue = true,
            esDron = true,
            esFantasma = false,
            rutaPatrulla = null,
            puntoPatrulla = 0,
            direccionPatrulla = 1,
            persigueJugador = false,
            tiempoModo = 0f,
            velocidadPersecucion = velocidadPersecucion,
            aturdido = 0f
        });
    }

    private void CrearPeligro(float x, float y, float amplitud, float velocidad, float fase, Color color)
    {
        RectTransform cont = UtilidadesDesafio.CrearRect("Peligro", capaJuego);
        cont.anchorMin = new Vector2(0.5f, 0.5f);
        cont.anchorMax = new Vector2(0.5f, 0.5f);
        cont.pivot = new Vector2(0.5f, 0.5f);
        cont.sizeDelta = new Vector2(50f, 60f);
        cont.anchoredPosition = new Vector2(x, y);

        Sprite luz = TemasEscape.ObtenerSuave(nivel, "luz");

        if (luz != null)
        {
            Image aura = UtilidadesDesafio.CrearPanel("Aura", cont, luz,
                new Color(color.r, color.g, color.b, 0.3f), false);
            UtilidadesDesafio.Colocar(aura.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(110f, 110f));
        }

        Sprite sprite = TemasEscape.Obtener(nivel, "enemigo");
        Image cuerpo;

        if (sprite != null)
        {
            float alto = 54f;
            float ancho = Mathf.Min(76f, alto * sprite.rect.width / sprite.rect.height);

            cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", cont, sprite, Color.white, false);
            cuerpo.preserveAspect = true;
            UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(ancho, alto));
        }
        else
        {
            cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", cont, SpriteRedondeado, color, false);
            UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(42f, 50f));

            Image ojoI = UtilidadesDesafio.CrearPanel("OjoI", cuerpo.transform, SpriteRedondeado,
                new Color(0.06f, 0.04f, 0.1f, 1f), false);
            UtilidadesDesafio.Colocar(ojoI.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(-9f, 6f), new Vector2(7f, 12f));

            Image ojoD = UtilidadesDesafio.CrearPanel("OjoD", cuerpo.transform, SpriteRedondeado,
                new Color(0.06f, 0.04f, 0.1f, 1f), false);
            UtilidadesDesafio.Colocar(ojoD.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(9f, 6f), new Vector2(7f, 12f));

            Image boca = UtilidadesDesafio.CrearPanel("Boca", cuerpo.transform, SpriteRedondeado,
                new Color(0.06f, 0.04f, 0.1f, 1f), false);
            UtilidadesDesafio.Colocar(boca.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -12f), new Vector2(14f, 5f));
        }

        peligros.Add(new PeligroEscape
        {
            rt = cont,
            cuerpo = cuerpo,
            colorCuerpo = cuerpo.color,
            baseX = x,
            baseY = y,
            amplitud = amplitud,
            velocidad = velocidad,
            fase = fase,
            ancho = 50f,
            alto = 60f,
            persigue = false,
            esDron = false,
            velocidadPersecucion = 0f,
            aturdido = 0f
        });
    }

    private void CrearLlavesNivel()
    {
        if (totalLlaves <= 0) return;

        float paso = (yTope - ySuelo) / 3f;

        if (nivel == 1)
        {
            CrearLlave(-150f, ySuelo + paso + 74f, 0f);
            CrearLlave(130f, ySuelo + paso * 2f + 74f, 1.7f);
            CrearLlave(-132f, yTope + 46f, 3.4f);
            return;
        }

        CrearLlave(-140f, ySuelo + paso + 110f, 0f);
        CrearLlave(-140f, ySuelo + paso * 2f + 110f, 1.7f);
        if (nivel == 2)
            CrearLlave(130f, ySuelo + paso * 2f + 110f, 3.4f);
    }

    private void CrearLlave(float x, float y, float fase)
    {
        RectTransform rt = UtilidadesDesafio.CrearRect("Llave", capaJuego);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(38f, 38f);
        rt.anchoredPosition = new Vector2(x, y);

        Sprite luz = TemasEscape.ObtenerSuave(nivel, "luz");

        if (luz != null)
        {
            Image aura = UtilidadesDesafio.CrearPanel("Aura", rt, luz,
                nivel == 2
                    ? new Color(1f, 0.48f, 0.68f, 0.42f)
                    : new Color(1f, 0.84f, 0.2f, 0.35f), false);
            UtilidadesDesafio.Colocar(aura.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(84f, 84f));
        }

        Sprite sprite = TemasEscape.Obtener(nivel, "coleccionable");
        Image cuerpo;

        if (nivel == 2)
        {
            Color[] coloresVelas =
            {
                new Color(1f, 0.35f, 0.55f, 1f),
                new Color(0.3f, 0.72f, 1f, 1f),
                new Color(0.45f, 0.9f, 0.55f, 1f)
            };
            cuerpo = CrearVelaVisual(rt, Vector2.zero, 1f,
                coloresVelas[llaves.Count % coloresVelas.Length], "Vela");
        }
        else if (nivel == 1)
        {
            cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", rt, TemasEscape.Caramelo(),
                new Color(1f, 0.45f, 0.72f, 1f), false);
            UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(34f, 21f));
        }
        else if (sprite != null)
        {
            cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", rt, sprite, Color.white, false);
            cuerpo.preserveAspect = true;
            UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(30f, 30f * sprite.rect.height / sprite.rect.width));
        }
        else
        {
            cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", rt, SpriteRedondeado, Amarillo, false);
            UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(30f, 30f));

            Image brillo = UtilidadesDesafio.CrearPanel("Brillo", cuerpo.transform, SpriteRedondeado,
                new Color(1f, 1f, 1f, 0.8f), false);
            UtilidadesDesafio.Colocar(brillo.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(-6f, 6f), new Vector2(9f, 9f));
        }

        llaves.Add(new LlaveEscape
        {
            rt = rt,
            imagen = cuerpo,
            baseY = y,
            fase = fase,
            recogida = false
        });
    }

    private Image CrearVelaVisual(Transform padre, Vector2 posicion, float escala, Color color, string nombre)
    {
        Image cuerpo = UtilidadesDesafio.CrearPanel(nombre, padre, SpriteRedondeado, color, false);
        UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f),
            posicion + new Vector2(0f, -3f * escala), new Vector2(12f * escala, 22f * escala));

        Image franja = UtilidadesDesafio.CrearPanel("Franja", cuerpo.transform, SpriteRedondeado,
            new Color(1f, 1f, 1f, 0.75f), false);
        UtilidadesDesafio.Colocar(franja.rectTransform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(13f * escala, 3f * escala));

        Image llama = UtilidadesDesafio.CrearPanel("Llama", padre, SpriteRedondeado,
            new Color(1f, 0.72f, 0.18f, 1f), false);
        UtilidadesDesafio.Colocar(llama.rectTransform, new Vector2(0.5f, 0.5f),
            posicion + new Vector2(0f, 12f * escala), new Vector2(9f * escala, 11f * escala));

        return cuerpo;
    }

    private void CrearSalida()
    {
        salida = UtilidadesDesafio.CrearRect("Salida", capaJuego);
        salida.anchorMin = new Vector2(0.5f, 0.5f);
        salida.anchorMax = new Vector2(0.5f, 0.5f);
        salida.pivot = new Vector2(0.5f, 0f);
        salida.sizeDelta = nivel == 2 ? new Vector2(124f, 172f) : new Vector2(90f, 92f);
        salida.anchoredPosition = new Vector2(0f, yTope);

        Sprite luz = TemasEscape.ObtenerSuave(nivel, "luz");

        if (luz != null)
        {
            brilloSalida = UtilidadesDesafio.CrearPanel("Brillo", salida, luz,
                new Color(0.2f, 1f, 0.5f, 0.2f), false);
            UtilidadesDesafio.Estirar(brilloSalida.rectTransform, -26f);
        }

        if (nivel == 2)
        {
            if (spritePastel == null)
                Debug.LogError("DesafioEscape necesita el sprite Assets/LevelSnapchat/Sprites/pastel.png.", this);

            marcoSalida = UtilidadesDesafio.CrearPanel("Pastel", salida, spritePastel, Color.white, false);
            marcoSalida.preserveAspect = true;
            UtilidadesDesafio.Colocar(marcoSalida.rectTransform, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(124f, 172f));
        }
        else
        {
            Sprite puerta = TemasEscape.Obtener(nivel, "salida");

            if (puerta != null)
            {
                marcoSalida = UtilidadesDesafio.CrearPanel("Marco", salida, puerta, Color.white, false);
                marcoSalida.preserveAspect = true;
                UtilidadesDesafio.Colocar(marcoSalida.rectTransform, new Vector2(0.5f, 0f),
                    new Vector2(0f, 44f), new Vector2(60f, 88f));
            }
            else
            {
                marcoSalida = UtilidadesDesafio.CrearPanel("Marco", salida, SpriteRedondeado,
                    new Color(0.35f, 0.35f, 0.45f, 1f));
                UtilidadesDesafio.Estirar(marcoSalida.rectTransform);

                Image hueco = UtilidadesDesafio.CrearPanel("Hueco", salida, null,
                    new Color(0.03f, 0.03f, 0.07f, 0.95f), false);
                UtilidadesDesafio.Estirar(hueco.rectTransform, 7f);
            }
        }

        textoSalida = UtilidadesDesafio.CrearTexto("Texto", salida, Fuente,
            nivel == 2 ? "PASTEL" : "SALIDA", 11f, Gris);
        UtilidadesDesafio.Colocar(textoSalida.rectTransform, new Vector2(0.5f, 0f),
            new Vector2(0f, nivel == 2 ? -4f : 44f), new Vector2(90f, 18f));
    }

    private void CrearDestello()
    {
        destello = UtilidadesDesafio.CrearPanel("Destello", raiz, null,
            new Color(0.95f, 0.15f, 0.2f, 0f), false);
        UtilidadesDesafio.Estirar(destello.rectTransform);
        destello.rectTransform.SetAsLastSibling();
    }

    private void ActualizarSalida()
    {
        if (marcoSalida == null || textoSalida == null) return;

        Color color = salidaAbierta ? Color.white : new Color(0.45f, 0.42f, 0.55f, 1f);
        marcoSalida.color = color;
        textoSalida.color = salidaAbierta ? Verde : Gris;
        textoSalida.text = nivel == 2
            ? (salidaAbierta ? "HBD!" : "VELAS")
            : (salidaAbierta ? "SALIDA" : "BLOQUEADA");

        if (brilloSalida != null)
            brilloSalida.color = salidaAbierta
                ? new Color(0.25f, 1f, 0.55f, 0.3f)
                : new Color(0.55f, 0.45f, 0.9f, 0.1f);
    }

    private void ActualizarProgreso()
    {
        if (textoProgreso == null) return;

        if (totalLlaves > 0)
        {
            string etiqueta = nivel == 1 ? "DULCES " : nivel == 2 ? "VELAS " : "SNAPS ";
            textoProgreso.text = salidaAbierta
                ? (nivel == 2 ? "PASTEL LISTO" : "SALIDA ABIERTA")
                : etiqueta + llavesRecogidas + "/" + totalLlaves;
            textoProgreso.color = salidaAbierta ? Verde : Amarillo;
        }
        else
        {
            textoProgreso.text = "SALIDA";
            textoProgreso.color = Verde;
        }
    }

    private void ActualizarCorazones()
    {
        if (corazones == null) return;

        for (int i = 0; i < corazones.Length; i++)
        {
            if (corazones[i] == null) continue;
            corazones[i].color = i < vidas ? Rojo : new Color(0.3f, 0.33f, 0.4f, 0.6f);
        }
    }

    private void Update()
    {
        if (online && !esAnfitrion)
        {
            ActualizarRemoto();
            return;
        }

        if (online && esAnfitrion && red != null)
            SincronizarRed();

        if (!activo || reiniciando) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        tiempo += dt;

        if (invulnerable > 0f)
            invulnerable -= dt;

        if (invulnerable2 > 0f)
            invulnerable2 -= dt;

        ActualizarMarea(dt);
        ActualizarPlataformas(dt);
        ActualizarPeligros(dt);
        ActualizarEscombros(dt);
        ActualizarLlaves();
        ActualizarAmbiente(dt);

        if (brilloSalida != null)
        {
            Color cb = brilloSalida.color;
            cb.a = (salidaAbierta ? 0.26f : 0.09f) + Mathf.Sin(tiempo * 3.2f) * 0.06f;
            brilloSalida.color = cb;
        }

        if (destello != null && destello.color.a > 0f)
        {
            Color c = destello.color;
            c.a = Mathf.MoveTowards(c.a, 0f, dt * 1.4f);
            destello.color = c;
        }

        if (ComprobarDano()) return;
        ComprobarSalida();
    }

    private void ActualizarRemoto()
    {
        if (red == null) return;

        float dt = Time.deltaTime;

        tiempo = red.EscapeTiempo;
        subidaY = red.EscapeMarea;
        vidas = red.EscapeVidas;
        llavesRecogidas = red.EscapeDulces;
        salidaAbierta = red.EscapeSalida;

        ActualizarMareaVisual();
        ActualizarCorazones();
        ActualizarSalida();
        ActualizarProgreso();

        int mascaraDulces = red.EscapeMascaraDulces;

        for (int i = 0; i < llaves.Count; i++)
        {
            LlaveEscape l = llaves[i];
            if (l == null || l.rt == null) continue;

            bool recogida = (mascaraDulces & (1 << i)) != 0;
            l.recogida = recogida;
            l.rt.gameObject.SetActive(!recogida);

            if (!recogida)
            {
                float y = l.baseY + Mathf.Sin(tiempo * 2.4f + l.fase) * 7f;
                l.rt.anchoredPosition = new Vector2(l.rt.anchoredPosition.x, y);
            }
        }

        int peligrosActivos = red.EscapePeligrosActivos;

        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];
            if (p == null || p.rt == null) continue;

            bool destruido = (peligrosActivos & (1 << i)) != 0;
            p.rt.gameObject.SetActive(!destruido);

            if (!destruido)
                p.rt.anchoredPosition = red.EscapePeligro(i);
        }

        bool plataformaVisible = red.EscapePlataformaRompible;

        for (int i = 0; i < plataformas.Count; i++)
        {
            PlataformaEscape p = plataformas[i];
            if (p == null || p.rt == null) continue;

            if (p.movil)
            {
                float x = p.baseX + Mathf.Sin(tiempo * p.velocidad + p.fase) * p.amplitud;
                p.rt.anchoredPosition = new Vector2(x, p.baseY);
            }

            if (p.rompible)
                p.rt.gameObject.SetActive(plataformaVisible);
        }

        if (jugadorRT != null)
            jugadorRT.anchoredPosition = red.EscapeP1;

        if (jugador2RT != null)
            jugador2RT.anchoredPosition = red.EscapeP2;

        ActualizarEscombrosRemotos();
        ActualizarAmbiente(dt);

        if (brilloSalida != null)
        {
            Color cb = brilloSalida.color;
            cb.a = (salidaAbierta ? 0.26f : 0.09f) + Mathf.Sin(tiempo * 3.2f) * 0.06f;
            brilloSalida.color = cb;
        }

        if (destello != null && destello.color.a > 0f)
        {
            Color c = destello.color;
            c.a = Mathf.MoveTowards(c.a, 0f, dt * 1.4f);
            destello.color = c;
        }

        if (red.EscapeVersion != ultimaVersionRemota)
        {
            ultimaVersionRemota = red.EscapeVersion;
            ProcesarEventoRemoto(red.EscapeEvento);
        }
    }

    private void ProcesarEventoRemoto(int evento)
    {
        if (evento == 1)
        {
            if (destello != null)
                destello.color = new Color(0.95f, 0.15f, 0.2f, 0.34f);

            UtilidadesDesafio.Sonido("sfx_fallo");
            UtilidadesDesafio.Sacudir(raiz, 16f, 0.3f);
            MostrarEstado("TE ATRAPARON", Rojo, 1.1f);
        }
        else if (evento == 2)
        {
            UtilidadesDesafio.Sonido("sfx_snap", 0.6f);
            UtilidadesDesafio.Sacudir(raiz, 4f, 0.12f);
            string etiqueta = nivel == 1 ? "DULCE " : nivel == 2 ? "VELA " : "SNAP ";
            MostrarEstado(etiqueta + llavesRecogidas + "/" + totalLlaves, Amarillo, 0.8f);
        }
        else if (evento == 3)
        {
            MostrarEstado(nivel == 2 ? "PASTEL LISTO" : "SALIDA ABIERTA", Verde, 1.2f);
            UtilidadesDesafio.Sonido("sfx_ronda", 0.7f);
        }
        else if (evento == 4)
        {
            MostrarEstado("OTRA TOMA", Amarillo, 1.2f);
        }
        else if (evento == 5)
        {
            MostrarEstado("ESPERA A TU COMPAÑERO", Amarillo, 0.8f);
        }
        else if (evento == 6)
        {
            MostrarEstado(nivel == 2 ? "FELIZ CUMPLE!" : "ESCAPASTE", Verde, 2f);
            UtilidadesDesafio.Sonido("sfx_acierto");
        }
    }

    private void ActualizarEscombrosRemotos()
    {
        if (red == null) return;

        int cantidad = red.escapeEscombros.Count;

        while (escombros.Count < cantidad)
            CrearEscombroRemoto();

        while (escombros.Count > cantidad)
        {
            int i = escombros.Count - 1;

            if (escombros[i] != null && escombros[i].rt != null)
                Destroy(escombros[i].rt.gameObject);

            escombros.RemoveAt(i);
        }

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 v = red.escapeEscombros[i];
            EscombroEscape e = escombros[i];

            if (e == null || e.rt == null) continue;

            e.rt.anchoredPosition = new Vector2(v.x, v.y);
            e.rt.localRotation = Quaternion.Euler(0f, 0f, v.z);
        }
    }

    private void CrearEscombroRemoto()
    {
        RectTransform rt = UtilidadesDesafio.CrearRect(nivel == 2 ? "RegaloRemoto" : "EscombroRemoto", capaJuego);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(nivel == 2 ? 22f : 28f, nivel == 2 ? 22f : 28f);

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = SpriteRedondeado;
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        ConfigurarEscombroVisual(img, rt, escombros.Count);

        escombros.Add(new EscombroEscape { rt = rt, velocidad = 0f, giro = 0f });
    }

    private void SincronizarRed()
    {
        if (red == null || !esAnfitrion) return;

        red.FijarEscapeJugadores(
            jugadorRT != null ? jugadorRT.anchoredPosition : Vector2.zero,
            jugador2RT != null ? jugador2RT.anchoredPosition : Vector2.zero);

        red.FijarEscapeTiempo(tiempo);
        red.FijarEscapeEstado(subidaY, vidas, llavesRecogidas, salidaAbierta);

        int mascaraDulces = 0;

        for (int i = 0; i < llaves.Count; i++)
        {
            if (llaves[i] != null && llaves[i].recogida)
                mascaraDulces |= 1 << i;
        }

        int peligrosActivos = 0;

        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];

            if (p == null || p.rt == null || !p.rt.gameObject.activeSelf)
                peligrosActivos |= 1 << i;
        }

        bool plataformaRompible = true;

        for (int i = 0; i < plataformas.Count; i++)
        {
            PlataformaEscape p = plataformas[i];

            if (p != null && p.rompible && p.rt != null)
                plataformaRompible = p.rt.gameObject.activeSelf;
        }

        red.FijarEscapeMascaras(mascaraDulces, peligrosActivos, plataformaRompible);

        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = new Vector2(0f, 99999f);

            if (i < peligros.Count && peligros[i] != null && peligros[i].rt != null)
                pos = peligros[i].rt.anchoredPosition;

            red.FijarEscapePeligro(i, pos);
        }

        Vector3[] lista = new Vector3[escombros.Count];

        for (int i = 0; i < escombros.Count; i++)
        {
            if (escombros[i] == null || escombros[i].rt == null) continue;

            Vector2 pos = escombros[i].rt.anchoredPosition;
            lista[i] = new Vector3(pos.x, pos.y, escombros[i].rt.localEulerAngles.z);
        }

        red.FijarEscapeEscombros(lista);
    }

    private void ActualizarMarea(float dt)
    {
        if (!usaMarea) return;

        subidaY += subidaVelocidad * dt;
        if (subidaY > yTope + 140f) subidaY = yTope + 140f;
        ActualizarMareaVisual();
    }

    private void ActualizarPlataformas(float dt)
    {
        for (int i = 0; i < plataformas.Count; i++)
        {
            PlataformaEscape p = plataformas[i];
            if (p == null || p.rt == null) continue;

            if (p.movil)
            {
                float x = p.baseX + Mathf.Sin(tiempo * p.velocidad + p.fase) * p.amplitud;
                p.rt.anchoredPosition = new Vector2(x, p.baseY);
            }

            if (!p.rompible) continue;

            if (p.oculta > 0f)
            {
                p.oculta -= dt;

                if (p.oculta <= 0f)
                {
                    p.rt.gameObject.SetActive(true);
                    p.pisada = 0f;
                    p.imagen.color = p.colorBase;
                }

                continue;
            }

            bool encima = jugador != null && jugador.PlataformaActual == p.colision;

            if (!encima && jugador2 != null)
                encima = jugador2.PlataformaActual == p.colision;

            if (encima)
            {
                p.pisada += dt;
                p.imagen.color = Color.Lerp(p.colorBase, Rojo, Mathf.Clamp01(p.pisada / 0.65f));

                if (p.pisada >= 0.65f)
                {
                    p.rt.gameObject.SetActive(false);
                    p.oculta = 1.4f;
                    p.pisada = 0f;
                    UtilidadesDesafio.Sonido("sfx_desaparece", 0.5f);
                }
            }
            else
            {
                p.pisada = Mathf.Max(0f, p.pisada - dt * 1.5f);
                p.imagen.color = Color.Lerp(p.colorBase, Rojo, Mathf.Clamp01(p.pisada / 0.65f));
            }
        }
    }

    private void ActualizarPeligros(float dt)
    {
        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];
            if (p == null || p.rt == null || !p.rt.gameObject.activeSelf) continue;

            Vector2 centro = CentroJugadorMasCercano(p.rt.anchoredPosition);

            if (p.esFantasma)
            {
                if (p.aturdido > 0f)
                {
                    p.aturdido -= dt;
                    if (p.cuerpo != null)
                    {
                        Color color = p.colorCuerpo;
                        color.a = p.aturdido > 0f ? 0.35f : 1f;
                        p.cuerpo.color = color;
                    }
                }

                MoverFantasma(p, dt, centro);
                continue;
            }

            if (p.persigue)
            {
                if (p.aturdido > 0f)
                {
                    p.aturdido -= dt;

                    if (p.cuerpo != null)
                    {
                        Color c = p.colorCuerpo;
                        c.a = 0.35f;
                        p.cuerpo.color = c;
                    }

                    if (p.aturdido <= 0f && p.cuerpo != null)
                        p.cuerpo.color = p.colorCuerpo;

                    continue;
                }

                Vector2 actual = p.rt.anchoredPosition;
                Vector2 destino = centro;
                float dx = destino.x - actual.x;
                float dy = destino.y - actual.y;
                float distancia = Mathf.Sqrt(dx * dx + dy * dy);

                if (distancia > 1f)
                {
                    float paso = Mathf.Min(distancia, p.velocidadPersecucion * dt);
                    actual.x += dx / distancia * paso;
                    actual.y += dy / distancia * paso;
                }

                if (p.esDron)
                {
                    actual.x += Mathf.Sin(tiempo * 3.1f + p.fase) * 10f * dt;
                    actual.y = Mathf.Clamp(actual.y, ySuelo + 30f, yTope + 60f);
                }

                actual.x = Mathf.Clamp(actual.x, -anchoCarta * 0.5f + 26f, anchoCarta * 0.5f - 26f);
                actual.y = Mathf.Clamp(actual.y, ySuelo - 20f, yTope + 70f);
                p.rt.anchoredPosition = actual;
                continue;
            }

            float x = p.baseX + Mathf.Sin(tiempo * p.velocidad + p.fase) * p.amplitud;
            p.rt.anchoredPosition = new Vector2(x, p.baseY + Mathf.Sin(tiempo * 2.1f + p.fase) * 5f);
        }
    }

    private void MoverFantasma(PeligroEscape fantasma, float dt, Vector2 jugadorCentro)
    {
        if (fantasma.rutaPatrulla == null || fantasma.rutaPatrulla.Length == 0) return;

        fantasma.tiempoModo -= dt;
        if (fantasma.tiempoModo <= 0f)
        {
            fantasma.persigueJugador = !fantasma.persigueJugador;
            fantasma.tiempoModo = fantasma.persigueJugador
                ? DuracionCazaFantasmas
                : DuracionPatrullaFantasmas;

            if (!fantasma.persigueJugador)
                fantasma.puntoPatrulla = PuntoRutaMasCercano(fantasma);
        }

        Vector2 posicionActual = fantasma.rt.anchoredPosition;
        if (fantasma.persigueJugador)
        {
            Vector2 haciaJugador = jugadorCentro - posicionActual;
            if (haciaJugador.sqrMagnitude <= DistanciaDetencionCaza * DistanciaDetencionCaza)
            {
                fantasma.persigueJugador = false;
                fantasma.tiempoModo = DuracionPatrullaFantasmas;
                fantasma.puntoPatrulla = PuntoRutaMasCercano(fantasma);
            }
        }

        if (fantasma.persigueJugador)
        {
            Vector2 haciaJugador = jugadorCentro - posicionActual;
            float distancia = haciaJugador.magnitude;
            if (distancia > 0.01f)
            {
                float paso = Mathf.Min(fantasma.velocidadPersecucion * dt, distancia);
                fantasma.rt.anchoredPosition = posicionActual + haciaJugador / distancia * paso;
            }

            SepararFantasmas();
            return;
        }

        float distanciaRestante = VelocidadPatrullaFantasmas * dt;
        int pasos = 0;
        while (distanciaRestante > 0f && pasos < fantasma.rutaPatrulla.Length)
        {
            Vector2 posicion = fantasma.rt.anchoredPosition;
            Vector2 objetivo = fantasma.rutaPatrulla[fantasma.puntoPatrulla];
            Vector2 haciaObjetivo = objetivo - posicion;
            float distanciaAlObjetivo = haciaObjetivo.magnitude;

            if (distanciaAlObjetivo <= distanciaRestante || distanciaAlObjetivo <= 0.01f)
            {
                fantasma.rt.anchoredPosition = objetivo;
                distanciaRestante -= distanciaAlObjetivo;
                fantasma.puntoPatrulla = SiguientePuntoPatrulla(fantasma);
                pasos++;
                continue;
            }

            fantasma.rt.anchoredPosition = posicion + haciaObjetivo / distanciaAlObjetivo * distanciaRestante;
            distanciaRestante = 0f;
        }

        SepararFantasmas();
    }

    private int SiguientePuntoPatrulla(PeligroEscape fantasma)
    {
        int siguiente = fantasma.puntoPatrulla + fantasma.direccionPatrulla;
        if (siguiente < 0) return fantasma.rutaPatrulla.Length - 1;
        if (siguiente >= fantasma.rutaPatrulla.Length) return 0;
        return siguiente;
    }

    private int PuntoRutaMasCercano(PeligroEscape fantasma)
    {
        int mejor = 0;
        float mejorDistancia = float.PositiveInfinity;
        Vector2 posicion = fantasma.rt.anchoredPosition;

        for (int i = 0; i < fantasma.rutaPatrulla.Length; i++)
        {
            float distancia = (fantasma.rutaPatrulla[i] - posicion).sqrMagnitude;
            if (distancia >= mejorDistancia) continue;

            mejorDistancia = distancia;
            mejor = i;
        }

        int siguiente = mejor + fantasma.direccionPatrulla;
        if (siguiente < 0) return fantasma.rutaPatrulla.Length - 1;
        if (siguiente >= fantasma.rutaPatrulla.Length) return 0;
        return siguiente;
    }

    private void SepararFantasmas()
    {
        float minX = -anchoCarta * 0.5f + 32f;
        float maxX = anchoCarta * 0.5f - 32f;
        float minY = -altoCarta * 0.5f + 48f;
        float maxY = altoCarta * 0.5f - 48f;
        float separacionCuadrada = SeparacionMinimaFantasmas * SeparacionMinimaFantasmas;

        for (int iteracion = 0; iteracion < 8; iteracion++)
        {
            for (int i = 0; i < peligros.Count; i++)
            {
                PeligroEscape primero = peligros[i];
                if (primero == null || primero.rt == null || !primero.esFantasma) continue;

                for (int j = i + 1; j < peligros.Count; j++)
                {
                    PeligroEscape segundo = peligros[j];
                    if (segundo == null || segundo.rt == null || !segundo.esFantasma) continue;

                    Vector2 diferencia = primero.rt.anchoredPosition - segundo.rt.anchoredPosition;
                    float distanciaCuadrada = diferencia.sqrMagnitude;
                    if (distanciaCuadrada >= separacionCuadrada) continue;

                    Vector2 direccion = distanciaCuadrada > 0.01f
                        ? diferencia / Mathf.Sqrt(distanciaCuadrada)
                        : new Vector2(i % 2 == 0 ? 1f : -1f, j % 2 == 0 ? 1f : -1f).normalized;
                    float correccion = (SeparacionMinimaFantasmas - Mathf.Sqrt(distanciaCuadrada)) * 0.5f;
                    Vector2 posicionPrimero = primero.rt.anchoredPosition + direccion * correccion;
                    Vector2 posicionSegundo = segundo.rt.anchoredPosition - direccion * correccion;

                    posicionPrimero.x = Mathf.Clamp(posicionPrimero.x, minX, maxX);
                    posicionPrimero.y = Mathf.Clamp(posicionPrimero.y, minY, maxY);
                    posicionSegundo.x = Mathf.Clamp(posicionSegundo.x, minX, maxX);
                    posicionSegundo.y = Mathf.Clamp(posicionSegundo.y, minY, maxY);
                    primero.rt.anchoredPosition = posicionPrimero;
                    segundo.rt.anchoredPosition = posicionSegundo;
                }
            }
        }
    }

    private void ActualizarEscombros(float dt)
    {
        if (!usaEscombros) return;

        if (salidaAbierta)
        {
            LimpiarEscombros();
            return;
        }

        siguienteEscombro -= dt;

        if (siguienteEscombro <= 0f)
        {
            CrearEscombro();
            siguienteEscombro = Random.Range(0.6f, 1f);
        }

        for (int i = escombros.Count - 1; i >= 0; i--)
        {
            EscombroEscape e = escombros[i];

            if (e == null || e.rt == null)
            {
                escombros.RemoveAt(i);
                continue;
            }

            Vector2 pos = e.rt.anchoredPosition;
            pos.y -= e.velocidad * dt;
            e.rt.anchoredPosition = pos;
            e.rt.Rotate(0f, 0f, e.giro * dt);

            if (pos.y < ySuelo - 120f)
            {
                Destroy(e.rt.gameObject);
                escombros.RemoveAt(i);
            }
        }
    }

    private void CrearEscombro()
    {
        RectTransform rt = UtilidadesDesafio.CrearRect(nivel == 2 ? "Regalo" : "Escombro", capaJuego);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(nivel == 2 ? 22f : 28f, nivel == 2 ? 22f : 28f);
        rt.anchoredPosition = new Vector2(Random.Range(-170f, 170f), yTope + 100f);

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = SpriteRedondeado;
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        ConfigurarEscombroVisual(img, rt, escombros.Count);

        escombros.Add(new EscombroEscape
        {
            rt = rt,
            velocidad = Random.Range(260f, 360f),
            giro = Random.Range(-140f, 140f)
        });
    }

    private void ConfigurarEscombroVisual(Image imagen, Transform padre, int indice)
    {
        if (nivel != 2)
        {
            imagen.color = new Color(1f, 0.45f, 0.12f, 1f);
            return;
        }

        Color[] coloresRegalo =
        {
            new Color(1f, 0.35f, 0.55f, 1f),
            new Color(0.3f, 0.72f, 1f, 1f),
            new Color(0.55f, 0.9f, 0.45f, 1f)
        };
        imagen.color = coloresRegalo[indice % coloresRegalo.Length];

        Image cintaVertical = UtilidadesDesafio.CrearPanel("CintaVertical", padre, SpriteRedondeado,
            new Color(1f, 0.84f, 0.3f, 1f), false);
        UtilidadesDesafio.Estirar(cintaVertical.rectTransform, 8f);

        Image cintaHorizontal = UtilidadesDesafio.CrearPanel("CintaHorizontal", padre, SpriteRedondeado,
            new Color(1f, 0.84f, 0.3f, 1f), false);
        UtilidadesDesafio.Colocar(cintaHorizontal.rectTransform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(22f, 4f));
    }

    private void LimpiarEscombros()
    {
        for (int i = 0; i < escombros.Count; i++)
        {
            if (escombros[i] != null && escombros[i].rt != null)
                Destroy(escombros[i].rt.gameObject);
        }

        escombros.Clear();
    }

    private void ActualizarLlaves()
    {
        if (llaves.Count == 0) return;

        Rect jugadorRect = RectJugador();
        Rect jugador2Rect = jugador2RT != null ? RectJugador(jugador2RT) : jugadorRect;

        for (int i = 0; i < llaves.Count; i++)
        {
            LlaveEscape l = llaves[i];
            if (l == null || l.rt == null || l.recogida) continue;

            float y = l.baseY + Mathf.Sin(tiempo * 2.4f + l.fase) * 7f;
            l.rt.anchoredPosition = new Vector2(l.rt.anchoredPosition.x, y);

            Rect r = new Rect(l.rt.anchoredPosition.x - 20f, y - 20f, 40f, 40f);
            if (!r.Overlaps(jugadorRect) && !r.Overlaps(jugador2Rect)) continue;

            l.recogida = true;
            l.rt.gameObject.SetActive(false);
            llavesRecogidas++;

            UtilidadesDesafio.Sonido("sfx_snap", 0.6f);
            UtilidadesDesafio.Sacudir(raiz, 4f, 0.12f);
            string etiqueta = nivel == 1 ? "DULCE " : nivel == 2 ? "VELA " : "SNAP ";
            MostrarEstado(etiqueta + llavesRecogidas + "/" + totalLlaves, Amarillo, 0.8f);
            ActualizarProgreso();

            if (online && esAnfitrion && red != null)
                red.FijarEscapeEvento(2);

            if (llavesRecogidas >= totalLlaves)
                AbrirSalida();
        }
    }

    private void AbrirSalida()
    {
        if (salidaAbierta) return;

        salidaAbierta = true;
        ActualizarSalida();
        ActualizarProgreso();
        MostrarEstado(nivel == 2 ? "PASTEL LISTO" : "SALIDA ABIERTA", Verde, 1.2f);
        UtilidadesDesafio.Sonido("sfx_ronda", 0.7f);

        if (online && esAnfitrion && red != null)
            red.FijarEscapeEvento(3);
    }

    private bool ComprobarDano()
    {
        if (ComprobarDanoJugador(jugador, jugadorRT, false)) return true;
        if (jugador2 != null && RedSnapchat.HayCliente &&
            ComprobarDanoJugador(jugador2, jugador2RT, true)) return true;

        return false;
    }

    private bool ComprobarDanoJugador(JugadorTutorial quien, RectTransform rt, bool esP2)
    {
        if (quien == null || rt == null) return false;
        if (esP2 ? invulnerable2 > 0f : invulnerable > 0f) return false;

        Rect jugadorRect = RectJugador(rt);

        if (usaMarea && jugadorRect.yMin < subidaY)
        {
            RecibirDano(nivel == 2 ? "TE ALCANZO EL GLASEADO" : "TE ALCANZO LA MAREA", esP2);
            return true;
        }

        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];
            if (p == null || p.rt == null || !p.rt.gameObject.activeSelf) continue;

            Rect r = new Rect(
                p.rt.anchoredPosition.x - p.ancho * 0.5f,
                p.rt.anchoredPosition.y - p.alto * 0.5f,
                p.ancho,
                p.alto);

            if (!r.Overlaps(jugadorRect)) continue;

            RecibirDano("TE ATRAPARON", esP2);
            return true;
        }

        for (int i = 0; i < escombros.Count; i++)
        {
            EscombroEscape e = escombros[i];
            if (e == null || e.rt == null) continue;

            Rect r = new Rect(
                e.rt.anchoredPosition.x - 15f,
                e.rt.anchoredPosition.y - 15f,
                30f,
                30f);

            if (!r.Overlaps(jugadorRect)) continue;

            Destroy(e.rt.gameObject);
            escombros.RemoveAt(i);
            RecibirDano(nivel == 2 ? "TE CAYO UN REGALO" : "TE GOLPEO UN ESCOMBRO", esP2);
            return true;
        }

        if (rt.anchoredPosition.y < lineaMuerteMundo)
        {
            RecibirDano("CAISTE AL VACIO", esP2);
            return true;
        }

        return false;
    }

    private void ComprobarSalida()
    {
        if (!salidaAbierta || salida == null || jugadorRT == null) return;

        Rect r = PlataformaUI.RectDe(raiz, salida);

        if (!r.Overlaps(RectJugador(jugadorRT)))
        {
            avisandoEspera = false;
            return;
        }

        if (jugador2RT != null && RedSnapchat.HayCliente && !r.Overlaps(RectJugador(jugador2RT)))
        {
            if (!avisandoEspera)
            {
                avisandoEspera = true;
                MostrarEstado("ESPERA A TU COMPAÑERO", Amarillo, 1.2f);

                if (online && esAnfitrion && red != null)
                    red.FijarEscapeEvento(5);
            }

            return;
        }

        avisandoEspera = false;
        Ganar();
    }

    private void RecibirDano(string motivo, bool esP2 = false)
    {
        if (!activo || reiniciando) return;
        if (esP2 ? invulnerable2 > 0f : invulnerable > 0f) return;

        vidas--;
        ActualizarCorazones();

        if (esP2)
            invulnerable2 = 1.4f;
        else
            invulnerable = 1.4f;

        if (destello != null)
            destello.color = new Color(0.95f, 0.15f, 0.2f, 0.34f);

        UtilidadesDesafio.Sonido("sfx_fallo");
        UtilidadesDesafio.Sacudir(raiz, 16f, 0.3f);
        MostrarEstado(motivo, Rojo, 1.1f);

        if (online && esAnfitrion && red != null)
            red.FijarEscapeEvento(1);

        if (vidas <= 0)
        {
            StartCoroutine(ReiniciarNivel());
            return;
        }

        StartCoroutine(Reaparecer(0.35f, esP2));
    }

    private IEnumerator Reaparecer(float espera, bool esP2)
    {
        yield return new WaitForSecondsRealtime(espera);
        if (!activo || reiniciando) yield break;

        subidaY = subidaInicial;
        ActualizarMareaVisual();

        JugadorTutorial quien = esP2 ? jugador2 : jugador;

        if (quien != null)
            quien.ColocarEn(new Vector2(esP2 ? inicioX2 : inicioX, sueloMundo));

        if (esP2)
            invulnerable2 = 1.2f;
        else
            invulnerable = 1.2f;
    }

    private IEnumerator ReiniciarNivel()
    {
        reiniciando = true;
        activo = false;

        if (online && esAnfitrion && red != null)
            red.FijarEscapeEvento(4);

        yield return new WaitForSecondsRealtime(1.5f);

        vidas = vidasIniciales;
        ActualizarCorazones();

        subidaY = subidaInicial;
        ActualizarMareaVisual();
        invulnerable = 1.6f;
        invulnerable2 = 1.6f;
        tiempo = 0f;
        siguienteEscombro = 0.7f;
        avisandoEspera = false;

        for (int i = 0; i < plataformas.Count; i++)
        {
            PlataformaEscape p = plataformas[i];
            if (p == null || p.rt == null) continue;

            p.rt.gameObject.SetActive(true);
            p.pisada = 0f;
            p.oculta = 0f;
            p.imagen.color = p.colorBase;
        }

        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];
            if (p == null || p.rt == null) continue;

            p.rt.gameObject.SetActive(true);
            p.aturdido = 0f;
        }

        for (int i = 0; i < llaves.Count; i++)
        {
            LlaveEscape l = llaves[i];
            if (l == null || l.rt == null) continue;

            l.recogida = false;
            l.rt.gameObject.SetActive(true);
        }

        llavesRecogidas = 0;
        salidaAbierta = totalLlaves == 0;
        ActualizarSalida();
        ActualizarProgreso();
        LimpiarEscombros();

        if (jugador != null)
            jugador.ColocarEn(new Vector2(inicioX, sueloMundo));

        if (jugador2 != null)
            jugador2.ColocarEn(new Vector2(inicioX2, sueloMundo));

        reiniciando = false;
        activo = true;
    }

    private void Ganar()
    {
        if (!activo) return;

        activo = false;
        MostrarEstado(nivel == 2 ? "FELIZ CUMPLE!" : "ESCAPASTE", Verde, 2f);
        UtilidadesDesafio.Sonido("sfx_acierto");
        LimpiarEscombros();

        if (online && esAnfitrion && red != null)
            red.FijarEscapeEvento(6);

        if (jugador != null)
            jugador.enabled = false;

        if (jugador2 != null)
            jugador2.enabled = false;

        StartCoroutine(Final());
    }

    private IEnumerator Final()
    {
        yield return new WaitForSecondsRealtime(0.9f);
        Completar();
    }

    private void MostrarEstado(string texto, Color color, float duracion)
    {
        if (textoEstado == null) return;

        textoEstado.text = texto;
        textoEstado.color = color;
        textoEstado.gameObject.SetActive(true);

        if (textoAyuda != null)
            textoAyuda.gameObject.SetActive(false);

        if (rutinaEstado != null)
            StopCoroutine(rutinaEstado);

        rutinaEstado = StartCoroutine(EstadoTemporal(duracion));
    }

    private IEnumerator EstadoTemporal(float duracion)
    {
        yield return new WaitForSecondsRealtime(duracion);

        if (textoEstado != null) textoEstado.gameObject.SetActive(false);
        if (textoAyuda != null) textoAyuda.gameObject.SetActive(true);

        rutinaEstado = null;
    }

    private void AlGolpear(JugadorTutorial quien)
    {
        if (!activo || reiniciando) return;
        if (quien == null) return;

        RectTransform rt = (RectTransform)quien.transform;
        Vector2 centro = RectJugador(rt).center;
        bool golpeo = false;

        for (int i = 0; i < peligros.Count; i++)
        {
            PeligroEscape p = peligros[i];
            if (p == null || p.rt == null || !p.rt.gameObject.activeSelf) continue;

            Vector2 pos = p.rt.anchoredPosition;
            if (Mathf.Abs(pos.x - centro.x) > 150f || Mathf.Abs(pos.y - centro.y) > 120f) continue;

            p.rt.gameObject.SetActive(false);
            golpeo = true;
        }

        for (int i = escombros.Count - 1; i >= 0; i--)
        {
            EscombroEscape e = escombros[i];
            if (e == null || e.rt == null) continue;

            Vector2 pos = e.rt.anchoredPosition;
            if (Mathf.Abs(pos.x - centro.x) > 130f || Mathf.Abs(pos.y - centro.y) > 110f) continue;

            Destroy(e.rt.gameObject);
            escombros.RemoveAt(i);
            golpeo = true;
        }

        if (!golpeo) return;

        UtilidadesDesafio.Sonido("sfx_snap", 0.6f);
        UtilidadesDesafio.Sacudir(raiz, 6f, 0.15f);
    }

    private Rect RectJugador()
    {
        return RectJugador(jugadorRT);
    }

    private Rect RectJugador(RectTransform rt)
    {
        if (rt == null || raiz == null) return new Rect();

        Rect r = rt.rect;
        Vector3 a = raiz.InverseTransformPoint(rt.TransformPoint(new Vector3(r.xMin, r.yMin, 0f)));
        Vector3 b = raiz.InverseTransformPoint(rt.TransformPoint(new Vector3(r.xMax, r.yMax, 0f)));

        return Rect.MinMaxRect(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y),
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y));
    }

    private Vector2 CentroJugador()
    {
        Rect r = RectJugador();
        return r.center;
    }

    private Vector2 CentroJugadorMasCercano(Vector2 desde)
    {
        Vector2 centro1 = CentroJugador();

        if (jugador2RT == null || !RedSnapchat.HayCliente)
            return centro1;

        Vector2 centro2 = RectJugador(jugador2RT).center;

        float d1 = (centro1 - desde).sqrMagnitude;
        float d2 = (centro2 - desde).sqrMagnitude;

        return d2 < d1 ? centro2 : centro1;
    }

    private void ActualizarMareaVisual()
    {
        if (marea == null) return;

        RectTransform rt = marea.rectTransform;
        rt.anchoredPosition = new Vector2(0f, subidaY);

        float fondo = -altoCarta * 0.5f - 40f;
        float alto = Mathf.Max(60f, subidaY - fondo);
        rt.sizeDelta = new Vector2(anchoCarta, alto);

        if (marea.texture != null)
            marea.uvRect = new Rect(0f, 0f, anchoCarta / 150f, alto / 150f);

        Color c = marea.color;
        c.a = 0.72f + Mathf.Sin(tiempo * 2.2f) * 0.1f;
        marea.color = c;
    }

    private void DesactivarPlataformasPrevias()
    {
        plataformasPrevias.Clear();
        estadosPrevios.Clear();

        PlataformaUI[] todas = PlataformaUI.Todas.ToArray();

        for (int i = 0; i < todas.Length; i++)
        {
            if (todas[i] == null) continue;

            plataformasPrevias.Add(todas[i]);
            estadosPrevios.Add(todas[i].enabled);
            todas[i].enabled = false;
        }
    }

    private void RestaurarPlataformasPrevias()
    {
        for (int i = 0; i < plataformasPrevias.Count; i++)
        {
            if (plataformasPrevias[i] != null)
                plataformasPrevias[i].enabled = estadosPrevios[i];
        }

        plataformasPrevias.Clear();
        estadosPrevios.Clear();
    }

    private void GuardarEstadoJugador()
    {
        if (jugador == null) return;

        velOriginal = jugador.velocidad;
        saltoOriginal = jugador.fuerzaSalto;
        gravedadOriginal = jugador.gravedad;
        alturaOriginal = jugador.alturaSuelo;
        esperaOriginal = jugador.esperaEntreGolpes;
        limitesOriginal = jugador.usarLimitesPersonalizados;
        limiteIzqOriginal = jugador.limiteIzquierdo;
        limiteDerOriginal = jugador.limiteDerecho;
        clickOriginal = jugador.usarClick;
        gatilloOriginal = jugador.usarGatillo;
        ignorarFocoDeTextoOriginal = jugador.ignorarFocoDeTexto;
        remotoOriginal = jugador.usarEntradaRemota;

        if (jugador2 == null) return;

        velOriginal2 = jugador2.velocidad;
        saltoOriginal2 = jugador2.fuerzaSalto;
        gravedadOriginal2 = jugador2.gravedad;
        alturaOriginal2 = jugador2.alturaSuelo;
        esperaOriginal2 = jugador2.esperaEntreGolpes;
        limitesOriginal2 = jugador2.usarLimitesPersonalizados;
        limiteIzqOriginal2 = jugador2.limiteIzquierdo;
        limiteDerOriginal2 = jugador2.limiteDerecho;
        clickOriginal2 = jugador2.usarClick;
        gatilloOriginal2 = jugador2.usarGatillo;
        ignorarFocoDeTextoOriginal2 = jugador2.ignorarFocoDeTexto;
        remotoOriginal2 = jugador2.usarEntradaRemota;
    }

    private void PrepararJugador()
    {
        if (jugador == null || jugadorRT == null || mundo == null || historia == null) return;

        Rect carta = PlataformaUI.RectDe(mundo, historia);
        anchoJugador = jugadorRT.rect.width;

        inicioX = carta.xMin + 30f;
        inicioX2 = carta.xMax - anchoJugador - 30f;
        sueloMundo = carta.center.y + ySuelo;
        lineaMuerteMundo = sueloMundo - 40f;
        xMinMundo = carta.xMin + 10f;
        xMaxMundo = Mathf.Max(xMinMundo, carta.xMax - anchoJugador - 10f);

        ConfigurarDino(jugador, inicioX, carta, false);
        ConfigurarDino(jugador2, inicioX2, carta, true);

        bool cliente = online && !esAnfitrion;

        jugador.usarEntradaRemota = cliente;
        jugador.enabled = !cliente;

        golpe1 = _ => AlGolpear(jugador);
        jugador.alGolpear += golpe1;

        if (jugador2 == null) return;

        jugador2.usarEntradaRemota = online && esAnfitrion;
        jugador2.enabled = !cliente;

        golpe2 = _ => AlGolpear(jugador2);
        jugador2.alGolpear += golpe2;
    }

    private void ConfigurarDino(JugadorTutorial dino, float x, Rect carta, bool esP2)
    {
        if (dino == null) return;

        dino.usarLimitesPersonalizados = true;
        dino.limiteIzquierdo = xMinMundo;
        dino.limiteDerecho = xMaxMundo;
        dino.alturaSuelo = carta.yMin - 200f;
        dino.usarClick = true;
        dino.usarGatillo = true;
        dino.ignorarFocoDeTexto = true;
        dino.esperaEntreGolpes = 0.4f;
        dino.velocidad = velocidadJugador;
        dino.fuerzaSalto = fuerzaSalto;
        dino.gravedad = 2300f;

        dino.ColocarEn(new Vector2(x, sueloMundo));

        if (!esP2)
            dino.enabled = true;
    }

    private void OnDestroy()
    {
        RestaurarPlataformasPrevias();

        if (rutinaEstado != null)
            StopCoroutine(rutinaEstado);

        if (jugador != null)
        {
            if (golpe1 != null)
                jugador.alGolpear -= golpe1;

            jugador.enabled = false;
            jugador.usarClick = clickOriginal;
            jugador.usarGatillo = gatilloOriginal;
            jugador.ignorarFocoDeTexto = ignorarFocoDeTextoOriginal;
            jugador.esperaEntreGolpes = esperaOriginal;
            jugador.velocidad = velOriginal;
            jugador.fuerzaSalto = saltoOriginal;
            jugador.gravedad = gravedadOriginal;
            jugador.alturaSuelo = alturaOriginal;
            jugador.usarLimitesPersonalizados = limitesOriginal;
            jugador.limiteIzquierdo = limiteIzqOriginal;
            jugador.limiteDerecho = limiteDerOriginal;
            jugador.usarEntradaRemota = remotoOriginal;
        }

        if (jugador2 == null) return;

        if (golpe2 != null)
            jugador2.alGolpear -= golpe2;

        jugador2.enabled = false;
        jugador2.usarClick = clickOriginal2;
        jugador2.usarGatillo = gatilloOriginal2;
        jugador2.ignorarFocoDeTexto = ignorarFocoDeTextoOriginal2;
        jugador2.esperaEntreGolpes = esperaOriginal2;
        jugador2.velocidad = velOriginal2;
        jugador2.fuerzaSalto = saltoOriginal2;
        jugador2.gravedad = gravedadOriginal2;
        jugador2.alturaSuelo = alturaOriginal2;
        jugador2.usarLimitesPersonalizados = limitesOriginal2;
        jugador2.limiteIzquierdo = limiteIzqOriginal2;
        jugador2.limiteDerecho = limiteDerOriginal2;
        jugador2.usarEntradaRemota = remotoOriginal2;
    }
}
