using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class JuegoDinoChrome : MonoBehaviour
{
    private const float SueloY = 112f;
    private const float DinoX = 150f;
    private const float VelocidadSalto = 800f;
    private const float VelocidadInicial = 420f;
    private const float VelocidadMaxima = 900f;
    private const string ClaveRecord = "JuegoDinoChrome_Record";
    private const int ColeccionablesNecesarios = 5;
    private const string Instrucciones = "ESPACIO/↑ O TOCA IZQ: SALTAR · ↓/S O MANTÉN ABAJO IZQ: AGACHARSE · CLIC/X/RT O TOCA DER: GOLPEAR · RECOGE 5 MONEDAS";
    private const float AnchoHitboxDino = 105f;
    private const float AnchoHitboxDinoAgachado = 84f;
    private const float AltoHitboxDino = 132f;
    private const float AltoHitboxDinoAgachado = 48f;
    private readonly List<Obstaculo> obstaculos = new List<Obstaculo>();
    [SerializeField] private List<RectTransform> nubes = new List<RectTransform>();
    private readonly List<Coleccionable> coleccionables = new List<Coleccionable>();
    public Sprite spriteColeccionable;
    private Sprite[] spritesAve;
    [SerializeField] private RectTransform dino;
    [SerializeField] private RectTransform suelo;
    [SerializeField] private TextMeshProUGUI puntuacion;
    private TextMeshProUGUI mejorPuntuacion;
    [SerializeField] private TextMeshProUGUI progresoColeccionables;
    [SerializeField] private TextMeshProUGUI aviso;
    [SerializeField] private RectTransform brazoGolpe;
    [SerializeField, HideInInspector] private bool interfazCreada;
    private GameObject panelFinal;
    private float velocidadY;
    private float relojObstaculo = 1.5f;
    private float relojSuelo;
    private float puntos;
    private float relojColeccionable = 2.4f;
    private float tiempoGolpe;
    private float tiempoAviso;
    private float proximoGolpe;
    private int recorde;
    private int coleccionablesRecogidos;
    private bool jugando = true;
    private bool agachado;
    private bool nuevoRecord;
    private bool victoriaEnCurso;
    private Vector2 escalaDino;

    private sealed class Obstaculo
    {
        public RectTransform rect;
        public bool esAve;
    }

    private sealed class Coleccionable
    {
        public RectTransform rect;
    }

    private void Start()
    {
        if (!interfazCreada) CrearInterfaz();
        else
        {
            AplicarEscalaGrande();
            escalaDino = dino.sizeDelta;
        }

        spritesAve = new[] { CrearSpriteAve(true), CrearSpriteAve(false) };
        AsegurarMarcasSuelo();
        recorde = PlayerPrefs.GetInt(ClaveRecord, 0);
        AsegurarEtiquetaRecord();
        ActualizarEtiquetaRecord();
    }

    private void AsegurarEtiquetaRecord()
    {
        if (mejorPuntuacion != null || puntuacion == null) return;

        float escala = interfazCreada ? 2f : 1f;
        Transform padre = puntuacion.transform.parent;
        mejorPuntuacion = Texto("RÉCORD 00000", padre, new Vector2(0f, 1f),
            new Vector2(150f * escala, -76f * escala),
            new Vector2(300f * escala, 52f * escala), 22f * escala,
            new Color(0.32f, 0.34f, 0.35f), FontStyles.Bold);
        mejorPuntuacion.alignment = TextAlignmentOptions.Left;
    }

    private void ActualizarEtiquetaRecord()
    {
        if (mejorPuntuacion != null)
            mejorPuntuacion.text = "RÉCORD  " + recorde.ToString("00000");
    }

    private void AplicarEscalaGrande()
    {
        // The scene may already contain an older, smaller baked Canvas.
        const float escala = 2f;
        dino.sizeDelta = new Vector2(220f, 190f);
        suelo.sizeDelta = new Vector2(suelo.sizeDelta.x, 8f);
        foreach (RectTransform nube in nubes)
            if (nube != null) nube.sizeDelta *= escala;

        RectTransform puntuacionRect = puntuacion.rectTransform;
        puntuacionRect.sizeDelta *= escala;
        puntuacion.fontSize *= escala;
        RectTransform progresoRect = progresoColeccionables.rectTransform;
        progresoRect.sizeDelta *= escala;
        progresoColeccionables.fontSize *= escala;
        RectTransform avisoRect = aviso.rectTransform;
        avisoRect.sizeDelta *= escala;
        aviso.fontSize *= escala;
        brazoGolpe.sizeDelta *= escala;
    }

    public void GenerarVistaEditable()
    {
        if (interfazCreada) return;
        CrearInterfaz();
        interfazCreada = true;
    }

    private void Update()
    {
        if (!jugando)
        {
            if (!victoriaEnCurso && (SaltarPresionado() || GolpePresionado())) Reiniciar();
            return;
        }

        bool saltar = SaltarPresionado();
        agachado = AgacharsePresionado();
        if (saltar && Mathf.Abs(dino.anchoredPosition.y - SueloY) < 0.5f && !agachado) velocidadY = VelocidadSalto;
        velocidadY -= 1800f * Time.deltaTime;
        Vector2 posDino = dino.anchoredPosition;
        posDino.y = Mathf.Max(SueloY, posDino.y + velocidadY * Time.deltaTime);
        if (posDino.y <= SueloY) velocidadY = 0f;
        dino.anchoredPosition = posDino;
        bool tumbado = agachado && posDino.y <= SueloY;
        dino.sizeDelta = tumbado ? new Vector2(190f, 110f) : escalaDino;
        dino.GetComponent<Image>().preserveAspect = !tumbado;

        puntos += Time.deltaTime * 10f;
        int puntosActuales = Mathf.FloorToInt(puntos);
        puntuacion.text = puntosActuales.ToString("00000");
        if (puntosActuales > recorde)
        {
            recorde = puntosActuales;
            nuevoRecord = true;
            PlayerPrefs.SetInt(ClaveRecord, recorde);
            ActualizarEtiquetaRecord();
        }
        relojSuelo += Time.deltaTime * 850f;
        suelo.anchoredPosition = new Vector2(-Mathf.Repeat(relojSuelo, 42f), 112f);
        for (int i = 0; i < nubes.Count; i++)
        {
            RectTransform nube = nubes[i];
            nube.anchoredPosition += Vector2.left * 30f * Time.deltaTime;
            if (nube.anchoredPosition.x < -180f) nube.anchoredPosition = new Vector2(2050f + i * 115f, nube.anchoredPosition.y);
        }

        ActualizarColeccionables(Time.deltaTime);
        if (!jugando) return;
        if (tiempoGolpe > 0f)
        {
            tiempoGolpe -= Time.deltaTime;
            brazoGolpe.gameObject.SetActive(tiempoGolpe > 0f);
            if (tiempoGolpe > 0f)
            {
                float extension = 20f + (1f - tiempoGolpe / 0.16f) * 95f;
                brazoGolpe.anchoredPosition = new Vector2(dino.anchoredPosition.x + dino.sizeDelta.x * 0.5f + extension, dino.anchoredPosition.y + 46f);
            }
        }
        if (tiempoAviso > 0f)
        {
            tiempoAviso -= Time.deltaTime;
            if (tiempoAviso <= 0f) aviso.text = "";
        }

        relojObstaculo -= Time.deltaTime;
        if (relojObstaculo <= 0f) CrearObstaculo();
        for (int i = obstaculos.Count - 1; i >= 0; i--)
        {
            Obstaculo obstaculo = obstaculos[i];
            RectTransform o = obstaculo.rect;
            o.anchoredPosition += Vector2.left * ObtenerVelocidad() * Time.deltaTime;
            if (o.anchoredPosition.x < -1050f) { Destroy(o.gameObject); obstaculos.RemoveAt(i); continue; }
            if (obstaculo.esAve)
            {
                Image imagenAve = o.GetComponent<Image>();
                if (imagenAve != null && spritesAve.Length > 0)
                    imagenAve.sprite = spritesAve[(Mathf.FloorToInt(Time.time * 8f) + i) % spritesAve.Length];
            }
            float obstaculoAbajo = obstaculo.esAve ? o.anchoredPosition.y - 28f : o.anchoredPosition.y;
            float obstaculoArriba = obstaculo.esAve ? o.anchoredPosition.y + 28f : o.anchoredPosition.y + o.sizeDelta.y;
            float anchoObstaculo = obstaculo.esAve ? 82f : 68f;
            float anchoDino = tumbado ? AnchoHitboxDinoAgachado : AnchoHitboxDino;
            float altoDino = tumbado ? AltoHitboxDinoAgachado : AltoHitboxDino;
            bool solapaX = Mathf.Abs(o.anchoredPosition.x - dino.anchoredPosition.x) < (anchoObstaculo + anchoDino) * 0.5f;
            bool solapaY = dino.anchoredPosition.y < obstaculoArriba &&
                dino.anchoredPosition.y + altoDino > obstaculoAbajo;
            if (jugando && solapaX && solapaY) Terminar();
        }

        if (jugando && GolpePresionado()) GolpearDelante();
    }

    private void CrearInterfaz()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject c = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = c.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = c.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        }
        RectTransform root = canvas.GetComponent<RectTransform>();
        Color tinta = new Color(0.32f, 0.34f, 0.35f);
        CrearImagen("Fondo", root, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1080), Color.white);
        puntuacion = Texto("00000", root, new Vector2(1f, 1f), new Vector2(-120f, -76f), new Vector2(240, 52), 30, tinta, FontStyles.Bold);
        puntuacion.alignment = TextAlignmentOptions.Right;
        progresoColeccionables = Texto("MONEDAS  0/5", root, new Vector2(1f, 1f), new Vector2(-120f, -122f), new Vector2(260, 38), 18, tinta, FontStyles.Normal);
        progresoColeccionables.alignment = TextAlignmentOptions.Right;
        aviso = Texto(Instrucciones, root, new Vector2(0.5f, 1f), new Vector2(0, -78f), new Vector2(1120, 46), 16, new Color(0.55f, 0.57f, 0.58f), FontStyles.Normal);
        CrearNube(root, 720f, 590f);
        CrearNube(root, 1240f, 720f);
        CrearNube(root, 1770f, 530f);
        suelo = CrearImagen("Suelo", root, new Vector2(0.5f, 0), new Vector2(0, 112), new Vector2(2200, 5), tinta);
        AsegurarMarcasSuelo();
        dino = CrearImagen("Dino Chrome", root, new Vector2(0, 0), new Vector2(DinoX, SueloY), new Vector2(220, 190), tinta);
        dino.pivot = new Vector2(0.5f, 0f);
        Sprite sprite = Resources.Load<Sprite>("Dino_Chrome");
        if (sprite == null) sprite = UnityEditorAssetSprite();
        if (sprite != null) dino.GetComponent<Image>().sprite = sprite;
        dino.GetComponent<Image>().preserveAspect = true; escalaDino = dino.sizeDelta;
        brazoGolpe = CrearImagen("Golpe", root, Vector2.zero, Vector2.zero, new Vector2(50f, 19f), tinta);
        brazoGolpe.gameObject.SetActive(false);
        MostrarAviso(Instrucciones, 5f);
    }

    private void ActualizarColeccionables(float dt)
    {
        relojColeccionable -= dt;
        if (relojColeccionable <= 0f && coleccionablesRecogidos < ColeccionablesNecesarios)
        {
            CrearColeccionable();
            relojColeccionable = Random.Range(2.8f, 4f);
        }

        for (int i = coleccionables.Count - 1; i >= 0; i--)
        {
            Coleccionable coleccionable = coleccionables[i];
            RectTransform rect = coleccionable.rect;
            rect.anchoredPosition += Vector2.left * ObtenerVelocidad() * dt;
            rect.Rotate(0f, 0f, 50f * dt);
            float pulso = 1f + Mathf.Sin(Time.time * 4f + i) * 0.08f;
            rect.localScale = new Vector3(pulso, pulso, 1f);

            if (rect.anchoredPosition.x < -100f)
            {
                Destroy(rect.gameObject);
                coleccionables.RemoveAt(i);
                continue;
            }

            float anchoDino = agachado ? AnchoHitboxDinoAgachado : AnchoHitboxDino;
            float altoDino = agachado ? AltoHitboxDinoAgachado : AltoHitboxDino;
            bool solapaX = Mathf.Abs(rect.anchoredPosition.x - dino.anchoredPosition.x) <
                (anchoDino + rect.sizeDelta.x * 0.55f) * 0.5f;
            bool solapaY = dino.anchoredPosition.y < rect.anchoredPosition.y + rect.sizeDelta.y * 0.4f &&
                dino.anchoredPosition.y + altoDino > rect.anchoredPosition.y - rect.sizeDelta.y * 0.4f;
            if (!solapaX || !solapaY) continue;

            coleccionablesRecogidos++;
            puntos += 50f;
            progresoColeccionables.text = "MONEDAS  " + coleccionablesRecogidos + "/" + ColeccionablesNecesarios;
            MostrarAviso("¡Moneda recogida!", 0.8f);
            Destroy(rect.gameObject);
            coleccionables.RemoveAt(i);

            if (coleccionablesRecogidos >= ColeccionablesNecesarios)
            {
                Ganar();
                return;
            }
        }
    }

    private void CrearColeccionable()
    {
        float y = Random.value > 0.5f ? SueloY + 235f : SueloY + 55f;
        RectTransform rect;

        if (spriteColeccionable != null)
        {
            rect = CrearImagen("Moneda coleccionable", dino.parent, Vector2.zero,
                new Vector2(2050f, y), new Vector2(72f, 72f), Color.white);
            Image imagen = rect.GetComponent<Image>();
            imagen.sprite = spriteColeccionable;
            imagen.preserveAspect = true;
        }
        else
        {
            TextMeshProUGUI estrella = Texto("★", dino.parent, Vector2.zero,
                new Vector2(2050f, y), new Vector2(82f, 82f), 76f,
                new Color(1f, 0.72f, 0.08f), FontStyles.Bold);
            estrella.raycastTarget = false;
            rect = estrella.rectTransform;
        }

        coleccionables.Add(new Coleccionable { rect = rect });
    }

    private bool GolpePresionado()
    {
        if (Time.unscaledTime < proximoGolpe) return false;
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        Mouse raton = Mouse.current;
        Gamepad mando = Gamepad.current;
        if ((teclado != null && teclado.xKey.wasPressedThisFrame) ||
            (raton != null && raton.leftButton.wasPressedThisFrame) ||
            (mando != null && (mando.rightTrigger.wasPressedThisFrame || mando.leftTrigger.wasPressedThisFrame)))
            return true;
#else
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton7))
            return true;
