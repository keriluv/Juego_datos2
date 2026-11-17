using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Jugador de red del nivel de TikTok.
/// El anfitrion simula a las 4 capsulas y publica sus posiciones.
/// Los clientes envian su entrada y reproducen las posiciones recibidas.
/// Tambien sincroniza el progreso compartido de los minijuegos.
/// </summary>
[DefaultExecutionOrder(100)]
public class RedTiktok : NetworkBehaviour
{
    public const int MaxJugadores = 4;

    public static RedTiktok Autoridad { get; private set; }
    public static RedTiktok Local { get; private set; }
    public static readonly List<RedTiktok> Todos = new List<RedTiktok>();

    public static bool HayRed =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    public static bool EsAnfitrion =>
        HayRed && NetworkManager.Singleton.IsServer;

    public static bool EsCliente =>
        HayRed && !NetworkManager.Singleton.IsServer;

    public static int CantidadJugadores
    {
        get
        {
            if (!HayRed) return 1;

            return Mathf.Clamp(
                NetworkManager.Singleton.ConnectedClientsIds.Count,
                1,
                MaxJugadores);
        }
    }

    [Header("Movimiento")]
    public float velocidadHorizontal = 400f;
    public float gravedadPersonalizada = 2000f;
    public float alturaSalto = 200f;

    private NetworkVariable<int> victorias = new NetworkVariable<int>(0);
    private NetworkVariable<int> mascaraSuperadas = new NetworkVariable<int>(0);
    private NetworkVariable<bool> final = new NetworkVariable<bool>(false);
    private NetworkVariable<int> version = new NetworkVariable<int>(0);

    private NetworkVariable<Vector2> pos0 = new NetworkVariable<Vector2>(new Vector2(418f, 90f));
    private NetworkVariable<Vector2> pos1 = new NetworkVariable<Vector2>(new Vector2(498f, 90f));
    private NetworkVariable<Vector2> pos2 = new NetworkVariable<Vector2>(new Vector2(578f, 90f));
    private NetworkVariable<Vector2> pos3 = new NetworkVariable<Vector2>(new Vector2(658f, 90f));

    private Rigidbody cuerpo;
    private CapsuleCollider colision;
    private MeshRenderer visual;
    private SkinJugador skin;
    private Material materialCapsula;
    private SecuenciaTiktokDorado secuenciaEnlazada;

    private int slot = -1;
    private bool enTikTok;
    private bool enSuelo;
    private bool saltoPedido;
    private float ultimaH;
    private bool ultimoCongelado;
    private float siguienteEnvio;

    private bool estadosGuardados;
    private Vector3 escalaGuardada;
    private bool gravedadGuardada;
    private bool cinematicoGuardado;
    private RigidbodyInterpolation interpolacionGuardada;
    private RigidbodyConstraints restriccionesGuardadas;

    private static readonly float[] entradaH = new float[MaxJugadores];
    private static readonly bool[] entradaSalto = new bool[MaxJugadores];
    private static readonly bool[] entradaCongelado = new bool[MaxJugadores];

    private static readonly Color[] Colores =
    {
        new Color(0.20f, 0.82f, 1.00f),
        new Color(1.00f, 0.55f, 0.15f),
        new Color(0.40f, 1.00f, 0.45f),
        new Color(1.00f, 0.30f, 0.75f),
    };

    public int Slot => slot;
    public int Victorias => victorias.Value;
    public int MascaraSuperadas => mascaraSuperadas.Value;
    public bool Final => final.Value;
    public int Version => version.Value;

    public Vector2 Posicion(int indice)
    {
        if (indice == 0) return pos0.Value;
        if (indice == 1) return pos1.Value;
        if (indice == 2) return pos2.Value;
        return pos3.Value;
    }

    public override void OnNetworkSpawn()
    {
        cuerpo = GetComponent<Rigidbody>();
        colision = GetComponent<CapsuleCollider>();
        visual = GetComponent<MeshRenderer>();
        skin = GetComponent<SkinJugador>();

        if (OwnerClientId == 0)
            Autoridad = this;

        if (IsOwner)
            Local = this;

        if (!Todos.Contains(this))
            Todos.Add(this);

        slot = IndiceJugador(OwnerClientId);
        ConfigurarSiTikTok();
    }

    public override void OnNetworkDespawn()
    {
        LimpiarEntrada(slot);
        if (Autoridad == this) Autoridad = null;
        if (Local == this) Local = null;
        Todos.Remove(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Autoridad == this) Autoridad = null;
        if (Local == this) Local = null;
        Todos.Remove(this);

        if (materialCapsula != null)
            Destroy(materialCapsula);
    }

