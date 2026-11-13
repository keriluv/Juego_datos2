using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class JuegoDinoChrome : MonoBehaviour
{
    private const float SueloY = 112f;
    private static readonly string[] NombresExtensiones = { "YOUTUBE", "MAPS", "DRIVE", "TRADUCTOR", "GMAIL" };
    private static readonly string[] SiglasExtensiones = { "YT", "M", "D", "TR", "G" };
    private readonly List<RectTransform> obstaculos = new List<RectTransform>();
    private readonly List<RectTransform> nubes = new List<RectTransform>();
    private readonly List<ObjetivoExtension> objetivos = new List<ObjetivoExtension>();
    private readonly HashSet<int> extensionesGolpeadas = new HashSet<int>();
    private RectTransform dino;
    private RectTransform suelo;
    private TextMeshProUGUI puntuacion;
    private TextMeshProUGUI progresoExtensiones;
    private TextMeshProUGUI aviso;
    private RectTransform brazoGolpe;
    private GameObject panelFinal;
    private float velocidadY;
    private float relojObstaculo = 1.5f;
    private float relojSuelo;
    private float puntos;
    private float relojObjetivo = 2.4f;
    private float tiempoGolpe;
    private float tiempoAviso;
    private float proximoGolpe;
    private bool jugando = true;
    private bool agachado;
    private Vector2 escalaDino;

    private sealed class ObjetivoExtension
    {
        public int indice;
        public RectTransform rect;
    }

    private void Start() { CrearInterfaz(); }

    private void Update()
    {
        if (!jugando)
        {
            if (SaltarPresionado()) Reiniciar();
            return;
        }

        bool saltar = SaltarPresionado();
        agachado = AgacharsePresionado();
        if (saltar && Mathf.Abs(dino.anchoredPosition.y - SueloY) < 0.5f && !agachado) velocidadY = 760f;
        velocidadY -= 1900f * Time.deltaTime;
        Vector2 posDino = dino.anchoredPosition;
        posDino.y = Mathf.Max(SueloY, posDino.y + velocidadY * Time.deltaTime);
        if (posDino.y <= SueloY) velocidadY = 0f;
        dino.anchoredPosition = posDino;
        bool tumbado = agachado && posDino.y <= SueloY;
        dino.sizeDelta = tumbado ? new Vector2(100f, 48f) : escalaDino;
        dino.GetComponent<Image>().preserveAspect = !tumbado;

        puntos += Time.deltaTime * 10f;
        puntuacion.text = Mathf.FloorToInt(puntos).ToString("00000");
        relojSuelo += Time.deltaTime * 850f;
        suelo.anchoredPosition = new Vector2(-Mathf.Repeat(relojSuelo, 42f), 112f);
        for (int i = 0; i < nubes.Count; i++)
        {
            RectTransform nube = nubes[i];
            nube.anchoredPosition += Vector2.left * 30f * Time.deltaTime;
            if (nube.anchoredPosition.x < -180f) nube.anchoredPosition = new Vector2(2050f + i * 115f, nube.anchoredPosition.y);
        }

        ActualizarObjetivos(Time.deltaTime);
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
            RectTransform o = obstaculos[i];
            o.anchoredPosition += Vector2.left * (420f + puntos * 1.2f) * Time.deltaTime;
            if (o.anchoredPosition.x < -1050f) { Destroy(o.gameObject); obstaculos.RemoveAt(i); continue; }
            bool solapaX = Mathf.Abs(o.anchoredPosition.x - dino.anchoredPosition.x) < (o.sizeDelta.x + dino.sizeDelta.x) * 0.38f;
            bool ave = o.name == "Ave pixel";
            float obstaculoAbajo = ave ? o.anchoredPosition.y - o.sizeDelta.y * 0.5f : o.anchoredPosition.y;
            float obstaculoArriba = ave ? o.anchoredPosition.y + o.sizeDelta.y * 0.5f : o.anchoredPosition.y + o.sizeDelta.y;
            bool solapaY = dino.anchoredPosition.y < obstaculoArriba && dino.anchoredPosition.y + dino.sizeDelta.y > obstaculoAbajo;
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
        puntuacion = Texto("00000", root, new Vector2(1f, 1f), new Vector2(-105f, -70f), new Vector2(210, 42), 24, tinta, FontStyles.Bold);
        puntuacion.alignment = TextAlignmentOptions.Right;
        progresoExtensiones = Texto("EXTENSIONES  0/5", root, new Vector2(1f, 1f), new Vector2(-105f, -108f), new Vector2(210, 30), 15, tinta, FontStyles.Normal);
        progresoExtensiones.alignment = TextAlignmentOptions.Right;
        aviso = Texto("ESPACIO: SALTAR  ·  ↓: AGACHARSE  ·  CLIC / X / RT: GOLPEAR", root, new Vector2(0.5f, 1f), new Vector2(0, -70f), new Vector2(620, 34), 15, new Color(0.55f, 0.57f, 0.58f), FontStyles.Normal);
        CrearNube(root, 720f, 590f);
        CrearNube(root, 1240f, 720f);
        CrearNube(root, 1770f, 530f);
        suelo = CrearImagen("Suelo", root, new Vector2(0.5f, 0), new Vector2(0, 112), new Vector2(2200, 3), tinta);
        for (int i = 0; i < 55; i++)
        {
            float x = i * 43f;
            CrearImagen("Marca del suelo", suelo, new Vector2(0, 0.5f), new Vector2(x, 0), new Vector2(8, 2), new Color(0.68f, 0.69f, 0.69f));
            if (i % 3 == 0) CrearImagen("Marca del suelo", suelo, new Vector2(0, 0.5f), new Vector2(x + 17f, -1f), new Vector2(3, 2), new Color(0.72f, 0.73f, 0.73f));
        }
        dino = CrearImagen("Dino Chrome", root, new Vector2(0, 0), new Vector2(300, SueloY), new Vector2(96, 84), tinta);
        dino.pivot = new Vector2(0.5f, 0f);
        Sprite sprite = Resources.Load<Sprite>("Dino_Chrome");
        if (sprite == null) sprite = UnityEditorAssetSprite();
        if (sprite != null) dino.GetComponent<Image>().sprite = sprite;
        dino.GetComponent<Image>().preserveAspect = true; escalaDino = dino.sizeDelta;
        brazoGolpe = CrearImagen("Golpe", root, Vector2.zero, Vector2.zero, new Vector2(38f, 15f), tinta);
        brazoGolpe.gameObject.SetActive(false);
        MostrarAviso("ESPACIO: SALTAR  ·  ↓: AGACHARSE  ·  CLIC / X / RT: GOLPEAR", 5f);
    }

    private void ActualizarObjetivos(float dt)
    {
        relojObjetivo -= dt;
        if (relojObjetivo <= 0f && extensionesGolpeadas.Count < NombresExtensiones.Length)
        {
            CrearObjetivo();
            relojObjetivo = Random.Range(3.8f, 5.2f);
        }

        for (int i = objetivos.Count - 1; i >= 0; i--)
        {
            ObjetivoExtension objetivo = objetivos[i];
            objetivo.rect.anchoredPosition += Vector2.left * (420f + puntos * 1.2f) * dt;
            if (objetivo.rect.anchoredPosition.x < -100f)
            {
                Destroy(objetivo.rect.gameObject);
                objetivos.RemoveAt(i);
            }
        }
    }

    private void CrearObjetivo()
    {
        List<int> disponibles = new List<int>();
        for (int i = 0; i < NombresExtensiones.Length; i++)
        {
            if (!extensionesGolpeadas.Contains(i))
            {
                bool yaEnPantalla = false;
                for (int j = 0; j < objetivos.Count; j++)
                    if (objetivos[j].indice == i) yaEnPantalla = true;
                if (!yaEnPantalla) disponibles.Add(i);
            }
        }
        if (disponibles.Count == 0) return;

        int indice = disponibles[Random.Range(0, disponibles.Count)];
        float y = Random.value > 0.5f ? SueloY + 68f : SueloY + 126f;
        RectTransform placa = CrearImagen("Extension " + NombresExtensiones[indice], dino.parent,
            Vector2.zero, new Vector2(2050f, y), new Vector2(42f, 42f), new Color(0.32f, 0.34f, 0.35f));
        RectTransform fondo = CrearImagen("Fondo icono", placa, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(34f, 34f), Color.white);
        Texto(SiglasExtensiones[indice], fondo, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(32f, 30f), 15f, new Color(0.32f, 0.34f, 0.35f), FontStyles.Bold);
        objetivos.Add(new ObjetivoExtension { indice = indice, rect = placa });
    }

    private bool GolpePresionado()
    {
        if (Time.unscaledTime < proximoGolpe) return false;
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        Mouse raton = Mouse.current;
        Gamepad mando = Gamepad.current;
        return (teclado != null && teclado.xKey.wasPressedThisFrame) ||
               (raton != null && raton.leftButton.wasPressedThisFrame) ||
               (mando != null && (mando.rightTrigger.wasPressedThisFrame || mando.leftTrigger.wasPressedThisFrame));
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton7);
#endif
    }

    private void GolpearDelante()
    {
        proximoGolpe = Time.unscaledTime + 0.32f;
        tiempoGolpe = 0.16f;
        brazoGolpe.gameObject.SetActive(true);

        float x = dino.anchoredPosition.x + dino.sizeDelta.x * 0.5f + 14f;
        float yMin = dino.anchoredPosition.y + 8f;
        float yMax = dino.anchoredPosition.y + dino.sizeDelta.y - 8f;
        bool impacto = false;

        for (int i = objetivos.Count - 1; i >= 0; i--)
        {
            ObjetivoExtension objetivo = objetivos[i];
            Vector2 pos = objetivo.rect.anchoredPosition;
            if (pos.x < x || pos.x > x + 155f || pos.y + 21f < yMin || pos.y - 21f > yMax) continue;

            extensionesGolpeadas.Add(objetivo.indice);
            puntos += 100f;
            progresoExtensiones.text = "EXTENSIONES  " + extensionesGolpeadas.Count + "/5";
            MostrarAviso("¡" + NombresExtensiones[objetivo.indice] + "!", 1.2f);
            Destroy(objetivo.rect.gameObject);
            objetivos.RemoveAt(i);
            impacto = true;
            if (extensionesGolpeadas.Count == NombresExtensiones.Length)
                MostrarAviso("¡LAS 5 EXTENSIONES APRENDIDAS!", 2.5f);
            break;
        }

        for (int i = obstaculos.Count - 1; i >= 0; i--)
        {
            RectTransform obstaculo = obstaculos[i];
            if (obstaculo.anchoredPosition.x < x || obstaculo.anchoredPosition.x > x + 155f) continue;
            float abajo = obstaculo.name == "Ave pixel"
                ? obstaculo.anchoredPosition.y - obstaculo.sizeDelta.y * 0.5f
                : obstaculo.anchoredPosition.y;
            float arriba = abajo + obstaculo.sizeDelta.y;
            if (arriba < yMin || abajo > yMax) continue;

            Destroy(obstaculo.gameObject);
            obstaculos.RemoveAt(i);
            puntos += 25f;
            if (!impacto) MostrarAviso("¡Buen golpe!", 0.8f);
            break;
        }
    }

    private void MostrarAviso(string texto, float duracion)
    {
        aviso.text = texto;
        tiempoAviso = duracion;
    }

    private void CrearNube(Transform root, float x, float y)
    {
        RectTransform nube = CrearImagen("Nube", root, Vector2.zero, new Vector2(x, y), new Vector2(112, 16), new Color(0.87f, 0.88f, 0.88f));
        nube.pivot = new Vector2(0.5f, 0.5f);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(-24, 9), new Vector2(28, 16), nube.GetComponent<Image>().color);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(1, 14), new Vector2(32, 22), nube.GetComponent<Image>().color);
        CrearImagen("Nube pixel", nube, new Vector2(0.5f, 0.5f), new Vector2(27, 7), new Vector2(27, 14), nube.GetComponent<Image>().color);
        nubes.Add(nube);
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
        Mouse mouse = Mouse.current;
        return (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame)) ||
               (mouse != null && mouse.leftButton.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetMouseButtonDown(0);
#endif
    }

    private bool AgacharsePresionado()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        return teclado != null && (teclado.downArrowKey.isPressed || teclado.sKey.isPressed);
