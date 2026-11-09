using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreadorCanvasTutorial
{
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";

    private static readonly Color Blanco = Color.white;
    private static readonly Color GrisFondo = new Color(0.945f, 0.953f, 0.961f, 1f);
    private static readonly Color GrisMarco = new Color(0.87f, 0.88f, 0.90f, 1f);
    private static readonly Color GrisBarra = new Color(0.945f, 0.953f, 0.961f, 1f);
    private static readonly Color GrisTexto = new Color(0.373f, 0.388f, 0.408f, 1f);
    private static readonly Color Azul = new Color(0.259f, 0.522f, 0.957f, 1f);

    [InitializeOnLoadMethod]
    private static void AutoCrear()
    {
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(RutaEscena)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            CrearEscena(false, false);
        };
    }

    [MenuItem("Tools/Tutorial Google/Crear escena")]
    public static void CrearDesdeMenu()
    {
        CrearEscena(true, false);
    }

    public static void CrearPorBatch()
    {
        CrearEscena(false, true);
    }

    private static void CrearEscena(bool preguntar, bool batch)
    {
        if (File.Exists(RutaEscena))
        {
            if (!preguntar) return;
            if (!EditorUtility.DisplayDialog("Tutorial Google", "La escena ya existe. Quieres sobrescribirla?", "Si", "No")) return;
        }

        Scene escenaAnterior = SceneManager.GetActiveScene();

        if (!batch && string.IsNullOrEmpty(escenaAnterior.path))
        {
            Debug.LogWarning("TUTORIAL_GOOGLE: hay una escena sin guardar abierta. Guardala o abre otra escena antes de crear el tutorial.");
            return;
        }

        Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, batch ? NewSceneMode.Single : NewSceneMode.Additive);

        if (!batch)
            SceneManager.SetActiveScene(escena);

        CrearCamara();
        CrearEventSystem();
        CrearCanvas();

        EditorSceneManager.SaveScene(escena, RutaEscena);

        if (!batch)
        {
            EditorSceneManager.CloseScene(escena, true);

            if (escenaAnterior.IsValid())
                SceneManager.SetActiveScene(escenaAnterior);
        }

        AssetDatabase.Refresh();
        Debug.Log("TUTORIAL_GOOGLE_OK: escena creada en " + RutaEscena);
    }

    private static void CrearCamara()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";

        Camera camara = go.AddComponent<Camera>();
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = GrisFondo;
        camara.orthographic = true;
        camara.orthographicSize = 5f;

        go.AddComponent<AudioListener>();
    }

    private static void CrearEventSystem()
    {
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
        modulo.AssignDefaultActions();
    }

    private static void CrearCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform safe = CrearRect("SafeArea", go.transform);
        Estirar(safe);
        safe.gameObject.AddComponent<AreaSegura>();

        Image fondo = CrearImagen("Fondo", safe, Blanco);
        Estirar(fondo.rectTransform);

        CrearMarcoSuperior(safe);
        CrearContenido(safe);
        CrearClippy(safe);
        CrearPanelControles(safe);
        CrearBotonSaltar(safe);
    }

    private static void CrearMarcoSuperior(RectTransform padre)
    {
        Image marco = CrearImagen("MarcoSuperior", padre, GrisMarco);
        Anclar(marco.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 96f), new Vector2(0.5f, 1f));

        RectTransform pestanas = CrearRect("Pestanas", marco.transform);
        Anclar(pestanas, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(24f, 0f), new Vector2(320f, 0f), new Vector2(0f, 0.5f));

        Image pestanaActiva = CrearImagen("PestanaActiva", pestanas, Blanco);
        Anclar(pestanaActiva.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(240f, -20f), new Vector2(0f, 0.5f));

        TextMeshProUGUI textoPestana = CrearTexto("TextoPestana", pestanaActiva.transform, "Nueva pestana", 20f, GrisTexto, TextAlignmentOptions.Left);
        Estirar(textoPestana.rectTransform, 12f);

        Image pestanaNueva = CrearImagen("PestanaNueva", pestanas, new Color(0f, 0f, 0f, 0f));
        Anclar(pestanaNueva.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(252f, 0f), new Vector2(40f, -20f), new Vector2(0f, 0.5f));

        TextMeshProUGUI textoMas = CrearTexto("TextoMas", pestanaNueva.transform, "+", 28f, GrisTexto, TextAlignmentOptions.Center);
        Estirar(textoMas.rectTransform);

        Image barra = CrearImagen("BarraDireccion", marco.transform, GrisBarra);
        Anclar(barra.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(-760f, 44f), new Vector2(0.5f, 0.5f));

        TextMeshProUGUI textoUrl = CrearTexto("TextoUrl", barra.transform, "google.com", 20f, GrisTexto, TextAlignmentOptions.Left);
        Estirar(textoUrl.rectTransform, 16f);

        RectTransform iconos = CrearRect("IconosVentana", marco.transform);
        Anclar(iconos, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-24f, 0f), new Vector2(160f, 0f), new Vector2(1f, 0.5f));

        TextMeshProUGUI textoIconos = CrearTexto("TextoIconos", iconos, "—  □  ✕", 20f, GrisTexto, TextAlignmentOptions.Center);
        Estirar(textoIconos.rectTransform);
    }

    private static void CrearContenido(RectTransform padre)
    {
        RectTransform contenido = CrearRect("Contenido", padre);
        Anclar(contenido, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(900f, 620f), new Vector2(0.5f, 0.5f));

        TextMeshProUGUI logo = CrearTexto(
            "Logo",
            contenido,
            "<color=#4285F4>G</color><color=#EA4335>o</color><color=#FBBC05>o</color><color=#4285F4>g</color><color=#34A853>l</color><color=#EA4335>e</color>",
            120f,
            GrisTexto,
            TextAlignmentOptions.Center);

        Anclar(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(720f, 170f), new Vector2(0.5f, 1f));

        Image barra = CrearImagen("BarraBusqueda", contenido, GrisBarra);
        Anclar(barra.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(760f, 64f), new Vector2(0.5f, 0.5f));

        TextMeshProUGUI textoBarra = CrearTexto("TextoBusqueda", barra.transform, "Buscar en Google o escribe una URL", 24f, new Color(0.45f, 0.47f, 0.49f, 1f), TextAlignmentOptions.Left);
        Estirar(textoBarra.rectTransform, 24f);

        RectTransform accesos = CrearRect("AccesosDirectos", contenido);
        Anclar(accesos, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(760f, 150f), new Vector2(0.5f, 0f));

        HorizontalLayoutGroup layout = accesos.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 32f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        for (int i = 1; i <= 5; i++)
        {
            Image acceso = CrearImagen("Acceso" + i, accesos, new Color(0.945f, 0.953f, 0.961f, 1f));
            acceso.rectTransform.sizeDelta = new Vector2(96f, 96f);

            LayoutElement elemento = acceso.gameObject.AddComponent<LayoutElement>();
            elemento.preferredWidth = 96f;
            elemento.preferredHeight = 96f;

            TextMeshProUGUI numero = CrearTexto("Numero" + i, acceso.transform, i.ToString(), 28f, GrisTexto, TextAlignmentOptions.Center);
            Estirar(numero.rectTransform);
        }
    }

    private static void CrearClippy(RectTransform padre)
    {
        RectTransform clippy = CrearRect("Clippy", padre);
        Anclar(clippy, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(420f, 260f), new Vector2(1f, 0f));

        Image burbuja = CrearImagen("Burbuja", clippy, Blanco);
        Anclar(burbuja.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 150f), new Vector2(0.5f, 1f));

        TextMeshProUGUI dialogo = CrearTexto(
            "TextoDialogo",
            burbuja.transform,
            "Hola, soy Clippy. Te enseno a jugar.",
            24f,
            GrisTexto,
            TextAlignmentOptions.Left);

        Estirar(dialogo.rectTransform, 20f);

        Image personaje = CrearImagen("Personaje", clippy, new Color(0.85f, 0.85f, 0.85f, 1f));
        Anclar(personaje.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(120f, 120f), new Vector2(1f, 0f));

        TextMeshProUGUI etiqueta = CrearTexto("EtiquetaPersonaje", personaje.transform, "Clippy", 20f, GrisTexto, TextAlignmentOptions.Center);
        Estirar(etiqueta.rectTransform);
    }

    private static void CrearPanelControles(RectTransform padre)
    {
        Image panel = CrearImagen("PanelControles", padre, new Color(1f, 1f, 1f, 0.95f));
        Anclar(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(420f, 220f), new Vector2(0f, 0f));

        TextMeshProUGUI texto = CrearTexto(
            "TextoControles",
            panel.transform,
            "MOVER\nWASD  ·  Stick izquierdo\n\nSALTAR\nEspacio  ·  A / Cross",
            24f,
            GrisTexto,
            TextAlignmentOptions.Left);

        Estirar(texto.rectTransform, 24f);
    }

    private static void CrearBotonSaltar(RectTransform padre)
    {
        Image fondo = CrearImagen("BotonSaltar", padre, Azul);
        Anclar(fondo.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(220f, 56f), new Vector2(1f, 1f));

        Button boton = fondo.gameObject.AddComponent<Button>();
        boton.targetGraphic = fondo;

        TextMeshProUGUI texto = CrearTexto("TextoBoton", fondo.transform, "Saltar tutorial", 22f, Blanco, TextAlignmentOptions.Center);
        Estirar(texto.rectTransform, 12f);
    }

    private static RectTransform CrearRect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(padre, false);
        return rect;
    }

    private static Image CrearImagen(string nombre, Transform padre, Color color)
    {
        RectTransform rect = CrearRect(nombre, padre);
        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = color;
        return imagen;
    }

    private static TextMeshProUGUI CrearTexto(string nombre, Transform padre, string contenido, float tamano, Color color, TextAlignmentOptions alineacion)
    {
        RectTransform rect = CrearRect(nombre, padre);
        TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = alineacion;
        texto.enableWordWrapping = true;
        texto.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            texto.font = TMP_Settings.defaultFontAsset;

        return texto;
    }

    private static void Estirar(RectTransform rect, float margen = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(margen, margen);
        rect.offsetMax = new Vector2(-margen, -margen);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void Anclar(RectTransform rect, Vector2 min, Vector2 max, Vector2 posicion, Vector2 tamano, Vector2 pivot)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
        rect.pivot = pivot;
    }
}