    private void Update()
    {
        if (!HayRed) return;

        ActualizarSlot();

        bool esTikTok = SceneManager.GetActiveScene().name == "EscenaTiktok";

        if (!esTikTok)
        {
            if (enTikTok)
            {
                enTikTok = false;
                RestaurarCapsula();
            }

            return;
        }

        if (!enTikTok)
            ConfigurarSiTikTok();

        if (!enTikTok) return;
        if (slot < 0 || slot >= MaxJugadores) return;

        if (IsOwner && secuenciaEnlazada == null)
            EnlazarSecuencia();

        if (!IsServer)
        {
            if (Autoridad != null)
            {
                Vector2 p = Autoridad.Posicion(slot);

                if (p.y < 90000f)
                    transform.position = new Vector3(p.x, p.y, transform.position.z);
            }

            if (IsOwner)
                EnviarEntradaLocal();

            return;
        }

        if (IsOwner)
        {
            entradaH[slot] = EntradaTiktok.Horizontal();

            if (EntradaTiktok.Salto())
                entradaSalto[slot] = true;

            entradaCongelado[slot] =
                GestorMinijuegos.Instancia != null &&
                GestorMinijuegos.Instancia.PanelAbierto;
        }

        if (this == Autoridad)
            PublicarPosiciones();
    }

    private void FixedUpdate()
    {
        if (!HayRed || !IsServer || !enTikTok) return;
        if (cuerpo == null) return;
        if (slot < 0 || slot >= MaxJugadores) return;

        if (entradaCongelado[slot])
        {
            cuerpo.linearVelocity = Vector3.zero;
            saltoPedido = false;
            entradaSalto[slot] = false;
            enSuelo = false;
            return;
        }

        float vx = Mathf.Clamp(entradaH[slot], -1f, 1f) * velocidadHorizontal;
        float vy = cuerpo.linearVelocity.y;

        if (enSuelo && (saltoPedido || entradaSalto[slot]))
            vy = Mathf.Sqrt(2f * gravedadPersonalizada * alturaSalto);
        else
            vy -= gravedadPersonalizada * Time.fixedDeltaTime;

        cuerpo.linearVelocity = new Vector3(vx, vy, 0f);

        saltoPedido = false;
        entradaSalto[slot] = false;
        enSuelo = false;
    }

    private void OnCollisionEnter(Collision choque)
    {
        if (IsServer) RevisarSuelo(choque);
    }

    private void OnCollisionStay(Collision choque)
    {
        if (IsServer) RevisarSuelo(choque);
    }

    private void RevisarSuelo(Collision choque)
    {
        for (int i = 0; i < choque.contactCount; i++)
        {
            if (choque.GetContact(i).normal.y > 0.5f)
            {
                enSuelo = true;
                return;
            }
        }
    }

    private void PublicarPosiciones()
    {
        if (!IsServer || this != Autoridad) return;

        Vector2[] posiciones = new Vector2[MaxJugadores];

        for (int i = 0; i < MaxJugadores; i++)
            posiciones[i] = new Vector2(0f, 99999f);

        for (int i = 0; i < Todos.Count; i++)
        {
            RedTiktok jugador = Todos[i];

            if (jugador == null || !jugador.enTikTok) continue;
            if (jugador.slot < 0 || jugador.slot >= MaxJugadores) continue;

            Vector3 p = jugador.transform.position;
            posiciones[jugador.slot] = new Vector2(p.x, p.y);
        }

        FijarPosicion(pos0, posiciones[0]);
        FijarPosicion(pos1, posiciones[1]);
        FijarPosicion(pos2, posiciones[2]);
        FijarPosicion(pos3, posiciones[3]);
    }

    private static void FijarPosicion(NetworkVariable<Vector2> variable, Vector2 valor)
    {
        if ((variable.Value - valor).sqrMagnitude > 0.25f)
            variable.Value = valor;
    }

    public void FijarProgreso(int cantidadVictorias, int mascara)
    {
        if (!IsServer || this != Autoridad) return;

        bool cambio = false;

        if (victorias.Value != cantidadVictorias)
        {
            victorias.Value = cantidadVictorias;
            cambio = true;
        }

        if (mascaraSuperadas.Value != mascara)
        {
            mascaraSuperadas.Value = mascara;
            cambio = true;
        }

        if (cambio) version.Value++;
    }

    public void MarcarFinal()
    {
        if (!IsServer || this != Autoridad) return;
        if (final.Value) return;

        final.Value = true;
        version.Value++;
    }

    public void ReportarVictoria(int indice)
    {
        RedTiktok emisor = IsSpawned && IsOwner ? this : Local;

        if (emisor == null || !emisor.IsSpawned) return;
        emisor.VictoriaServerRpc(indice);
    }

