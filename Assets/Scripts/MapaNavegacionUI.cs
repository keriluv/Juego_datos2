using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Guarda qué niveles ya se completaron (persiste entre escenas y sesiones)
public static class ProgresoNiveles
{
    private const string Prefijo = "completado_";

    public static void Completar(string escena)
    {
        PlayerPrefs.SetInt(Prefijo + escena, 1);
        PlayerPrefs.Save();
    }

    public static bool EstaCompletado(string escena)
    {
        return PlayerPrefs.GetInt(Prefijo + escena, 0) == 1;
    }

    public static void Reiniciar(string escena)
    {
        PlayerPrefs.DeleteKey(Prefijo + escena);
    }
}

[Serializable]
public class NivelConfig
{
    public int id;
    public string nombre;
    public string escena;       
    public Vector2 posicion;     
    public bool inicial;
    public bool falso;           
}

[Serializable]
public class ConexionConfig
{
    public int origen;
    public int destino;
    public float peso = 1f;      
    public bool bidireccional;
}

public class MapaNavegacionUI : MonoBehaviour
{
    private enum Modo { Navegar, Ruta, Completar }

    private class AristaVisual
    {
        public ConexionConfig conexion;
        public List<Image> partes = new List<Image>();
    }

    [Header("Grafo (editable en el Inspector)")]
    public List<NivelConfig> niveles = new List<NivelConfig>();
    public List<ConexionConfig> conexiones = new List<ConexionConfig>();

    [Header("Colores")]
    public Color colorFondo = new Color(0.08f, 0.09f, 0.13f);
    public Color colorBloqueado = new Color(0.35f, 0.35f, 0.38f);
    public Color colorDisponible = new Color(0.20f, 0.55f, 0.85f);
    public Color colorCompletado = new Color(0.25f, 0.70f, 0.35f);
    public Color colorActual = new Color(0.85f, 0.55f, 0.10f);
    public Color colorArista = new Color(0.65f, 0.65f, 0.70f);
    public Color colorRuta = new Color(0.95f, 0.35f, 0.35f);

    private GrafoNiveles grafo;
    private readonly Dictionary<int, NivelConfig> config = new Dictionary<int, NivelConfig>();
    private readonly Dictionary<int, Image> imagenNodo = new Dictionary<int, Image>();
    private readonly Dictionary<int, Text> textoNodo = new Dictionary<int, Text>();
    private readonly List<AristaVisual> aristasVisuales = new List<AristaVisual>();

    private RectTransform raiz;
    private Font fuente;
    private Text textoInfo;
    private Modo modo = Modo.Navegar;
    private int idActual;

    private const float AnchoNodo = 140f;
    private const float AltoNodo = 60f;

    private void Reset()
    {
        CargarEjemplo();
    }

    private void Awake()
    {
        if (niveles.Count == 0) CargarEjemplo();

        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ConstruirGrafo();
        CrearInterfaz();
        DeterminarNivelActual();
        Refrescar();
        MostrarInfo("Modo navegar: haz clic en un nivel disponible para jugarlo.");
    }

    private void CargarEjemplo()
    {
        niveles = new List<NivelConfig>
        {
            new NivelConfig { id = 1, nombre = "MainMenu", escena = "MainMenu", posicion = new Vector2(-290, -50), inicial = true },
            new NivelConfig { id = 2, nombre = "LevelSelector", escena = "LevelSelector", posicion = new Vector2(110, -50) },
            new NivelConfig { id = 3, nombre = "Tutorial", escena = "Tutorial", posicion = new Vector2(-130, 100) },
            new NivelConfig { id = 4, nombre = "LevelYoutube", escena = "LevelYoutube", posicion = new Vector2(110, 120) },
            new NivelConfig { id = 5, nombre = "TikTok", escena = "", posicion = new Vector2(330, 120), falso = true },
            new NivelConfig { id = 6, nombre = "Instagram", escena = "", posicion = new Vector2(330, -50), falso = true },
            new NivelConfig { id = 7, nombre = "WhatsApp", escena = "", posicion = new Vector2(540, 110), falso = true },
        };

        conexiones = new List<ConexionConfig>
        {
            new ConexionConfig { origen = 1, destino = 2, peso = 1f, bidireccional = true },
            new ConexionConfig { origen = 1, destino = 3, peso = 1f },
            new ConexionConfig { origen = 3, destino = 4, peso = 2f },
            new ConexionConfig { origen = 4, destino = 5, peso = 3f },
            new ConexionConfig { origen = 4, destino = 6, peso = 2f },
            new ConexionConfig { origen = 5, destino = 7, peso = 1f },
            new ConexionConfig { origen = 6, destino = 7, peso = 3f },
            new ConexionConfig { origen = 2, destino = 3, peso = 2f },
            new ConexionConfig { origen = 2, destino = 4, peso = 5f },
            new ConexionConfig { origen = 2, destino = 5, peso = 8f },
            new ConexionConfig { origen = 2, destino = 6, peso = 7f },
            new ConexionConfig { origen = 2, destino = 7, peso = 9f },
        };
    }