#else
        return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
#endif
    }

    private void CrearObstaculo()
    {
        bool ave = Random.value > 0.76f && puntos > 80;
        float alto = ave ? 44f : Random.Range(54f, 86f);
        RectTransform o = CrearImagen(ave ? "Ave pixel" : "Cactus", dino.parent, new Vector2(0, 0), new Vector2(2050, SueloY), new Vector2(ave ? 72 : 38, alto), new Color(0.24f, 0.27f, 0.28f));
        if (ave)
        {
            o.sizeDelta = new Vector2(58f, 18f);
            o.anchoredPosition = new Vector2(2050, SueloY + 94f);
            CrearImagen("Ala pixel", o, new Vector2(0.5f, 0.5f), new Vector2(-4, 10), new Vector2(15, 20), o.GetComponent<Image>().color);
            CrearImagen("Pico pixel", o, new Vector2(0.5f, 0.5f), new Vector2(34, 2), new Vector2(13, 7), o.GetComponent<Image>().color);
            CrearImagen("Cola pixel", o, new Vector2(0.5f, 0.5f), new Vector2(-33, 1), new Vector2(11, 12), o.GetComponent<Image>().color);
        }
        else
        {
            o.pivot = new Vector2(0.5f, 0f);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(-23, 5), new Vector2(18, 12), o.GetComponent<Image>().color);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(-30, 14), new Vector2(12, 26), o.GetComponent<Image>().color);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(23, 18), new Vector2(18, 12), o.GetComponent<Image>().color);
            CrearImagen("Brazo cactus", o, new Vector2(0.5f, 0.5f), new Vector2(30, 28), new Vector2(12, 28), o.GetComponent<Image>().color);
        }
        obstaculos.Add(o); relojObstaculo = Random.Range(1.25f, 2.15f);
    }

    private void Terminar()
    {
        jugando = false;
        panelFinal = new GameObject("Fin de partida", typeof(RectTransform));
        RectTransform p = panelFinal.GetComponent<RectTransform>(); p.SetParent(dino.parent, false); p.anchorMin = p.anchorMax = new Vector2(0.5f, 0.5f); p.sizeDelta = new Vector2(700, 150);
        Texto("GAME OVER", p, new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(600, 50), 28, new Color(0.32f, 0.34f, 0.35f), FontStyles.Bold);
        Texto("Pulsa ESPACIO o CLIC para reiniciar", p, new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(600, 42), 20, new Color(0.45f, 0.47f, 0.48f), FontStyles.Normal);
    }

    private void Reiniciar()
    {
        foreach (RectTransform o in obstaculos) if (o != null) Destroy(o.gameObject);
        obstaculos.Clear(); if (panelFinal != null) Destroy(panelFinal);
        foreach (ObjetivoExtension objetivo in objetivos) if (objetivo.rect != null) Destroy(objetivo.rect.gameObject);
        objetivos.Clear();
        puntos = 0; velocidadY = 0; relojObstaculo = 1.5f; dino.anchoredPosition = new Vector2(300, SueloY); dino.sizeDelta = escalaDino; jugando = true;
        relojObjetivo = 2.4f; relojSuelo = 0f; extensionesGolpeadas.Clear(); tiempoGolpe = 0f; progresoExtensiones.text = "EXTENSIONES  0/5"; puntuacion.text = "00000"; brazoGolpe.gameObject.SetActive(false);
        MostrarAviso("ESPACIO: SALTAR  ·  ↓: AGACHARSE  ·  CLIC / X / RT: GOLPEAR", 5f);
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
