using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;

public class LobbyUI : MonoBehaviour
{
    [Header("UI (opcional: se pueden dejar vacíos)")]
    [SerializeField] TMP_InputField campoCodigo;
    [SerializeField] TMP_Text textoCodigo;
    [SerializeField] TMP_Text textoEstado;

    [Header("Configuración")]
    [SerializeField] int maxJugadores = 4;
    [Tooltip("Nombre exacto de la escena del juego (debe estar en Build Settings)")]
    [SerializeField] string escenaJuego = "";
    [Tooltip("Muestra botones temporales con OnGUI para probar sin Canvas")]
    [SerializeField] bool usarBotonesTemporales = true;

    ISession sesion;
    bool listo;
    bool ocupado;
    string codigoTemporal = "";

    async void Start()
    {
        try
        {
            var opciones = new InitializationOptions();
            opciones.SetProfile(System.Guid.NewGuid().ToString("N").Substring(0, 12));

            await UnityServices.InitializeAsync(opciones);

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            listo = true;
            MostrarEstado("Listo");
        }
        catch (System.Exception e)
        {
            MostrarEstado("Error al iniciar: " + e.Message);
            Debug.LogException(e);
        }
    }
    public async void CrearPartida()
    {
        if (!listo || ocupado || sesion != null) return;
        ocupado = true;
        try
        {
            MostrarEstado("Creando partida...");
            var opciones = new SessionOptions { MaxPlayers = maxJugadores }.WithRelayNetwork();
            sesion = await MultiplayerService.Instance.CreateSessionAsync(opciones);

            if (textoCodigo != null) textoCodigo.text = "Código: " + sesion.Code;
            Debug.Log("Código de la partida: " + sesion.Code);
            MostrarEstado("Esperando jugadores...");
        }
        catch (System.Exception e)
        {
            MostrarEstado("Error al crear: " + e.Message);
            Debug.LogException(e);
        }
        finally { ocupado = false; }
    }
    public void Unirse()
    {
        string codigo = campoCodigo != null ? campoCodigo.text : codigoTemporal;
        UnirseConCodigo(codigo);
    }
    public async void UnirseConCodigo(string codigo)
    {
        if (!listo || ocupado || sesion != null) return;
        codigo = (codigo ?? "").Trim().ToUpper();
        if (string.IsNullOrEmpty(codigo))
        {
            MostrarEstado("Escribe un código");
            return;
        }

        ocupado = true;
        try
        {
            MostrarEstado("Uniéndose...");
            sesion = await MultiplayerService.Instance.JoinSessionByCodeAsync(codigo);
            MostrarEstado("Conectado");
        }
        catch (System.Exception e)
        {
            MostrarEstado("Error al unirse: " + e.Message);
            Debug.LogException(e);
        }
        finally { ocupado = false; }
    }
    public void IniciarJuego()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            MostrarEstado("Solo el anfitrión puede iniciar");
            return;
        }
        if (string.IsNullOrEmpty(escenaJuego))
        {
            MostrarEstado("Falta el nombre de la escena del juego");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }
    public async void SalirDePartida()
    {
        if (sesion == null) return;
        try
        {
            await sesion.LeaveAsync();
            sesion = null;
            if (textoCodigo != null) textoCodigo.text = "";
            MostrarEstado("Saliste de la partida");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    void MostrarEstado(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
        Debug.Log("[Lobby] " + mensaje);
    }

    void OnGUI()
    {
        if (!usarBotonesTemporales) return;

        GUILayout.BeginArea(new Rect(10, 10, 260, 260));
        if (GUILayout.Button("Crear partida")) CrearPartida();
        codigoTemporal = GUILayout.TextField(codigoTemporal);
        if (GUILayout.Button("Unirse")) UnirseConCodigo(codigoTemporal);
        if (GUILayout.Button("Iniciar juego (host)")) IniciarJuego();
        if (GUILayout.Button("Salir")) SalirDePartida();
        GUILayout.EndArea();
    }
}