using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ControlGolpeTutorial
{
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";
    private const string RutaPaso3 = "Assets/TutorialGoogle/UI/Paso3.png";
    private const string RutaClick = "Assets/TutorialGoogle/UI/Click.png";
    private const string RutaGatillo = "Assets/TutorialGoogle/Icons/Mando/xbox_rt.png";
    private const string RutaTarjeta = "Assets/TutorialGoogle/UI/Kenney/Kenney_Input_Gris.png";

    private static readonly Color TextoOscuro = new Color(0.125f, 0.129f, 0.141f, 1f);
    private static readonly Color TextoGris = new Color(0.373f, 0.388f, 0.408f, 1f);
    private static readonly Color TeclaOscura = new Color(0.24f, 0.28f, 0.33f, 1f);
    private static readonly Color FondoPaso = new Color(0.91f, 0.945f, 0.996f, 1f);

    static ControlGolpeTutorial()
    {
        EditorApplication.delayCall += AplicarSiProcede;
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += AplicarSiProcede;
        };
    }

    [MenuItem("Tools/Tutorial Google/Agregar control Golpear")]
    public static void AplicarMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != RutaEscena)
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        if (Aplicar(escena))
        {
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.Refresh();
            Debug.Log("TUTORIAL_GOLPE_OK: control Golpear añadido a la escena.");
        }
        else
        {
            Debug.Log("TUTORIAL_GOLPE: la escena ya tiene el control Golpear.");
        }
    }

    private static void AplicarSiProcede()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (BuildPipeline.isBuildingPlayer) return;
        if (!File.Exists(RutaEscena)) return;

        try
        {
            if (File.ReadAllText(RutaEscena).Contains("TarjetaPaso3")) return;
        }
        catch
        {
            return;
        }

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        bool abierta = escena.IsValid() && escena.isLoaded;
        bool aditiva = false;

        if (!abierta)
        {
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);
            aditiva = true;
        }

        if (!escena.IsValid()) return;

        bool cambiada = Aplicar(escena);

        if (cambiada)
        {
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("TUTORIAL_GOLPE_OK: control Golpear añadido automáticamente.");
        }

        if (aditiva)
            EditorSceneManager.CloseScene(escena, true);

        if (cambiada)
            AssetDatabase.Refresh();
    }

    private static bool Aplicar(Scene escena)
    {
        GameObject panelGO = BuscarPanel(escena);
        if (panelGO == null)
        {
            Debug.LogWarning("TUTORIAL_GOLPE: no encontré Canvas/SafeArea/PanelControles.");
            return false;
        }

        return AgregarFilaGolpe(panelGO.transform, false);
    }

    private static GameObject BuscarPanel(Scene escena)
    {
        if (!escena.IsValid()) return null;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            foreach (Transform t in raiz.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != "PanelControles") continue;

                Transform padre = t.parent;
                if (padre != null && padre.name == "SafeArea" &&
                    padre.parent != null && padre.parent.name == "Canvas")
                    return t.gameObject;
            }
        }

        return null;
    }

    public static bool AgregarFilaGolpe(Transform panelT, bool forzar = false)
    {
        if (panelT == null) return false;
        if (!forzar && panelT.Find("TarjetaPaso3") != null)
            return false;

        RectTransform panelRT = panelT as RectTransform;
        if (panelRT != null)
            panelRT.sizeDelta = new Vector2(620f, 373f);

        Transform pie = panelT.Find("Pie");
        if (pie != null)
        {
            RectTransform pieRT = pie as RectTransform;
            if (pieRT != null)
                pieRT.anchoredPosition = new Vector2(24f, -340f);
        }

        Transform subtitulo = panelT.Find("Subtitulo");
        if (subtitulo != null)
        {
            TextMeshProUGUI textoSubtitulo = subtitulo.GetComponent<TextMeshProUGUI>();
            if (textoSubtitulo != null)
                textoSubtitulo.text = "Aprende en 3 pasos";
        }

        Sprite tarjeta = CargarSprite(RutaTarjeta);
        Sprite paso3 = CargarSprite(RutaPaso3);
        Sprite click = CargarSprite(RutaClick);
        Sprite gatillo = CargarSprite(RutaGatillo);

        Image tarjetaPaso3 = CrearImagen(panelT, "TarjetaPaso3", tarjeta,
            new Vector2(0f, 1f), new Vector2(16f, -234f), new Vector2(588f, 78f),
            FondoPaso, new Vector2(0f, 1f));
        tarjetaPaso3.type = Image.Type.Sliced;
        LatidoColorUI latido = tarjetaPaso3.gameObject.AddComponent<LatidoColorUI>();
        latido.colorA = FondoPaso;
        latido.colorB = new Color(0.84f, 0.90f, 1f, 1f);
        latido.velocidad = 1.1f;

        CrearImagen(panelT, "Paso3", paso3,
            new Vector2(0f, 1f), new Vector2(30f, -236f), new Vector2(46f, 46f),
            Color.white, new Vector2(0f, 1f));

        CrearImagen(panelT, "Click", click,
            new Vector2(0f, 1f), new Vector2(92f, -246f), new Vector2(50f, 50f),
            Color.white, new Vector2(0f, 1f));

        CrearTexto(panelT, "TextoClick", "CLICK", 22f, TeclaOscura,
            new Vector2(0f, 1f), new Vector2(146f, -246f), new Vector2(90f, 50f),
            TextAlignmentOptions.Left, FontStyles.Bold);

        CrearTexto(panelT, "Separador3", "o", 22f, TextoGris,
            new Vector2(0f, 1f), new Vector2(246f, -246f), new Vector2(24f, 50f),
            TextAlignmentOptions.Center, FontStyles.Normal);

        CrearImagen(panelT, "Gatillo", gatillo,
            new Vector2(0f, 1f), new Vector2(280f, -246f), new Vector2(50f, 50f),
            TeclaOscura, new Vector2(0f, 1f));

        CrearTexto(panelT, "TextoGolpe", "Golpea", 26f, TextoOscuro,
            new Vector2(0f, 1f), new Vector2(342f, -246f), new Vector2(180f, 50f),
            TextAlignmentOptions.Left, FontStyles.Bold);

        tarjetaPaso3.transform.SetAsFirstSibling();

        return true;
    }

    private static Sprite CargarSprite(string ruta)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (s == null)
        {
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
            s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }
        if (s == null) Debug.LogWarning("TUTORIAL_GOLPE: falta sprite " + ruta);
        return s;
    }

    private static Image CrearImagen(Transform padre, string nombre, Sprite sprite,
        Vector2 anclaje, Vector2 posicion, Vector2 tamano, Color color, Vector2 pivote)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = anclaje;
        rt.anchorMax = anclaje;
        rt.pivot = pivote;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, float tamano,
        Color color, Vector2 anclaje, Vector2 posicion, Vector2 tamanoRect,
        TextAlignmentOptions alineacion, FontStyles estilo)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = anclaje;
        rt.anchorMax = anclaje;
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamanoRect;

        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = alineacion;
        texto.fontStyle = estilo;
        texto.enableWordWrapping = false;
        texto.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            texto.font = TMP_Settings.defaultFontAsset;

        return texto;
    }
}