#endif
        Vector2 posicionToque;
        bool inicioToque;
        bool toqueSostenido;
        return ObtenerEstadoToque(out posicionToque, out inicioToque, out toqueSostenido) &&
            inicioToque && posicionToque.x >= Screen.width * 0.5f;
    }

    private void GolpearDelante()
    {
        proximoGolpe = Time.unscaledTime + 0.32f;
        tiempoGolpe = 0.16f;
        brazoGolpe.gameObject.SetActive(true);

        float x = dino.anchoredPosition.x + dino.sizeDelta.x * 0.5f + 14f;
        float yMin = dino.anchoredPosition.y + 8f;
        float yMax = dino.anchoredPosition.y + dino.sizeDelta.y - 8f;
        for (int i = obstaculos.Count - 1; i >= 0; i--)
        {
            Obstaculo objetivo = obstaculos[i];
            RectTransform obstaculo = objetivo.rect;
            if (obstaculo.anchoredPosition.x < x || obstaculo.anchoredPosition.x > x + 155f) continue;
            float abajo = objetivo.esAve
                ? obstaculo.anchoredPosition.y - 28f
                : obstaculo.anchoredPosition.y;
            float arriba = objetivo.esAve
                ? obstaculo.anchoredPosition.y + 28f
                : obstaculo.anchoredPosition.y + obstaculo.sizeDelta.y;
            if (arriba < yMin || abajo > yMax) continue;

            Destroy(obstaculo.gameObject);
            obstaculos.RemoveAt(i);
            puntos += 25f;
            MostrarAviso("¡Buen golpe!", 0.8f);
            break;
        }
    }

    private void MostrarAviso(string texto, float duracion)
    {
        aviso.text = texto;
        tiempoAviso = duracion;
    }

    private float ObtenerVelocidad()
    {
        return Mathf.Min(VelocidadInicial + puntos * 1.2f, VelocidadMaxima);
    }

    private void CrearNube(Transform root, float x, float y)
    {
        RectTransform nube = CrearImagen("Nube", root, Vector2.zero, new Vector2(x, y), new Vector2(132, 20), new Color(0.87f, 0.88f, 0.88f));
        nube.pivot = new Vector2(0.5f, 0.5f);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(-28, 12), new Vector2(34, 20), nube.GetComponent<Image>().color);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(1, 17), new Vector2(40, 28), nube.GetComponent<Image>().color);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(31, 9), new Vector2(32, 18), nube.GetComponent<Image>().color);
        nubes.Add(nube);
    }

    private void AsegurarMarcasSuelo()
    {
        if (suelo == null) return;

        foreach (Transform hijo in suelo)
            if (hijo.name == "Marca del suelo") return;

        for (int i = 0; i < 55; i++)
        {
            float x = i * 43f;
            float y = i % 2 == 0 ? 5f : -4f;
            CrearImagen("Marca del suelo", suelo, new Vector2(0, 0.5f),
                new Vector2(x, y), new Vector2(9f, 3f),
                new Color(0.68f, 0.69f, 0.69f));
            if (i % 3 == 0)
                CrearImagen("Marca del suelo", suelo, new Vector2(0, 0.5f),
                    new Vector2(x + 17f, -6f), new Vector2(5f, 2f),
                    new Color(0.72f, 0.73f, 0.73f));
        }
    }

    private Sprite UnityEditorAssetSprite()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/TutorialGoogle/Sprites/Dino_Chrome.png");
