using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class RedSnapchat : NetworkBehaviour
{
    public static RedSnapchat Autoridad { get; private set; }
    public static RedSnapchat Local { get; private set; }

    public static bool HayRed =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    public static bool EsAnfitrion =>
        HayRed && NetworkManager.Singleton.IsServer;

    public static bool EsCliente =>
        HayRed && !NetworkManager.Singleton.IsServer;

    public static bool HayCliente =>
        HayRed && NetworkManager.Singleton.IsServer &&
        NetworkManager.Singleton.ConnectedClientsIds.Count > 1;

    private NetworkVariable<int> historia = new NetworkVariable<int>(-1);

    private NetworkVariable<float> fotoVisor = new NetworkVariable<float>(0f);
    private NetworkVariable<float> fotoObjetivo = new NetworkVariable<float>(0f);
    private NetworkVariable<int> fotoSnaps = new NetworkVariable<int>(0);
    private NetworkVariable<int> fotoFallos = new NetworkVariable<int>(0);
    private NetworkVariable<int> fotoEvento = new NetworkVariable<int>(0);
    private NetworkVariable<bool> fotoResultado = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> fotoReinicio = new NetworkVariable<bool>(false);

    private NetworkVariable<int> escapeDulces = new NetworkVariable<int>(0);
    private NetworkVariable<int> escapeVidas = new NetworkVariable<int>(0);
    private NetworkVariable<float> escapeMarea = new NetworkVariable<float>(0f);
    private NetworkVariable<bool> escapeSalida = new NetworkVariable<bool>(false);
    private NetworkVariable<Vector2> escapeP1 = new NetworkVariable<Vector2>(Vector2.zero);
    private NetworkVariable<Vector2> escapeP2 = new NetworkVariable<Vector2>(Vector2.zero);
    private NetworkVariable<float> escapeTiempo = new NetworkVariable<float>(0f);
    private NetworkVariable<Vector2> escapePeligro0 = new NetworkVariable<Vector2>(new Vector2(0f, 99999f));
    private NetworkVariable<Vector2> escapePeligro1 = new NetworkVariable<Vector2>(new Vector2(0f, 99999f));
    private NetworkVariable<Vector2> escapePeligro2 = new NetworkVariable<Vector2>(new Vector2(0f, 99999f));
    private NetworkVariable<int> escapeMascaraDulces = new NetworkVariable<int>(0);
    private NetworkVariable<int> escapePeligrosActivos = new NetworkVariable<int>(0);
    private NetworkVariable<bool> escapePlataformaRompible = new NetworkVariable<bool>(true);
    private NetworkVariable<int> escapeEvento = new NetworkVariable<int>(0);
    private NetworkVariable<int> escapeVersion = new NetworkVariable<int>(0);

    public NetworkList<Vector3> escapeEscombros = new NetworkList<Vector3>();

    private JugadorTutorial jugador1;
    private JugadorTutorial jugador2;
    private float entradaH;
    private bool entradaSaltoPend;
    private bool entradaGolpePend;
    private bool entradaAgachadoPend;
    private int disparosPendientes;
    private float siguienteEnvio;
    private bool capsulaOculta;

    public int HistoriaActual => historia.Value;
    public float FotoVisor => fotoVisor.Value;
    public float FotoObjetivo => fotoObjetivo.Value;
    public int FotoSnaps => fotoSnaps.Value;
    public int FotoFallos => fotoFallos.Value;
    public int FotoEvento => fotoEvento.Value;
    public bool FotoResultado => fotoResultado.Value;
    public bool FotoReinicio => fotoReinicio.Value;

    public int EscapeDulces => escapeDulces.Value;
    public int EscapeVidas => escapeVidas.Value;
    public float EscapeMarea => escapeMarea.Value;
    public bool EscapeSalida => escapeSalida.Value;
    public Vector2 EscapeP1 => escapeP1.Value;
    public Vector2 EscapeP2 => escapeP2.Value;
    public float EscapeTiempo => escapeTiempo.Value;
    public int EscapeMascaraDulces => escapeMascaraDulces.Value;
    public int EscapePeligrosActivos => escapePeligrosActivos.Value;
    public bool EscapePlataformaRompible => escapePlataformaRompible.Value;
    public int EscapeEvento => escapeEvento.Value;
    public int EscapeVersion => escapeVersion.Value;

    public Vector2 EscapePeligro(int indice)
    {
        if (indice == 0) return escapePeligro0.Value;
        if (indice == 1) return escapePeligro1.Value;
        return escapePeligro2.Value;
    }

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 0)
            Autoridad = this;

        if (IsOwner)
            Local = this;

        DesactivarEventSystem();
    }

    public override void OnNetworkDespawn()
    {
        if (Autoridad == this)
            Autoridad = null;

        if (Local == this)
            Local = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Autoridad == this)
            Autoridad = null;

        if (Local == this)
            Local = null;
    }

    private void Update()
    {
        if (!HayRed) return;

        if (SceneManager.GetActiveScene().name == "LevelSnapchat" && !capsulaOculta)
            OcultarCapsula();
        else if (SceneManager.GetActiveScene().name != "LevelSnapchat" && capsulaOculta)
            MostrarCapsula();

        if (IsServer || !IsOwner || !IsClient) return;
        if (Autoridad == null) return;
        if (SceneManager.GetActiveScene().name != "LevelSnapchat") return;
        if (Autoridad.HistoriaActual < 1) return;

        float h = EntradaSnap.Horizontal();
        bool salto = EntradaSnap.Salto();
        bool golpe = EntradaSnap.Golpe();
        bool agachado = EntradaSnap.Agachado();

        if (Time.unscaledTime < siguienteEnvio && !salto && !golpe) return;
        siguienteEnvio = Time.unscaledTime + 0.05f;

        EnviarEntrada(h, salto, golpe, agachado);
    }

    private void OcultarCapsula()
    {
        capsulaOculta = true;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        Rigidbody[] cuerpos = GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < cuerpos.Length; i++)
        {
            cuerpos[i].useGravity = false;
            cuerpos[i].isKinematic = true;
        }

        Camera[] camaras = GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < camaras.Length; i++)
            camaras[i].enabled = false;

        AudioListener[] listeners = GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].enabled = false;

        MovePlayer1[] movimientos = GetComponentsInChildren<MovePlayer1>(true);
        for (int i = 0; i < movimientos.Length; i++)
            movimientos[i].enabled = false;

        NetworkTransform[] transformaciones = GetComponentsInChildren<NetworkTransform>(true);
        for (int i = 0; i < transformaciones.Length; i++)
            transformaciones[i].enabled = false;

        NetworkRigidbody[] redes = GetComponentsInChildren<NetworkRigidbody>(true);
        for (int i = 0; i < redes.Length; i++)
            redes[i].enabled = false;

        hit[] golpes = GetComponentsInChildren<hit>(true);
        for (int i = 0; i < golpes.Length; i++)
            golpes[i].enabled = false;

        PlayerHit[] golpesJugador = GetComponentsInChildren<PlayerHit>(true);
        for (int i = 0; i < golpesJugador.Length; i++)
            golpesJugador[i].enabled = false;

        UnityEngine.EventSystems.EventSystem[] sistemas =
            GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
            sistemas[i].enabled = false;

        UnityEngine.EventSystems.BaseInputModule[] modulos =
            GetComponentsInChildren<UnityEngine.EventSystems.BaseInputModule>(true);
        for (int i = 0; i < modulos.Length; i++)
            modulos[i].enabled = false;
    }

    private void DesactivarEventSystem()
    {
        UnityEngine.EventSystems.EventSystem[] sistemas =
            GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
            sistemas[i].enabled = false;

        UnityEngine.EventSystems.BaseInputModule[] modulos =
            GetComponentsInChildren<UnityEngine.EventSystems.BaseInputModule>(true);
        for (int i = 0; i < modulos.Length; i++)
            modulos[i].enabled = false;
    }

    private void MostrarCapsula()
    {
        capsulaOculta = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = true;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = true;

        Rigidbody[] cuerpos = GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < cuerpos.Length; i++)
        {
            cuerpos[i].useGravity = true;
            cuerpos[i].isKinematic = false;
        }

        Camera[] camaras = GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < camaras.Length; i++)
            camaras[i].enabled = true;

        AudioListener[] listeners = GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].enabled = true;

        MovePlayer1[] movimientos = GetComponentsInChildren<MovePlayer1>(true);
        for (int i = 0; i < movimientos.Length; i++)
            movimientos[i].enabled = true;

        NetworkTransform[] transformaciones = GetComponentsInChildren<NetworkTransform>(true);
        for (int i = 0; i < transformaciones.Length; i++)
            transformaciones[i].enabled = true;

        NetworkRigidbody[] redes = GetComponentsInChildren<NetworkRigidbody>(true);
        for (int i = 0; i < redes.Length; i++)
            redes[i].enabled = true;

        hit[] golpes = GetComponentsInChildren<hit>(true);
        for (int i = 0; i < golpes.Length; i++)
            golpes[i].enabled = true;

        PlayerHit[] golpesJugador = GetComponentsInChildren<PlayerHit>(true);
        for (int i = 0; i < golpesJugador.Length; i++)
            golpesJugador[i].enabled = true;
    }

    public void RegistrarJugadores(JugadorTutorial p1, JugadorTutorial p2)
    {
        jugador1 = p1;
        jugador2 = p2;
        AplicarEntradaRemota();
    }

    public void FijarHistoria(int indice)
    {
        if (!IsServer) return;
        historia.Value = indice;
    }

    public void FijarFoto(float visor, float objetivo, int snaps, int fallos)
    {
        if (!IsServer) return;

        if (!Mathf.Approximately(fotoVisor.Value, visor))
            fotoVisor.Value = visor;

        if (!Mathf.Approximately(fotoObjetivo.Value, objetivo))
            fotoObjetivo.Value = objetivo;

        if (fotoSnaps.Value != snaps)
            fotoSnaps.Value = snaps;

        if (fotoFallos.Value != fallos)
            fotoFallos.Value = fallos;
    }

    public void FijarFotoEvento(bool resultado, bool reinicio)
    {
        if (!IsServer) return;

        fotoResultado.Value = resultado;
        fotoReinicio.Value = reinicio;
        fotoEvento.Value++;
    }

    public void FijarEscapeJugadores(Vector2 p1, Vector2 p2)
    {
        if (!IsServer) return;

        if ((escapeP1.Value - p1).sqrMagnitude > 0.25f)
            escapeP1.Value = p1;

        if ((escapeP2.Value - p2).sqrMagnitude > 0.25f)
            escapeP2.Value = p2;
    }

    public void FijarEscapeTiempo(float tiempo)
    {
        if (!IsServer) return;
        escapeTiempo.Value = tiempo;
    }

    public void FijarEscapeEstado(float marea, int vidas, int dulces, bool salida)
    {
        if (!IsServer) return;

        if (!Mathf.Approximately(escapeMarea.Value, marea))
            escapeMarea.Value = marea;

        if (escapeVidas.Value != vidas)
            escapeVidas.Value = vidas;

        if (escapeDulces.Value != dulces)
            escapeDulces.Value = dulces;

        if (escapeSalida.Value != salida)
            escapeSalida.Value = salida;
    }

    public void FijarEscapeMascaras(int dulces, int peligros, bool plataforma)
    {
        if (!IsServer) return;

        if (escapeMascaraDulces.Value != dulces)
            escapeMascaraDulces.Value = dulces;

        if (escapePeligrosActivos.Value != peligros)
            escapePeligrosActivos.Value = peligros;

        if (escapePlataformaRompible.Value != plataforma)
            escapePlataformaRompible.Value = plataforma;
    }

    public void FijarEscapePeligro(int indice, Vector2 posicion)
    {
        if (!IsServer) return;

        if (indice == 0)
        {
            if ((escapePeligro0.Value - posicion).sqrMagnitude > 0.25f)
                escapePeligro0.Value = posicion;
        }
        else if (indice == 1)
        {
            if ((escapePeligro1.Value - posicion).sqrMagnitude > 0.25f)
                escapePeligro1.Value = posicion;
        }
        else
        {
            if ((escapePeligro2.Value - posicion).sqrMagnitude > 0.25f)
                escapePeligro2.Value = posicion;
        }
    }

    public void FijarEscapeEscombros(Vector3[] lista)
    {
        if (!IsServer) return;

        int cantidad = lista != null ? lista.Length : 0;

        if (escapeEscombros.Count != cantidad)
        {
            escapeEscombros.Clear();

            for (int i = 0; i < cantidad; i++)
                escapeEscombros.Add(lista[i]);
        }
        else
        {
            for (int i = 0; i < cantidad; i++)
                escapeEscombros[i] = lista[i];
        }
    }

    public void FijarEscapeEvento(int tipo)
    {
        if (!IsServer) return;

        escapeEvento.Value = tipo;
        escapeVersion.Value++;
    }

    public void RecibirEntradaCliente(ulong clientId, float h, bool salto, bool golpe, bool agachado)
    {
        if (clientId == 0) return;

        entradaH = h;

        if (salto)
            entradaSaltoPend = true;

        if (golpe)
            entradaGolpePend = true;

        entradaAgachadoPend = agachado;
        AplicarEntradaRemota();
    }

    private void AplicarEntradaRemota()
    {
        if (jugador2 == null) return;

        jugador2.entradaHorizontal = entradaH;
        jugador2.entradaAgachado = entradaAgachadoPend;

        if (entradaSaltoPend)
        {
            jugador2.entradaSalto = true;
            entradaSaltoPend = false;
        }

        if (entradaGolpePend)
        {
            jugador2.entradaGolpe = true;
            entradaGolpePend = false;
        }
    }

    public void RecibirDisparoCliente(ulong clientId)
    {
        if (clientId == 0) return;
        disparosPendientes++;
    }

    public bool ConsumirDisparoCliente()
    {
        if (disparosPendientes <= 0) return false;

        disparosPendientes--;
        return true;
    }

    public void EnviarEntrada(float h, bool salto, bool golpe, bool agachado)
    {
        RedSnapchat emisor = IsSpawned && IsOwner ? this : Local;

        if (emisor == null || !emisor.IsSpawned) return;
        emisor.EntradaServerRpc(h, salto, golpe, agachado);
    }

    public void EnviarDisparo()
    {
        RedSnapchat emisor = IsSpawned && IsOwner ? this : Local;

        if (emisor == null || !emisor.IsSpawned) return;
        emisor.DisparoServerRpc();
    }

    [ServerRpc]
    private void EntradaServerRpc(float h, bool salto, bool golpe, bool agachado,
        ServerRpcParams parametros = default)
    {
        if (OwnerClientId == 0) return;
        Autoridad?.RecibirEntradaCliente(parametros.Receive.SenderClientId, h, salto, golpe, agachado);
    }

    [ServerRpc]
    private void DisparoServerRpc(ServerRpcParams parametros = default)
    {
        if (OwnerClientId == 0) return;
        Autoridad?.RecibirDisparoCliente(parametros.Receive.SenderClientId);
    }
}

