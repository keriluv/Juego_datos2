using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class NivelConfig
{
    public int id;
    public string nombre;
    public string escena;
    public Vector2 posicion;
    public bool inicial;
    public bool falso;
    public Color acento = Color.clear;
    [Tooltip("Formats: level:Scene, objective:Id, milestone:Id, keys:Scene:N, or bare scene name")]
    public string[] requisitosProgreso;
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
    [SerializeField] private RectTransform panelMapa;

    private class NodoVisual
    {
        public RectTransform rect;
        public Image borde;
        public Image barra;
        public Text nombre;
        public Text estado;
    }

    private class AristaVisual
    {
        public ConexionConfig conexion;
        public Vector2 p1, p2;
        public RectTransform linea;
        public List<Image> partes = new List<Image>();
        public Image pastilla;
        public Text textoPeso;
    }

    [Header("Grafo (editable en el Inspector)")]
    public List<NivelConfig> niveles = new List<NivelConfig>();
    public List<ConexionConfig> conexiones = new List<ConexionConfig>();

    [Header("Colores")]
    public Color colorFondoArriba = new Color(0.11f, 0.13f, 0.22f);
    public Color colorFondoAbajo = new Color(0.04f, 0.05f, 0.09f);
    public Color colorBloqueado = new Color(0.45f, 0.48f, 0.56f);
    public Color colorDisponible = new Color(0.27f, 0.62f, 0.96f);
    public Color colorCompletado = new Color(0.30f, 0.78f, 0.45f);
    public Color colorActual = new Color(0.98f, 0.70f, 0.20f);
    public Color colorArista = new Color(0.42f, 0.47f, 0.60f);
    public Color colorRuta = new Color(1.00f, 0.38f, 0.42f);

    private readonly Color colorTarjeta = new Color(0.12f, 0.14f, 0.21f);
    private readonly Color colorPastilla = new Color(0.08f, 0.09f, 0.15f);
    private readonly Color colorBordePastilla = new Color(0.62f, 0.67f, 0.82f);
    private readonly Color colorBoton = new Color(0.16f, 0.19f, 0.28f);
    private readonly Color colorTextoSuave = new Color(0.78f, 0.80f, 0.87f);

    private const float AnchoNodo = 140f;
    private const float AltoNodo = 60f;
    private const float GrosorArista = 3f;
    private const float GrosorRuta = 5f;
    private const string MensajeNavegar = "Modo navegar: haz clic en un nivel disponible para jugarlo.\nEl número de cada línea es el peso (costo) de ese camino.";

    private GrafoNiveles grafo;
    private readonly Dictionary<int, NivelConfig> config = new Dictionary<int, NivelConfig>();
    private readonly Dictionary<int, NodoVisual> nodos = new Dictionary<int, NodoVisual>();
    private readonly List<AristaVisual> aristasVisuales = new List<AristaVisual>();
    private readonly Dictionary<int, Sprite> spritesRedondeados = new Dictionary<int, Sprite>();
    private readonly Image[] botonesModo = new Image[3];

    private Sprite spriteFlecha;
    private RectTransform raiz;
    private Font fuente;
    private Text textoInfo;
    private Modo modo = Modo.Navegar;
    private int idActual;
    private int idHover = -1;

    private void Reset()
    {
        CargarEjemplo();
    }

    private void Awake()
    {
        // Mantiene el acceso historico al mapa, pero abre la escena 3D separada.
        if (SceneManager.GetActiveScene().name == "MapaNiveles")
        {
            if (Application.CanStreamedLevelBeLoaded("MapaNiveles3D"))
                SceneManager.LoadScene("MapaNiveles3D");
            else
                Debug.LogError("MapaNiveles3D no esta habilitada en la lista de escenas del proyecto.");
            return;
        }

        if (niveles.Count == 0) CargarEjemplo();

        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ConstruirGrafo();
        CrearInterfaz();
        DeterminarNivelActual();
        Refrescar();
       // MostrarInfo(MensajeNavegar);
    }

    private void Update()
    {
        float t = Time.unscaledTime;
        foreach (var kv in nodos)
        {
            float objetivo = 1f;
            if (kv.Key == idHover) objetivo = 1.07f;
            else if (kv.Key == idActual) objetivo = 1f + 0.025f * Mathf.Sin(t * 3f);

            float actual = kv.Value.rect.localScale.x;
            float nuevo = Mathf.Lerp(actual, objetivo, 12f * Time.unscaledDeltaTime);
            kv.Value.rect.localScale = new Vector3(nuevo, nuevo, 1f);
        }
    }

    private void CargarEjemplo()
    {
        // Only scenes present in Editor Build Settings (skips MapaNiveles3D + SampleScene).
        niveles = new List<NivelConfig>
        {
            new NivelConfig { id = 1, nombre = "MainMenu", escena = "MainMenu", posicion = new Vector2(-320, 0), inicial = true, acento = new Color(1f, 0.68f, 0.24f) },
            new NivelConfig { id = 2, nombre = "LevelSelector", escena = "LevelSelector", posicion = new Vector2(0, 0), acento = new Color(0.18f, 0.74f, 0.90f) },
            new NivelConfig { id = 3, nombre = "TutorialGoogle", escena = "TutorialGoogle", posicion = new Vector2(-200, 160), acento = new Color(0.65f, 0.55f, 1.00f) },
            new NivelConfig { id = 4, nombre = "LevelYoutube", escena = "LevelYoutube", posicion = new Vector2(140, 160), acento = new Color(1.00f, 0.20f, 0.20f) },
            new NivelConfig { id = 5, nombre = "LevelYoutubev2", escena = "LevelYoutubev2", posicion = new Vector2(300, 160), acento = new Color(0.95f, 0.35f, 0.20f), requisitosProgreso = new[] { "level:LevelYoutube" } },
            new NivelConfig { id = 6, nombre = "LevelYoutubev3", escena = "LevelYoutubev3", posicion = new Vector2(460, 160), acento = new Color(0.85f, 0.15f, 0.35f), requisitosProgreso = new[] { "level:LevelYoutubev2" } },
            new NivelConfig { id = 7, nombre = "LevelSnapchat", escena = "LevelSnapchat", posicion = new Vector2(140, -140), acento = new Color(1.00f, 0.90f, 0.15f) },
            new NivelConfig { id = 8, nombre = "EscenaTiktok", escena = "EscenaTiktok", posicion = new Vector2(300, -140), acento = new Color(0.10f, 0.90f, 0.90f) },
            new NivelConfig { id = 9, nombre = "EscenaVR", escena = "EscenaVR", posicion = new Vector2(-200, -140), acento = new Color(0.45f, 0.35f, 0.95f) },
            new NivelConfig { id = 10, nombre = "JuegoDinoChrome", escena = "JuegoDinoChrome", posicion = new Vector2(460, -140), acento = new Color(0.55f, 0.55f, 0.58f) },
            new NivelConfig { id = 11, nombre = "pruebaLobby", escena = "pruebaLobby", posicion = new Vector2(0, -260), acento = new Color(0.25f, 0.85f, 0.55f) },
        };

        conexiones = new List<ConexionConfig>
        {
            new ConexionConfig { origen = 1, destino = 2, peso = 1f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 3, peso = 2f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 4, peso = 2f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 7, peso = 3f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 8, peso = 3f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 9, peso = 3f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 10, peso = 4f, bidireccional = true },
            new ConexionConfig { origen = 2, destino = 11, peso = 2f, bidireccional = true },
            new ConexionConfig { origen = 4, destino = 5, peso = 2f },
            new ConexionConfig { origen = 5, destino = 6, peso = 2f },
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
        if (panelMapa == null)
        {
            Debug.LogError("No has asignado el PanelMapa.");
            return;
        }

        raiz = panelMapa;

        spriteFlecha = CrearSpriteFlecha();

        foreach (var c in conexiones)
            DibujarArista(c);

        foreach (var n in niveles)
            DibujarNodo(n);

        CrearPastillas();
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
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float borde = DistanciaAlBorde(u);

        var visual = new AristaVisual { conexion = c, p1 = p1, p2 = p2 };

        var linea = CrearRect("Arista", raiz);
        linea.pivot = new Vector2(0f, 0.5f);
        linea.sizeDelta = new Vector2(Mathf.Max(largo - 2f * borde, 1f), GrosorArista);
        linea.anchoredPosition = p1 + u * borde;
        linea.localRotation = Quaternion.Euler(0f, 0f, angulo);
        var img = linea.gameObject.AddComponent<Image>();
        img.color = colorArista;
        img.raycastTarget = false;
        visual.linea = linea;
        visual.partes.Add(img);

        visual.partes.Add(CrearFlecha(p2 - u * (borde + 3f), angulo));
        if (c.bidireccional) visual.partes.Add(CrearFlecha(p1 + u * (borde + 3f), angulo + 180f));

        aristasVisuales.Add(visual);
    }

    private Image CrearFlecha(Vector2 punta, float angulo)
    {
        var r = CrearRect("Flecha", raiz);
        r.pivot = new Vector2(1f, 0.5f);
        r.sizeDelta = new Vector2(16f, 16f);
        r.anchoredPosition = punta;
        r.localRotation = Quaternion.Euler(0f, 0f, angulo);
        var img = r.gameObject.AddComponent<Image>();
        img.sprite = spriteFlecha;
        img.color = colorArista;
        img.raycastTarget = false;
        return img;
    }

    private void CrearPastillas()
    {
        var usados = new List<Vector2>();
        float[] fracciones = { 0.30f, 0.35f, 0.40f, 0.45f, 0.50f, 0.55f, 0.60f, 0.65f, 0.70f };

        for (int i = 0; i < aristasVisuales.Count; i++)
        {
            var v = aristasVisuales[i];
            Vector2 dir = (v.p2 - v.p1).normalized;
            float margen = DistanciaAlBorde(dir);
            Vector2 inicio = v.p1 + dir * margen;
            Vector2 fin = v.p2 - dir * margen;

            Vector2 elegido = Vector2.Lerp(v.p1, v.p2, 0.5f);
            bool hayElegido = false;
            float mejorPuntaje = float.MinValue;

            foreach (float f in fracciones)
            {
                Vector2 candidato = Vector2.Lerp(v.p1, v.p2, f);
                if (ChocaConNodo(candidato)) continue;
                if ((candidato - inicio).magnitude < 26f || (candidato - fin).magnitude < 26f) continue;

                float distLineas = float.MaxValue;
                for (int j = 0; j < aristasVisuales.Count; j++)
                {
                    if (j == i) continue;
                    distLineas = Mathf.Min(distLineas,
                        DistanciaASegmento(candidato, aristasVisuales[j].p1, aristasVisuales[j].p2));
                }

                float distPastillas = float.MaxValue;
                foreach (var otro in usados)
                    distPastillas = Mathf.Min(distPastillas, (otro - candidato).magnitude);

                float puntaje = Mathf.Min(distLineas, distPastillas * 0.6f) - 15f * Mathf.Abs(f - 0.5f);
                if (!hayElegido || puntaje > mejorPuntaje)
                {
                    hayElegido = true;
                    mejorPuntaje = puntaje;
                    elegido = candidato;
                }
            }
            usados.Add(elegido);

            string texto = v.conexion.peso.ToString("0.#");
            float ancho = 30f + 9f * (texto.Length - 1);
            Text textoPeso;
            v.pastilla = CrearPastillaPeso(raiz, texto, elegido, ancho, 26f, 17, out textoPeso);
            v.textoPeso = textoPeso;
        }
    }

    private Image CrearPastillaPeso(Transform padre, string texto, Vector2 posicion, float ancho, float alto,
                                    int tamanoFuente, out Text textoPeso)
    {
        var r = CrearRect("Peso", padre);
        r.sizeDelta = new Vector2(ancho, alto);
        r.anchoredPosition = posicion;

        var anillo = r.gameObject.AddComponent<Image>();
        anillo.sprite = SpriteRedondeado(12);
        anillo.type = Image.Type.Sliced;
        anillo.color = colorBordePastilla;
        anillo.raycastTarget = false;

        var interior = CrearRect("Interior", r);
        Estirar(interior);
        interior.offsetMin = new Vector2(2f, 2f);
        interior.offsetMax = new Vector2(-2f, -2f);
        var interiorImg = interior.gameObject.AddComponent<Image>();
        interiorImg.sprite = SpriteRedondeado(10);
        interiorImg.type = Image.Type.Sliced;
        interiorImg.color = colorPastilla;
        interiorImg.raycastTarget = false;

        textoPeso = CrearTexto("Texto", r, texto, tamanoFuente, TextAnchor.MiddleCenter);
        textoPeso.fontStyle = FontStyle.Bold;
        Estirar(textoPeso.rectTransform);
        return anillo;
    }

    private float DistanciaASegmento(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + ab * t)).magnitude;
    }

    private bool ChocaConNodo(Vector2 p)
    {
        foreach (var n in niveles)
        {
            if (Mathf.Abs(p.x - n.posicion.x) < AnchoNodo / 2f + 22f &&
                Mathf.Abs(p.y - n.posicion.y) < AltoNodo / 2f + 18f)
                return true;
        }
        return false;
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

        var borde = r.gameObject.AddComponent<Image>();
        borde.sprite = SpriteRedondeado(14);
        borde.type = Image.Type.Sliced;

        var sombra = r.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.45f);
        sombra.effectDistance = new Vector2(0f, -4f);

        var relleno = CrearRect("Relleno", r);
        Estirar(relleno);
        relleno.offsetMin = new Vector2(3f, 3f);
        relleno.offsetMax = new Vector2(-3f, -3f);
        var rellenoImg = relleno.gameObject.AddComponent<Image>();
        rellenoImg.sprite = SpriteRedondeado(11);
        rellenoImg.type = Image.Type.Sliced;
        rellenoImg.color = colorTarjeta;
        rellenoImg.raycastTarget = false;

        var barraRect = CrearRect("Barra", r);
        barraRect.anchorMin = new Vector2(0f, 0f);
        barraRect.anchorMax = new Vector2(0f, 1f);
        barraRect.pivot = new Vector2(0f, 0.5f);
        barraRect.sizeDelta = new Vector2(5f, -22f);
        barraRect.anchoredPosition = new Vector2(10f, 0f);
        var barra = barraRect.gameObject.AddComponent<Image>();
        barra.sprite = SpriteRedondeado(2);
        barra.type = Image.Type.Sliced;
        barra.raycastTarget = false;

        var nombre = CrearTexto("Nombre", r, n.nombre, 17, TextAnchor.MiddleCenter);
        nombre.fontStyle = FontStyle.Bold;
        nombre.rectTransform.anchorMin = new Vector2(0f, 0.42f);
        nombre.rectTransform.anchorMax = new Vector2(1f, 0.95f);
        nombre.rectTransform.offsetMin = new Vector2(18f, 0f);
        nombre.rectTransform.offsetMax = new Vector2(-6f, 0f);

        var estado = CrearTexto("Estado", r, "", 12, TextAnchor.MiddleCenter);
        estado.rectTransform.anchorMin = new Vector2(0f, 0.05f);
        estado.rectTransform.anchorMax = new Vector2(1f, 0.45f);
        estado.rectTransform.offsetMin = new Vector2(18f, 0f);
        estado.rectTransform.offsetMax = new Vector2(-6f, 0f);
        int id = n.id;

        nodos[id] = new NodoVisual { rect = r, borde = borde, barra = barra, nombre = nombre, estado = estado };
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
        foreach (var id in ruta)
        {
            nombres.Add(Nombre(id));
            if (nodos.ContainsKey(id))
            {
                nodos[id].borde.color = colorRuta;
                nodos[id].rect.localScale = Vector3.one * 1.08f;
            }
        }

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
        //MostrarInfo(tipo + " desde " + Nombre(idActual) + ":\n" + string.Join(" -> ", nombres));
    }

    private void ReiniciarProgreso()
    {
        foreach (var n in niveles) ProgresoNiveles.Reiniciar(Clave(n));
        LimpiarRuta();
        Refrescar();
    }

    private void OnEnable()
    {
        GameProgressionManager.OnProgressionChanged += AlCambiarProgreso;
    }

    private void OnDisable()
    {
        GameProgressionManager.OnProgressionChanged -= AlCambiarProgreso;
    }

    private void AlCambiarProgreso()
    {
        // LimpiarRuta already refreshes node unlock state.
        LimpiarRuta();
    }

    private bool CumpleRequisitosNodo(NivelConfig n)
    {
        if (n.requisitosProgreso == null || n.requisitosProgreso.Length == 0)
            return false;
        return GameProgressionManager.Asegurar().CumpleTodos(n.requisitosProgreso);
    }

    private void Refrescar()
    {
        foreach (var n in niveles)
            grafo.Obtener(n.id).desbloqueado = n.inicial || CumpleRequisitosNodo(n);
        foreach (var n in niveles)
        {
            if (n.inicial || ProgresoNiveles.EstaCompletado(Clave(n)) || CumpleRequisitosNodo(n))
                grafo.CompletarNivel(n.id);
        }

        foreach (var n in niveles)
        {
            var nivel = grafo.Obtener(n.id);
            var visual = nodos[n.id];

            Color color;
            string estado;
            bool bloqueado = false;

            if (n.id == idActual) { color = colorActual; estado = "Estás aquí"; }
            else if (ProgresoNiveles.EstaCompletado(Clave(n))) { color = colorCompletado; estado = "Completado"; }
            else if (nivel.desbloqueado) { color = colorDisponible; estado = "Disponible"; }
            else { color = colorBloqueado; estado = "Bloqueado"; bloqueado = true; }

            if (n.falso) estado += " · sin escena";

            visual.borde.color = color;
            visual.barra.color = n.acento.a > 0f ? n.acento : color;
            visual.nombre.text = n.nombre;
            visual.nombre.color = bloqueado ? colorTextoSuave : Color.white;
            visual.estado.text = estado;
            visual.estado.color = color;
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
            v.linea.sizeDelta = new Vector2(v.linea.sizeDelta.x, GrosorRuta);
            if (v.pastilla != null) v.pastilla.color = colorRuta;
        }
    }

    private void LimpiarRuta()
    {
        foreach (var v in aristasVisuales)
        {
            foreach (var parte in v.partes) parte.color = colorArista;
            v.linea.sizeDelta = new Vector2(v.linea.sizeDelta.x, GrosorArista);
            if (v.pastilla != null) v.pastilla.color = colorBordePastilla;
        }

        foreach (var kv in nodos)
            kv.Value.rect.localScale = Vector3.one;

        Refrescar();
    }

    private void MostrarInfo(string mensaje)
    {
        if (textoInfo != null) textoInfo.text = mensaje;
        Debug.Log("[MapaUI] " + mensaje);
    }

    private Sprite SpriteRedondeado(int radio)
    {
        Sprite existente;
        if (spritesRedondeados.TryGetValue(radio, out existente)) return existente;

        const int tam = 64;
        var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var pixeles = new Color32[tam * tam];
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float cx = Mathf.Clamp(px, radio, tam - radio);
                float cy = Mathf.Clamp(py, radio, tam - radio);
                float dist = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                byte alfa = (byte)(Mathf.Clamp01(radio - dist + 0.5f) * 255f);
                pixeles[y * tam + x] = new Color32(255, 255, 255, alfa);
            }
        }
        tex.SetPixels32(pixeles);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radio, radio, radio, radio));
        spritesRedondeados[radio] = sprite;
        return sprite;
    }

    private Sprite CrearSpriteFlecha()
    {
        const int ancho = 32;
        const int alto = 32;
        var tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var pixeles = new Color32[ancho * alto];
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                float semialto = 14f * (1f - (x + 0.5f) / ancho);
                float d = Mathf.Abs(y + 0.5f - alto / 2f);
                byte alfa = (byte)(Mathf.Clamp01(semialto - d + 0.5f) * 255f);
                pixeles[y * ancho + x] = new Color32(255, 255, 255, alfa);
            }
        }
        tex.SetPixels32(pixeles);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, ancho, alto), new Vector2(1f, 0.5f), 100f);
    }

    private Sprite CrearSpriteDegradado()
    {
        const int alto = 128;
        var tex = new Texture2D(2, alto, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < alto; y++)
        {
            Color c = Color.Lerp(colorFondoAbajo, colorFondoArriba, y / (alto - 1f));
            tex.SetPixel(0, y, c);
            tex.SetPixel(1, y, c);
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 2, alto), new Vector2(0.5f, 0.5f), 100f);
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
    public void MostrarBFS()
    {
        var orden = grafo.BFS(idActual);
        Debug.Log("BFS: " + string.Join(" -> ", orden));
    }

    public void MostrarDFS()
    {
        var orden = grafo.DFS(idActual);
        Debug.Log("DFS: " + string.Join(" -> ", orden));
    }

    public void MostrarRutaDesdeBoton(int destino)
    {
        MostrarRuta(destino);
    }

    public void CompletarDesdeBoton(int id)
    {
        ProgresoNiveles.Completar(Clave(config[id]));
        Refrescar();
    }

    public void ReiniciarDesdeBoton()
    {
        ReiniciarProgreso();
    }

}
