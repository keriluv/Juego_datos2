using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Archipielago 3D para navegar por los mundos del juego.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class MapaNavegacion3D : MonoBehaviour
{
    [Header("Configuracion del archipielago")]
    public List<NivelConfig> niveles = new List<NivelConfig>();
    public List<ConexionConfig> conexiones = new List<ConexionConfig>();
    public Color colorBloqueado = new Color(0.30f, 0.25f, 0.40f);
    public Color colorDisponible = new Color(0.10f, 0.90f, 1f);
    public Color colorCompletado = new Color(0.58f, 0.34f, 0.86f);
    public Color colorActual = new Color(1f, 0.22f, 0.72f);
    public Color colorRuta = new Color(1f, 0.18f, 0.76f);

    private enum Modo { Navegar, Ruta, Completar }

    private class Isla
    {
        public Transform raiz;
        public Transform decoracion;
        public Renderer plataforma;
        public Renderer aro;
        public TextMesh etiqueta;
        public TextMesh estado;
        public Collider clic;
        public Color colorBase;
        public bool bloqueada;
    }

    private class Puente
    {
        public int origen;
        public int destino;
        public ConexionConfig conexion;
        public readonly List<Renderer> tablones = new List<Renderer>();
        public readonly List<Renderer> bordes = new List<Renderer>();
        public TextMesh costo;
    }

    private const string PrefijoProgreso = "completado_";
    private readonly Dictionary<int, NivelConfig> configuracion = new Dictionary<int, NivelConfig>();
    private readonly Dictionary<int, Isla> islas = new Dictionary<int, Isla>();
    private readonly Dictionary<Collider, int> islaPorCollider = new Dictionary<Collider, int>();
    private readonly List<Puente> puentes = new List<Puente>();
    private readonly Dictionary<int, Color> colores = new Dictionary<int, Color>();
    private readonly Image[] botonesModo = new Image[3];

    private GrafoNiveles grafo;
    private Camera camara;
    private CharacterController controladorCamara;
    private Transform escenario;
    private Canvas lienzo;
    private Text textoInfo;
    private Font fuente;
    private int idActual;
    private int idHover = -1;
    private float yawPrimeraPersona;
    private float pitchPrimeraPersona;
    private float velocidadVertical;
    private bool ratonCapturado;
    private bool capturoRatonEsteFrame;
    private Modo modo = Modo.Navegar;
    private Color colorMadera = new Color(0.12f, 0.105f, 0.19f);
    private readonly Color colorNeonCian = new Color(0.10f, 0.90f, 1f);
    private readonly Color colorNeonRosa = new Color(1f, 0.16f, 0.74f);
    private bool progresoListo;

    private void Awake()
    {
        DesactivarInterfazAnterior();
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        CargarGrafoDeEscena();
        PrepararCamara();
        PrepararEscenario();
        CrearFondoCiudad();
        CrearPuentes();
        CrearIslas();
        CrearHUD();
        DeterminarNivelActual();
        RefrescarProgreso();
        ActualizarBotonesModo();
        MostrarInfo("Sigue las carreteras y selecciona un marcador disponible para entrar.");
    }

    private void DesactivarInterfazAnterior()
    {
        // Limpia interfaces persistentes que puedan quedar de otra escena.
        Canvas[] canvasesAnteriores = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvasAnterior in canvasesAnteriores)
        {
            if (canvasAnterior == null || canvasAnterior.gameObject.name == "HUDMapa3D") continue;
            canvasAnterior.gameObject.SetActive(false);
            Destroy(canvasAnterior.gameObject);
        }
    }

    private void Update()
    {
        ActualizarVistaPrimeraPersona();
        float tiempo = Time.unscaledTime;
        foreach (var par in islas)
        {
            Isla isla = par.Value;
            bool mostrarEtiqueta = Vector3.Distance(camara.transform.position, isla.raiz.position) < 5.5f || par.Key == idHover;
            if (isla.decoracion.gameObject.activeSelf != mostrarEtiqueta)
                isla.decoracion.gameObject.SetActive(mostrarEtiqueta);
            float objetivo = par.Key == idHover ? 1.08f : 1f;
            isla.raiz.localScale = Vector3.Lerp(isla.raiz.localScale, Vector3.one * objetivo,
                10f * Time.unscaledDeltaTime);
            Vector3 posicion = isla.decoracion.localPosition;
            posicion.y = 0.07f * Mathf.Sin(tiempo * 1.2f + par.Key * 0.8f);
            isla.decoracion.localPosition = posicion;
        }

        ActualizarHover();
        if (Input.GetMouseButtonDown(0) && !capturoRatonEsteFrame &&
            (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) && idHover >= 0)
            AlSeleccionarIsla(idHover);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CargarGrafoDeEscena()
    {
        if (niveles == null || niveles.Count == 0)
        {
            Debug.LogError("El mapa 3D no tiene niveles configurados.");
            enabled = false;
            return;
        }

        grafo = new GrafoNiveles();
        foreach (NivelConfig nivel in niveles)
        {
            configuracion[nivel.id] = nivel;
            grafo.AgregarNivel(new Nivel(nivel.id, nivel.nombre, nivel.escena));
            colores[nivel.id] = ColorApp(nivel);
        }
        foreach (ConexionConfig conexion in conexiones)
        {
            if (!configuracion.ContainsKey(conexion.origen) || !configuracion.ContainsKey(conexion.destino)) continue;
            grafo.AgregarCamino(conexion.origen, conexion.destino, conexion.peso, conexion.bidireccional);
        }

    }

    private void PrepararCamara()
    {
        camara = Camera.main;
        if (camara == null) camara = FindObjectOfType<Camera>();
        if (camara == null)
        {
            GameObject nueva = new GameObject("CamaraMapa3D");
            nueva.tag = "MainCamera";
            camara = nueva.AddComponent<Camera>();
            nueva.AddComponent<AudioListener>();
        }

        camara.orthographic = false;
        camara.fieldOfView = 70f;
        NivelConfig inicio = null;
        foreach (NivelConfig nivel in niveles)
            if (nivel.inicial) { inicio = nivel; break; }
        if (inicio == null) inicio = niveles[0];
        Vector3 direccion = Vector3.right;
        foreach (ConexionConfig conexion in conexiones)
        {
            if (conexion.origen == inicio.id && configuracion.ContainsKey(conexion.destino))
            {
                direccion = PosicionIsla(configuracion[conexion.destino]) - PosicionIsla(inicio);
                break;
            }
            if (conexion.bidireccional && conexion.destino == inicio.id && configuracion.ContainsKey(conexion.origen))
            {
                direccion = PosicionIsla(configuracion[conexion.origen]) - PosicionIsla(inicio);
                break;
            }
        }
        direccion.y = 0f;
        direccion.Normalize();
        Vector3 posicionInicio = PosicionIsla(inicio) + direccion * 1.25f;
        posicionInicio.y = 1.62f;
        camara.transform.position = posicionInicio;
        yawPrimeraPersona = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
        pitchPrimeraPersona = -2f;
        camara.transform.rotation = Quaternion.Euler(pitchPrimeraPersona, yawPrimeraPersona, 0f);
        camara.enabled = true;
        camara.targetDisplay = 0;
        camara.rect = new Rect(0f, 0f, 1f, 1f);
        camara.cullingMask = ~0;
        camara.farClipPlane = 200f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.025f, 0.018f, 0.065f);
        controladorCamara = camara.GetComponent<CharacterController>();
        if (controladorCamara == null) controladorCamara = camara.gameObject.AddComponent<CharacterController>();
        controladorCamara.height = 1.8f;
        controladorCamara.radius = 0.28f;
        controladorCamara.center = new Vector3(0f, -0.79f, 0f);
        controladorCamara.stepOffset = 0.3f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ActualizarVistaPrimeraPersona()
    {
        capturoRatonEsteFrame = false;
        bool sobreInterfaz = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetKeyDown(KeyCode.Escape) && ratonCapturado)
        {
            ratonCapturado = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (!ratonCapturado && Input.GetMouseButtonDown(0) && !sobreInterfaz && idHover < 0)
        {
            capturoRatonEsteFrame = true;
            ratonCapturado = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (!ratonCapturado) return;

        yawPrimeraPersona += Input.GetAxis("Mouse X") * 2.2f;
        pitchPrimeraPersona = Mathf.Clamp(pitchPrimeraPersona - Input.GetAxis("Mouse Y") * 2.0f, -70f, 70f);
        camara.transform.rotation = Quaternion.Euler(pitchPrimeraPersona, yawPrimeraPersona, 0f);

        Vector3 adelante = camara.transform.forward;
        adelante.y = 0f;
        adelante.Normalize();
        Vector3 derecha = camara.transform.right;
        derecha.y = 0f;
        derecha.Normalize();
        Vector3 entrada = adelante * Input.GetAxisRaw("Vertical") + derecha * Input.GetAxisRaw("Horizontal");
        if (entrada.sqrMagnitude > 1f) entrada.Normalize();
        if (controladorCamara.isGrounded && velocidadVertical < 0f) velocidadVertical = -1.5f;
        velocidadVertical -= 18f * Time.deltaTime;
        controladorCamara.Move((entrada * 4.5f + Vector3.up * velocidadVertical) * Time.deltaTime);
    }

    private void PrepararEscenario()
    {
        var root = new GameObject("ArchipielagoDigital");
        escenario = root.transform;
        RenderSettings.fog = false;
        RenderSettings.fogColor = new Color(0.66f, 0.87f, 0.96f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.008f;
        RenderSettings.ambientLight = new Color(0.25f, 0.18f, 0.38f);
    }

    private void CrearFondoCiudad()
    {
        Material suelo = NuevoMaterial(new Color(0.045f, 0.035f, 0.10f), 0.08f, 0.38f);
        Material edificio = NuevoMaterial(new Color(0.075f, 0.055f, 0.14f), 0.12f, 0.42f);
        GameObject baseCiudad = CrearPrimitiva(PrimitiveType.Plane, "CiudadMapa", escenario,
            new Vector3(2f, -0.12f, 0f), new Vector3(3.5f, 1f, 2.3f), suelo, true);
        baseCiudad.isStatic = true;

        // Volumenes low poly y fachadas con letreros de barrio japones.
        Vector3[] manzanas =
        {
            new Vector3(-5.8f, 4.6f, -6.0f), new Vector3(-1.8f, 3.8f, -6.8f),
            new Vector3(3.0f, 5.4f, -6.4f), new Vector3(8.0f, 4.2f, -6.0f),
            new Vector3(-5.0f, 3.9f, 6.2f), new Vector3(0.4f, 5.0f, 6.8f),
            new Vector3(5.4f, 4.1f, 6.4f), new Vector3(9.0f, 5.2f, 6.0f),
            new Vector3(13.2f, 6.3f, -7.1f), new Vector3(13.5f, 5.6f, 7.2f),
            new Vector3(18.0f, 7.2f, -1.8f)
        };
        string[] letreros = { "RAMEN", "TOKYO", "SUSHI", "OPEN 24/7", "NEON", "ARCADE", "NOODLE", "IZAKAYA", "TOKYO", "NIGHT", "CITY" };
        Material ventana = MaterialNeon(new Color(0.06f, 0.20f, 0.30f));
        Material poste = NuevoMaterial(new Color(0.035f, 0.03f, 0.08f), 0.12f, 0.5f);
        Material luzCalle = MaterialNeon(new Color(0.08f, 0.72f, 0.90f));
        for (int i = 0; i < manzanas.Length; i++)
        {
            Vector3 pos = manzanas[i];
            CrearPrimitiva(PrimitiveType.Cube, "EdificioBajo", escenario,
                new Vector3(pos.x, pos.y * 0.5f, pos.z), new Vector3(2.7f, pos.y, 1.9f), edificio, true);
            CrearPrimitiva(PrimitiveType.Cube, "CornisaTecho", escenario,
                new Vector3(pos.x, pos.y + 0.12f, pos.z), new Vector3(2.9f, 0.24f, 2.05f), edificio, false);
            Material letrero = MaterialNeon(i % 2 == 0 ? new Color(0.06f, 0.55f, 0.72f) : new Color(0.64f, 0.16f, 0.53f));
            float frenteZ = pos.z + (pos.z < 0f ? 0.98f : -0.98f);
            float frente = pos.z < 0f ? 1f : -1f;
            CrearPrimitiva(PrimitiveType.Cube, "LetreroBarrio", escenario,
                new Vector3(pos.x, 2.65f, frenteZ), new Vector3(1.05f, 1.05f, 0.10f), letrero, false);
            TextMesh nombreTienda = CrearTexto3D(letreros[i], escenario,
                new Vector3(pos.x, 2.65f, frenteZ + frente * 0.07f), 54, 0.025f, Color.white, TextAlignment.Center);
            nombreTienda.transform.rotation = OrientarAlJugador(nombreTienda.transform.position);
            CrearPrimitiva(PrimitiveType.Cube, "MarquesinaNeon", escenario,
                new Vector3(pos.x, 1.72f, frenteZ), new Vector3(2.65f, 0.14f, 0.52f), letrero, false);
            CrearPrimitiva(PrimitiveType.Cube, "PuertaTienda", escenario,
                new Vector3(pos.x, 0.94f, frenteZ + frente * 0.035f), new Vector3(0.48f, 1.72f, 0.08f), poste, false);
            for (int ventanaId = -1; ventanaId <= 1; ventanaId++)
            {
                CrearPrimitiva(PrimitiveType.Cube, "VentanaNeon", escenario,
                    new Vector3(pos.x + ventanaId * 0.68f, 1.05f, frenteZ), new Vector3(0.42f, 0.56f, 0.06f), ventana, false);
                if (ventanaId != 0)
                    CrearPrimitiva(PrimitiveType.Cube, "VentanaAlta", escenario,
                        new Vector3(pos.x + ventanaId * 0.68f, 2.22f, frenteZ), new Vector3(0.42f, 0.50f, 0.06f), ventana, false);
            }
            if (i % 2 == 0)
            {
                CrearPrimitiva(PrimitiveType.Cube, "LetreroVertical", escenario,
                    new Vector3(pos.x - 1.05f, 3.45f, frenteZ + frente * 0.06f), new Vector3(0.34f, 1.65f, 0.12f), letrero, false);
                CrearPrimitiva(PrimitiveType.Cube, "MolduraFachada", escenario,
                    new Vector3(pos.x, 3.55f, frenteZ), new Vector3(2.72f, 0.10f, 0.12f), luzCalle, false);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            float x = -3.4f + i * 3.1f;
            float lado = i % 2 == 0 ? -1f : 1f;
            float z = lado * 4.05f;
            CrearPrimitiva(PrimitiveType.Cylinder, "PosteCalle", escenario,
                new Vector3(x, 1.55f, z), new Vector3(0.075f, 1.55f, 0.075f), poste, true);
            CrearPrimitiva(PrimitiveType.Cube, "BrazoPoste", escenario,
                new Vector3(x + 0.30f, 3.05f, z), new Vector3(0.62f, 0.07f, 0.07f), poste, false);
            CrearPrimitiva(PrimitiveType.Sphere, "LuzPoste", escenario,
                new Vector3(x + 0.48f, 2.98f, z), new Vector3(0.24f, 0.12f, 0.24f), luzCalle, false);
        }
    }

    private void CrearPuentes()
    {
        Material asfalto = NuevoMaterial(new Color(0.12f, 0.105f, 0.19f), 0.12f, 0.58f);
        Material guiaNeon = MaterialNeon(new Color(0.05f, 0.48f, 0.66f));
        Material linea = NuevoMaterial(new Color(0.78f, 0.84f, 0.96f), 0f, 0.2f);
        Material flechaNeon = MaterialNeon(colorNeonRosa);
        int indiceConexion = 0;
        foreach (ConexionConfig conexion in conexiones)
        {
            if (!configuracion.ContainsKey(conexion.origen) || !configuracion.ContainsKey(conexion.destino)) continue;
            Vector3 a = PosicionIsla(configuracion[conexion.origen]);
            Vector3 b = PosicionIsla(configuracion[conexion.destino]);
            a.y = b.y = 0.025f;
            Vector3 delta = b - a;
            float largo = delta.magnitude;
            if (largo < 0.2f) continue;

            var puente = new Puente { origen = conexion.origen, destino = conexion.destino, conexion = conexion };
            Transform grupo = new GameObject("Carretera_" + conexion.origen + "_" + conexion.destino).transform;
            grupo.SetParent(escenario, false);
            Vector3 lateral = Vector3.Cross(Vector3.up, delta.normalized);
            float curva = (indiceConexion++ % 2 == 0 ? 1f : -1f) * Mathf.Min(0.9f, largo * 0.12f);
            Vector3 control = (a + b) * 0.5f + lateral * curva;
            int muestras = Mathf.Max(12, Mathf.CeilToInt(largo * 4f));
            List<Vector3> puntos = new List<Vector3>(muestras + 1);
            for (int i = 0; i <= muestras; i++)
            {
                float t = i / (float)muestras;
                Vector3 punto = (1f - t) * (1f - t) * a + 2f * (1f - t) * t * control + t * t * b;
                punto.y = 0.025f;
                puntos.Add(punto);
            }

            MeshRenderer calzada = CrearCinta(grupo, "Calzada", puntos, 0.78f, asfalto, 0.045f);
            puente.tablones.Add(calzada);
            puente.bordes.Add(CrearCinta(grupo, "BordeCianA", DesplazarCurva(puntos, 0.385f), 0.035f, guiaNeon, 0.075f));
            puente.bordes.Add(CrearCinta(grupo, "BordeCianB", DesplazarCurva(puntos, -0.385f), 0.035f, guiaNeon, 0.075f));

            int guiones = Mathf.Max(4, Mathf.RoundToInt(largo * 1.35f));
            for (int i = 1; i < guiones; i++)
            {
                float t = i / (float)guiones;
                Vector3 punto = PuntoCurva(puntos, t);
                Vector3 direccion = TangenteCurva(puntos, t);
                GameObject guion = CrearPrimitiva(PrimitiveType.Cube, "LineaCentral", grupo,
                    punto + Vector3.up * 0.075f, new Vector3(0.035f, 0.018f, 0.22f), linea, false);
                guion.transform.rotation = Quaternion.LookRotation(direccion, Vector3.up);
            }

            CrearFlechaPuente(grupo, PuntoCurva(puntos, conexion.bidireccional ? 0.35f : 0.72f),
                TangenteCurva(puntos, conexion.bidireccional ? 0.35f : 0.72f), flechaNeon);
            if (conexion.bidireccional)
                CrearFlechaPuente(grupo, PuntoCurva(puntos, 0.65f), -TangenteCurva(puntos, 0.65f), flechaNeon);

            Vector3 centro = PuntoCurva(puntos, 0.5f) + Vector3.up * 0.14f;
            GameObject moneda = CrearPrimitiva(PrimitiveType.Cylinder, "CostoRuta", grupo,
                centro, new Vector3(0.34f, 0.045f, 0.34f), MaterialNeon(new Color(0.46f, 0.22f, 0.66f)), false);
            puente.costo = CrearTexto3D(conexion.peso.ToString("0.#"), grupo, centro,
                60, 0.042f, Color.white, TextAlignment.Center);
            puente.costo.transform.rotation = OrientarAlJugador(puente.costo.transform.position);
            puente.costo.transform.position += camara.transform.up * -0.015f;
            puentes.Add(puente);
        }
    }

    private List<Vector3> DesplazarCurva(List<Vector3> puntos, float distancia)
    {
        List<Vector3> desplazados = new List<Vector3>(puntos.Count);
        for (int i = 0; i < puntos.Count; i++)
        {
            int antes = Mathf.Max(0, i - 1);
            int despues = Mathf.Min(puntos.Count - 1, i + 1);
            Vector3 tangente = (puntos[despues] - puntos[antes]).normalized;
            desplazados.Add(puntos[i] + Vector3.Cross(Vector3.up, tangente) * distancia);
        }
        return desplazados;
    }

    private Vector3 PuntoCurva(List<Vector3> puntos, float t)
    {
        return puntos[Mathf.Clamp(Mathf.RoundToInt(t * (puntos.Count - 1)), 0, puntos.Count - 1)];
    }

    private Vector3 TangenteCurva(List<Vector3> puntos, float t)
    {
        int i = Mathf.Clamp(Mathf.RoundToInt(t * (puntos.Count - 1)), 0, puntos.Count - 1);
        return (puntos[Mathf.Min(puntos.Count - 1, i + 1)] - puntos[Mathf.Max(0, i - 1)]).normalized;
    }

    private MeshRenderer CrearCinta(Transform grupo, string nombre, List<Vector3> puntos, float ancho, Material material, float altura)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[puntos.Count * 2];
        int[] triangulos = new int[(puntos.Count - 1) * 6];
        for (int i = 0; i < puntos.Count; i++)
        {
            Vector3 tangente = (puntos[Mathf.Min(puntos.Count - 1, i + 1)] - puntos[Mathf.Max(0, i - 1)]).normalized;
            Vector3 lateral = Vector3.Cross(Vector3.up, tangente).normalized * (ancho * 0.5f);
            Vector3 centro = puntos[i] + Vector3.up * altura;
            vertices[i * 2] = centro - lateral;
            vertices[i * 2 + 1] = centro + lateral;
            if (i == puntos.Count - 1) continue;
            int vi = i * 2, ti = i * 6;
            triangulos[ti] = vi; triangulos[ti + 1] = vi + 2; triangulos[ti + 2] = vi + 1;
            triangulos[ti + 3] = vi + 1; triangulos[ti + 4] = vi + 2; triangulos[ti + 5] = vi + 3;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.RecalculateNormals();
        GameObject carretera = new GameObject(nombre);
        carretera.transform.SetParent(grupo, false);
        carretera.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = carretera.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    private void CrearFlechaPuente(Transform grupo, Vector3 punto, Vector3 direccion, Material material)
    {
        Vector3 lateral = Vector3.Cross(direccion, Vector3.up).normalized;
        for (int lado = -1; lado <= 1; lado += 2)
        {
            Vector3 inicio = punto - direccion * 0.14f + lateral * (lado * 0.12f);
            GameObject ala = CrearPrimitiva(PrimitiveType.Cube, "FlechaSentido", grupo,
                (punto + inicio) * 0.5f + Vector3.up * 0.09f, new Vector3(0.035f, 0.025f, 0.21f), material, false);
            ala.transform.rotation = Quaternion.LookRotation(punto - inicio, Vector3.up);
        }
    }

    private void CrearIslas()
    {
        foreach (NivelConfig nivel in niveles)
        {
            Vector3 posicion = PosicionIsla(nivel);
            var raiz = new GameObject("Marcador_" + NombreVisible(nivel)).transform;
            raiz.SetParent(escenario, false);
            raiz.position = posicion;

            var decoracion = new GameObject("EtiquetaMarcador").transform;
            decoracion.SetParent(raiz, false);
            Color app = ColorApp(nivel);
            Material matBase = NuevoMaterial(new Color(0.075f, 0.055f, 0.14f), 0.08f, 0.4f);
            Material matAro = MaterialNeon(app);
            CrearPrimitiva(PrimitiveType.Cylinder, "AroMarcador", raiz,
                new Vector3(0f, 0.035f, 0f), new Vector3(1.30f, 0.11f, 1.30f), matAro, false);
            GameObject pasto = CrearPrimitiva(PrimitiveType.Cylinder, "BaseMarcador", raiz,
                new Vector3(0f, 0.12f, 0f), new Vector3(1.08f, 0.10f, 1.08f), matBase, true);
            Collider collider = pasto.GetComponent<Collider>();
            MeshRenderer pin = CrearPin(raiz, app);
            CrearPrimitiva(PrimitiveType.Sphere, "EmblemaPin", raiz,
                new Vector3(0f, 0.88f, -0.30f), new Vector3(0.43f, 0.43f, 0.10f), matBase, false);
            TextMesh sigla = CrearTexto3D(SiglaNivel(nivel), raiz, new Vector3(0f, 0.86f, -0.36f),
                70, 0.024f, Color.white, TextAlignment.Center);
            sigla.transform.rotation = OrientarAlJugador(sigla.transform.position);
            CrearEtiqueta(decoracion, nivel);

            var isla = new Isla
            {
                raiz = raiz,
                decoracion = decoracion,
                plataforma = pasto.GetComponent<Renderer>(),
                aro = pin,
                etiqueta = decoracion.Find("EtiquetaNombre").GetComponent<TextMesh>(),
                estado = decoracion.Find("EtiquetaEstado").GetComponent<TextMesh>(),
                clic = collider,
                colorBase = app
            };
            islas[nivel.id] = isla;
            islaPorCollider[collider] = nivel.id;
        }
    }

    private MeshRenderer CrearPin(Transform padre, Color color)
    {
        float[] alturas = { 0f, 0.16f, 0.38f, 0.62f, 0.84f, 1.04f, 1.20f, 1.29f };
        float[] radios =  { 0.012f, 0.10f, 0.22f, 0.32f, 0.35f, 0.29f, 0.16f, 0.012f };
        const int lados = 20;
        Mesh mesh = new Mesh { name = "MallaPin" };
        Vector3[] vertices = new Vector3[alturas.Length * lados];
        int[] triangulos = new int[(alturas.Length - 1) * lados * 6];
        for (int anillo = 0; anillo < alturas.Length; anillo++)
            for (int lado = 0; lado < lados; lado++)
            {
                float angulo = lado * Mathf.PI * 2f / lados;
                vertices[anillo * lados + lado] = new Vector3(Mathf.Cos(angulo) * radios[anillo], alturas[anillo], Mathf.Sin(angulo) * radios[anillo]);
            }
        int indice = 0;
        for (int anillo = 0; anillo < alturas.Length - 1; anillo++)
            for (int lado = 0; lado < lados; lado++)
            {
                int a = anillo * lados + lado;
                int b = anillo * lados + (lado + 1) % lados;
                int c = (anillo + 1) * lados + lado;
                int d = (anillo + 1) * lados + (lado + 1) % lados;
                triangulos[indice++] = a; triangulos[indice++] = c; triangulos[indice++] = b;
                triangulos[indice++] = b; triangulos[indice++] = c; triangulos[indice++] = d;
            }
        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.RecalculateNormals();
        GameObject objeto = new GameObject("PinUbicacion");
        objeto.transform.SetParent(padre, false);
        objeto.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        objeto.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = objeto.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = MaterialNeon(color);
        return renderer;
    }

    private string SiglaNivel(NivelConfig nivel)
    {
        switch (nivel.nombre)
        {
            case "MainMenu": return "M";
            case "LevelSelector": return "LS";
            case "Tutorial": return "T";
            case "LevelYoutube": return "YT";
            case "TikTok": return "TT";
            case "Instagram": return "IG";
            case "WhatsApp": return "WA";
            default: return "N";
        }
    }

    private void CrearRocasDecorativas(Transform raiz, Material material, int semilla)
    {
        System.Random random = new System.Random(semilla * 107);
        for (int i = 0; i < 5; i++)
        {
            float angulo = (float)random.NextDouble() * Mathf.PI * 2f;
            float radio = 0.75f + (float)random.NextDouble() * 0.34f;
            float tam = 0.20f + (float)random.NextDouble() * 0.20f;
            CrearPrimitiva(PrimitiveType.Sphere, "RocaSaliente", raiz,
                new Vector3(Mathf.Cos(angulo) * radio, -0.40f - (i % 2) * 0.20f, Mathf.Sin(angulo) * radio),
                new Vector3(tam * 1.1f, tam * 1.5f, tam), material, false);
        }
    }

    private void CrearVegetacion(Transform raiz, int semilla)
    {
        Material tronco = NuevoMaterial(new Color(0.12f, 0.075f, 0.18f), 0.05f, 0.4f);
        Color tonoHojas = semilla % 2 == 0 ? colorNeonCian : colorNeonRosa;
        Material hojas = MaterialNeon(Color.Lerp(tonoHojas, new Color(0.58f, 0.20f, 0.82f), 0.25f));
        System.Random random = new System.Random(semilla * 701);
        int cantidad = semilla == 1 ? 3 : 2;
        for (int i = 0; i < cantidad; i++)
        {
            float angulo = i * 2.4f + (float)random.NextDouble();
            Vector3 baseArbol = new Vector3(Mathf.Cos(angulo) * 0.70f, 0.05f, Mathf.Sin(angulo) * 0.52f);
            GameObject tallo = CrearPrimitiva(PrimitiveType.Cylinder, "Tronco", raiz,
                baseArbol + Vector3.up * 0.27f, new Vector3(0.12f, 0.34f, 0.12f), tronco, false);
            tallo.transform.rotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? -1f : 1f) * 7f);
            CrearPrimitiva(PrimitiveType.Sphere, "Copa", raiz,
                baseArbol + Vector3.up * 0.70f, new Vector3(0.57f, 0.43f, 0.52f), hojas, false);
        }
    }

    private void CrearDecoracionApp(Transform padre, NivelConfig nivel, Color color)
    {
        Material acento = MaterialNeon(color);
        Material claro = NuevoMaterial(Color.white, 0.05f, 0.35f);
        string nombre = nivel.nombre;
        if (nombre == "LevelYoutube")
        {
            CrearPrimitiva(PrimitiveType.Cube, "PantallaVideo", padre, new Vector3(0f, 0.62f, 0f),
                new Vector3(0.88f, 0.56f, 0.12f), acento, false);
            CrearMallaTriangulo(padre, new Vector3(0f, 0.62f, -0.09f), 0.28f, claro);
        }
        else if (nombre == "TikTok")
        {
            CrearPrimitiva(PrimitiveType.Cylinder, "DiscoMusica", padre, new Vector3(0f, 0.54f, 0f),
                new Vector3(0.46f, 0.10f, 0.46f), acento, false);
            CrearPrimitiva(PrimitiveType.Cylinder, "EjeMusica", padre, new Vector3(0.14f, 0.88f, 0f),
                new Vector3(0.09f, 0.36f, 0.09f), claro, false);
            CrearPrimitiva(PrimitiveType.Sphere, "NotaMusical", padre, new Vector3(0.14f, 1.22f, 0f),
                new Vector3(0.24f, 0.20f, 0.20f), acento, false);
        }
        else if (nombre == "Instagram")
        {
            CrearPrimitiva(PrimitiveType.Cube, "CamaraSocial", padre, new Vector3(0f, 0.67f, 0f),
                new Vector3(0.72f, 0.56f, 0.22f), acento, false);
            CrearPrimitiva(PrimitiveType.Cylinder, "Lente", padre, new Vector3(0f, 0.67f, -0.14f),
                new Vector3(0.22f, 0.035f, 0.22f), claro, false);
        }
        else if (nombre == "WhatsApp")
        {
            CrearPrimitiva(PrimitiveType.Sphere, "Mensaje", padre, new Vector3(0f, 0.68f, 0f),
                new Vector3(0.74f, 0.58f, 0.28f), acento, false);
            for (int i = -1; i <= 1; i++)
                CrearPrimitiva(PrimitiveType.Sphere, "PuntoMensaje", padre,
                    new Vector3(i * 0.16f, 0.68f, -0.25f), new Vector3(0.065f, 0.065f, 0.04f), claro, false);
        }
        else
        {
            CrearPrimitiva(PrimitiveType.Sphere, "FaroDigital", padre, new Vector3(0f, 0.74f, 0f),
                new Vector3(0.62f, 0.62f, 0.62f), acento, false);
        }
    }

    private void CrearEtiqueta(Transform padre, NivelConfig nivel)
    {
        Color color = ColorApp(nivel);
        string nombreVisible = NombreVisible(nivel);
        float anchoPlaca = Mathf.Clamp(1.55f + Mathf.Max(0, nombreVisible.Length - 9) * 0.055f, 1.55f, 2.05f);
        GameObject placa = CrearPrimitiva(PrimitiveType.Cube, "PlacaNombre", padre,
            new Vector3(0f, 1.66f, 0f), new Vector3(anchoPlaca, 0.27f, 0.07f),
            NuevoMaterial(new Color(0.055f, 0.12f, 0.18f, 0.90f), 0f, 0.2f), false);
        placa.transform.rotation = OrientarAlJugador(placa.transform.position);
        TextMesh nombre = CrearTexto3D(nombreVisible, padre, new Vector3(0f, 1.67f, -0.05f),
            80, nombreVisible.Length > 10 ? 0.027f : 0.031f, Color.white, TextAlignment.Center);
        nombre.gameObject.name = "EtiquetaNombre";
        nombre.transform.rotation = OrientarAlJugador(nombre.transform.position);
        TextMesh estado = CrearTexto3D("", padre, new Vector3(0f, 1.49f, -0.05f),
            60, 0.022f, color, TextAlignment.Center);
        estado.gameObject.name = "EtiquetaEstado";
        estado.transform.rotation = OrientarAlJugador(estado.transform.position);
    }

    private void CrearMallaTriangulo(Transform padre, Vector3 centro, float radio, Material material)
    {
        var go = new GameObject("PlayIcono");
        go.transform.SetParent(padre, false);
        go.transform.localPosition = centro;
        var mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(-radio * 0.62f, -radio, 0f),
            new Vector3(-radio * 0.62f, radio, 0f),
            new Vector3(radio, 0f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private void CrearHUD()
    {
        GameObject canvasGO = new GameObject("HUDMapa3D", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        lienzo = canvasGO.GetComponent<Canvas>();
        lienzo.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        TextoUI("IndicacionesFPS", "CLIC EN EL FONDO PARA MIRAR  ·  WASD MOVERSE  ·  RATON GIRAR  ·  ESC LIBERAR", 13, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(850f, 25f), new Vector2(0f, -24f),
            new Color(0.62f, 0.90f, 1f), false);

        PanelUI("PanelInfo", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(820f, 78f), new Vector2(160f, 35f), new Color(0.055f, 0.025f, 0.12f, 0.92f));
        textoInfo = TextoUI("Info", "", 18, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, Color.white, false);
        textoInfo.transform.SetParent(lienzo.transform.Find("PanelInfo"), false);
        RectTransform infoRect = textoInfo.rectTransform;
        infoRect.offsetMin = new Vector2(15f, 7f);
        infoRect.offsetMax = new Vector2(-15f, -7f);

        botonesModo[0] = CrearBotonUI("01  Explorar mundos", 0, () => CambiarModo(Modo.Navegar));
        botonesModo[1] = CrearBotonUI("02  Ruta mas corta", 1, () => CambiarModo(Modo.Ruta));
        botonesModo[2] = CrearBotonUI("03  Simular avance", 2, () => CambiarModo(Modo.Completar));
        CrearBotonUI("04  Recorrido por capas", 3, () => MostrarRecorrido(true));
        CrearBotonUI("05  Recorrido en cadena", 4, () => MostrarRecorrido(false));
        CrearBotonUI("06  Reiniciar progreso", 5, ReiniciarProgreso);
        CrearLeyenda();
    }

    private void CrearLeyenda()
    {
        TextoUI("LeyendaConexiones", "Flechas: sentido  ·  Numero: costo", 13, TextAnchor.MiddleRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(255f, 22f), new Vector2(-22f, -18f),
            new Color(0.90f, 0.95f, 1f), false);
        string[] nombres = { "Ubicacion actual", "Disponible", "Completado", "Bloqueado" };
        Color[] tonos = { colorActual, colorDisponible, colorCompletado, colorBloqueado };
        for (int i = 0; i < nombres.Length; i++)
        {
            float y = 132f - i * 28f;
            Image punto = CrearPuntoUI("Estado", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(15f, 15f), new Vector2(-206f, -y), tonos[i]);
            punto.rectTransform.pivot = new Vector2(0f, 0.5f);
            TextoUI("Leyenda", nombres[i], 13, TextAnchor.MiddleLeft, new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(175f, 20f), new Vector2(-184f, -y),
                new Color(0.90f, 0.95f, 1f), false);
        }
    }

    private Image CrearBotonUI(string nombre, int indice, UnityEngine.Events.UnityAction accion)
    {
        RectTransform rect = CrearRectUI("Boton_" + indice, lienzo.transform);
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(235f, 43f);
        rect.anchoredPosition = new Vector2(22f, -112f - indice * 51f);
        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteRedondeado(10);
        imagen.type = Image.Type.Sliced;
        imagen.color = new Color(0.075f, 0.035f, 0.15f, 0.94f);
        Button boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;
        boton.onClick.AddListener(accion);
        ColorBlock coloresUI = boton.colors;
        coloresUI.highlightedColor = new Color(0.62f, 0.12f, 0.72f);
        coloresUI.pressedColor = new Color(0.08f, 0.70f, 0.88f);
        boton.colors = coloresUI;
        Text etiqueta = TextoUI("Texto", nombre, 14, TextAnchor.MiddleLeft,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white, false);
        etiqueta.transform.SetParent(rect, false);
        etiqueta.rectTransform.offsetMin = new Vector2(13f, 3f);
        etiqueta.rectTransform.offsetMax = new Vector2(-8f, -3f);
        return imagen;
    }

    private RectTransform PanelUI(string nombre, Vector2 min, Vector2 max, Vector2 tam, Vector2 posicion, Color color)
    {
        RectTransform rect = CrearRectUI(nombre, lienzo.transform);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = tam;
        rect.anchoredPosition = posicion;
        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.sprite = SpriteRedondeado(16);
        imagen.type = Image.Type.Sliced;
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    private Image CrearPuntoUI(string nombre, Vector2 min, Vector2 max, Vector2 tam, Vector2 posicion, Color color)
    {
        RectTransform rect = CrearRectUI(nombre, lienzo.transform);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = tam;
        rect.anchoredPosition = posicion;
        Image punto = rect.gameObject.AddComponent<Image>();
        punto.sprite = SpriteRedondeado(8);
        punto.type = Image.Type.Sliced;
        punto.color = color;
        punto.raycastTarget = false;
        return punto;
    }

    private Text TextoUI(string nombre, string valor, int tamano, TextAnchor alineacion,
        Vector2 min, Vector2 max, Vector2 tam, Vector2 posicion, Color color, bool negrita)
    {
        RectTransform rect = CrearRectUI(nombre, lienzo.transform);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.sizeDelta = tam;
        rect.anchoredPosition = posicion;
        Text texto = rect.gameObject.AddComponent<Text>();
        texto.font = fuente;
        texto.text = valor;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = color;
        texto.fontStyle = negrita ? FontStyle.Bold : FontStyle.Normal;
        texto.raycastTarget = false;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        return texto;
    }

    private void DeterminarNivelActual()
    {
        idActual = configuracion.Count > 0 ? FirstId() : 0;
        foreach (NivelConfig nivel in configuracion.Values)
            if (nivel.inicial) { idActual = nivel.id; break; }
        string escenaActual = SceneManager.GetActiveScene().name;
        foreach (NivelConfig nivel in configuracion.Values)
            if (nivel.escena == escenaActual) { idActual = nivel.id; break; }
    }

    private int FirstId()
    {
        foreach (int id in configuracion.Keys) return id;
        return 0;
    }

    private void RefrescarProgreso()
    {
        if (progresoListo) return;
        foreach (NivelConfig nivel in configuracion.Values)
            grafo.Obtener(nivel.id).desbloqueado = nivel.inicial;
        foreach (NivelConfig nivel in configuracion.Values)
            if (nivel.inicial || EstaCompletado(nivel)) grafo.CompletarNivel(nivel.id);

        foreach (NivelConfig nivel in configuracion.Values)
        {
            Nivel estado = grafo.Obtener(nivel.id);
            Isla isla = islas[nivel.id];
            bool hecho = EstaCompletado(nivel);
            Color color = nivel.id == idActual ? colorActual :
                (hecho ? colorCompletado : (estado.desbloqueado ? colorDisponible : colorBloqueado));
            isla.bloqueada = !estado.desbloqueado && !hecho && nivel.id != idActual;
            isla.colorBase = color;
            isla.plataforma.material.color = isla.bloqueada
                ? new Color(0.12f, 0.10f, 0.18f)
                : new Color(0.17f, 0.10f, 0.25f);
            isla.aro.material.color = isla.bloqueada
                ? new Color(0.30f, 0.26f, 0.38f)
                : color;
            if (isla.aro.material.HasProperty("_EmissionColor"))
                isla.aro.material.SetColor("_EmissionColor", (isla.bloqueada ? colorBloqueado : color) * 1.35f);
            isla.etiqueta.text = NombreVisible(nivel);
            isla.estado.text = nivel.id == idActual ? "Estas aqui" :
                (hecho ? "Completado" : (nivel.falso ? "Proximamente" : (estado.desbloqueado ? "Disponible" : "Bloqueado")));
            isla.estado.color = isla.bloqueada ? new Color(0.78f, 0.82f, 0.87f) : color;
        }
        progresoListo = true;
    }

    private void ActualizarHover()
    {
        int nuevoHover = -1;
        if (camara != null && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Ray rayo = camara.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(rayo, out hit, 100f))
            {
                int encontrado;
                if (islaPorCollider.TryGetValue(hit.collider, out encontrado)) nuevoHover = encontrado;
            }
        }
        if (nuevoHover == idHover) return;
        idHover = nuevoHover;
        if (idHover >= 0 && configuracion.ContainsKey(idHover))
        {
            NivelConfig nivel = configuracion[idHover];
            string estado = islas[idHover].estado.text;
            MostrarInfo(NombreVisible(nivel) + "  |  " + estado + "\n" + Descripcion(nivel));
        }
        else MostrarInfo(modo == Modo.Navegar ?
            "Sigue las carreteras y selecciona un marcador disponible para entrar." : "Selecciona un marcador para continuar.");
    }

    private void AlSeleccionarIsla(int id)
    {
        NivelConfig nivel = configuracion[id];
        Nivel estado = grafo.Obtener(id);
        if (modo == Modo.Ruta) { MostrarRuta(id); return; }
        if (modo == Modo.Completar)
        {
            if (!estado.desbloqueado && id != idActual)
            {
                MostrarInfo(NombreVisible(nivel) + " esta bloqueado; completa un mundo vecino primero.");
                return;
            }
            Completar(nivel);
            progresoListo = false;
            RefrescarProgreso();
            MostrarInfo("Mundo completado: " + NombreVisible(nivel) + ". Sus conexiones ya estan disponibles.");
            return;
        }
        if (id == idActual) { MostrarInfo("Ya estas en " + NombreVisible(nivel) + "."); return; }
        if (!estado.desbloqueado) { MostrarInfo(NombreVisible(nivel) + " esta bloqueado."); return; }
        if (nivel.falso || string.IsNullOrEmpty(nivel.escena))
        { MostrarInfo(NombreVisible(nivel) + " estara disponible proximamente."); return; }
        if (!Application.CanStreamedLevelBeLoaded(nivel.escena))
        { MostrarInfo("No se encuentra la escena " + nivel.escena + " en la lista de escenas."); return; }
        SceneManager.LoadScene(nivel.escena);
    }

    private void CambiarModo(Modo nuevo)
    {
        modo = nuevo;
        LimpiarRuta();
        ActualizarBotonesModo();
        if (modo == Modo.Navegar) MostrarInfo("Sigue las carreteras y selecciona un marcador disponible para entrar.");
        else if (modo == Modo.Ruta) MostrarInfo("Ruta mas corta: selecciona un marcador para trazar el camino desde " + NombreVisible(configuracion[idActual]) + ".");
        else MostrarInfo("Simulacion: selecciona un marcador disponible para marcarlo como completado.");
    }

    private void MostrarRuta(int destino)
    {
        LimpiarRuta();
        float costo;
        List<int> ruta = grafo.CaminoMasCorto(idActual, destino, out costo);
        if (ruta.Count == 0) { MostrarInfo("No hay ruta disponible hacia " + NombreVisible(configuracion[destino]) + "."); return; }
        for (int i = 0; i < ruta.Count - 1; i++) ResaltarPuente(ruta[i], ruta[i + 1]);
        var nombres = new List<string>();
        foreach (int id in ruta) nombres.Add(NombreVisible(configuracion[id]));
        MostrarInfo("Ruta mas corta: " + string.Join("  >  ", nombres) + "\nCosto total: " + costo.ToString("0.#"));
    }

    private void MostrarRecorrido(bool anchura)
    {
        LimpiarRuta();
        List<int> recorrido = anchura ? grafo.BFS(idActual) : grafo.DFS(idActual);
        for (int i = 0; i < recorrido.Count - 1; i++) ResaltarPuente(recorrido[i], recorrido[i + 1]);
        var nombres = new List<string>();
        foreach (int id in recorrido) nombres.Add(NombreVisible(configuracion[id]));
        MostrarInfo((anchura ? "Recorrido por capas (BFS)" : "Recorrido en cadena (DFS)") + ":\n" + string.Join("  >  ", nombres));
    }

    private void ResaltarPuente(int origen, int destino)
    {
        foreach (Puente puente in puentes)
        {
            bool coincide = (puente.origen == origen && puente.destino == destino) ||
                (puente.conexion.bidireccional && puente.origen == destino && puente.destino == origen);
            if (!coincide) continue;
            foreach (Renderer renderer in puente.tablones) PintarRenderer(renderer, colorRuta);
            foreach (Renderer renderer in puente.bordes) PintarRenderer(renderer, colorRuta);
            if (puente.costo != null) puente.costo.color = colorRuta;
        }
    }

    private void LimpiarRuta()
    {
        foreach (Puente puente in puentes)
        {
            RestablecerPuente(puente);
        }
    }

    private void ActualizarBotonesModo()
    {
        for (int i = 0; i < botonesModo.Length; i++)
            if (botonesModo[i] != null)
                botonesModo[i].color = i == (int)modo ? new Color(0.10f, 0.54f, 0.69f, 0.98f) :
                    new Color(0.035f, 0.12f, 0.19f, 0.91f);
    }

    private void ReiniciarProgreso()
    {
        foreach (NivelConfig nivel in configuracion.Values)
            PlayerPrefs.DeleteKey(PrefijoProgreso + Clave(nivel));
        PlayerPrefs.Save();
        foreach (Puente puente in puentes) RestablecerPuente(puente);
        progresoListo = false;
        RefrescarProgreso();
        MostrarInfo("Progreso reiniciado. Los marcadores vuelven a su estado inicial.");
    }

    private void RestablecerPuente(Puente puente)
    {
        foreach (Renderer renderer in puente.tablones) PintarRenderer(renderer, colorMadera);
        foreach (Renderer renderer in puente.bordes) PintarRenderer(renderer, new Color(0.05f, 0.48f, 0.66f));
        if (puente.costo != null) puente.costo.color = Color.white;
    }

    private void PintarRenderer(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        renderer.material.color = color;
        if (renderer.material.HasProperty("_EmissionColor"))
            renderer.material.SetColor("_EmissionColor", color * 1.45f);
    }

    private void Completar(NivelConfig nivel)
    {
        PlayerPrefs.SetInt(PrefijoProgreso + Clave(nivel), 1);
        PlayerPrefs.Save();
    }

    private bool EstaCompletado(NivelConfig nivel)
    {
        return PlayerPrefs.GetInt(PrefijoProgreso + Clave(nivel), 0) == 1;
    }

    private string Clave(NivelConfig nivel)
    {
        return string.IsNullOrEmpty(nivel.escena) ? nivel.nombre : nivel.escena;
    }

    private void MostrarInfo(string mensaje)
    {
        if (textoInfo != null) textoInfo.text = mensaje;
    }

    private string NombreVisible(NivelConfig nivel)
    {
        if (nivel.nombre == "MainMenu") return "Inicio";
        if (nivel.nombre == "LevelSelector") return "LevelSelector";
        if (nivel.nombre == "LevelYoutube") return "YouTube";
        return nivel.nombre;
    }

    private string Descripcion(NivelConfig nivel)
    {
        switch (nivel.nombre)
        {
            case "MainMenu": return "El punto de partida de tu aventura digital.";
            case "LevelSelector": return "El corazon de la ciudad neon: elige tu siguiente mundo.";
            case "Tutorial": return "Aprende las reglas y controles del juego.";
            case "LevelYoutube": return "Un mundo de videos, creadores y desafios.";
            case "TikTok": return "Zona de tendencias y videos cortos.";
            case "Instagram": return "Un mundo de imagenes, historias y conexiones.";
            case "WhatsApp": return "Zona de mensajes y retos compartidos.";
            default: return "Un nuevo mundo conectado a la red.";
        }
    }

    private Color ColorApp(NivelConfig nivel)
    {
        if (nivel.acento.a > 0f) return nivel.acento;
        switch (nivel.nombre)
        {
            case "MainMenu": return new Color(1f, 0.68f, 0.24f);
            case "LevelSelector": return new Color(0.18f, 0.74f, 0.90f);
            case "Tutorial": return new Color(0.68f, 0.55f, 0.94f);
            case "LevelYoutube": return new Color(0.94f, 0.20f, 0.18f);
            case "TikTok": return new Color(0.16f, 0.83f, 0.83f);
            case "Instagram": return new Color(0.90f, 0.30f, 0.58f);
            case "WhatsApp": return new Color(0.18f, 0.78f, 0.40f);
            default: return new Color(0.28f, 0.75f, 0.83f);
        }
    }

    private Vector3 PosicionIsla(NivelConfig nivel)
    {
        return new Vector3(nivel.posicion.x * 0.017f + 0.1f,
            0f, nivel.posicion.y * 0.017f);
    }

    private GameObject CrearPrimitiva(PrimitiveType tipo, string nombre, Transform padre,
        Vector3 posicion, Vector3 escala, Material material, bool mantenerCollider)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre, false);
        go.transform.localPosition = posicion;
        go.transform.localScale = escala;
        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        Collider collider = go.GetComponent<Collider>();
        if (!mantenerCollider && collider != null) Destroy(collider);
        return go;
    }

    private Material NuevoMaterial(Color color, float metalico, float suavidad)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metalico);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", suavidad);
        return material;
    }

    private Material MaterialNeon(Color color)
    {
        Material material = NuevoMaterial(color, 0.15f, 0.72f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.45f);
        }
        return material;
    }

    private TextMesh CrearTexto3D(string valor, Transform padre, Vector3 posicion,
        int tamano, float escala, Color color, TextAlignment alineacion)
    {
        var go = new GameObject("Texto3D_" + valor);
        go.transform.SetParent(padre, false);
        go.transform.localPosition = posicion;
        TextMesh texto = go.AddComponent<TextMesh>();
        texto.text = valor;
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.characterSize = escala;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.alignment = alineacion;
        texto.color = color;
        texto.richText = false;
        texto.GetComponent<MeshRenderer>().sharedMaterial = fuente.material;
        return texto;
    }

    private Quaternion OrientarAlJugador(Vector3 posicion)
    {
        Vector3 haciaCamara = camara.transform.position - posicion;
        if (haciaCamara.sqrMagnitude < 0.001f) return camara.transform.rotation;
        return Quaternion.LookRotation(haciaCamara.normalized, Vector3.up);
    }

    private RectTransform CrearRectUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    private Sprite SpriteRedondeado(int radio)
    {
        const int tam = 64;
        var textura = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        var pixeles = new Color32[tam * tam];
        for (int y = 0; y < tam; y++)
        for (int x = 0; x < tam; x++)
        {
            float px = x + 0.5f, py = y + 0.5f;
            float cx = Mathf.Clamp(px, radio, tam - radio);
            float cy = Mathf.Clamp(py, radio, tam - radio);
            float distancia = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
            byte alfa = (byte)(Mathf.Clamp01(radio - distancia + 0.5f) * 255f);
            pixeles[y * tam + x] = new Color32(255, 255, 255, alfa);
        }
        textura.SetPixels32(pixeles);
        textura.Apply();
        return Sprite.Create(textura, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radio, radio, radio, radio));
    }
}
