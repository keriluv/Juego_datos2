using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DecoradorEscenaTutorial
{
    private const string RutaEscena = "Assets/Scenes/TutorialGoogle.unity";

    private static readonly Color TextoOscuro = new Color(0.125f, 0.129f, 0.141f, 1f);
    private static readonly Color TextoGris = new Color(0.373f, 0.388f, 0.408f, 1f);
    private static readonly Color IconoGris = new Color(0.373f, 0.388f, 0.408f, 1f);
    private static readonly Color TeclaOscura = new Color(0.24f, 0.28f, 0.33f, 1f);
    private static readonly Color Azul = new Color(0.102f, 0.451f, 0.910f, 1f);

    private class DatosExtension
    {
        public string nombre;
        public string descripcion;
        public string icono;
        public bool activa;
    }

    [MenuItem("Tools/Tutorial Google/Decorar escena")]
    public static void DecorarMenu()
    {
        ImportadorAssetsTutorial.Importar();
        Decorar();
    }

    public static void PrepararPorBatch()
    {
        ImportadorAssetsTutorial.Importar();
        Decorar();
    }

    private static void Decorar()
    {
        Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        DecorarEscena(escena);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.Refresh();

        Debug.Log("TUTORIAL_DECOR_OK: escena decorada con sprites");
    }

    public static void DecorarEscenaAbierta()
    {
        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != RutaEscena)
        {
            Debug.LogWarning("TUTORIAL_DECOR: la escena activa no es " + RutaEscena);
            return;
        }

        DecorarEscena(escena);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.Refresh();

        Debug.Log("TUTORIAL_DECOR_OK: escena abierta decorada");
    }

    private static void DecorarEscena(Scene escena)
    {
        Transform safe = Buscar("Canvas/SafeArea")?.transform;
        if (safe == null)
        {
            Debug.LogError("TUTORIAL_DECOR: falta Canvas/SafeArea");
            return;
        }

        Sprite mascota = Sprite("Assets/TutorialGoogle/Sprites/Dino_Chrome.png");
        Sprite jugadorSprite = Sprite("Assets/TutorialGoogle/Sprites/JugadorTutorial.png");
        Sprite panelSprite = Sprite("Assets/TutorialGoogle/UI/Kenney/Kenney_Input_Outline.png");
        Sprite tarjeta = Sprite("Assets/TutorialGoogle/UI/Kenney/Kenney_Input_Gris.png");
        Sprite botonAzul = Sprite("Assets/TutorialGoogle/UI/Kenney/Kenney_Boton_Azul.png");
        Sprite circulo = Sprite("Assets/TutorialGoogle/UI/Kenney/Kenney_BotonRedondo_Gris.png");

        Image fondoPagina = Imagen("Canvas/SafeArea/Fondo");
        if (fondoPagina != null)
            fondoPagina.color = Color.white;

        RectTransform clippy = Rect("Canvas/SafeArea/Clippy");
        if (clippy != null)
        {
            clippy.sizeDelta = new Vector2(480f, 450f);
            clippy.anchoredPosition = new Vector2(-40f, 56f);
        }

        Image personaje = Imagen("Canvas/SafeArea/Clippy/Personaje");
        if (personaje != null)
        {
            personaje.sprite = mascota;
            personaje.color = Color.white;
            personaje.preserveAspect = true;
            RectTransform rt = personaje.rectTransform;
            rt.sizeDelta = new Vector2(270f, 270f);

            FlotarUI flotar = personaje.GetComponent<FlotarUI>();
            if (flotar == null) flotar = personaje.gameObject.AddComponent<FlotarUI>();
            flotar.amplitud = 8f;
            flotar.velocidad = 1.1f;
        }

        RectTransform burbujaRect = Rect("Canvas/SafeArea/Clippy/Burbuja");
        if (burbujaRect != null)
            burbujaRect.sizeDelta = new Vector2(0f, 150f);

        GameObject etiqueta = Buscar("Canvas/SafeArea/Clippy/Personaje/EtiquetaPersonaje");
        if (etiqueta != null) etiqueta.SetActive(false);

        Image fondoBurbuja = Imagen("Canvas/SafeArea/Clippy/Burbuja");
        if (fondoBurbuja != null)
        {
            fondoBurbuja.sprite = panelSprite;
            fondoBurbuja.type = Image.Type.Sliced;
            fondoBurbuja.color = Color.white;
        }

        TextMeshProUGUI dialogo = Texto("Canvas/SafeArea/Clippy/Burbuja/TextoDialogo");
        if (dialogo != null)
        {
            dialogo.text = "¡Hola! Soy el Dino de Chrome.\nSalta sobre las 5 extensiones.";
            dialogo.fontSize = 24f;
            dialogo.color = TextoOscuro;
            dialogo.enableWordWrapping = true;
        }

        Image boton = Imagen("Canvas/SafeArea/BotonSaltar");
        if (boton != null)
        {
            boton.sprite = botonAzul;
            boton.type = Image.Type.Sliced;
            boton.color = Color.white;
            boton.rectTransform.anchoredPosition = new Vector2(-40f, -176f);
        }

        TextMeshProUGUI textoBoton = Texto("Canvas/SafeArea/BotonSaltar/TextoBoton");
        if (textoBoton != null)
        {
            textoBoton.text = "Saltar tutorial";
            textoBoton.color = Color.white;
            textoBoton.fontStyle = FontStyles.Bold;
        }

        if (boton != null)
        {
            BotonSaltarTutorial saltar = boton.GetComponent<BotonSaltarTutorial>();
            if (saltar == null) saltar = boton.gameObject.AddComponent<BotonSaltarTutorial>();
            saltar.escena = "MainMenu";
        }

        TextMeshProUGUI textoPestana = Texto("Canvas/SafeArea/MarcoSuperior/Pestanas/PestanaActiva/TextoPestana");
        if (textoPestana != null) textoPestana.text = "Google";

        Image barraUrl = Imagen("Canvas/SafeArea/MarcoSuperior/BarraDireccion");
        if (barraUrl != null)
        {
            barraUrl.sprite = tarjeta;
            barraUrl.type = Image.Type.Sliced;
            barraUrl.color = Color.white;
        }

        Transform barraUrlT = Buscar("Canvas/SafeArea/MarcoSuperior/BarraDireccion")?.transform;
        if (barraUrlT != null)
        {
            CrearIcono(barraUrlT, "IconoAtras", Icono("arrow_back"),
                new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(0f, 0.5f));
            CrearIcono(barraUrlT, "IconoAdelante", Icono("arrow_forward"),
                new Vector2(0f, 0.5f), new Vector2(54f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(0f, 0.5f));
            CrearIcono(barraUrlT, "IconoRefrescar", Icono("refresh"),
                new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(0f, 0.5f));
            CrearIcono(barraUrlT, "IconoCandado", Icono("lock"),
                new Vector2(0f, 0.5f), new Vector2(124f, 0f), new Vector2(20f, 20f), IconoGris, new Vector2(0f, 0.5f));
            CrearIcono(barraUrlT, "IconoMicroUrl", Icono("mic"),
                new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(26f, 26f), IconoGris, new Vector2(1f, 0.5f));
        }

        TextMeshProUGUI textoIconos = Texto("Canvas/SafeArea/MarcoSuperior/IconosVentana/TextoIconos");
        if (textoIconos != null)
            textoIconos.text = "—   □   ×";

        TextMeshProUGUI textoUrl = Texto("Canvas/SafeArea/MarcoSuperior/BarraDireccion/TextoUrl");
        if (textoUrl != null)
        {
            textoUrl.text = "google.com";
            textoUrl.rectTransform.offsetMin = new Vector2(154f, 0f);
        }

        Transform marcoT = Buscar("Canvas/SafeArea/MarcoSuperior")?.transform;
        Image iconoPuzzle = CrearIcono(marcoT, "BotonExtensiones", Icono("extension"),
            new Vector2(1f, 0.5f), new Vector2(-196f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(1f, 0.5f));
        Button botonPuzzle = Boton(iconoPuzzle);

        TextMeshProUGUI progreso;
        ConstruirBarraGoogle(safe, out progreso);

        RectTransform contenido = Rect("Canvas/SafeArea/Contenido");
        if (contenido != null) Estirar(contenido);

        TextMeshProUGUI logo = Texto("Canvas/SafeArea/Contenido/Logo");
        if (logo != null)
        {
            AnclarArriba(logo.rectTransform, 180f, 700f, 160f);
            PlataformaUI plataformaLogo = Plataforma(logo.gameObject);
            plataformaLogo.etiqueta = "el logo de Google";
            plataformaLogo.esExtension = false;
        }

        Image barraBusqueda = Imagen("Canvas/SafeArea/Contenido/BarraBusqueda");
        if (barraBusqueda != null)
        {
            barraBusqueda.sprite = tarjeta;
            barraBusqueda.type = Image.Type.Sliced;
            barraBusqueda.color = Color.white;
            AnclarArriba(barraBusqueda.rectTransform, 356f, 700f, 64f);

            PlataformaUI plataformaBusqueda = Plataforma(barraBusqueda.gameObject);
            plataformaBusqueda.etiqueta = "la barra de búsqueda";
            plataformaBusqueda.esExtension = false;
        }

        Transform contenidoBarra = Buscar("Canvas/SafeArea/Contenido/BarraBusqueda")?.transform;
        if (contenidoBarra != null)
        {
            CrearIcono(contenidoBarra, "IconoBuscar", Icono("search"),
                new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(0f, 0.5f));
            CrearIcono(contenidoBarra, "IconoCamara", Icono("photo_camera"),
                new Vector2(1f, 0.5f), new Vector2(-62f, 0f), new Vector2(26f, 26f), IconoGris, new Vector2(1f, 0.5f));
            CrearIcono(contenidoBarra, "IconoMicro", Icono("mic"),
                new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(28f, 28f), IconoGris, new Vector2(1f, 0.5f));
        }

        GameObject textoBusquedaViejo = Buscar("Canvas/SafeArea/Contenido/BarraBusqueda/TextoBusqueda");
        if (textoBusquedaViejo != null) Object.DestroyImmediate(textoBusquedaViejo);

        TMP_InputField campoBusqueda = null;
        if (contenidoBarra != null)
            campoBusqueda = ConstruirCampoBusqueda(contenidoBarra);

        RectTransform anuncio = RectHijo(contenido, "Anuncio");
        if (anuncio != null)
        {
            AnclarArriba(anuncio, 430f, 700f, 40f);
            Image fondoAnuncio = anuncio.gameObject.AddComponent<Image>();
            fondoAnuncio.color = new Color(1f, 0.973f, 0.882f, 1f);
            fondoAnuncio.raycastTarget = false;
            TextoSimple(anuncio, "TextoAnuncio", "Anuncio: ¡Gana dinero saltando!  ·  ad.example.com", 14f,
                new Color(0.55f, 0.45f, 0.15f, 1f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(660f, 22f),
                TextAlignmentOptions.Left, FontStyles.Normal, new Vector2(0f, 0.5f));
        }

        RectTransform filaBotones = RectHijo(contenido, "BotonesGoogle");
        Estirar(filaBotones);

        Button botonBuscar = null;
        Button botonSuerte = null;

        Image imgBuscar = CrearImagen(filaBotones, "BotonBuscar", tarjeta, Color.white, true);
        if (imgBuscar != null)
        {
            ColocarCentrado(imgBuscar.rectTransform, -132f, 250f, 260f, 56f);
            TextoCentrado(imgBuscar.transform, "Texto", "Buscar con Google", 17f, TextoOscuro);
            botonBuscar = Boton(imgBuscar);
            EstiloBoton(botonBuscar);
            PlataformaUI plataformaBuscar = Plataforma(imgBuscar.gameObject);
            plataformaBuscar.etiqueta = "«Buscar con Google»";
        }

        Image imgSuerte = CrearImagen(filaBotones, "BotonSuerte", tarjeta, Color.white, true);
        if (imgSuerte != null)
        {
            ColocarCentrado(imgSuerte.rectTransform, 142f, 250f, 240f, 56f);
            TextoCentrado(imgSuerte.transform, "Texto", "Voy a tener suerte", 17f, TextoOscuro);
            botonSuerte = Boton(imgSuerte);
            EstiloBoton(botonSuerte);
            PlataformaUI plataformaSuerte = Plataforma(imgSuerte.gameObject);
            plataformaSuerte.etiqueta = "«Voy a tener suerte»";
        }

        RectTransform accesos = Rect("Canvas/SafeArea/Contenido/AccesosDirectos");
        if (accesos != null) Estirar(accesos);

        HorizontalLayoutGroup layout = accesos != null ? accesos.GetComponent<HorizontalLayoutGroup>() : null;
        if (layout != null) Object.DestroyImmediate(layout);

        string[] nombresAcceso = { "YouTube", "Gmail", "Drive", "Maps", "Translate" };
        string[] iconosAcceso = { "youtube", "gmail", "drive", "maps", "translate" };

        List<PlataformaUI> plataformasExtension = new List<PlataformaUI>();
        List<Button> botonesAcceso = new List<Button>();

        for (int i = 0; i < nombresAcceso.Length; i++)
        {
            string ruta = "Canvas/SafeArea/Contenido/AccesosDirectos/Acceso" + (i + 1);
            Image acceso = Imagen(ruta);
            if (acceso == null) continue;

            acceso.sprite = tarjeta;
            acceso.type = Image.Type.Sliced;
            acceso.color = Color.white;
            ColocarCentrado(acceso.rectTransform, -336f + i * 168f, 380f, 128f, 128f);

            GameObject numero = Buscar(ruta + "/Numero" + (i + 1));
            if (numero != null) numero.SetActive(false);

            GameObject circuloViejo = Buscar(ruta + "/Circulo");
            if (circuloViejo != null) Object.DestroyImmediate(circuloViejo);

            GameObject iconoViejo = Buscar(ruta + "/Icono");
            if (iconoViejo != null) Object.DestroyImmediate(iconoViejo);

            Sprite iconoMarca = Sprite("Assets/TutorialGoogle/Icons/Marcas/" + iconosAcceso[i] + ".png");
            CrearIcono(acceso.transform, "Icono", iconoMarca,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(64f, 64f), Color.white, new Vector2(0.5f, 0.5f));

            TextoSimple(acceso.transform, "Etiqueta", nombresAcceso[i], 18f, TextoOscuro,
                new Vector2(0.5f, 0f), new Vector2(0f, -8f), new Vector2(180f, 26f),
                TextAlignmentOptions.Center, FontStyles.Normal, new Vector2(0.5f, 1f));

            Button botonAcceso = Boton(acceso);
            EstiloBoton(botonAcceso);
            botonesAcceso.Add(botonAcceso);

            PlataformaUI plataforma = Plataforma(acceso.gameObject);
            plataforma.etiqueta = nombresAcceso[i];
            plataforma.esExtension = true;
            plataforma.indiceExtension = i;
            plataformasExtension.Add(plataforma);
        }

        GameObject panelResultadosViejo = Buscar("Canvas/SafeArea/Contenido/PanelResultados");
        if (panelResultadosViejo != null) Object.DestroyImmediate(panelResultadosViejo);

        List<ResultadoBusquedaUI> resultados;
        TextMeshProUGUI tituloResultados;
        Button botonCerrarResultados;
        GameObject panelResultados = ConstruirPanelResultados(safe, tarjeta,
            out resultados, out tituloResultados, out botonCerrarResultados);

        ConstruirPiePagina(safe);

        RectTransform mundo = RectHijo(safe, "Mundo");
        Estirar(mundo);
        mundo.SetSiblingIndex(3);

        RectTransform jugadorRT = RectHijo(mundo, "Jugador");
        jugadorRT.anchorMin = Vector2.zero;
        jugadorRT.anchorMax = Vector2.zero;
        jugadorRT.pivot = Vector2.zero;
        jugadorRT.sizeDelta = new Vector2(72f, 96f);
        jugadorRT.anchoredPosition = new Vector2(960f, 48f);

        RectTransform spriteRT = RectHijo(jugadorRT, "Sprite");
        spriteRT.anchorMin = Vector2.zero;
        spriteRT.anchorMax = Vector2.one;
        spriteRT.offsetMin = Vector2.zero;
        spriteRT.offsetMax = Vector2.zero;
        spriteRT.pivot = new Vector2(0.5f, 0.5f);

        Image spriteJugador = spriteRT.gameObject.AddComponent<Image>();
        spriteJugador.sprite = jugadorSprite;
        spriteJugador.color = Color.white;
        spriteJugador.preserveAspect = true;
        spriteJugador.raycastTarget = false;

        JugadorTutorial jugador = jugadorRT.gameObject.GetComponent<JugadorTutorial>();
        if (jugador == null) jugador = jugadorRT.gameObject.AddComponent<JugadorTutorial>();
        jugador.mundo = mundo;
        jugador.sprite = spriteJugador;
        jugador.alturaSuelo = 48f;
        jugador.posicionInicial = new Vector2(960f, 48f);
        jugador.velocidad = 520f;
        jugador.fuerzaSalto = 1100f;
        jugador.gravedad = 2000f;

        List<FilaExtensionUI> filas;
        Button botonCerrar;
        GameObject panelExtensiones = ConstruirPanelExtensiones(safe, panelSprite, tarjeta, circulo,
            out filas, out botonCerrar);

        RectTransform panel = Rect("Canvas/SafeArea/PanelControles");
        if (panel != null)
        {
            panel.sizeDelta = new Vector2(620f, 285f);
            panel.anchoredPosition = new Vector2(40f, 56f);
            Image fondo = panel.GetComponent<Image>();
            if (fondo != null)
            {
                fondo.sprite = panelSprite;
                fondo.type = Image.Type.Sliced;
                fondo.color = Color.white;
            }
        }

        GameObject textoViejo = Buscar("Canvas/SafeArea/PanelControles/TextoControles");
        if (textoViejo != null) Object.DestroyImmediate(textoViejo);

        Transform panelT = Buscar("Canvas/SafeArea/PanelControles")?.transform;
        if (panelT != null)
        {
            TextoSimple(panelT, "Titulo", "CONTROLES", 28f, TextoOscuro,
                new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(320f, 34f), TextAlignmentOptions.Left, FontStyles.Bold);
            TextoSimple(panelT, "Subtitulo", "Aprende en 2 pasos", 17f, TextoGris,
                new Vector2(0f, 1f), new Vector2(360f, -20f), new Vector2(236f, 24f), TextAlignmentOptions.Right, FontStyles.Normal);

            Image tarjetaPaso1 = CrearIcono(panelT, "TarjetaPaso1", tarjeta,
                new Vector2(0f, 1f), new Vector2(16f, -58f), new Vector2(588f, 78f),
                new Color(0.91f, 0.945f, 0.996f, 1f), new Vector2(0f, 1f));
            tarjetaPaso1.type = Image.Type.Sliced;
            LatidoColorUI latido1 = tarjetaPaso1.gameObject.AddComponent<LatidoColorUI>();
            latido1.colorA = new Color(0.91f, 0.945f, 0.996f, 1f);
            latido1.colorB = new Color(0.84f, 0.90f, 1f, 1f);
            latido1.velocidad = 1.1f;

            Image tarjetaPaso2 = CrearIcono(panelT, "TarjetaPaso2", tarjeta,
                new Vector2(0f, 1f), new Vector2(16f, -146f), new Vector2(588f, 78f),
                new Color(0.91f, 0.945f, 0.996f, 1f), new Vector2(0f, 1f));
            tarjetaPaso2.type = Image.Type.Sliced;
            LatidoColorUI latido2 = tarjetaPaso2.gameObject.AddComponent<LatidoColorUI>();
            latido2.colorA = new Color(0.91f, 0.945f, 0.996f, 1f);
            latido2.colorB = new Color(0.84f, 0.90f, 1f, 1f);
            latido2.velocidad = 1.1f;

            float y1 = -70f;
            CrearIcono(panelT, "Paso1", Sprite("Assets/TutorialGoogle/UI/Paso1.png"),
                new Vector2(0f, 1f), new Vector2(30f, y1 - 2f), new Vector2(46f, 46f), Color.white, new Vector2(0f, 1f));
            Tecla(panelT, "TeclaW", "keyboard_w", 92f, y1, 50f);
            Tecla(panelT, "TeclaA", "keyboard_a", 146f, y1, 50f);
            Tecla(panelT, "TeclaS", "keyboard_s", 200f, y1, 50f);
            Tecla(panelT, "TeclaD", "keyboard_d", 254f, y1, 50f);
            TextoSimple(panelT, "Separador1", "o", 22f, TextoGris,
                new Vector2(0f, 1f), new Vector2(318f, y1), new Vector2(24f, 50f), TextAlignmentOptions.Center, FontStyles.Normal);
            CrearIcono(panelT, "Stick", IconoMando("xbox_stick_l"),
                new Vector2(0f, 1f), new Vector2(352f, y1), new Vector2(50f, 50f), TeclaOscura, new Vector2(0f, 1f));
            TextoSimple(panelT, "TextoMover", "Muévete", 26f, TextoOscuro,
                new Vector2(0f, 1f), new Vector2(414f, y1), new Vector2(180f, 50f), TextAlignmentOptions.Left, FontStyles.Bold);

            float y2 = -158f;
            CrearIcono(panelT, "Paso2", Sprite("Assets/TutorialGoogle/UI/Paso2.png"),
                new Vector2(0f, 1f), new Vector2(30f, y2 - 2f), new Vector2(46f, 46f), Color.white, new Vector2(0f, 1f));
            Image teclaEspacio = CrearIcono(panelT, "TeclaEspacio", IconoTecla("keyboard_space"),
                new Vector2(0f, 1f), new Vector2(92f, y2), new Vector2(150f, 50f), TeclaOscura, new Vector2(0f, 1f));
            if (teclaEspacio != null) teclaEspacio.preserveAspect = false;
            TextoSimple(panelT, "Separador2", "o", 22f, TextoGris,
                new Vector2(0f, 1f), new Vector2(256f, y2), new Vector2(24f, 50f), TextAlignmentOptions.Center, FontStyles.Normal);
            CrearIcono(panelT, "BotonA", IconoMando("xbox_button_color_a"),
                new Vector2(0f, 1f), new Vector2(290f, y2), new Vector2(50f, 50f), Color.white, new Vector2(0f, 1f));
            TextoSimple(panelT, "TextoBotonA", "Salta", 26f, TextoOscuro,
                new Vector2(0f, 1f), new Vector2(352f, y2), new Vector2(180f, 50f), TextAlignmentOptions.Left, FontStyles.Bold);

            TextoSimple(panelT, "Pie", "Haz clic en el buscador para escribir. También puedes usar el mando.", 16f, TextoGris,
                new Vector2(0f, 1f), new Vector2(24f, -252f), new Vector2(570f, 24f), TextAlignmentOptions.Left, FontStyles.Normal);

            ControlGolpeTutorial.AgregarFilaGolpe(panelT, true);
        }

        InteraccionTutorial inter = safe.GetComponent<InteraccionTutorial>();
        if (inter == null) inter = safe.gameObject.AddComponent<InteraccionTutorial>();

        inter.jugador = jugador;
        inter.dialogo = dialogo;
        inter.textoProgreso = progreso;
        inter.clippy = clippy != null ? clippy.gameObject : null;
        inter.panelExtensiones = panelExtensiones;
        inter.panelControles = panel != null ? panel.gameObject : null;
        inter.anuncio = anuncio != null ? anuncio.gameObject : null;
        inter.fondoPagina = fondoPagina;
        inter.botonExtensiones = botonPuzzle;
        inter.botonCerrar = botonCerrar;
        inter.botonBuscar = botonBuscar;
        inter.botonSuerte = botonSuerte;
        inter.filas = filas;
        inter.extensiones = plataformasExtension;
        inter.accesosBotones = botonesAcceso;
        inter.campoBusqueda = campoBusqueda;
        inter.panelResultados = panelResultados;
        inter.tituloResultados = tituloResultados;
        inter.botonCerrarResultados = botonCerrarResultados;
        inter.textoUrl = textoUrl;
        inter.resultados = resultados;

        if (campoBusqueda != null)
        {
            BuscadorGoogle buscador = campoBusqueda.GetComponent<BuscadorGoogle>();
            if (buscador == null) buscador = campoBusqueda.gameObject.AddComponent<BuscadorGoogle>();
            buscador.campo = campoBusqueda;
            buscador.manager = inter;
        }

        for (int i = 0; i < filas.Count; i++)
            if (filas[i] != null) filas[i].manager = inter;

        if (panelExtensiones != null) panelExtensiones.SetActive(false);
        if (panelResultados != null) panelResultados.SetActive(false);
        if (anuncio != null) anuncio.gameObject.SetActive(false);

        if (panelResultados != null) panelResultados.transform.SetAsLastSibling();

        List<EditorBuildSettingsScene> escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!escenas.Exists(e => e.path == RutaEscena))
        {
            escenas.Add(new EditorBuildSettingsScene(RutaEscena, true));
            EditorBuildSettings.scenes = escenas.ToArray();
        }
    }

    private static void ConstruirBarraGoogle(Transform safe, out TextMeshProUGUI progreso)
    {
        RectTransform barra = RectHijo(safe, "BarraGoogle");
        barra.anchorMin = new Vector2(0f, 1f);
        barra.anchorMax = new Vector2(1f, 1f);
        barra.pivot = new Vector2(0.5f, 1f);
        barra.anchoredPosition = new Vector2(0f, -96f);
        barra.sizeDelta = new Vector2(0f, 56f);

        Image fondo = barra.gameObject.AddComponent<Image>();
        fondo.color = Color.white;
        fondo.raycastTarget = false;

        TextoSimple(barra, "EnlaceGmail", "Gmail", 16f, TextoGris,
            new Vector2(1f, 0.5f), new Vector2(-250f, 0f), new Vector2(90f, 26f),
            TextAlignmentOptions.Right, FontStyles.Normal, new Vector2(1f, 0.5f));

        TextoSimple(barra, "EnlaceImagenes", "Imágenes", 16f, TextoGris,
            new Vector2(1f, 0.5f), new Vector2(-148f, 0f), new Vector2(100f, 26f),
            TextAlignmentOptions.Right, FontStyles.Normal, new Vector2(1f, 0.5f));

        CrearIcono(barra, "IconoApps", Icono("apps"),
            new Vector2(1f, 0.5f), new Vector2(-66f, 0f), new Vector2(24f, 24f), IconoGris, new Vector2(1f, 0.5f));

        CrearIcono(barra, "Avatar", Icono("account_circle"),
            new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(32f, 32f), Azul, new Vector2(1f, 0.5f));

        progreso = TextoSimple(barra, "Progreso", "0/5 extensiones", 15f, TextoGris,
            new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(220f, 24f),
            TextAlignmentOptions.Left, FontStyles.Bold, new Vector2(0f, 0.5f));
    }

    private static void ConstruirPiePagina(Transform safe)
    {
        RectTransform pie = RectHijo(safe, "PiePagina");
        pie.anchorMin = new Vector2(0f, 0f);
        pie.anchorMax = new Vector2(1f, 0f);
        pie.pivot = new Vector2(0.5f, 0f);
        pie.anchoredPosition = Vector2.zero;
        pie.sizeDelta = new Vector2(0f, 48f);

        Image fondo = pie.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.949f, 0.949f, 0.949f, 1f);
        fondo.raycastTarget = false;

        TextoSimple(pie, "Pais", "España", 15f, TextoGris,
            new Vector2(0f, 0.5f), new Vector2(32f, 0f), new Vector2(200f, 22f),
            TextAlignmentOptions.Left, FontStyles.Normal, new Vector2(0f, 0.5f));

        TextoSimple(pie, "Enlaces", "Privacidad          Términos          Configuración", 15f, TextoGris,
            new Vector2(1f, 0.5f), new Vector2(-32f, 0f), new Vector2(480f, 22f),
            TextAlignmentOptions.Right, FontStyles.Normal, new Vector2(1f, 0.5f));
    }

    private static GameObject ConstruirPanelExtensiones(Transform safe, Sprite panelSprite, Sprite filaSprite,
        Sprite circulo, out List<FilaExtensionUI> filas, out Button botonCerrar)
    {
        RectTransform panel = RectHijo(safe, "PanelExtensiones");
        panel.anchorMin = new Vector2(1f, 1f);
        panel.anchorMax = new Vector2(1f, 1f);
        panel.pivot = new Vector2(1f, 1f);
        panel.anchoredPosition = new Vector2(-24f, -152f);
        panel.sizeDelta = new Vector2(420f, 600f);

        Image fondo = panel.gameObject.AddComponent<Image>();
        fondo.sprite = panelSprite;
        fondo.type = Image.Type.Sliced;
        fondo.color = Color.white;

        TextoSimple(panel, "Titulo", "Extensiones", 24f, TextoOscuro,
            new Vector2(0f, 1f), new Vector2(20f, -16f), new Vector2(300f, 32f),
            TextAlignmentOptions.Left, FontStyles.Bold);

        TextoSimple(panel, "Subtitulo", "5 instaladas", 14f, TextoGris,
            new Vector2(0f, 1f), new Vector2(20f, -48f), new Vector2(300f, 22f),
            TextAlignmentOptions.Left, FontStyles.Normal);

        Image imgCerrar = CrearIcono(panel, "Cerrar", Icono("close"),
            new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(24f, 24f), IconoGris, new Vector2(1f, 1f));
        botonCerrar = Boton(imgCerrar);

        DatosExtension[] datos =
        {
            new DatosExtension { nombre = "AdBlock Plus", descripcion = "Bloquea anuncios y ventanas emergentes.", icono = "block", activa = true },
            new DatosExtension { nombre = "Google Translate", descripcion = "Traduce la página a más de 100 idiomas.", icono = "translate", activa = true },
            new DatosExtension { nombre = "Grammarly", descripcion = "Corrige tu escritura mientras navegas.", icono = "check_circle", activa = true },
            new DatosExtension { nombre = "Dark Reader", descripcion = "Activa el modo oscuro en todas las webs.", icono = "visibility", activa = false },
            new DatosExtension { nombre = "LastPass", descripcion = "Guarda y rellena tus contraseñas.", icono = "lock", activa = true }
        };

        filas = new List<FilaExtensionUI>();

        for (int i = 0; i < datos.Length; i++)
        {
            Image fila = CrearImagen(panel, "Fila" + i, filaSprite, new Color(0.973f, 0.980f, 0.988f, 1f), true);
            RectTransform rt = fila.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(16f, -80f - i * 96f);
            rt.sizeDelta = new Vector2(388f, 88f);
            fila.raycastTarget = true;

            CrearIcono(fila.transform, "Icono", Icono(datos[i].icono),
                new Vector2(0f, 1f), new Vector2(14f, -14f), new Vector2(32f, 32f), IconoGris, new Vector2(0f, 1f));

            TextoSimple(fila.transform, "Nombre", datos[i].nombre, 18f, TextoOscuro,
                new Vector2(0f, 1f), new Vector2(58f, -10f), new Vector2(230f, 24f),
                TextAlignmentOptions.Left, FontStyles.Bold);

            TextMeshProUGUI descripcion = TextoSimple(fila.transform, "Descripcion", datos[i].descripcion, 12f, TextoGris,
                new Vector2(0f, 1f), new Vector2(58f, -34f), new Vector2(250f, 44f),
                TextAlignmentOptions.TopLeft, FontStyles.Normal);
            descripcion.enableWordWrapping = true;

            Image interruptor = CrearIcono(fila.transform, "Interruptor", filaSprite,
                new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(46f, 24f),
                new Color(0.10f, 0.65f, 0.42f, 1f), new Vector2(1f, 0.5f));
            interruptor.type = Image.Type.Sliced;
            interruptor.preserveAspect = false;

            Image perilla = CrearIcono(interruptor.transform, "Perilla", Icono("check_circle"),
                new Vector2(0.5f, 0.5f), new Vector2(11f, 0f), new Vector2(20f, 20f),
                Color.white, new Vector2(0.5f, 0.5f));

            FilaExtensionUI componente = fila.gameObject.GetComponent<FilaExtensionUI>();
            if (componente == null) componente = fila.gameObject.AddComponent<FilaExtensionUI>();
            componente.nombre = datos[i].nombre;
            componente.descripcion = datos[i].descripcion;
            componente.activa = datos[i].activa;
            componente.fondoFila = fila;
            componente.fondoInterruptor = interruptor;
            componente.perilla = perilla.rectTransform;
            componente.AplicarVisual();

            filas.Add(componente);
        }

        TextoSimple(panel, "Gestionar", "Gestionar extensiones", 13f, Azul,
            new Vector2(0f, 0f), new Vector2(20f, 16f), new Vector2(300f, 22f),
            TextAlignmentOptions.Left, FontStyles.Bold, new Vector2(0f, 0f));

        return panel.gameObject;
    }

    private static GameObject ConstruirPanelResultados(Transform contenido, Sprite tarjeta,
        out List<ResultadoBusquedaUI> resultados, out TextMeshProUGUI titulo, out Button cerrar)
    {
        RectTransform panel = RectHijo(contenido, "PanelResultados");
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -424f);
        panel.sizeDelta = new Vector2(800f, 424f);

        Image fondo = panel.gameObject.AddComponent<Image>();
        fondo.sprite = tarjeta;
        fondo.type = Image.Type.Sliced;
        fondo.color = Color.white;
        fondo.raycastTarget = true;

        titulo = TextoSimple(panel, "Titulo", "Resultados para «cómo saltar sobre extensiones»", 15f, TextoGris,
            new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(740f, 22f),
            TextAlignmentOptions.Left, FontStyles.Normal);

        Image imgCerrar = CrearIcono(panel, "Cerrar", Icono("close"),
            new Vector2(1f, 1f), new Vector2(-14f, -12f), new Vector2(22f, 22f), IconoGris, new Vector2(1f, 1f));
        cerrar = Boton(imgCerrar);

        resultados = new List<ResultadoBusquedaUI>();

        for (int i = 0; i < 4; i++)
        {
            Image fila = CrearImagen(panel, "Resultado" + i, tarjeta, Color.white, true);
            RectTransform fr = fila.rectTransform;
            fr.anchorMin = new Vector2(0f, 1f);
            fr.anchorMax = new Vector2(0f, 1f);
            fr.pivot = new Vector2(0f, 1f);
            fr.anchoredPosition = new Vector2(24f, -46f - i * 92f);
            fr.sizeDelta = new Vector2(752f, 86f);
            fila.raycastTarget = true;

            Button boton = Boton(fila);
            EstiloBoton(boton);

            TextMeshProUGUI t = TextoSimple(fila.transform, "Titulo", "", 19f,
                new Color(0.102f, 0.051f, 0.671f, 1f),
                new Vector2(0f, 1f), new Vector2(16f, -6f), new Vector2(720f, 24f),
                TextAlignmentOptions.Left, FontStyles.Normal);
            TextMeshProUGUI u = TextoSimple(fila.transform, "Url", "", 13f,
                new Color(0f, 0.4f, 0.13f, 1f),
                new Vector2(0f, 1f), new Vector2(16f, -30f), new Vector2(720f, 18f),
                TextAlignmentOptions.Left, FontStyles.Normal);
            TextMeshProUGUI r = TextoSimple(fila.transform, "Resumen", "", 13f, TextoGris,
                new Vector2(0f, 1f), new Vector2(16f, -50f), new Vector2(720f, 30f),
                TextAlignmentOptions.TopLeft, FontStyles.Normal);
            r.enableWordWrapping = true;

            resultados.Add(new ResultadoBusquedaUI
            {
                fila = fila.gameObject,
                titulo = t,
                url = u,
                resumen = r,
                boton = boton
            });
        }

        InteraccionTutorial.RellenarResultados(resultados, "cómo saltar sobre extensiones");
        return panel.gameObject;
    }

    private static TMP_InputField ConstruirCampoBusqueda(Transform barra)
    {
        Transform existente = barra.Find("CampoBusqueda");
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject("CampoBusqueda", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(barra, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(58f, 8f);
        rt.offsetMax = new Vector2(-100f, -8f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Image fondo = go.AddComponent<Image>();
        fondo.color = new Color(1f, 1f, 1f, 0f);
        fondo.raycastTarget = true;

        GameObject areaGO = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        RectTransform area = areaGO.GetComponent<RectTransform>();
        area.SetParent(go.transform, false);
        area.anchorMin = Vector2.zero;
        area.anchorMax = Vector2.one;
        area.offsetMin = Vector2.zero;
        area.offsetMax = Vector2.zero;
        area.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI texto = TextoSimple(area, "Text", "", 20f, TextoOscuro,
            Vector2.zero, Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, FontStyles.Normal);
        EstirarTexto(texto);
        texto.overflowMode = TextOverflowModes.Ellipsis;

        TextMeshProUGUI placeholder = TextoSimple(area, "Placeholder", "Buscar en Google o escribe una URL", 20f,
            new Color(0.45f, 0.47f, 0.50f, 1f),
            Vector2.zero, Vector2.zero, Vector2.zero, TextAlignmentOptions.Left, FontStyles.Normal);
        EstirarTexto(placeholder);
        placeholder.overflowMode = TextOverflowModes.Ellipsis;

        TMP_InputField campo = go.AddComponent<TMP_InputField>();
        campo.textViewport = area;
        campo.textComponent = texto;
        campo.placeholder = placeholder;
        campo.targetGraphic = fondo;
        campo.transition = Selectable.Transition.None;
        campo.lineType = TMP_InputField.LineType.SingleLine;
        campo.contentType = TMP_InputField.ContentType.Standard;
        campo.characterLimit = 80;
        campo.caretWidth = 2;
        campo.customCaretColor = true;
        campo.caretColor = Azul;
        campo.selectionColor = new Color(0.102f, 0.451f, 0.910f, 0.25f);
        campo.pointSize = 20f;
        if (texto.font != null) campo.fontAsset = texto.font;
        campo.onFocusSelectAll = true;
        campo.richText = false;

        return campo;
    }

    private static void EstirarTexto(TextMeshProUGUI texto)
    {
        if (texto == null) return;

        RectTransform rt = texto.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0f, 0.5f);
    }

    private static void Tecla(Transform padre, string nombre, string archivo, float x, float y, float tam)
    {
        CrearIcono(padre, nombre, IconoTecla(archivo),
            new Vector2(0f, 1f), new Vector2(x, y), new Vector2(tam, tam), TeclaOscura, new Vector2(0f, 1f));
    }

    private static Sprite Sprite(string ruta)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (s == null) Debug.LogWarning("TUTORIAL_DECOR: falta sprite " + ruta);
        return s;
    }

    private static Sprite Icono(string nombre) => Sprite("Assets/TutorialGoogle/Icons/Material/" + nombre + ".png");
    private static Sprite IconoTecla(string nombre) => Sprite("Assets/TutorialGoogle/Icons/Teclado/" + nombre + ".png");
    private static Sprite IconoMando(string nombre) => Sprite("Assets/TutorialGoogle/Icons/Mando/" + nombre + ".png");

    private static GameObject Buscar(string ruta) => GameObject.Find(ruta);

    private static RectTransform Rect(string ruta)
    {
        GameObject go = Buscar(ruta);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    private static Image Imagen(string ruta)
    {
        GameObject go = Buscar(ruta);
        return go != null ? go.GetComponent<Image>() : null;
    }

    private static TextMeshProUGUI Texto(string ruta)
    {
        GameObject go = Buscar(ruta);
        return go != null ? go.GetComponent<TextMeshProUGUI>() : null;
    }

    private static PlataformaUI Plataforma(GameObject go)
    {
        PlataformaUI p = go.GetComponent<PlataformaUI>();
        if (p == null) p = go.AddComponent<PlataformaUI>();
        return p;
    }

    private static Button Boton(Image img)
    {
        if (img == null) return null;

        img.raycastTarget = true;

        Button b = img.GetComponent<Button>();
        if (b == null) b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        return b;
    }

    private static void EstiloBoton(Button b)
    {
        if (b == null) return;

        ColorBlock colores = b.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(0.92f, 0.95f, 1f, 1f);
        colores.pressedColor = new Color(0.85f, 0.90f, 1f, 1f);
        colores.selectedColor = Color.white;
        colores.fadeDuration = 0.08f;
        b.colors = colores;
    }

    private static RectTransform RectHijo(Transform padre, string nombre)
    {
        if (padre == null) return null;

        Transform existente = padre.Find(nombre);
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject(nombre, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        return rt;
    }

    private static Image CrearImagen(Transform padre, string nombre, Sprite sprite, Color color, bool sliced)
    {
        RectTransform rt = RectHijo(padre, nombre);
        if (rt == null) return null;

        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        if (sliced) img.type = Image.Type.Sliced;
        return img;
    }

    private static void Estirar(RectTransform rt)
    {
        if (rt == null) return;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = Vector2.zero;
    }

    private static void AnclarArriba(RectTransform rt, float desdeArriba, float ancho, float alto)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -desdeArriba);
        rt.sizeDelta = new Vector2(ancho, alto);
    }

    private static void ColocarCentrado(RectTransform rt, float desdeCentroX, float y, float ancho, float alto)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(desdeCentroX, y);
        rt.sizeDelta = new Vector2(ancho, alto);
    }

    private static Image CrearIcono(Transform padre, string nombre, Sprite sprite,
        Vector2 anclaje, Vector2 posicion, Vector2 tamano, Color color, Vector2 pivot)
    {
        if (padre == null) return null;

        Transform existente = padre.Find(nombre);
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = anclaje;
        rt.anchorMax = anclaje;
        rt.pivot = pivot;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI TextoSimple(Transform padre, string nombre, string contenido, float tamano,
        Color color, Vector2 anclaje, Vector2 posicion, Vector2 tamanoRect, TextAlignmentOptions alineacion,
        FontStyles estilo, Vector2? pivote = null)
    {
        if (padre == null) return null;

        Transform existente = padre.Find(nombre);
        if (existente != null) Object.DestroyImmediate(existente.gameObject);

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = anclaje;
        rt.anchorMax = anclaje;
        rt.pivot = pivote ?? new Vector2(0f, 1f);
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

    private static TextMeshProUGUI TextoCentrado(Transform padre, string nombre, string contenido, float tamano, Color color)
    {
        TextMeshProUGUI texto = TextoSimple(padre, nombre, contenido, tamano, color,
            Vector2.zero, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, FontStyles.Normal);

        if (texto == null) return null;

        RectTransform rt = texto.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(8f, 0f);
        rt.offsetMax = new Vector2(-8f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        return texto;
    }
}