    [ServerRpc]
    private void VictoriaServerRpc(int indice, ServerRpcParams parametros = default)
    {
        if (OwnerClientId == 0) return;
        Autoridad?.RecibirVictoriaCliente(indice);
    }

    private void RecibirVictoriaCliente(int indice)
    {
        if (!IsServer) return;
        GestorMinijuegos.Instancia?.RegistrarVictoriaRemota(indice);
    }

    [ServerRpc]
    private void EntradaServerRpc(float h, bool salto, bool congelado,
        ServerRpcParams parametros = default)
    {
        if (OwnerClientId == 0) return;

        int indice = IndiceJugador(parametros.Receive.SenderClientId);

        if (indice <= 0 || indice >= MaxJugadores) return;

        entradaH[indice] = Mathf.Clamp(h, -1f, 1f);
        entradaCongelado[indice] = congelado;

        if (salto)
            entradaSalto[indice] = true;
    }

    private void EnviarEntradaLocal()
    {
        float h = EntradaTiktok.Horizontal();
        bool salto = EntradaTiktok.Salto();
        bool congelado =
            GestorMinijuegos.Instancia != null &&
            GestorMinijuegos.Instancia.PanelAbierto;

        bool cambio = !Mathf.Approximately(h, ultimaH) ||
                      congelado != ultimoCongelado;

        if (!salto && !cambio && Time.unscaledTime < siguienteEnvio)
            return;

        ultimaH = h;
        ultimoCongelado = congelado;
        siguienteEnvio = Time.unscaledTime + 0.05f;

        EntradaServerRpc(h, salto, congelado);
    }

    public static int IndiceJugador(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return -1;

        IReadOnlyList<ulong> ids =
            NetworkManager.Singleton.ConnectedClientsIds;

        int indice = 0;

        for (int i = 0; i < ids.Count; i++)
        {
            if (ids[i] < clientId)
                indice++;
        }

        return indice;
    }

    private void ConfigurarSiTikTok()
    {
        if (SceneManager.GetActiveScene().name != "EscenaTiktok")
        {
            enTikTok = false;
            return;
        }

        enTikTok = true;

        DesactivarControlesDeOtrosNiveles();
        OcultarJugadorDeLaEscena();
        PrepararCapsula();
    }

    private void DesactivarControlesDeOtrosNiveles()
    {
        MovePlayer1[] movimientos = GetComponentsInChildren<MovePlayer1>(true);
        for (int i = 0; i < movimientos.Length; i++)
            movimientos[i].enabled = false;

        PlayerHit[] golpes = GetComponentsInChildren<PlayerHit>(true);
        for (int i = 0; i < golpes.Length; i++)
            golpes[i].enabled = false;

        hit[] golpesSimples = GetComponentsInChildren<hit>(true);
        for (int i = 0; i < golpesSimples.Length; i++)
            golpesSimples[i].enabled = false;

        NetworkTransform[] transformaciones =
            GetComponentsInChildren<NetworkTransform>(true);
        for (int i = 0; i < transformaciones.Length; i++)
            transformaciones[i].enabled = false;

        NetworkRigidbody[] redes =
            GetComponentsInChildren<NetworkRigidbody>(true);
        for (int i = 0; i < redes.Length; i++)
            redes[i].enabled = false;
    }

    private void OcultarJugadorDeLaEscena()
    {
        MovimientoTiktok[] jugadores =
            FindObjectsByType<MovimientoTiktok>(FindObjectsInactive.Include);

        for (int i = 0; i < jugadores.Length; i++)
        {
            if (jugadores[i] == null) continue;
            jugadores[i].gameObject.SetActive(false);
        }
    }

    private void PrepararCapsula()
    {
        if (!estadosGuardados && cuerpo != null)
        {
            estadosGuardados = true;
            escalaGuardada = transform.localScale;
            gravedadGuardada = cuerpo.useGravity;
            cinematicoGuardado = cuerpo.isKinematic;
            interpolacionGuardada = cuerpo.interpolation;
            restriccionesGuardadas = cuerpo.constraints;
        }

        transform.localScale = Vector3.one * 40f;
        transform.rotation = Quaternion.Euler(0f, 23f, 0f);

        if (slot >= 0 && slot < MaxJugadores)
        {
            float x = 538f + (slot - 1.5f) * 80f;
            transform.position = new Vector3(x, 90f, 0f);
        }

        OcultarSkins();
        MostrarCapsulaDeColor();

        if (colision != null)
            colision.enabled = IsServer;

        if (cuerpo != null)
        {
            cuerpo.useGravity = false;
            cuerpo.isKinematic = !IsServer;
            cuerpo.interpolation = RigidbodyInterpolation.Interpolate;
            cuerpo.constraints =
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotation;
            cuerpo.linearVelocity = Vector3.zero;
        }

        if (IsOwner)
            EnlazarSecuencia();
    }