#else
        return null;
#endif
    }

    private bool SaltarPresionado()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame))
            return true;
#else
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            return true;
#endif
        Vector2 posicionToque;
        bool inicioToque;
        bool toqueSostenido;
        return ObtenerEstadoToque(out posicionToque, out inicioToque, out toqueSostenido) &&
            inicioToque && posicionToque.x < Screen.width * 0.5f &&
            posicionToque.y > Screen.height * 0.22f;
    }

    private bool AgacharsePresionado()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado != null && (teclado.downArrowKey.isPressed || teclado.sKey.isPressed))
            return true;
#else
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            return true;
#endif
        Vector2 posicionToque;
        bool inicioToque;
        bool toqueSostenido;
        return ObtenerEstadoToque(out posicionToque, out inicioToque, out toqueSostenido) &&
            toqueSostenido && posicionToque.x < Screen.width * 0.5f &&
            posicionToque.y <= Screen.height * 0.22f;
    }

    private static bool ObtenerEstadoToque(out Vector2 posicion, out bool inicio, out bool sostenido)
    {
        posicion = Vector2.zero;
        inicio = false;
        sostenido = false;
#if ENABLE_INPUT_SYSTEM
        Touchscreen pantalla = Touchscreen.current;
        if (pantalla == null) return false;

        var toque = pantalla.primaryTouch;
        posicion = toque.position.ReadValue();
        inicio = toque.press.wasPressedThisFrame;
        sostenido = toque.press.isPressed;
#else
        if (Input.touchCount == 0) return false;

        Touch toque = Input.GetTouch(0);
        posicion = toque.position;
        inicio = toque.phase == TouchPhase.Began;
        sostenido = toque.phase == TouchPhase.Began ||
            toque.phase == TouchPhase.Moved || toque.phase == TouchPhase.Stationary;
#endif
        return inicio || sostenido;
    }

    private void CrearObstaculo()
    {
        bool ave = Random.value > 0.52f && puntos > 10f;
        Color tinta = new Color(0.32f, 0.34f, 0.35f);
        RectTransform o;
        if (ave)
        {
            bool aveAlta = Random.value > 0.5f;
            float altura = aveAlta ? SueloY + 235f : SueloY + 94f;
            o = CrearImagen(aveAlta ? "Ave alta" : "Ave baja", dino.parent,
                Vector2.zero, new Vector2(2050f, altura), new Vector2(128f, 80f), Color.white);
            Image imagenAve = o.GetComponent<Image>();
            imagenAve.sprite = spritesAve[0];
            imagenAve.preserveAspect = true;
        }
        else
        {
            float alto = Random.Range(78f, 124f);
            o = CrearImagen("Cactus", dino.parent, Vector2.zero,
                new Vector2(2050f, SueloY), new Vector2(32f, alto), tinta);
            o.pivot = new Vector2(0.5f, 0f);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(-22, 8), new Vector2(18, 12), tinta);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(-30, 16), new Vector2(11, 28), tinta);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(22, 20), new Vector2(18, 12), tinta);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(30, 30), new Vector2(11, 29), tinta);
        }
        obstaculos.Add(new Obstaculo { rect = o, esAve = ave });
        relojObstaculo = Random.Range(1.25f, 2.15f);
    }

    private static Sprite CrearSpriteAve(bool alasArriba)
    {
        const int ancho = 128;
        const int alto = 80;
        Color[] pixeles = new Color[ancho * alto];
        Color negro = new Color(0.12f, 0.12f, 0.12f);

        DibujarTriangulo(pixeles, ancho, alto, new Vector2(91, 33), new Vector2(123, 48), new Vector2(116, 22), negro);
        DibujarElipse(pixeles, ancho, alto, new Vector2(72, 32), 32f, 18f, negro);
        if (alasArriba)
            DibujarTriangulo(pixeles, ancho, alto, new Vector2(51, 35), new Vector2(70, 76), new Vector2(91, 38), negro);
        else
            DibujarTriangulo(pixeles, ancho, alto, new Vector2(52, 35), new Vector2(78, 4), new Vector2(98, 36), negro);
        DibujarElipse(pixeles, ancho, alto, new Vector2(45, 49), 15f, 14f, negro);
        DibujarTriangulo(pixeles, ancho, alto, new Vector2(32, 52), new Vector2(10, 46), new Vector2(32, 42), negro);
        DibujarTriangulo(pixeles, ancho, alto, new Vector2(61, 20), new Vector2(67, 8), new Vector2(73, 20), negro);
        DibujarTriangulo(pixeles, ancho, alto, new Vector2(78, 20), new Vector2(84, 9), new Vector2(90, 20), negro);

        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        textura.name = alasArriba ? "Ave alas arriba" : "Ave alas abajo";
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.SetPixels(pixeles);
        textura.Apply();
        return Sprite.Create(textura, new Rect(0f, 0f, ancho, alto), new Vector2(0.5f, 0.5f), 100f);
    }

    private static void DibujarElipse(Color[] pixeles, int ancho, int alto, Vector2 centro, float radioX, float radioY, Color color)
    {
        for (int y = Mathf.Max(0, Mathf.FloorToInt(centro.y - radioY)); y <= Mathf.Min(alto - 1, Mathf.CeilToInt(centro.y + radioY)); y++)
        {
            for (int x = Mathf.Max(0, Mathf.FloorToInt(centro.x - radioX)); x <= Mathf.Min(ancho - 1, Mathf.CeilToInt(centro.x + radioX)); x++)
            {
                float dx = (x - centro.x) / radioX;
                float dy = (y - centro.y) / radioY;
                if (dx * dx + dy * dy <= 1f)
                    pixeles[y * ancho + x] = color;
            }
        }
    }

    private static void DibujarTriangulo(Color[] pixeles, int ancho, int alto, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
        int maxX = Mathf.Min(ancho - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
        int maxY = Mathf.Min(alto - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 punto = new Vector2(x, y);
                float ab = (punto.x - b.x) * (a.y - b.y) - (a.x - b.x) * (punto.y - b.y);
                float bc = (punto.x - c.x) * (b.y - c.y) - (b.x - c.x) * (punto.y - c.y);
                float ca = (punto.x - a.x) * (c.y - a.y) - (c.x - a.x) * (punto.y - a.y);
                if ((ab >= 0f && bc >= 0f && ca >= 0f) || (ab <= 0f && bc <= 0f && ca <= 0f))
                    pixeles[y * ancho + x] = color;
            }
        }
    }

    private void Terminar()
    {
        jugando = false;
        proximoGolpe = 0f;
        PlayerPrefs.Save();
        MostrarPanelFinal("FIN DE PARTIDA", "ESPACIO o toca para volver a jugar");
    }

    private void Ganar()
    {
        if (!jugando) return;
        jugando = false;
        victoriaEnCurso = true;
        proximoGolpe = 0f;
        PlayerPrefs.Save();
        MostrarPanelFinal("¡GANASTE!", "Volviendo al menú principal...");
        StartCoroutine(VolverAlMenu());
    }

    private IEnumerator VolverAlMenu()
    {
        yield return new WaitForSecondsRealtime(2.2f);
        SceneManager.LoadScene("MainMenu");
    }

    private void MostrarPanelFinal(string titulo, string mensajeFinal)
    {
        panelFinal = new GameObject("Fin de partida", typeof(RectTransform));
        RectTransform p = panelFinal.GetComponent<RectTransform>(); p.SetParent(dino.parent, false); p.anchorMin = p.anchorMax = new Vector2(0.5f, 0.5f); p.sizeDelta = new Vector2(760, 230);
        Texto(titulo, p, new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(700, 48), 32, new Color(0.32f, 0.34f, 0.35f), FontStyles.Bold);
        Texto("PUNTAJE: " + Mathf.FloorToInt(puntos).ToString("00000"), p, new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(700, 40), 24, new Color(0.38f, 0.4f, 0.41f), FontStyles.Bold);
        Texto(nuevoRecord ? "¡NUEVO RÉCORD!" : "RÉCORD: " + recorde.ToString("00000"), p, new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(700, 38), 22, new Color(0.38f, 0.4f, 0.41f), FontStyles.Normal);
        Texto(mensajeFinal, p, new Vector2(0.5f, 0.16f), Vector2.zero, new Vector2(720, 36), 18, new Color(0.45f, 0.47f, 0.48f), FontStyles.Normal);
    }

    private void Reiniciar()
    {
        foreach (Obstaculo o in obstaculos) if (o.rect != null) Destroy(o.rect.gameObject);
        obstaculos.Clear(); if (panelFinal != null) Destroy(panelFinal);
        foreach (Coleccionable coleccionable in coleccionables) if (coleccionable.rect != null) Destroy(coleccionable.rect.gameObject);
        coleccionables.Clear();
        puntos = 0; velocidadY = 0; relojObstaculo = 1.5f; dino.anchoredPosition = new Vector2(DinoX, SueloY); dino.sizeDelta = escalaDino; jugando = true;
        relojColeccionable = 2.4f; relojSuelo = 0f; coleccionablesRecogidos = 0; tiempoGolpe = 0f; tiempoAviso = 0f; proximoGolpe = 0f;
        agachado = false; nuevoRecord = false; victoriaEnCurso = false;
        progresoColeccionables.text = "MONEDAS  0/" + ColeccionablesNecesarios; puntuacion.text = "00000"; brazoGolpe.gameObject.SetActive(false);
        MostrarAviso(Instrucciones, 5f);
    }

    private RectTransform CrearImagen(string nombre, Transform padre, Vector2 ancla, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image)); RectTransform r = go.GetComponent<RectTransform>(); r.SetParent(padre, false); r.anchorMin = r.anchorMax = ancla; r.anchoredPosition = pos; r.sizeDelta = size; go.GetComponent<Image>().color = color; return r;
    }

    private TextMeshProUGUI Texto(string valor, Transform padre, Vector2 ancla, Vector2 pos, Vector2 size, float font, Color color, FontStyles estilo)
    {
        GameObject go = new GameObject(valor, typeof(RectTransform), typeof(TextMeshProUGUI)); RectTransform r = go.GetComponent<RectTransform>(); r.SetParent(padre, false); r.anchorMin = r.anchorMax = ancla; r.anchoredPosition = pos; r.sizeDelta = size; TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>(); t.font = TMP_Settings.defaultFontAsset; t.text = valor; t.fontSize = font; t.color = color; t.fontStyle = estilo; t.alignment = TextAlignmentOptions.Center; return t;
    }
}