    private void ConstruirGrafo()
    {
        config.Clear();
        grafo = new GrafoNiveles();

        foreach (var n in niveles)
        {
            config[n.id] = n;
            grafo.AgregarNivel(new Nivel(n.id, n.nombre, n.escena));
        }

        foreach (var c in conexiones)
        {
            if (!config.ContainsKey(c.origen) || !config.ContainsKey(c.destino))
            {
                Debug.LogWarning("Conexión ignorada: id inexistente (" + c.origen + " -> " + c.destino + ")");
                continue;
            }
            grafo.AgregarCamino(c.origen, c.destino, c.peso, c.bidireccional);
        }
    }

    private void DeterminarNivelActual()
    {
        string activa = SceneManager.GetActiveScene().name;
        idActual = niveles[0].id;

        foreach (var n in niveles)
        {
            if (n.inicial) { idActual = n.id; break; }
        }
        foreach (var n in niveles)
        {
            if (n.escena == activa) { idActual = n.id; break; }
        }
    }

    private string Nombre(int id)
    {
        return config[id].nombre;
    }

    private string Clave(NivelConfig n)
    {
        return string.IsNullOrEmpty(n.escena) ? n.nombre : n.escena;
    }


    private void CrearInterfaz()
    {
        var canvasGO = new GameObject("CanvasMapa", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        raiz = CrearRect("Fondo", canvasGO.transform);
        Estirar(raiz);
        raiz.gameObject.AddComponent<Image>().color = colorFondo;

        
        foreach (var c in conexiones) DibujarArista(c);
        foreach (var n in niveles) DibujarNodo(n);
        CrearHUD();
    }

    private void DibujarArista(ConexionConfig c)
    {
        NivelConfig a, b;
        if (!config.TryGetValue(c.origen, out a) || !config.TryGetValue(c.destino, out b)) return;

        Vector2 p1 = a.posicion;
        Vector2 p2 = b.posicion;
        Vector2 dir = p2 - p1;
        float largo = dir.magnitude;
        if (largo < 1f) return;
        Vector2 u = dir / largo;

        var visual = new AristaVisual { conexion = c };

        var linea = CrearRect("Arista", raiz);
        linea.pivot = new Vector2(0f, 0.5f);
        linea.sizeDelta = new Vector2(largo, 4f);
        linea.anchoredPosition = p1;
        linea.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        var img = linea.gameObject.AddComponent<Image>();
        img.color = colorArista;
        img.raycastTarget = false;
        visual.partes.Add(img);

       
        float margen = DistanciaAlBorde(u) + 10f;
        visual.partes.Add(CrearPunta(p2 - u * margen));
        if (c.bidireccional) visual.partes.Add(CrearPunta(p1 + u * margen));

        var etiqueta = CrearTexto("Peso", raiz, c.peso.ToString("0.#"), 18, TextAnchor.MiddleCenter);
        Vector2 perp = new Vector2(-u.y, u.x);
        etiqueta.rectTransform.sizeDelta = new Vector2(40f, 24f);
        etiqueta.rectTransform.anchoredPosition = (p1 + p2) / 2f + perp * 16f;

        aristasVisuales.Add(visual);
    }

    private Image CrearPunta(Vector2 posicion)
    {
        var r = CrearRect("Punta", raiz);
        r.sizeDelta = new Vector2(14f, 14f);
        r.anchoredPosition = posicion;
        r.localRotation = Quaternion.Euler(0f, 0f, 45f);
        var img = r.gameObject.AddComponent<Image>();
        img.color = colorArista;
        img.raycastTarget = false;
        return img;
    }

    private float DistanciaAlBorde(Vector2 u)
    {
        float tx = Mathf.Abs(u.x) < 0.0001f ? float.MaxValue : (AnchoNodo / 2f) / Mathf.Abs(u.x);
        float ty = Mathf.Abs(u.y) < 0.0001f ? float.MaxValue : (AltoNodo / 2f) / Mathf.Abs(u.y);
        return Mathf.Min(tx, ty);
    }

    private void DibujarNodo(NivelConfig n)
    {
        var r = CrearRect("Nodo_" + n.nombre, raiz);
        r.sizeDelta = new Vector2(AnchoNodo, AltoNodo);
        r.anchoredPosition = n.posicion;

        var img = r.gameObject.AddComponent<Image>();
        var boton = r.gameObject.AddComponent<Button>();
        boton.targetGraphic = img;

        int id = n.id;
        boton.onClick.AddListener(() => AlHacerClicNodo(id));

        var texto = CrearTexto("Etiqueta", r, n.nombre, 18, TextAnchor.MiddleCenter);
        Estirar(texto.rectTransform);

        imagenNodo[id] = img;
        textoNodo[id] = texto;
    }

    private void CrearHUD()
    {
        var titulo = CrearTexto("Titulo", raiz, "Mapa de navegación", 30, TextAnchor.MiddleCenter);
        titulo.rectTransform.anchorMin = titulo.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        titulo.rectTransform.sizeDelta = new Vector2(600f, 50f);
        titulo.rectTransform.anchoredPosition = new Vector2(0f, -40f);

        textoInfo = CrearTexto("Info", raiz, "", 22, TextAnchor.UpperCenter);
        textoInfo.rectTransform.anchorMin = textoInfo.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        textoInfo.rectTransform.sizeDelta = new Vector2(900f, 110f);
        textoInfo.rectTransform.anchoredPosition = new Vector2(100f, 80f);

        CrearBotonHUD("Navegar", 0, () => CambiarModo(Modo.Navegar));
        CrearBotonHUD("Ruta más corta", 1, () => CambiarModo(Modo.Ruta));
        CrearBotonHUD("Completar (demo)", 2, () => CambiarModo(Modo.Completar));
        CrearBotonHUD("Recorrido BFS", 3, () => MostrarRecorrido(true));
        CrearBotonHUD("Recorrido DFS", 4, () => MostrarRecorrido(false));
        CrearBotonHUD("Reiniciar progreso", 5, ReiniciarProgreso);
    }

    private void CrearBotonHUD(string texto, int indice, UnityEngine.Events.UnityAction accion)
    {
        var r = CrearRect("Boton_" + texto, raiz);
        r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
        r.pivot = new Vector2(0f, 1f);
        r.sizeDelta = new Vector2(200f, 44f);
        r.anchoredPosition = new Vector2(20f, -100f - indice * 54f);

        var img = r.gameObject.AddComponent<Image>();
        img.color = new Color(0.18f, 0.20f, 0.27f);
        var boton = r.gameObject.AddComponent<Button>();
        boton.targetGraphic = img;
        boton.onClick.AddListener(accion);

        var t = CrearTexto("Texto", r, texto, 18, TextAnchor.MiddleCenter);
        Estirar(t.rectTransform);
    }

   

    private void CambiarModo(Modo nuevo)
    {
        modo = nuevo;
        LimpiarRuta();

        switch (modo)
        {
            case Modo.Navegar:
                MostrarInfo("Modo navegar: haz clic en un nivel disponible para jugarlo.");
                break;
            case Modo.Ruta:
                MostrarInfo("Modo ruta: haz clic en un nivel destino para ver el camino más corto desde " + Nombre(idActual) + ".");
                break;
            case Modo.Completar:
                MostrarInfo("Modo demo: haz clic en un nivel disponible para marcarlo como completado.");
                break;
        }
    }

    private void AlHacerClicNodo(int id)
    {
        var nivel = grafo.Obtener(id);

        switch (modo)
        {
            case Modo.Navegar:
            {
                if (id == idActual)
                {
                    MostrarInfo("Ya estás en " + nivel.nombre + ".");
                }
                else if (!nivel.desbloqueado)
                {
                    MostrarInfo(nivel.nombre + " está bloqueado: completa un nivel que conduzca a él.");
                }
                else if (config[id].falso)
                {
                    MostrarInfo(nivel.nombre + " es un nivel de relleno: todavía no tiene escena.");
                }
                else if (!Application.CanStreamedLevelBeLoaded(nivel.escena))
                {
                    MostrarInfo("La escena " + nivel.escena + " no está en Build Profiles > Scene List, o el nombre no coincide.");
                }
                else
                {
                    SceneManager.LoadScene(nivel.escena);
                }
                break;
            }
            case Modo.Ruta:
                MostrarRuta(id);
                break;
            case Modo.Completar:
            {
                if (!nivel.desbloqueado)
                {
                    MostrarInfo(nivel.nombre + " está bloqueado, no se puede completar todavía.");
                    break;
                }
                ProgresoNiveles.Completar(Clave(config[id]));
                Refrescar();
                MostrarInfo("Completado: " + nivel.nombre + ". Se desbloquearon sus niveles vecinos.");
                break;
            }
        }
    }

    private void MostrarRuta(int destino)
    {
        LimpiarRuta();

        float costo;
        var ruta = grafo.CaminoMasCorto(idActual, destino, out costo);
        if (ruta.Count == 0)
        {
            MostrarInfo("No hay ruta desde " + Nombre(idActual) + " hasta " + Nombre(destino) + ".");
            return;
        }

        var nombres = new List<string>();
        foreach (var id in ruta) nombres.Add(Nombre(id));
        for (int i = 0; i < ruta.Count - 1; i++) ResaltarArista(ruta[i], ruta[i + 1]);

        MostrarInfo("Ruta más corta (Dijkstra): " + string.Join(" -> ", nombres) + "\nCosto total: " + costo);
    }

    private void MostrarRecorrido(bool anchura)
    {
        LimpiarRuta();

        var orden = anchura ? grafo.BFS(idActual) : grafo.DFS(idActual);
        var nombres = new List<string>();
        foreach (var id in orden) nombres.Add(Nombre(id));

        string tipo = anchura ? "Recorrido en anchura (BFS)" : "Recorrido en profundidad (DFS)";
        MostrarInfo(tipo + " desde " + Nombre(idActual) + ":\n" + string.Join(" -> ", nombres));
    }

    private void ReiniciarProgreso()
    {
        foreach (var n in niveles) ProgresoNiveles.Reiniciar(Clave(n));
        LimpiarRuta();
        Refrescar();
        MostrarInfo("Progreso reiniciado.");
    }

  
    private void Refrescar()
    {
       
        foreach (var n in niveles) grafo.Obtener(n.id).desbloqueado = n.inicial;
        foreach (var n in niveles)
        {
            if (n.inicial || ProgresoNiveles.EstaCompletado(Clave(n)))
                grafo.CompletarNivel(n.id);
        }

        foreach (var n in niveles)
        {
            var nivel = grafo.Obtener(n.id);
            Color color;
            string estado = "";

            if (n.id == idActual) color = colorActual;
            else if (ProgresoNiveles.EstaCompletado(Clave(n))) color = colorCompletado;
            else if (nivel.desbloqueado) color = colorDisponible;
            else { color = colorBloqueado; estado = "\n(bloqueado)"; }
            if (n.falso) estado = "\n(sin escena)";

            imagenNodo[n.id].color = color;
            textoNodo[n.id].text = n.nombre + estado;
        }
    }

    private void ResaltarArista(int a, int b)
    {
        foreach (var v in aristasVisuales)
        {
            var c = v.conexion;
            bool coincide = (c.origen == a && c.destino == b) ||
                            (c.bidireccional && c.origen == b && c.destino == a);
            if (!coincide) continue;
            foreach (var parte in v.partes) parte.color = colorRuta;
        }
    }

    private void LimpiarRuta()
    {
        foreach (var v in aristasVisuales)
            foreach (var parte in v.partes) parte.color = colorArista;
    }

    private void MostrarInfo(string mensaje)
    {
        textoInfo.text = mensaje;
    }


    private RectTransform CrearRect(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    private void Estirar(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    private Text CrearTexto(string nombre, Transform padre, string contenido, int tamano, TextAnchor ancla)
    {
        var rt = CrearRect(nombre, padre);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = fuente;
        t.text = contenido;
        t.fontSize = tamano;
        t.alignment = ancla;
        t.color = Color.white;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}