    private void ActualizarSlot()
    {
        int nuevoSlot = IndiceJugador(OwnerClientId);
        if (nuevoSlot == slot) return;

        LimpiarEntrada(slot);
        slot = nuevoSlot;

        if (enTikTok && slot >= 0 && slot < MaxJugadores)
            MostrarCapsulaDeColor();
    }

    private static void LimpiarEntrada(int indice)
    {
        if (indice < 0 || indice >= MaxJugadores) return;

        entradaH[indice] = 0f;
        entradaSalto[indice] = false;
        entradaCongelado[indice] = false;
    }

    private void OcultarSkins()
    {
        if (skin == null || skin.Skins == null) return;

        for (int i = 0; i < skin.Skins.Length; i++)
        {
            if (skin.Skins[i].modelo != null)
                skin.Skins[i].modelo.SetActive(false);
        }
    }

    private void RestaurarSkins()
    {
        if (skin == null || skin.Skins == null || skin.Skins.Length == 0) return;

        int activa = Mathf.Clamp(skin.IndiceSkin, 0, skin.Skins.Length - 1);

        for (int i = 0; i < skin.Skins.Length; i++)
        {
            if (skin.Skins[i].modelo != null)
                skin.Skins[i].modelo.SetActive(i == activa);
        }
    }

    private void MostrarCapsulaDeColor()
    {
        if (visual == null) return;

        visual.enabled = true;

        Material materialBase = visual.sharedMaterial;
        if (materialBase == null) return;

        if (materialCapsula == null || materialCapsula.shader != materialBase.shader)
        {
            if (materialCapsula != null)
                Destroy(materialCapsula);

            materialCapsula = new Material(materialBase);
            materialCapsula.name = "CapsulaTiktok";
        }

        materialCapsula.color =
            Colores[Mathf.Clamp(slot, 0, Colores.Length - 1)];

        visual.material = materialCapsula;
    }

    private void EnlazarSecuencia()
    {
        SecuenciaTiktokDorado secuencia =
            FindAnyObjectByType<SecuenciaTiktokDorado>();

        if (secuencia != null)
        {
            secuencia.EstablecerJugador(transform);
            secuenciaEnlazada = secuencia;
        }
    }

    private void RestaurarCapsula()
    {
        // En Snapchat el RedSnapchat controla la capsula; no la toquemos aqui.
        if (SceneManager.GetActiveScene().name == "LevelSnapchat")
            return;

        LimpiarEntrada(slot);
        secuenciaEnlazada = null;

        transform.localScale = escalaGuardada == Vector3.zero
            ? Vector3.one * 0.5f
            : escalaGuardada;

        RestaurarSkins();

        if (visual != null)
            visual.enabled = false;

        if (colision != null)
            colision.enabled = true;

        if (cuerpo != null)
        {
            cuerpo.useGravity = estadosGuardados ? gravedadGuardada : true;
            cuerpo.isKinematic = estadosGuardados ? cinematicoGuardado : !IsOwner;
            cuerpo.interpolation = estadosGuardados
                ? interpolacionGuardada
                : RigidbodyInterpolation.Interpolate;
            cuerpo.constraints = estadosGuardados
                ? restriccionesGuardadas
                : RigidbodyConstraints.FreezeRotation;
            cuerpo.linearVelocity = Vector3.zero;
        }

        estadosGuardados = false;

        MovePlayer1[] movimientos = GetComponentsInChildren<MovePlayer1>(true);
        for (int i = 0; i < movimientos.Length; i++)
            movimientos[i].enabled = true;

        PlayerHit[] golpes = GetComponentsInChildren<PlayerHit>(true);
        for (int i = 0; i < golpes.Length; i++)
            golpes[i].enabled = true;

        hit[] golpesSimples = GetComponentsInChildren<hit>(true);
        for (int i = 0; i < golpesSimples.Length; i++)
            golpesSimples[i].enabled = true;

        NetworkTransform[] transformaciones =
            GetComponentsInChildren<NetworkTransform>(true);
        for (int i = 0; i < transformaciones.Length; i++)
            transformaciones[i].enabled = true;

        NetworkRigidbody[] redes =
            GetComponentsInChildren<NetworkRigidbody>(true);
        for (int i = 0; i < redes.Length; i++)
            redes[i].enabled = true;
    }
}

public static class EntradaTiktok
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

        if (mando != null &&
            (mando.buttonSouth.wasPressedThisFrame ||
             mando.buttonNorth.wasPressedThisFrame))
        {
            return true;
        }
#endif

        return false;
    }
}