public static class EntradaSnap
{
    public static float Horizontal()
    {
        float h = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;

        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) h -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) h += 1f;
        }

        Gamepad mando = Gamepad.current;

        if (mando != null)
        {
            float eje = mando.leftStick.ReadValue().x;

            if (Mathf.Abs(eje) > 0.18f) h = eje;
            else if (mando.dpad.left.isPressed) h = -1f;
            else if (mando.dpad.right.isPressed) h = 1f;
        }
#else
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
#endif

        return Mathf.Clamp(h, -1f, 1f);
    }

    public static bool Salto()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;

        if (teclado != null &&
            (teclado.spaceKey.wasPressedThisFrame ||
             teclado.wKey.wasPressedThisFrame ||
             teclado.upArrowKey.wasPressedThisFrame))
        {
            return true;
        }

        Gamepad mando = Gamepad.current;

        if (mando != null && (mando.buttonSouth.wasPressedThisFrame || mando.buttonNorth.wasPressedThisFrame))
            return true;
#endif

        return false;
    }

    public static bool Golpe()
    {
        bool click = Input.GetMouseButtonDown(0);

#if ENABLE_INPUT_SYSTEM
        Mouse raton = Mouse.current;

        if (raton != null && raton.leftButton.wasPressedThisFrame)
            click = true;
#endif

        if (click) return true;

        try
        {
            if (Input.GetAxis("Fire3") > 0.5f) return true;
        }
        catch
        {
        }

#if ENABLE_INPUT_SYSTEM
        Gamepad mando = Gamepad.current;

        if (mando != null &&
            (mando.rightTrigger.wasPressedThisFrame || mando.leftTrigger.wasPressedThisFrame))
            return true;
#endif

        return false;
    }

    public static bool Agachado()
    {
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            return true;

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;

        if (teclado != null && (teclado.downArrowKey.isPressed || teclado.sKey.isPressed))
            return true;
#endif

        return false;
    }
}
