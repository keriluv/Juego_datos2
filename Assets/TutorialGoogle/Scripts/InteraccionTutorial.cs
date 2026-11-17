using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ResultadoBusquedaUI
{
    public GameObject fila;
    public TextMeshProUGUI titulo;
    public TextMeshProUGUI url;
    public TextMeshProUGUI resumen;
    public Button boton;
}

public class InteraccionTutorial : MonoBehaviour
{
    [Header("Referencias")]
    public JugadorTutorial jugador;
    public TextMeshProUGUI dialogo;
    public TextMeshProUGUI textoProgreso;
    public GameObject clippy;
    public GameObject panelExtensiones;
    public GameObject panelControles;
    public GameObject anuncio;
    public Image fondoPagina;

    [Header("Botones")]
    public Button botonExtensiones;
    public Button botonCerrar;
    public Button botonBuscar;
    public Button botonSuerte;

    [Header("Extensiones")]
    public List<FilaExtensionUI> filas = new List<FilaExtensionUI>();
    public List<PlataformaUI> extensiones = new List<PlataformaUI>();
    public List<Button> accesosBotones = new List<Button>();

    [Header("Buscador")]
    public TMP_InputField campoBusqueda;
    public GameObject panelResultados;
    public TextMeshProUGUI tituloResultados;
    public Button botonCerrarResultados;
    public TextMeshProUGUI textoUrl;
    public List<ResultadoBusquedaUI> resultados = new List<ResultadoBusquedaUI>();

    private readonly HashSet<int> visitadas = new HashSet<int>();
    private bool panelAbierto;
    private bool transicionIniciada;
    private float tiempoMensaje;
    private string mensajeBase = "";
    private Color colorFondoClaro = Color.white;
    private Color colorFondoOscuro = new Color(0.12f, 0.13f, 0.15f, 1f);

    private void Start()
    {
        if (dialogo != null) mensajeBase = dialogo.text;

        if (panelExtensiones != null) panelExtensiones.SetActive(false);
        if (panelResultados != null) panelResultados.SetActive(false);
        panelAbierto = false;

        if (botonExtensiones != null) botonExtensiones.onClick.AddListener(AlternarPanel);
        if (botonCerrar != null) botonCerrar.onClick.AddListener(CerrarPanel);
        if (botonCerrarResultados != null) botonCerrarResultados.onClick.AddListener(CerrarResultados);

        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarCampo);

        if (botonSuerte != null)
            botonSuerte.onClick.AddListener(Suerte);

        for (int i = 0; i < resultados.Count; i++)
        {
            int indice = i;
            if (resultados[i] != null && resultados[i].boton != null)
                resultados[i].boton.onClick.AddListener(() => AbrirResultado(indice));
        }

        for (int i = 0; i < accesosBotones.Count; i++)
        {
            int indice = i;
            if (accesosBotones[i] != null)
                accesosBotones[i].onClick.AddListener(() => IrAExtension(indice));
        }

        if (jugador != null) jugador.alAterrizar += Aterrizar;
        if (jugador != null) jugador.alGolpear += Golpear;

        for (int i = 0; i < filas.Count; i++)
            if (filas[i] != null) filas[i].AplicarVisual();

