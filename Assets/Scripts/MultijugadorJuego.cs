using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)]
public class MultijugadorJuego : MonoBehaviour
{
    public static MultijugadorJuego Instancia { get; private set; }

    [Header("Configuración")]
    [Tooltip("Niveles reales que el anfitrión puede cargar en red.")]
    public string[] escenasDisponibles = { "LevelYoutube", "LevelSelector", "LevelYoutubev2", "LevelSnapchat" };

    public int indiceEscena;
    public int maxJugadores = 4;
    public bool cargarEscenaAlCrear = false;

    private ISession sesion;
    private NetworkManager red;
    private bool serviciosListos;
    private bool iniciandoServicios;
    private bool ocupado;
    private string codigoActual = "";
    private string codigoTemporal = "";
    public string mensajeEstado = "Pulsa para crear o unirte a una partida.";
    private bool panelAbierto = true;

    private bool autoAnfitrion;
    private bool autoCliente;
    private string codigoAuto = "";

    public string CodigoActual => codigoActual;
    public bool EnPartida => sesion != null;
    public NetworkManager Red => red;
    public static System.Action<NetworkManager> AlCrearRed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    [UnityEngine.Scripting.Preserve]
    private static void Crear()
    {
        if (Instancia != null) return;

        GameObject objeto = new GameObject("MultijugadorJuego");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<MultijugadorJuego>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
        //SceneManager.sceneLoaded += AlCargarEscena;

        LeerArgumentosDePrueba();
    }
    private void Update()
    {
        if (red != null && NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "[DIAGNOSTICO] RED EXISTE PERO SINGLETON ES NULL"
            );
        }
    }

    private void LeerArgumentosDePrueba()
    {
        string[] argumentos = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < argumentos.Length; i++)
        {
            if (argumentos[i] == "-hostOnline")
            {
                autoAnfitrion = true;
            }
            else if (argumentos[i] == "-joinOnline" && i + 1 < argumentos.Length)
            {
                autoCliente = true;
                codigoAuto = argumentos[i + 1];
            }
        }

        if (autoAnfitrion)
            Invoke(nameof(AutoAnfitrion), 2f);
        else if (autoCliente)
            Invoke(nameof(AutoCliente), 2f);
    }

    private void AutoAnfitrion()
    {
        Debug.Log("[Multijugador] AUTO_ANFITRION");
        CrearPartida();
    }

    private void AutoCliente()
    {
        Debug.Log("[Multijugador] AUTO_CLIENTE codigo=" + codigoAuto);
        UnirseConCodigo(codigoAuto);
    }

    private void OnDestroy()
    {
        //SceneManager.sceneLoaded -= AlCargarEscena;

        if (Instancia == this) Instancia = null;
    }

    /*
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "pruebaLobby")
            panelAbierto = true;
        else
            panelAbierto = false;
    }
    */

    private string EscenaElegida
    {
        get
        {
            if (escenasDisponibles == null || escenasDisponibles.Length == 0)
                return "LevelYoutube";

            indiceEscena = Mathf.Clamp(indiceEscena, 0, escenasDisponibles.Length - 1);
            return escenasDisponibles[indiceEscena];
        }
    }

    private async Task AsegurarServicios()
    {
        if (serviciosListos) return;

        while (iniciandoServicios)
            await Task.Yield();

        if (serviciosListos) return;

        iniciandoServicios = true;
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var opciones = new InitializationOptions();

                opciones.SetProfile(System.Guid.NewGuid().ToString("N").Substring(0, 12));

                await UnityServices.InitializeAsync(opciones);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            serviciosListos = true;
        }
        finally
        {
            iniciandoServicios = false;
        }
    }

    private void AsegurarNetworkManager()
    {
        Debug.Log("[RED] Entrando en AsegurarNetworkManager");

        if (red != null)
        {
            Debug.Log("[RED] Ya existe: " + red.name);
            return;
        }

        if (NetworkManager.Singleton != null)
        {
            red = NetworkManager.Singleton;
            Debug.Log("[RED] Recuperada desde Singleton");
            return;
        }
        if (red != null) return;

        if (NetworkManager.Singleton != null)
        {
            red = NetworkManager.Singleton;
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("Capsule");

        if (prefab == null)
        {
            throw new System.Exception(
                "Falta Assets/Resources/JugadorRedJuego.prefab. " +
                "Ejecuta Herramientas > Preparar multijugador del juego.");
        }

        GameObject objeto = new GameObject("NetworkManagerOnline");
        UnityTransport transporte = objeto.AddComponent<UnityTransport>();
        red = objeto.AddComponent<NetworkManager>();

        if (red.NetworkConfig == null)
        {
            Debug.Log("[Multijugador] NetworkConfig era null; se crea uno nuevo.");
            red.NetworkConfig = new NetworkConfig();
        }

        Debug.Log(
            "[Multijugador] NETWORKMANAGER_CREADO config=" + (red.NetworkConfig != null) +
            " transporte=" + (transporte != null) +
            " prefab=" + (prefab != null));

        red.NetworkConfig.NetworkTransport = transporte;
        red.NetworkConfig.PlayerPrefab = prefab;
        red.NetworkConfig.EnableSceneManagement = true;
        red.NetworkConfig.ConnectionApproval = false;

        Debug.Log("[Multijugador] NETWORKMANAGER_CONFIGURADO");
        AlCrearRed?.Invoke(red);
    }

    public async void CrearPartida()
    {
        if (ocupado || sesion != null) return;

        if (EscenaElegida == "LevelSnapchat")
            maxJugadores = 2;

        ocupado = true;
        try
        {
            mensajeEstado = "Iniciando servicios...";
            await AsegurarServicios();

            AsegurarNetworkManager();

            mensajeEstado = "Creando partida...";
            var opciones = new SessionOptions { MaxPlayers = maxJugadores }.WithRelayNetwork();
            sesion = await MultiplayerService.Instance.CreateSessionAsync(opciones);

            codigoActual = sesion.Code;
            mensajeEstado = "Partida creada. Comparte el código.";
            Debug.Log("[Multijugador] CODIGO=" + codigoActual);

            if (cargarEscenaAlCrear)
                StartCoroutine(CargarEscenaCuandoListo());
        }
        catch (System.Exception e)
        {
            mensajeEstado = "Error al crear: " + e.Message;
            Debug.LogException(e);
        }
        finally
        {
            ocupado = false;
        }
    }

    public void CrearPartidaSnapchat()
    {
        if (escenasDisponibles == null) return;

        int indice = System.Array.IndexOf(escenasDisponibles, "LevelSnapchat");
        if (indice < 0) return;

        indiceEscena = indice;
        maxJugadores = 2;
        CrearPartida();
    }

    public async void UnirseConCodigo(string codigo)
    {
        if (ocupado || sesion != null) return;

        codigo = (codigo ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(codigo))
        {
            mensajeEstado = "Escribe un código.";
            return;
        }

        ocupado = true;
        try
        {
            mensajeEstado = "Iniciando servicios...";
            await AsegurarServicios();

            AsegurarNetworkManager();

            mensajeEstado = "Uniéndose...";
            sesion = await MultiplayerService.Instance.JoinSessionByCodeAsync(codigo);

            codigoActual = sesion.Code;
            mensajeEstado = "Conectado. Esperando al anfitrión...";
            Debug.Log("[Multijugador] CONECTADO=" + codigoActual);
        }
        catch (System.Exception e)
        {
            mensajeEstado = "Error al unirse: " + e.Message;
            Debug.LogException(e);
        }
        finally
        {
            ocupado = false;
        }
    }

    public IEnumerator CargarEscenaCuandoListo()
    {
        float limite = Time.realtimeSinceStartup + 10f;

        while (Time.realtimeSinceStartup < limite)
        {
            if (red != null && red.IsListening && red.IsServer)
                break;

            yield return null;
        }

        if (red == null || !red.IsServer)
        {
            mensajeEstado = "No se pudo iniciar la red.";
            yield break;
        }

        CargarEscenaElegida();
    }

    public void CargarEscenaElegida()
    {
        if (red == null || !red.IsServer) return;

        string escena = EscenaElegida;

        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            mensajeEstado = "La escena " + escena + " no está en Build Settings.";
            return;
        }

        mensajeEstado = "Cargando " + escena + "...";
        Debug.Log("[Multijugador] CARGANDO_ESCENA=" + escena);
        red.SceneManager.LoadScene(escena, LoadSceneMode.Single);
    }

    public async void SalirDePartida()
    {
        if (sesion == null && (red == null || !red.IsListening)) return;

        try
        {
            if (sesion != null)
            {
                await sesion.LeaveAsync();
                sesion = null;
            }

            if (red != null && (red.IsListening || red.IsClient || red.IsServer))
                red.Shutdown();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        codigoActual = "";
        mensajeEstado = "Saliste de la partida.";

        if (SceneManager.GetActiveScene().name != "MainMenu")
            SceneManager.LoadScene("MainMenu");
    }
    
    /*
    private void OnGUI()
    {
        string escena = SceneManager.GetActiveScene().name;

        if (escena == "pruebaLobby") return;

        if (sesion == null && escena != "MainMenu") return;

        float ancho = panelAbierto ? 370f : 230f;
        float alto = panelAbierto ? 500f : 58f;

        GUILayout.BeginArea(new Rect(10f, 10f, ancho, alto), GUI.skin.box);

        if (GUILayout.Button(panelAbierto ? "MULTIJUGADOR ONLINE  ▲" : "MULTIJUGADOR ONLINE  ▼"))
            panelAbierto = !panelAbierto;

        if (!panelAbierto)
        {
            if (!string.IsNullOrEmpty(codigoActual))
                GUILayout.Label("Código: " + codigoActual);

            GUILayout.EndArea();
            return;
        }

        GUILayout.Label("Estado: " + mensajeEstado);

        if (!string.IsNullOrEmpty(codigoActual))
        {
            GUILayout.Label("Código: " + codigoActual);

            if (GUILayout.Button("Copiar código"))
                GUIUtility.systemCopyBuffer = codigoActual;
        }

        GUILayout.Space(6);
        bool sinSesion = sesion == null;

        GUI.enabled = sinSesion && !ocupado;

        if (GUILayout.Button("Crear partida (anfitrión)"))
            CrearPartida();

        if (GUILayout.Button("Crear partida Snapchat co-op (2)"))
            CrearPartidaSnapchat();

        GUILayout.Label("Código para unirse:");
        codigoTemporal = GUILayout.TextField((codigoTemporal ?? "").ToUpperInvariant());

        if (GUILayout.Button("Unirse"))
            UnirseConCodigo(codigoTemporal);

        if (sinSesion)
        {
            GUILayout.Space(4);
            GUILayout.Label("Nivel: " + EscenaElegida);

            if (GUILayout.Button("Cambiar nivel"))
            {
                int total = escenasDisponibles != null ? escenasDisponibles.Length : 0;
                if (total > 0) indiceEscena = (indiceEscena + 1) % total;
            }
        }

        GUI.enabled = !sinSesion && red != null && red.IsServer;

        if (GUILayout.Button("Cargar nivel ahora"))
            CargarEscenaElegida();

        GUI.enabled = !sinSesion;

        if (GUILayout.Button("Salir de la partida"))
            SalirDePartida();

        GUI.enabled = true;

        GUILayout.Space(6);
        GUILayout.Label("WASD / flechas: mover. Espacio: saltar.");
        GUILayout.Label("El anfitrión comparte el código. El invitado lo escribe.");

        GUILayout.EndArea();
    }
    */
}
