using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioFantasma : DesafioSnap
{
    private const int VidasIniciales = 3;
    private const int TotalSnaps = 3;
    private const float DuracionFantasma = 3.2f;
    private const float RecargaPorSegundo = 0.4f;
    private const float RetrasoRecarga = 0.55f;
    private const float AnchoRayo = 66f;
    private const float SueloDesdeAbajo = 0.285f;

    private class Espectro
    {
        public RectTransform rt;
        public float baseX;
        public float amplitud;
        public float velocidad;
        public float fase;
    }

    private readonly Espectro[] espectros = new Espectro[3];
    private readonly Image[] snaps = new Image[TotalSnaps];
    private readonly bool[] recogidos = new bool[TotalSnaps];

    private RectTransform capa;
    private RectTransform salida;
    private Image marcoSalida;
    private TextMeshProUGUI textoSalida;
    private Image destello;
    private Image barraEnergia;
    private Image[] corazones;
    private TextMeshProUGUI textoEstado;
    private TextMeshProUGUI textoEnergia;
    private TextMeshProUGUI textoSnaps;

    private float ySuelo;
    private float yEspectro;
    private float sueloMundo;
    private float inicioX;
    private float energia;
    private float retraso;
    private float invulnerable;
    private float tiempoEstado;
    private int vidas;
    private int snapsRecogidos;
    private bool activo;
    private bool modoFantasma;
    private bool salidaAbierta;
    private bool reiniciando;

    private static readonly Color Violeta = new Color(0.62f, 0.35f, 0.95f, 1f);
    private static readonly Color Azul = new Color(0.25f, 0.62f, 0.95f, 1f);
    private static readonly Color Verde = new Color(0.2f, 0.85f, 0.45f, 1f);
    private static readonly Color Rojo = new Color(0.95f, 0.25f, 0.28f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);
    private static readonly Color SnapColor = new Color(1f, 0.84f, 0.2f, 1f);

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
        ySuelo = -altoCarta * 0.5f + altoCarta * SueloDesdeAbajo;
        yEspectro = ySuelo + 300f;

        Image velo = UtilidadesDesafio.CrearPanel("Velo", raiz, null,
            new Color(0.03f, 0.01f, 0.08f, 0.72f), false);
        UtilidadesDesafio.Estirar(velo.rectTransform);

        CrearMapa();

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "MODO FANTASMA", 27f, new Color(0.82f, 0.66f, 1f, 1f));
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(430f, 36f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Recoge los snaps y sal sin que te vean", 13f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -231f), new Vector2(430f, 22f));

        CrearCorazones();

        textoSnaps = UtilidadesDesafio.CrearTexto("Snaps", raiz, Fuente,
            "SNAPS 0/" + TotalSnaps, 15f, SnapColor, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoSnaps.rectTransform, new Vector2(1f, 1f), new Vector2(-104f, -262f), new Vector2(180f, 24f));

        textoEnergia = UtilidadesDesafio.CrearTexto("Energia", raiz, Fuente,
            "CLICK: FANTASMA", 12f, Gris, TextAlignmentOptions.Right);
        UtilidadesDesafio.Colocar(textoEnergia.rectTransform, new Vector2(1f, 1f), new Vector2(-104f, -286f), new Vector2(180f, 20f));

        Image fondoBarra = UtilidadesDesafio.CrearPanel("FondoEnergia", raiz, SpriteRedondeado,
            new Color(0.08f, 0.07f, 0.15f, 0.95f));
        UtilidadesDesafio.Colocar(fondoBarra.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(220f, 14f));

        barraEnergia = UtilidadesDesafio.CrearPanel("EnergiaRelleno", fondoBarra.transform, SpriteRedondeado, Violeta);
        UtilidadesDesafio.Estirar(barraEnergia.rectTransform, 3f);
        barraEnergia.type = Image.Type.Filled;
        barraEnergia.fillMethod = Image.FillMethod.Horizontal;
        barraEnergia.fillOrigin = 0;
        barraEnergia.fillAmount = 1f;

        Image suelo = UtilidadesDesafio.CrearPanel("Suelo", raiz, null,
            new Color(0.06f, 0.04f, 0.12f, 0.95f), false);
        UtilidadesDesafio.Colocar(suelo.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, ySuelo - 15f), new Vector2(452f, 30f));

        capa = UtilidadesDesafio.CrearRect("Capa", raiz);
        UtilidadesDesafio.Estirar(capa);

        CrearEspectros();
        CrearSnaps();
        CrearSalida();

        textoEstado = UtilidadesDesafio.CrearTexto("Estado", raiz, Fuente, "", 22f, Color.white);
        UtilidadesDesafio.Colocar(textoEstado.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 168f), new Vector2(420f, 30f));

        TextMeshProUGUI ayuda = UtilidadesDesafio.CrearTexto("Ayuda", raiz, Fuente,
            "A/D mover   Espacio saltar   Click fantasma", 11f, Gris);
        UtilidadesDesafio.Colocar(ayuda.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, ySuelo - 62f), new Vector2(430f, 20f));

        destello = UtilidadesDesafio.CrearPanel("Destello", raiz, null,
            new Color(0.95f, 0.15f, 0.2f, 0f), false);
        UtilidadesDesafio.Estirar(destello.rectTransform);
        destello.rectTransform.SetAsLastSibling();

        PrepararJugador();

        vidas = VidasIniciales;
        ActualizarCorazones();

        energia = 1f;
        retraso = 0f;
        invulnerable = 1.6f;
        snapsRecogidos = 0;
        modoFantasma = false;
        salidaAbierta = false;
        activo = true;
    }

    private void CrearMapa()
    {
        Color linea = new Color(0.5f, 0.4f, 0.9f, 0.07f);

        for (int i = 0; i < 3; i++)
        {
            Image horizontal = UtilidadesDesafio.CrearPanel("MapaH_" + i, raiz, null, linea, false);
            UtilidadesDesafio.Colocar(horizontal.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -160f + i * 130f), new Vector2(452f, 2f));

            Image vertical = UtilidadesDesafio.CrearPanel("MapaV_" + i, raiz, null, linea, false);
            UtilidadesDesafio.Colocar(vertical.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(-140f + i * 140f, 0f), new Vector2(2f, 956f));
        }
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

    private void CrearEspectros()
    {
        espectros[0] = CrearEspectro(0, -40f, 55f, 1.1f, 0f);
        espectros[1] = CrearEspectro(1, 40f, 100f, 1.45f, 1.9f);
        espectros[2] = CrearEspectro(2, 150f, 45f, 1.8f, 3.4f);
    }

    private Espectro CrearEspectro(int indice, float baseX, float amplitud, float velocidad, float fase)
    {
        RectTransform cont = UtilidadesDesafio.CrearRect("Espectro_" + indice, capa);
        cont.anchorMin = new Vector2(0.5f, 0.5f);
        cont.anchorMax = new Vector2(0.5f, 0.5f);
        cont.pivot = new Vector2(0.5f, 0.5f);
        cont.sizeDelta = Vector2.zero;
        cont.anchoredPosition = new Vector2(baseX, yEspectro);

        Image rayo = UtilidadesDesafio.CrearPanel("Rayo", cont, SpriteRedondeado,
            new Color(0.62f, 0.35f, 0.95f, 0.16f), false);
        RectTransform rayoRT = rayo.rectTransform;
        rayoRT.anchorMin = new Vector2(0.5f, 1f);
        rayoRT.anchorMax = new Vector2(0.5f, 1f);
        rayoRT.pivot = new Vector2(0.5f, 1f);
        rayoRT.anchoredPosition = new Vector2(0f, 40f);
        rayoRT.sizeDelta = new Vector2(AnchoRayo, yEspectro - ySuelo + 80f);

        Image nucleo = UtilidadesDesafio.CrearPanel("Nucleo", cont, SpriteRedondeado,
            new Color(0.72f, 0.48f, 1f, 0.35f), false);
        RectTransform nucleoRT = nucleo.rectTransform;
        nucleoRT.anchorMin = new Vector2(0.5f, 1f);
        nucleoRT.anchorMax = new Vector2(0.5f, 1f);
        nucleoRT.pivot = new Vector2(0.5f, 1f);
        nucleoRT.anchoredPosition = new Vector2(0f, 40f);
        nucleoRT.sizeDelta = new Vector2(16f, yEspectro - ySuelo + 80f);

        Image aura = UtilidadesDesafio.CrearPanel("Aura", cont, SpriteRedondeado,
            new Color(0.62f, 0.35f, 0.95f, 0.22f), false);
        UtilidadesDesafio.Colocar(aura.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(94f, 94f));

        Image cuerpo = UtilidadesDesafio.CrearPanel("Cuerpo", cont, SpriteRedondeado,
            new Color(0.88f, 0.9f, 1f, 1f), false);
        UtilidadesDesafio.Colocar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54f, 62f));

        Image ojoIzq = UtilidadesDesafio.CrearPanel("OjoI", cuerpo.transform, SpriteRedondeado,
            new Color(0.08f, 0.06f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(ojoIzq.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-12f, 7f), new Vector2(9f, 15f));

        Image ojoDer = UtilidadesDesafio.CrearPanel("OjoD", cuerpo.transform, SpriteRedondeado,
            new Color(0.08f, 0.06f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(ojoDer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(12f, 7f), new Vector2(9f, 15f));

        Image boca = UtilidadesDesafio.CrearPanel("Boca", cuerpo.transform, SpriteRedondeado,
            new Color(0.08f, 0.06f, 0.14f, 1f), false);
        UtilidadesDesafio.Colocar(boca.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(18f, 6f));

        return new Espectro
        {
            rt = cont,
            baseX = baseX,
            amplitud = amplitud,
            velocidad = velocidad,
            fase = fase
        };
    }

    private void CrearSnaps()
    {
        Vector2[] posiciones =
        {
            new Vector2(-150f, ySuelo + 52f),
            new Vector2(30f, ySuelo + 168f),
            new Vector2(150f, ySuelo + 52f)
        };

        for (int i = 0; i < TotalSnaps; i++)
        {
            Image snap = UtilidadesDesafio.CrearPanel("Snap_" + i, capa, SpriteRedondeado, SnapColor);
            UtilidadesDesafio.Colocar(snap.rectTransform, new Vector2(0.5f, 0.5f), posiciones[i], new Vector2(36f, 36f));

            Image brillo = UtilidadesDesafio.CrearPanel("Brillo", snap.transform, SpriteRedondeado,
                new Color(1f, 1f, 1f, 0.75f), false);
            UtilidadesDesafio.Colocar(brillo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-6f, 6f), new Vector2(10f, 10f));

            snaps[i] = snap;
        }
    }

    private void CrearSalida()
    {
        salida = UtilidadesDesafio.CrearRect("Salida", capa);
        salida.anchorMin = new Vector2(0.5f, 0.5f);
        salida.anchorMax = new Vector2(0.5f, 0.5f);
        salida.pivot = new Vector2(0.5f, 0.5f);
        salida.sizeDelta = new Vector2(56f, 152f);
        salida.anchoredPosition = new Vector2(182f, ySuelo + 76f);

        marcoSalida = UtilidadesDesafio.CrearPanel("Marco", salida, SpriteRedondeado,
            new Color(0.35f, 0.35f, 0.45f, 1f));
        UtilidadesDesafio.Estirar(marcoSalida.rectTransform);

        Image hueco = UtilidadesDesafio.CrearPanel("Hueco", salida, null,
            new Color(0.03f, 0.03f, 0.07f, 0.95f), false);
        UtilidadesDesafio.Estirar(hueco.rectTransform, 6f);

        textoSalida = UtilidadesDesafio.CrearTexto("Texto", salida, Fuente,
            "BLOQUEADA", 11f, Gris);
        UtilidadesDesafio.Estirar(textoSalida.rectTransform, 4f);
    }

    private void PrepararJugador()
    {
        if (jugador == null || jugadorRT == null || mundo == null || historia == null) return;

        Rect carta = PlataformaUI.RectDe(mundo, historia);
        float ancho = jugadorRT.rect.width;

        sueloMundo = carta.yMin + carta.height * SueloDesdeAbajo;
        inicioX = carta.xMin + 12f;

        jugador.usarLimitesPersonalizados = true;
        jugador.limiteIzquierdo = carta.xMin + 6f;
        jugador.limiteDerecho = Mathf.Max(carta.xMin + 6f, carta.xMax - ancho - 6f);
        jugador.alturaSuelo = sueloMundo;
        jugador.usarClick = false;
        jugador.usarGatillo = false;
        jugador.velocidad = 520f;

        jugador.ColocarEn(new Vector2(inicioX, sueloMundo));
        jugador.enabled = true;
    }

    private void Update()
    {
        if (!activo || reiniciando) return;

        float dt = Time.unscaledDeltaTime;

        MoverEspectros();

        if (invulnerable > 0f)
            invulnerable -= dt;

        ActualizarEnergia(dt);
        ActualizarDestello(dt);

        if (tiempoEstado > 0f)
        {
            tiempoEstado -= dt;
            if (tiempoEstado <= 0f) textoEstado.text = "";
        }

        ComprobarSnaps();
        ComprobarEspectros();

        if (salidaAbierta)
            ComprobarSalida();
    }

    private void MoverEspectros()
    {
        float t = Time.unscaledTime;

        for (int i = 0; i < espectros.Length; i++)
        {
            Espectro e = espectros[i];
            if (e == null || e.rt == null) continue;

            float x = e.baseX + Mathf.Sin(t * e.velocidad + e.fase) * e.amplitud;
            e.rt.anchoredPosition = new Vector2(x, yEspectro);
        }
    }

    private void ActualizarEnergia(float dt)
    {
        if (UtilidadesDesafio.GolpePresionado())
            AlternarFantasma();

        if (modoFantasma)
        {
            energia -= dt / DuracionFantasma;

            if (energia <= 0f)
            {
                energia = 0f;
                SalirFantasma();
            }
        }
        else if (retraso > 0f)
        {
            retraso -= dt;
        }
        else
        {
            energia = Mathf.Min(1f, energia + RecargaPorSegundo * dt);
        }

        barraEnergia.fillAmount = Mathf.Clamp01(energia);
        barraEnergia.color = modoFantasma ? Azul : Violeta;

        textoEnergia.text = modoFantasma ? "INVISIBLE" : "CLICK: FANTASMA";
        textoEnergia.color = modoFantasma ? Azul : Gris;

        if (jugador != null)
        {
            jugador.velocidad = modoFantasma ? 360f : 520f;

            if (jugador.sprite != null)
            {
                Color c = jugador.sprite.color;
                c.a = modoFantasma ? 0.32f : 1f;
                jugador.sprite.color = c;
            }
        }
    }

    private void AlternarFantasma()
    {
        if (modoFantasma)
        {
            SalirFantasma();
            return;
        }

        if (energia <= 0.12f) return;

        modoFantasma = true;
        retraso = 0f;
        UtilidadesDesafio.Sonido("sfx_lente", 0.5f);
    }

    private void SalirFantasma()
    {
        modoFantasma = false;
        retraso = RetrasoRecarga;
        UtilidadesDesafio.Sonido("sfx_fallo", 0.25f);
    }

    private void ActualizarDestello(float dt)
    {
        if (destello.color.a <= 0f) return;

        Color c = destello.color;
        c.a = Mathf.MoveTowards(c.a, 0f, dt * 1.3f);
        destello.color = c;
    }

    private void ComprobarSnaps()
    {
        if (jugadorRT == null) return;

        Rect jugadorRect = RectJugador();

        for (int i = 0; i < TotalSnaps; i++)
        {
            if (recogidos[i] || snaps[i] == null) continue;

            Vector2 pos = snaps[i].rectTransform.anchoredPosition;
            Rect r = new Rect(pos.x - 18f, pos.y - 18f, 36f, 36f);

            if (!jugadorRect.Overlaps(r)) continue;

            recogidos[i] = true;
            snaps[i].gameObject.SetActive(false);
            snapsRecogidos++;

            textoSnaps.text = "SNAPS " + snapsRecogidos + "/" + TotalSnaps;
            textoEstado.text = "SNAP RECOGIDO";
            textoEstado.color = SnapColor;
            tiempoEstado = 0.7f;

            UtilidadesDesafio.Sonido("sfx_snap", 0.6f);
            UtilidadesDesafio.Sacudir(raiz, 4f, 0.12f);

            if (snapsRecogidos >= TotalSnaps)
                AbrirSalida();
        }
    }

    private void AbrirSalida()
    {
        salidaAbierta = true;
        marcoSalida.color = Verde;
        textoSalida.text = "SALIR";
        textoSalida.color = Verde;

        textoEstado.text = "SALIDA ABIERTA";
        textoEstado.color = Verde;
        tiempoEstado = 1.1f;

        UtilidadesDesafio.Sonido("sfx_ronda", 0.7f);
    }

    private void ComprobarEspectros()
    {
        if (invulnerable > 0f || modoFantasma || jugadorRT == null) return;

        Rect jugadorRect = RectJugador();

        for (int i = 0; i < espectros.Length; i++)
        {
            Espectro e = espectros[i];
            if (e == null || e.rt == null) continue;

            Rect rayo = new Rect(
                e.rt.anchoredPosition.x - AnchoRayo * 0.5f,
                ySuelo - 40f,
                AnchoRayo,
                yEspectro - ySuelo + 80f);

            if (!rayo.Overlaps(jugadorRect)) continue;

            Atrapado();
            return;
        }
    }

    private void ComprobarSalida()
    {
        if (salida == null || jugadorRT == null) return;

        Rect jugadorRect = RectJugador();
        Rect r = new Rect(
            salida.anchoredPosition.x - salida.sizeDelta.x * 0.5f,
            salida.anchoredPosition.y - salida.sizeDelta.y * 0.5f,
            salida.sizeDelta.x,
            salida.sizeDelta.y);

        if (!jugadorRect.Overlaps(r)) return;

        Ganar();
    }

    private void Atrapado()
    {
        vidas--;
        ActualizarCorazones();

        invulnerable = 1.7f;
        modoFantasma = false;
        retraso = RetrasoRecarga;
        energia = Mathf.Max(energia, 0.25f);

        destello.color = new Color(0.95f, 0.15f, 0.2f, 0.3f);

        UtilidadesDesafio.Sonido("sfx_fallo");
        UtilidadesDesafio.Sacudir(raiz, 20f, 0.3f);

        if (vidas <= 0)
        {
            StartCoroutine(Reiniciar());
            return;
        }

        textoEstado.text = "TE VIO UN FANTASMA";
        textoEstado.color = Rojo;
        tiempoEstado = 1.3f;

        StartCoroutine(VolverAlInicio(0.45f));
    }

    private IEnumerator VolverAlInicio(float espera)
    {
        yield return new WaitForSecondsRealtime(espera);

        if (activo && jugador != null)
            jugador.ColocarEn(new Vector2(inicioX, sueloMundo));
    }

    private void Ganar()
    {
        activo = false;
        salidaAbierta = false;

        if (jugador != null)
        {
            jugador.enabled = false;
            jugador.velocidad = 520f;

            if (jugador.sprite != null)
            {
                Color c = jugador.sprite.color;
                c.a = 1f;
                jugador.sprite.color = c;
            }
        }

        textoEstado.text = "ESCAPASTE";
        textoEstado.color = Verde;

        UtilidadesDesafio.Sonido("sfx_acierto");
        StartCoroutine(Final());
    }

    private IEnumerator Final()
    {
        yield return new WaitForSecondsRealtime(0.9f);
        Completar();
    }

    private IEnumerator Reiniciar()
    {
        reiniciando = true;
        activo = false;

        textoEstado.text = "TE ATRAPARON";
        textoEstado.color = Rojo;

        yield return new WaitForSecondsRealtime(1.4f);

        vidas = VidasIniciales;
        ActualizarCorazones();

        energia = 1f;
        retraso = 0f;
        invulnerable = 1.6f;
        modoFantasma = false;
        snapsRecogidos = 0;
        salidaAbierta = false;

        for (int i = 0; i < TotalSnaps; i++)
        {
            recogidos[i] = false;
            if (snaps[i] != null) snaps[i].gameObject.SetActive(true);
        }

        textoSnaps.text = "SNAPS 0/" + TotalSnaps;
        marcoSalida.color = new Color(0.35f, 0.35f, 0.45f, 1f);
        textoSalida.text = "BLOQUEADA";
        textoSalida.color = Gris;
        textoEstado.text = "";
        destello.color = new Color(0.95f, 0.15f, 0.2f, 0f);

        if (jugador != null)
            jugador.ColocarEn(new Vector2(inicioX, sueloMundo));

        reiniciando = false;
        activo = true;
    }

    private Rect RectJugador()
    {
        Rect r = jugadorRT.rect;
        Vector3 a = raiz.InverseTransformPoint(jugadorRT.TransformPoint(new Vector3(r.xMin, r.yMin, 0f)));
        Vector3 b = raiz.InverseTransformPoint(jugadorRT.TransformPoint(new Vector3(r.xMax, r.yMax, 0f)));

        return Rect.MinMaxRect(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y),
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y));
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

    private void OnDestroy()
    {
        if (jugador == null) return;

        jugador.enabled = false;
        jugador.velocidad = 520f;
        jugador.usarClick = false;
        jugador.usarGatillo = false;

        if (jugador.sprite != null)
        {
            Color c = jugador.sprite.color;
            c.a = 1f;
            jugador.sprite.color = c;
        }
    }
}
