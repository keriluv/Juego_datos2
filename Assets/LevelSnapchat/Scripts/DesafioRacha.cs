using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioRacha : DesafioSnap
{
    private const int VidasIniciales = 3;
    private const int MetaDias = 5;
    private const float SueloDesdeAbajo = 0.285f;
    private const float AlcanceEnvio = 100f;

    private class Amigo
    {
        public string nombre;
        public RectTransform rt;
        public Image avatar;
        public Image barra;
        public TextMeshProUGUI textoDias;
        public Color colorBase;
        public float x;
        public float tiempo;
        public float tiempoMax;
        public float golpe;
        public int dias;
    }

    private readonly Amigo[] amigos = new Amigo[3];

    private Image destello;
    private Image[] corazones;
    private TextMeshProUGUI textoEstado;
    private TextMeshProUGUI textoTotal;

    private float sueloMundo;
    private float inicioX;
    private float tiempoEstado;
    private int vidas;
    private int ultimo = -1;
    private bool activo;
    private bool reiniciando;

    private static readonly Color Verde = new Color(0.2f, 0.85f, 0.45f, 1f);
    private static readonly Color Naranja = new Color(1f, 0.6f, 0.15f, 1f);
    private static readonly Color Rojo = new Color(0.95f, 0.25f, 0.28f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);
    private static readonly Color Fuego = new Color(1f, 0.48f, 0.1f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.pivot = new Vector2(0.5f, 0.5f);
        raiz.offsetMin = Vector2.zero;
        raiz.offsetMax = Vector2.zero;

        Canvas.ForceUpdateCanvases();

        float altoCarta = raiz.rect.height > 1f ? raiz.rect.height : 956f;
        float ySuelo = -altoCarta * 0.5f + altoCarta * SueloDesdeAbajo;

        Image velo = UtilidadesDesafio.CrearPanel("Velo", raiz, null,
            new Color(0.05f, 0.03f, 0.09f, 0.88f), false);
        UtilidadesDesafio.Estirar(velo.rectTransform);

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "RACHA EN PELIGRO", 26f, Fuego);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(430f, 36f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Manda snaps alternando amigos y sube a " + MetaDias + " dias", 12f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -231f), new Vector2(430f, 22f));

        CrearCorazones();

        textoTotal = UtilidadesDesafio.CrearTexto("Total", raiz, Fuente,
            "RACHA 3/" + (MetaDias * 3), 15f, Naranja, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoTotal.rectTransform, new Vector2(1f, 1f), new Vector2(-110f, -262f), new Vector2(200f, 24f));

        CrearAmigo(0, "ANA", -136f, new Color(0.2f, 0.8f, 0.75f, 1f), 3.3f, 2.2f);
        CrearAmigo(1, "LUIS", 0f, new Color(1f, 0.76f, 0.2f, 1f), 3f, 2.8f);
        CrearAmigo(2, "SOFIA", 136f, new Color(0.95f, 0.35f, 0.6f, 1f), 3.2f, 2.4f);

        Image suelo = UtilidadesDesafio.CrearPanel("Suelo", raiz, null,
            new Color(0.07f, 0.05f, 0.12f, 0.95f), false);
        UtilidadesDesafio.Colocar(suelo.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, ySuelo - 15f), new Vector2(452f, 30f));

        textoEstado = UtilidadesDesafio.CrearTexto("Estado", raiz, Fuente, "", 20f, Color.white);
        UtilidadesDesafio.Colocar(textoEstado.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 165f), new Vector2(420f, 28f));

        TextMeshProUGUI ayuda = UtilidadesDesafio.CrearTexto("Ayuda", raiz, Fuente,
            "A/D moverte   Click enviar snap", 11f, Gris);
        UtilidadesDesafio.Colocar(ayuda.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, ySuelo - 62f), new Vector2(430f, 20f));

        destello = UtilidadesDesafio.CrearPanel("Destello", raiz, null,
            new Color(0.95f, 0.15f, 0.2f, 0f), false);
        UtilidadesDesafio.Estirar(destello.rectTransform);
        destello.rectTransform.SetAsLastSibling();

        PrepararJugador();

        vidas = VidasIniciales;
        ultimo = -1;
        ActualizarCorazones();
        ActualizarTotal();

        activo = true;
    }

    private void CrearCorazones()
    {
        corazones = new Image[VidasIniciales];

        for (int i = 0; i < VidasIniciales; i++)
        {
            Image corazon = UtilidadesDesafio.CrearPanel("Vida_" + i, raiz, SpriteRedondeado, Rojo, false);
            UtilidadesDesafio.Colocar(corazon.rectTransform, new Vector2(0f, 1f),
                new Vector2(44f + i * 28f, -262f), new Vector2(20f, 20f));
            corazones[i] = corazon;
        }
    }

    private void CrearAmigo(int indice, string nombre, float x, Color color, float tiempoMax, float tiempoInicial)
    {
        RectTransform columna = UtilidadesDesafio.CrearRect("Amigo_" + nombre, raiz);
        UtilidadesDesafio.Colocar(columna, new Vector2(0.5f, 0.5f), new Vector2(x, 20f), new Vector2(112f, 210f));

        Image aura = UtilidadesDesafio.CrearPanel("Aura", columna, SpriteRedondeado,
            new Color(color.r, color.g, color.b, 0.16f), false);
        UtilidadesDesafio.Colocar(aura.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(96f, 96f));

        Image avatar = UtilidadesDesafio.CrearPanel("Avatar", columna, SpriteRedondeado, color);
        UtilidadesDesafio.Colocar(avatar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(66f, 66f));

        Image brillo = UtilidadesDesafio.CrearPanel("Brillo", avatar.transform, SpriteRedondeado,
            new Color(1f, 1f, 1f, 0.35f), false);
        UtilidadesDesafio.Colocar(brillo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-14f, 14f), new Vector2(18f, 18f));

        TextMeshProUGUI textoNombre = UtilidadesDesafio.CrearTexto("Nombre", columna, Fuente,
            nombre, 13f, Color.white);
        UtilidadesDesafio.Colocar(textoNombre.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(100f, 20f));

        Image llamaExterior = UtilidadesDesafio.CrearPanel("Llama", columna, SpriteRedondeado, Fuego, false);
        UtilidadesDesafio.Colocar(llamaExterior.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-16f, -44f), new Vector2(24f, 30f));

        Image llamaInterior = UtilidadesDesafio.CrearPanel("BrilloLlama", llamaExterior.transform, SpriteRedondeado,
            new Color(1f, 0.88f, 0.25f, 1f), false);
        UtilidadesDesafio.Colocar(llamaInterior.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(12f, 16f));

        TextMeshProUGUI textoDias = UtilidadesDesafio.CrearTexto("Dias", columna, Fuente,
            "x1", 17f, Color.white, TextAlignmentOptions.Left);
        UtilidadesDesafio.Colocar(textoDias.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(18f, -44f), new Vector2(50f, 24f));

        Image fondoBarra = UtilidadesDesafio.CrearPanel("FondoBarra", columna, SpriteRedondeado,
            new Color(0.07f, 0.06f, 0.11f, 0.95f));
        UtilidadesDesafio.Colocar(fondoBarra.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(92f, 12f));

        Image barra = UtilidadesDesafio.CrearPanel("Barra", fondoBarra.transform, SpriteRedondeado, Verde);
        UtilidadesDesafio.Estirar(barra.rectTransform, 3f);
        barra.type = Image.Type.Filled;
        barra.fillMethod = Image.FillMethod.Horizontal;
        barra.fillOrigin = 0;
        barra.fillAmount = 1f;

        amigos[indice] = new Amigo
        {
            nombre = nombre,
            rt = columna,
            avatar = avatar,
            barra = barra,
            textoDias = textoDias,
            colorBase = color,
            x = x,
            tiempo = tiempoInicial,
            tiempoMax = tiempoMax,
            dias = 1,
            golpe = 0f
        };
    }

    private void PrepararJugador()
    {
        if (jugador == null || jugadorRT == null || mundo == null || historia == null) return;

        Rect carta = PlataformaUI.RectDe(mundo, historia);
        float ancho = jugadorRT.rect.width;

        sueloMundo = carta.yMin + carta.height * SueloDesdeAbajo;
        inicioX = carta.center.x - ancho * 0.5f;

        jugador.usarLimitesPersonalizados = true;
        jugador.limiteIzquierdo = carta.xMin + 6f;
        jugador.limiteDerecho = Mathf.Max(carta.xMin + 6f, carta.xMax - ancho - 6f);
        jugador.alturaSuelo = sueloMundo;
        jugador.usarClick = true;
        jugador.usarGatillo = true;
        jugador.velocidad = 520f;
        jugador.esperaEntreGolpes = 0.45f;

        jugador.ColocarEn(new Vector2(inicioX, sueloMundo));
        jugador.enabled = true;
        jugador.alGolpear += AlGolpear;
    }

    private void Update()
    {
        if (!activo || reiniciando) return;

        float dt = Time.unscaledDeltaTime;

        if (destello.color.a > 0f)
        {
            Color c = destello.color;
            c.a = Mathf.MoveTowards(c.a, 0f, dt * 1.3f);
            destello.color = c;
        }

        if (tiempoEstado > 0f)
        {
            tiempoEstado -= dt;
            if (tiempoEstado <= 0f) textoEstado.text = "";
        }

        float total = TotalDias();
        float progreso = Mathf.Clamp01((total - 3f) / 12f);
        float drenaje = 1f + progreso * 0.8f;

        Vector2 centro = CentroJugador();

        int cercano = -1;
        float mejor = float.MaxValue;

        for (int i = 0; i < amigos.Length; i++)
        {
            Amigo a = amigos[i];
            if (a == null) continue;

            float distancia = Mathf.Abs(a.x - centro.x);
            if (distancia < mejor)
            {
                mejor = distancia;
                cercano = i;
            }
        }

        bool enRango = mejor <= AlcanceEnvio;

        for (int i = 0; i < amigos.Length; i++)
        {
            Amigo a = amigos[i];
            if (a == null) continue;

            if (a.golpe > 0f)
                a.golpe = Mathf.Max(0f, a.golpe - dt * 3f);

            float escala = 1f + a.golpe * 0.12f + (i == cercano && enRango ? 0.04f : 0f);
            a.rt.localScale = new Vector3(escala, escala, 1f);

            a.avatar.color = i == cercano && enRango
                ? Color.Lerp(a.colorBase, Color.white, 0.4f)
                : a.colorBase;

            a.tiempo -= dt * drenaje;

            if (a.tiempo <= 0f)
            {
                PerderRacha(i);
                return;
            }

            float k = Mathf.Clamp01(a.tiempo / Mathf.Max(0.01f, a.tiempoMax));
            a.barra.fillAmount = k;
            a.barra.color = k > 0.55f ? Verde : (k > 0.28f ? Naranja : Rojo);
        }
    }

    private void AlGolpear(PlataformaUI plataforma)
    {
        if (!activo || reiniciando) return;

        Vector2 centro = CentroJugador();

        int cercano = -1;
        float mejor = float.MaxValue;

        for (int i = 0; i < amigos.Length; i++)
        {
            Amigo a = amigos[i];
            if (a == null) continue;

            float distancia = Mathf.Abs(a.x - centro.x);
            if (distancia < mejor)
            {
                mejor = distancia;
                cercano = i;
            }
        }

        if (cercano < 0 || mejor > AlcanceEnvio)
        {
            textoEstado.text = "ACERCATE A UN AMIGO";
            textoEstado.color = Gris;
            tiempoEstado = 0.8f;
            return;
        }

        if (cercano == ultimo)
        {
            textoEstado.text = "OTRO AMIGO PRIMERO";
            textoEstado.color = Rojo;
            tiempoEstado = 0.9f;
            UtilidadesDesafio.Sonido("sfx_fallo", 0.3f);
            return;
        }

        Enviar(cercano);
    }

    private void Enviar(int indice)
    {
        Amigo a = amigos[indice];
        if (a == null) return;

        ultimo = indice;
        a.dias++;
        a.tiempo = a.tiempoMax;
        a.golpe = 1f;
        a.textoDias.text = "x" + a.dias;

        textoEstado.text = "SNAP ENVIADO A " + a.nombre;
        textoEstado.color = Verde;
        tiempoEstado = 0.55f;

        UtilidadesDesafio.Sonido("sfx_snap", 0.65f);
        UtilidadesDesafio.Sacudir(raiz, 4f, 0.12f);

        ActualizarTotal();

        if (TodosCumplen())
            Victoria();
    }

    private void PerderRacha(int indice)
    {
        Amigo a = amigos[indice];
        if (a == null) return;

        vidas--;
        ActualizarCorazones();

        a.dias = 1;
        a.tiempo = a.tiempoMax;
        a.golpe = 0f;
        a.textoDias.text = "x1";

        destello.color = new Color(0.95f, 0.15f, 0.2f, 0.3f);

        textoEstado.text = "RACHA DE " + a.nombre + " ROTA";
        textoEstado.color = Rojo;
        tiempoEstado = 1.2f;

        UtilidadesDesafio.Sonido("sfx_fallo");
        UtilidadesDesafio.Sacudir(raiz, 16f, 0.3f);

        ActualizarTotal();

        if (vidas <= 0)
        {
            StartCoroutine(Reiniciar());
            return;
        }

        ultimo = -1;
    }

    private bool TodosCumplen()
    {
        for (int i = 0; i < amigos.Length; i++)
        {
            if (amigos[i] == null) continue;
            if (amigos[i].dias < MetaDias) return false;
        }

        return true;
    }

    private int TotalDias()
    {
        int total = 0;

        for (int i = 0; i < amigos.Length; i++)
        {
            if (amigos[i] == null) continue;
            total += amigos[i].dias;
        }

        return total;
    }

    private void ActualizarTotal()
    {
        textoTotal.text = "RACHA " + TotalDias() + "/" + (MetaDias * 3);
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

    private void Victoria()
    {
        activo = false;

        if (jugador != null)
            jugador.enabled = false;

        textoEstado.text = "RACHA SALVADA";
        textoEstado.color = Verde;

        UtilidadesDesafio.Sonido("sfx_ronda");
        StartCoroutine(Final());
    }

    private IEnumerator Final()
    {
        yield return new WaitForSecondsRealtime(1f);
        Completar();
    }

    private IEnumerator Reiniciar()
    {
        reiniciando = true;
        activo = false;

        textoEstado.text = "RACHA PERDIDA";
        textoEstado.color = Rojo;

        yield return new WaitForSecondsRealtime(1.5f);

        for (int i = 0; i < amigos.Length; i++)
        {
            Amigo a = amigos[i];
            if (a == null) continue;

            a.dias = 1;
            a.tiempo = a.tiempoMax;
            a.golpe = 0f;
            a.textoDias.text = "x1";
            a.barra.fillAmount = 1f;
            a.rt.localScale = Vector3.one;
        }

        vidas = VidasIniciales;
        ultimo = -1;

        ActualizarCorazones();
        ActualizarTotal();

        textoEstado.text = "";
        destello.color = new Color(0.95f, 0.15f, 0.2f, 0f);

        if (jugador != null)
            jugador.ColocarEn(new Vector2(inicioX, sueloMundo));

        reiniciando = false;
        activo = true;
    }

    private Vector2 CentroJugador()
    {
        if (jugadorRT == null) return Vector2.zero;

        Vector2 centro = jugadorRT.rect.center;
        return raiz.InverseTransformPoint(jugadorRT.TransformPoint(new Vector3(centro.x, centro.y, 0f)));
    }

    private void OnDestroy()
    {
        if (jugador == null) return;

        jugador.alGolpear -= AlGolpear;
        jugador.enabled = false;
        jugador.usarClick = false;
        jugador.usarGatillo = false;
        jugador.esperaEntreGolpes = 0.35f;
        jugador.velocidad = 520f;
    }
}