        AplicarAdBlock();
        ActualizarProgreso();
    }

    private void Update()
    {
        if (tiempoMensaje <= 0f) return;

        tiempoMensaje -= Time.unscaledDeltaTime;
        if (tiempoMensaje <= 0f && dialogo != null)
            dialogo.text = mensajeBase;
    }

    private void Mensaje(string texto)
    {
        if (dialogo == null) return;
        dialogo.text = texto;
        tiempoMensaje = 4.5f;
    }

    private void ActualizarProgreso()
    {
        if (textoProgreso != null)
            textoProgreso.text = visitadas.Count + "/5 extensiones";
    }

    public void BuscarCampo()
    {
        Buscar(campoBusqueda != null ? campoBusqueda.text : "");
    }

    public void Buscar(string consulta)
    {
        consulta = (consulta ?? "").Trim();
        if (consulta.Length == 0) consulta = "extensiones de Chrome";
        if (consulta.Length > 60) consulta = consulta.Substring(0, 60) + "…";

        if (panelAbierto) AlternarPanel();

        if (tituloResultados != null)
            tituloResultados.text = "Resultados para «" + consulta + "»";

        if (textoUrl != null)
            textoUrl.text = "google.com/search?q=" + consulta.Replace(' ', '+');

        RellenarResultados(resultados, consulta);

        MostrarResultados(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Mensaje("Resultados de prueba para «" + consulta + "».\nPulsa uno para abrirlo.");
    }

    public static void RellenarResultados(List<ResultadoBusquedaUI> lista, string consulta)
    {
        if (lista == null) return;

        string compacta = consulta.Replace(' ', '_');

        string[] titulos =
        {
            consulta + " - Buscar en Google",
            consulta + " - Wikipedia, la enciclopedia libre",
            consulta + " - YouTube",
            consulta + " - Google Maps"
        };

        string[] urls =
        {
            "www.google.com › search",
            "es.wikipedia.org › wiki › " + compacta,
            "www.youtube.com › results",
            "www.google.com › maps › search"
        };

        string[] resumenes =
        {
            "Resultados, imágenes y respuestas rápidas sobre «" + consulta + "».",
            "Artículo de la enciclopedia libre sobre «" + consulta + "». Historia, usos y curiosidades.",
            "Vídeos, tutoriales y trucos relacionados con «" + consulta + "».",
            "Mapas, rutas y lugares cerca de ti relacionados con «" + consulta + "»."
        };

        for (int i = 0; i < lista.Count; i++)
        {
            ResultadoBusquedaUI r = lista[i];
            if (r == null) continue;

            if (r.titulo != null) r.titulo.text = titulos[i % titulos.Length];
            if (r.url != null) r.url.text = urls[i % urls.Length];
            if (r.resumen != null) r.resumen.text = resumenes[i % resumenes.Length];
        }
    }

    public void CerrarResultados()
    {
        MostrarResultados(false);
        if (textoUrl != null) textoUrl.text = "google.com";

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (!panelAbierto && clippy != null) clippy.SetActive(true);

        Mensaje(mensajeBase);
    }

    private void MostrarResultados(bool mostrar)
    {
        if (panelResultados != null) panelResultados.SetActive(mostrar);
        if (panelControles != null) panelControles.SetActive(!mostrar);
    }

    private void AbrirResultado(int indice)
    {
        if (indice < 0 || indice >= resultados.Count) return;

        ResultadoBusquedaUI r = resultados[indice];
        if (r == null) return;

        if (textoUrl != null && r.url != null) textoUrl.text = r.url.text;
        MostrarResultados(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Mensaje("Abriendo «" + (r.titulo != null ? r.titulo.text : "resultado") + "»…");
    }

    private void Aterrizar(PlataformaUI plataforma)
    {
        if (plataforma == null) return;

        if (!plataforma.esExtension)
        {
            Mensaje("Estás sobre " + plataforma.etiqueta + ".\nBusca una extensión y salta encima.");
            return;
        }

        RegistrarExtension(plataforma.indiceExtension,
            "¡Bien! " + plataforma.etiqueta + ".\nLlevas " + (visitadas.Count + 1) + " de 5.");
    }

    private void Golpear(PlataformaUI plataforma)
    {
        if (plataforma == null) return;

        StartCoroutine(PulsoGolpe(plataforma.RectPlataforma));

        if (!plataforma.esExtension)
        {
            Mensaje("Golpeaste " + plataforma.etiqueta + ".");
            return;
        }

        int indice = plataforma.indiceExtension;
        if (visitadas.Contains(indice))
        {
            Mensaje("Ya golpeaste " + plataforma.etiqueta + ".");
            return;
        }

        RegistrarExtension(indice,
            "¡Golpeaste " + plataforma.etiqueta + "!\nLlevas " + (visitadas.Count + 1) + " de 5.");
    }

    private void RegistrarExtension(int indice, string mensajeProgreso)
    {
        if (!visitadas.Add(indice)) return;

        ActualizarProgreso();
        if (visitadas.Count >= 5)
        {
            mensajeBase = "¡Ganaste! Ya conoces las 5 extensiones.";
            Mensaje(mensajeBase);
            if (!transicionIniciada) StartCoroutine(AnimacionVictoria());
            return;
        }

        Mensaje(mensajeProgreso);
    }

    private IEnumerator AnimacionVictoria()
    {
        transicionIniciada = true;
        Canvas canvas = dialogo != null ? dialogo.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Debug.LogError("No se encontró el Canvas para mostrar la victoria del tutorial.", this);
            yield return new WaitForSecondsRealtime(1.5f);
            IrALevelSelector();
            yield break;
        }

        GameObject panel = new GameObject("AnimacionVictoria", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(canvas.transform, false);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.SetAsLastSibling();

        CanvasGroup grupo = panel.GetComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = true;
        Image fondo = panel.GetComponent<Image>();
        fondo.color = new Color(0.055f, 0.09f, 0.18f, 0.88f);

        GameObject mensaje = new GameObject("MensajeVictoria", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform mensajeRect = mensaje.GetComponent<RectTransform>();
        mensajeRect.SetParent(panel.transform, false);
        mensajeRect.anchorMin = Vector2.zero;
        mensajeRect.anchorMax = Vector2.one;
        mensajeRect.offsetMin = new Vector2(70f, 70f);
        mensajeRect.offsetMax = new Vector2(-70f, -70f);

        TextMeshProUGUI texto = mensaje.GetComponent<TextMeshProUGUI>();
        texto.font = dialogo != null ? dialogo.font : TMP_Settings.defaultFontAsset;
        texto.text = "¡GANASTE!\n<size=55%>Volviendo a Level Selector...</size>";
        texto.fontSize = 68f;
        texto.fontStyle = FontStyles.Bold;
        texto.color = Color.white;
        texto.alignment = TextAlignmentOptions.Center;
        texto.raycastTarget = false;

        float duracion = 0.65f;
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracion);
            grupo.alpha = Mathf.SmoothStep(0f, 1f, progreso);
            mensajeRect.localScale = Vector3.one * Mathf.Lerp(0.65f, 1.08f, progreso);
            yield return null;
        }

        mensajeRect.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(1.1f);
        IrALevelSelector();
    }

    private void IrALevelSelector()
    {
        GameProgressionManager.Asegurar().RegisterLevelCompleted("TutorialGoogle");

        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.IrAEscena("LevelSelector");
        else
            SceneManager.LoadScene("LevelSelector");
    }

    private IEnumerator PulsoGolpe(RectTransform objetivo)
    {
        if (objetivo == null) yield break;

        Vector3 escalaBase = objetivo.localScale;
        float duracion = 0.18f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(tiempo / duracion) * Mathf.PI);
            objetivo.localScale = escalaBase * (1f + 0.08f * k);
            yield return null;
        }

        objetivo.localScale = escalaBase;
    }

    public void AlternarPanel()
    {
        panelAbierto = !panelAbierto;

        if (panelAbierto)
            MostrarResultados(false);

        if (panelExtensiones != null) panelExtensiones.SetActive(panelAbierto);
        if (clippy != null) clippy.SetActive(!panelAbierto);

        if (panelAbierto)
            Mensaje("Tus extensiones. Pulsa una para activarla o desactivarla.");
    }

    public void CerrarPanel()
    {
        if (!panelAbierto) return;
        AlternarPanel();
    }

    public void AlternarExtension(FilaExtensionUI fila)
    {
        if (fila == null) return;

        if (fila.nombre == "AdBlock Plus") AplicarAdBlock();

        if (fila.nombre == "Dark Reader" && fondoPagina != null)
            fondoPagina.color = fila.activa ? colorFondoOscuro : colorFondoClaro;

        Mensaje(fila.activa
            ? fila.nombre + " activado.\n" + fila.descripcion
            : fila.nombre + " desactivado.");
    }

    private void AplicarAdBlock()
    {
        if (anuncio == null) return;

        bool bloqueado = true;
        for (int i = 0; i < filas.Count; i++)
        {
            if (filas[i] != null && filas[i].nombre == "AdBlock Plus")
            {
                bloqueado = filas[i].activa;
                break;
            }
        }

        anuncio.SetActive(!bloqueado);
    }

    private void Suerte()
    {
        if (extensiones.Count == 0) return;
        IrAExtension(Random.Range(0, extensiones.Count));
    }

    private void IrAExtension(int indice)
    {
        if (jugador == null) return;
        if (indice < 0 || indice >= extensiones.Count) return;

        PlataformaUI plataforma = extensiones[indice];
        if (plataforma == null) return;

        if (panelAbierto) AlternarPanel();

        jugador.IrA(plataforma.RectPlataforma);
        Mensaje("¡A " + plataforma.etiqueta + "!");
    }
}
