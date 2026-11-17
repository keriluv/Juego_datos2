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
    public Sprite spriteAndy;
    public Sprite spriteFantasma;
    public Sprite spriteGlobosCumpleanos;
    public Sprite spriteGloboRojo;
    public Sprite spriteGloboVerde;
    public Sprite spriteGloboMorado;
    public Sprite spritePastel;

    [Header("Zoom")]
    public float escalaZoom = 1.7f;
    public float escalaJugadorHistoria = 0.52f;
    public float duracionZoom = 1.05f;
    public float pausaEntreHistorias = 0.3f;
    public float duracionAviso = 0.25f;

    private RectTransform raizDesafio;
    private DesafioSnap desafioActual;
    private Image overlayHalloween;
    private RawImage fondoHistoria3;
    private int historiaActual = -1;
    private bool ocupado;
    private bool terminado;
    private bool botonesBloqueados;
    private Vector2 tamanoJugadorBase = new Vector2(72f, 96f);

    private RedSnapchat red;
    private readonly JugadorTutorial[] jugadoresExtra = new JugadorTutorial[RedSnapchat.MaxJugadoresSnap - 1];
    private readonly RectTransform[] jugadoresExtraRT = new RectTransform[RedSnapchat.MaxJugadoresSnap - 1];
    private bool online;
    private bool esAnfitrion;
    private int ultimaHistoriaRemota = -1;
    private bool finalMostrado;

    private void Awake()
    {
        if (spriteFantasma == null)
        {
#if UNITY_EDITOR
            spriteFantasma = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/LevelSnapchat/Sprites/fantasma.png");
#endif
            if (spriteFantasma == null)
                Debug.LogError("GestorDesafiosSnap no tiene asignado el sprite Assets/LevelSnapchat/Sprites/fantasma.png.", this);
        }

        if (spriteGlobosCumpleanos == null || spriteGloboRojo == null ||
            spriteGloboVerde == null || spriteGloboMorado == null || spritePastel == null)
        {
#if UNITY_EDITOR
            spriteGlobosCumpleanos = spriteGlobosCumpleanos != null ? spriteGlobosCumpleanos :
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LevelSnapchat/Sprites/globos_sin_fondo.png");
            spriteGloboRojo = spriteGloboRojo != null ? spriteGloboRojo :
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LevelSnapchat/Sprites/globo_rojo_sin_fondo.png");
            spriteGloboVerde = spriteGloboVerde != null ? spriteGloboVerde :
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LevelSnapchat/Sprites/globo_verde_sin_fondo.png");
            spriteGloboMorado = spriteGloboMorado != null ? spriteGloboMorado :
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LevelSnapchat/Sprites/globo_morado_sin_fondo.png");
            spritePastel = spritePastel != null ? spritePastel :
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LevelSnapchat/Sprites/pastel.png");
#endif
            if (spriteGlobosCumpleanos == null || spriteGloboRojo == null ||
                spriteGloboVerde == null || spriteGloboMorado == null || spritePastel == null)
                Debug.LogError("GestorDesafiosSnap necesita los sprites de globos y pastel de la historia HBD.", this);
        }

        if (jugador == null)
            jugador = FindAnyObjectByType<JugadorTutorial>(FindObjectsInactive.Include);

        if (snap == null)
            snap = FindAnyObjectByType<SnapWebApp>(FindObjectsInactive.Include);

        if (vineta == null)
            vineta = FindAnyObjectByType<VinetaHalloween>(FindObjectsInactive.Include);

        if (ventana == null && snap != null)
            ventana = snap.transform.parent as RectTransform;

        if (mundo == null && ventana != null)
            mundo = ventana.Find("MundoSnap") as RectTransform;

        if (panelPrincipal == null && ventana != null)
            panelPrincipal = ventana.Find("Main") as RectTransform;

        if (pausaEntreHistorias <= 0f) pausaEntreHistorias = 0.3f;
        if (duracionAviso <= 0f) duracionAviso = 0.25f;

        if (jugador != null)
        {
            RectTransform rtJugador = (RectTransform)jugador.transform;
            if (rtJugador.rect.width > 1f)
                tamanoJugadorBase = rtJugador.rect.size;
        }

        if (snap != null)
            snap.gestorDesafios = this;
    }

    private void Start()
    {
        AplicarColoresTextoHistorias();
        Canvas.ForceUpdateCanvases();
        CrearFondoHistoria3();
    }

    private void AplicarColoresTextoHistorias()
    {
        if (historias == null) return;

        foreach (RectTransform historia in historias)
        {
            if (historia == null) continue;

            TextMeshProUGUI[] textos = historia.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI texto in textos)
            {
                bool esTextoBadge = texto.transform.parent != null &&
                    texto.transform.parent.name == "Badge";
                texto.color = esTextoBadge ? Color.black : Color.white;
            }
        }
    }

    private void CrearFondoHistoria3()
    {
        if (ventana == null || ventana.parent == null || historias == null ||
            historias.Length < 3 || historias[2] == null)
        {
            Debug.LogError("GestorDesafiosSnap no puede crear el fondo de la historia 3.", this);
            return;
        }

        RectTransform contenedor = ventana.parent as RectTransform;
        if (contenedor == null)
        {
            Debug.LogError("GestorDesafiosSnap necesita un Canvas válido para el fondo de la historia 3.", this);
            return;
        }

        GameObject fondo = new GameObject("FondoHistoria3",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        fondo.layer = ventana.gameObject.layer;

        RectTransform rt = fondo.GetComponent<RectTransform>();
        rt.SetParent(contenedor, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        fondoHistoria3 = fondo.GetComponent<RawImage>();
        fondoHistoria3.texture = Texture2D.whiteTexture;
        fondoHistoria3.color = Color.black;
        fondoHistoria3.raycastTarget = false;
        rt.SetSiblingIndex(ventana.GetSiblingIndex());
        fondo.SetActive(false);
    }

    private void Update()
    {
        if (red == null)
        {
            if (!RedSnapchat.HayRed) return;

            red = RedSnapchat.Autoridad;
            if (red == null) return;

            ConfigurarRed();
        }

        if (online)
        {
            if (desafioActual == null)
                AsegurarCopias();

            if (esAnfitrion)
                red.RegistrarJugadores(JugadoresActuales());
            else
                ProcesarHistoriaRemota();
        }

        if (desafioActual == null)
            SincronizarCopiasVisual();
    }

    private void ConfigurarRed()
    {
        online = true;
        esAnfitrion = RedSnapchat.EsAnfitrion;

        AsegurarCopias();
        red.RegistrarJugadores(JugadoresActuales());

        if (esAnfitrion)
            red.FijarHistoria(-1);
    }

    private void AsegurarCopias()
    {
        if (jugador == null) return;

        int cantidad = RedSnapchat.HayRed ? RedSnapchat.CantidadJugadores : 1;
        cantidad = Mathf.Clamp(cantidad, 1, RedSnapchat.MaxJugadoresSnap);

        for (int slot = 1; slot < RedSnapchat.MaxJugadoresSnap; slot++)
        {
            int indice = slot - 1;
            bool debeExistir = slot < cantidad;

            if (debeExistir && jugadoresExtra[indice] == null)
                CrearJugadorExtra(slot);

            if (jugadoresExtra[indice] != null &&
                jugadoresExtra[indice].gameObject.activeSelf != debeExistir)
                jugadoresExtra[indice].gameObject.SetActive(debeExistir);
        }
    }

    private void CrearJugadorExtra(int slot)
    {
        if (jugador == null || slot <= 0 || slot >= RedSnapchat.MaxJugadoresSnap)
            return;

        int indice = slot - 1;
        if (jugadoresExtra[indice] != null) return;

        GameObject copia = Instantiate(jugador.gameObject, jugador.transform.parent);
        copia.name = "JugadorSnap" + (slot + 1);

        JugadorTutorial extra = copia.GetComponent<JugadorTutorial>();
        RectTransform rt = (RectTransform)copia.transform;

        jugadoresExtra[indice] = extra;
        jugadoresExtraRT[indice] = rt;

        if (extra != null)
        {
            extra.enabled = false;
            extra.usarEntradaRemota = true;

            if (extra.sprite != null)
                extra.sprite.color = ColorJugador(slot);
        }

        rt.anchoredPosition = ((RectTransform)jugador.transform).anchoredPosition
            + OffsetJugador(slot);
    }

    private void SincronizarCopiasVisual()
    {
        if (jugador == null) return;

        RectTransform rt = (RectTransform)jugador.transform;
        bool activo = jugador.gameObject.activeSelf;
        int cantidad = RedSnapchat.HayRed ? RedSnapchat.CantidadJugadores : 1;

        for (int slot = 1; slot < RedSnapchat.MaxJugadoresSnap; slot++)
        {
            JugadorTutorial extra = JugadorSlot(slot);
            RectTransform extraRT = RTJugadorSlot(slot);

            if (extra == null || extraRT == null) continue;

            bool debeEstarActivo = activo && slot < cantidad;

            if (extra.gameObject.activeSelf != debeEstarActivo)
                extra.gameObject.SetActive(debeEstarActivo);

            if (!debeEstarActivo) continue;

            extraRT.sizeDelta = rt.sizeDelta;
            extraRT.anchoredPosition = rt.anchoredPosition + OffsetJugador(slot);
        }
    }

    private JugadorTutorial JugadorSlot(int slot)
    {
        if (slot == 0) return jugador;
        if (slot <= 0 || slot >= RedSnapchat.MaxJugadoresSnap) return null;

        return jugadoresExtra[slot - 1];
    }

    private RectTransform RTJugadorSlot(int slot)
    {
        JugadorTutorial j = JugadorSlot(slot);
        return j != null ? (RectTransform)j.transform : null;
    }

    private JugadorTutorial[] JugadoresActuales()
    {
        JugadorTutorial[] lista = new JugadorTutorial[RedSnapchat.MaxJugadoresSnap];
        lista[0] = jugador;

        for (int slot = 1; slot < RedSnapchat.MaxJugadoresSnap; slot++)
        {
            JugadorTutorial extra = JugadorSlot(slot);

            if (extra != null && extra.gameObject.activeSelf)
                lista[slot] = extra;
        }

        return lista;
    }

    private RectTransform[] RTsJugadoresActuales()
    {
        JugadorTutorial[] jugadores = JugadoresActuales();
        RectTransform[] rts = new RectTransform[jugadores.Length];

        for (int i = 0; i < jugadores.Length; i++)
            rts[i] = jugadores[i] != null ? (RectTransform)jugadores[i].transform : null;

        return rts;
    }

    private static Color ColorJugador(int slot)
    {
        if (slot == 1) return new Color(0.42f, 0.74f, 1f, 1f);
        if (slot == 2) return new Color(0.42f, 1f, 0.58f, 1f);
        return new Color(1f, 0.72f, 0.35f, 1f);
    }

    private static Vector2 OffsetJugador(int slot)
    {
        if (slot == 1) return new Vector2(84f, 0f);
        if (slot == 2) return new Vector2(-84f, 0f);
        return new Vector2(168f, 0f);
    }

    private void ProcesarHistoriaRemota()
    {
        if (ocupado) return;

        int historia = red.HistoriaActual;
        if (historia == ultimaHistoriaRemota) return;

        ultimaHistoriaRemota = historia;

        if (historia == -2)
        {
            if (!finalMostrado)
                StartCoroutine(RutinaFinalRemota());

            return;
        }

        if (historia < 0) return;

        StartCoroutine(RutinaEntrarRemota(historia));
    }

    private IEnumerator RutinaEntrarRemota(int indice)
    {
        ocupado = true;

        if (historiaActual >= 0)
        {
            DestruirDesafio();

            if (historiaActual == 1)
            {
                if (vineta != null)
                    vineta.Salir();

                if (overlayHalloween != null)
                    yield return FadeOverlay(overlayHalloween, 0f, 0.6f);
            }

            yield return ZoomSalir(duracionZoom);
        }

        historiaActual = indice;

        if (!botonesBloqueados)
            BloquearBotonesHistoria(true);

        if (indice == 1)
            PrepararHalloween();

        ColocarJugadorEnHistoria(indice);

        if (indice == 0 && jugador != null)
            jugador.gameObject.SetActive(false);

        yield return ZoomAHistoria(historias[indice], escalaZoom, duracionZoom);

        Canvas.ForceUpdateCanvases();

        if (indice == 1 && overlayHalloween != null)
            StartCoroutine(FadeOverlay(overlayHalloween, 0.42f, 0.8f));

        yield return new WaitForSecondsRealtime(pausaEntreHistorias);

        CrearDesafio(indice);
        ocupado = false;
    }

    private IEnumerator RutinaFinalRemota()
    {
        ocupado = true;
        finalMostrado = true;

        DestruirDesafio();

        if (historiaActual == 1)
        {
            if (vineta != null)
                vineta.Salir();

            if (overlayHalloween != null)
                yield return FadeOverlay(overlayHalloween, 0f, 0.6f);
        }

        yield return ZoomSalir(duracionZoom);

        MostrarFinal();
        terminado = true;
        ocupado = false;
    }

    public void IniciarEnHistoria(int indice)
    {
        if (terminado || ocupado) return;
        if (RedSnapchat.HayRed && !RedSnapchat.EsAnfitrion) return;
        if (indice < 0 || indice >= historias.Length || historias[indice] == null) return;

        if (red == null && RedSnapchat.HayRed)
        {
            red = RedSnapchat.Autoridad;

            if (red != null && !online)
                ConfigurarRed();
        }

        if (online && esAnfitrion && red != null)
            red.FijarHistoria(indice);

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

        if (indice == 0 && jugador != null)
            jugador.gameObject.SetActive(false);

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

        if (online && esAnfitrion && red != null)
            red.FijarHistoria(indice + 1 < historias.Length ? indice + 1 : -2);

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

        if (indice == 0)
        {
            DesafioSnapPerfecto snapPerfecto = go.AddComponent<DesafioSnapPerfecto>();
            snapPerfecto.spriteObjetivo = spriteAndy;
            desafio = snapPerfecto;
        }
        else
        {
            DesafioEscape escape = go.AddComponent<DesafioEscape>();
            escape.nivel = indice;
            if (indice == 1)
                escape.spriteFantasma = spriteFantasma;
            else if (indice == 2)
            {
                escape.spriteGlobosCumpleanos = spriteGlobosCumpleanos;
                escape.spriteGloboRojo = spriteGloboRojo;
                escape.spriteGloboVerde = spriteGloboVerde;
                escape.spriteGloboMorado = spriteGloboMorado;
                escape.spritePastel = spritePastel;
            }
            desafio = escape;
        }

        desafio.Completado = () => StartCoroutine(CompletarDesafio(indice));

        JugadorTutorial[] jugadoresContexto = JugadoresActuales();
        RectTransform[] jugadoresRTContexto = new RectTransform[jugadoresContexto.Length];

        for (int i = 0; i < jugadoresContexto.Length; i++)
            jugadoresRTContexto[i] = jugadoresContexto[i] != null
                ? (RectTransform)jugadoresContexto[i].transform
                : null;

        ContextoDesafio contexto = new ContextoDesafio
        {
            snap = snap,
            raiz = rt,
            historia = historia,
            mundo = mundo,
            jugador = jugador,
            jugadorRT = jugador != null ? (RectTransform)jugador.transform : null,
            jugador2 = JugadorSlot(1),
            jugador2RT = RTJugadorSlot(1),
            jugadores = jugadoresContexto,
            jugadoresRT = jugadoresRTContexto,
            vineta = vineta,
            online = online,
            esAnfitrion = esAnfitrion,
            red = red
        };

        raizDesafio = rt;
        desafioActual = desafio;
        desafio.Iniciar(contexto);

        Debug.Log("SNAP_DESAFIO creado para la historia " + indice
            + " | jugador=" + (jugador != null)
            + " snap=" + (snap != null)
            + " mundo=" + (mundo != null));
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

        if (fondoHistoria3 != null)
            fondoHistoria3.gameObject.SetActive(historias != null &&
                historias.Length > 2 && historia == historias[2]);

        ventana.localScale = Vector3.one;
        ventana.anchoredPosition = Vector2.zero;
        Canvas.ForceUpdateCanvases();

        Vector2 centro = ventana.InverseTransformPoint(historia.TransformPoint(historia.rect.center));
        Vector2 objetivo = ventana.InverseTransformPoint(panelPrincipal.TransformPoint(panelPrincipal.rect.center));

        Vector3 escalaInicial = ventana.localScale;
        Vector2 posicionInicial = ventana.anchoredPosition;
        Vector3 escalaFinal = new Vector3(escala, escala, 1f);
        Vector2 posicionFinal = objetivo - centro * escala;

        RectTransform[] jugadoresRTZoom = RTsJugadoresActuales();
        Vector2[] tamanosIniciales = new Vector2[jugadoresRTZoom.Length];

        for (int i = 0; i < jugadoresRTZoom.Length; i++)
            tamanosIniciales[i] = jugadoresRTZoom[i] != null
                ? jugadoresRTZoom[i].rect.size
                : tamanoJugadorBase;

        Vector2 tamanoJugadorFinal = tamanoJugadorBase * escalaJugadorHistoria;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float suave = k * k * (3f - 2f * k);

            ventana.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, suave);
            ventana.anchoredPosition = Vector2.LerpUnclamped(posicionInicial, posicionFinal, suave);

            for (int i = 0; i < jugadoresRTZoom.Length; i++)
            {
                if (jugadoresRTZoom[i] != null)
                    jugadoresRTZoom[i].sizeDelta = Vector2.LerpUnclamped(
                        tamanosIniciales[i], tamanoJugadorFinal, suave);
            }

            yield return null;
        }

        ventana.localScale = escalaFinal;
        ventana.anchoredPosition = posicionFinal;

        for (int i = 0; i < jugadoresRTZoom.Length; i++)
        {
            if (jugadoresRTZoom[i] != null)
                jugadoresRTZoom[i].sizeDelta = tamanoJugadorFinal;
        }
    }

    private IEnumerator ZoomSalir(float duracion)
    {
        if (ventana == null)
            yield break;

        Vector3 escalaInicial = ventana.localScale;
        Vector2 posicionInicial = ventana.anchoredPosition;
        Vector3 escalaFinal = Vector3.one;
        Vector2 posicionFinal = Vector2.zero;

        RectTransform[] jugadoresRTZoom = RTsJugadoresActuales();
        Vector2[] tamanosIniciales = new Vector2[jugadoresRTZoom.Length];

        for (int i = 0; i < jugadoresRTZoom.Length; i++)
            tamanosIniciales[i] = jugadoresRTZoom[i] != null
                ? jugadoresRTZoom[i].rect.size
                : tamanoJugadorBase;

        float t = 0f;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float suave = k * k * (3f - 2f * k);

            ventana.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, suave);
            ventana.anchoredPosition = Vector2.LerpUnclamped(posicionInicial, posicionFinal, suave);

            for (int i = 0; i < jugadoresRTZoom.Length; i++)
            {
                if (jugadoresRTZoom[i] != null)
                    jugadoresRTZoom[i].sizeDelta = Vector2.LerpUnclamped(
                        tamanosIniciales[i], tamanoJugadorBase, suave);
            }

            yield return null;
        }

        ventana.localScale = Vector3.one;
        ventana.anchoredPosition = Vector2.zero;

        for (int i = 0; i < jugadoresRTZoom.Length; i++)
        {
            if (jugadoresRTZoom[i] != null)
                jugadoresRTZoom[i].sizeDelta = tamanoJugadorBase;
        }

        if (fondoHistoria3 != null)
            fondoHistoria3.gameObject.SetActive(false);
    }

    private void ColocarJugadorEnHistoria(int indice)
    {
        if (jugador == null || mundo == null) return;

        RectTransform historia = historias[indice];
        if (historia == null) return;

        if (!jugador.gameObject.activeSelf)
            jugador.gameObject.SetActive(true);

        if (jugador.enabled)
            jugador.enabled = false;

        RectTransform rt = (RectTransform)jugador.transform;

        Vector3 punto = historia.TransformPoint(new Vector3(
            historia.rect.width * 0.5f,
            -historia.rect.height * 0.78f,
            0f));

        Vector2 local = mundo.InverseTransformPoint(punto);
        rt.anchoredPosition = new Vector2(local.x - rt.rect.width * 0.5f, local.y);

        int cantidad = RedSnapchat.HayRed ? RedSnapchat.CantidadJugadores : 1;

        for (int slot = 1; slot < RedSnapchat.MaxJugadoresSnap; slot++)
        {
            JugadorTutorial extra = JugadorSlot(slot);
            RectTransform extraRT = RTJugadorSlot(slot);

            if (extra == null || extraRT == null) continue;

            bool debeEstarActivo = slot < cantidad;

            if (extra.gameObject.activeSelf != debeEstarActivo)
                extra.gameObject.SetActive(debeEstarActivo);

            if (!debeEstarActivo) continue;

            extra.enabled = false;
            extraRT.sizeDelta = rt.sizeDelta;
            extraRT.anchoredPosition = rt.anchoredPosition + OffsetJugador(slot);
        }
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

        CambiarTexto(historia, "Etiqueta", "HALLOWEEN", Color.white);
        CambiarTexto(historia, "Subtitulo", "Noche de fantasmas", Color.white);
        CambiarTexto(historia, "Badge/Texto", "ESPECIAL", Color.black);

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

        TextMeshProUGUI estado = UtilidadesDesafio.CrearTexto("Estado", tarjeta.transform, fuente,
            "NIVEL COMPLETADO", 22f, new Color(0.35f, 1f, 0.55f, 1f));
        UtilidadesDesafio.Colocar(estado.rectTransform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, -92f), new Vector2(420f, 44f));
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
