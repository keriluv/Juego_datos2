using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10001)]
public class MovePlayer1 : NetworkBehaviour
{
    [SerializeField] private float velocity = 1f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float friction = 5f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform target;
    private bool mover = true;
    private Vector3 startSize;

    [SerializeField] private bool isPlatform = false;

    [Header("Red (opcionales: solo si la cámara está dentro del prefab)")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerListener;

    private Rigidbody rb;
    private NetworkTransform transformRed;
    private NetworkRigidbody[] cuerposRed;
    private bool canJump = true;
    private Vector2 movement = Vector2.zero;
    private Vector3 anclaLobby;
    private bool hayAnclaLobby;
    private bool redLobbyDesactivada;

    private NetworkVariable<float> networkSpeed =
       new NetworkVariable<float>(
           0f,
           NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Owner
       );

    private NetworkVariable<bool> networkJump =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    [SerializeField] private Animator animator;

    [SerializeField] private float runMultiplier = 1.5f;
    private bool ControlaEsteCliente => !IsSpawned || IsOwner;

    void Awake()
    {
        startSize = transform.localScale;
        Debug.Log("PLAYER: Awake ejecutado");

        rb = GetComponent<Rigidbody>();
        transformRed = GetComponent<NetworkTransform>();
        cuerposRed = GetComponents<NetworkRigidbody>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Nested non-kinematic rigidbodies (e.g. hitbox "fff") fight the player body
        // and can shove it through the floor.
        foreach (Rigidbody hijo in GetComponentsInChildren<Rigidbody>(true))
        {
            if (hijo == rb) continue;
            hijo.isKinematic = true;
            hijo.useGravity = false;
        }

        // Capsule prefab accidentally has two NetworkRigidbody components; keep one.
        for (int i = 1; i < cuerposRed.Length; i++)
        {
            if (cuerposRed[i] != null)
                cuerposRed[i].enabled = false;
        }

        if (GetComponent<ReaparicionJugador>() == null)
            gameObject.AddComponent<ReaparicionJugador>();
        if (GetComponent<RelayTransicionMinecraft>() == null)
            gameObject.AddComponent<RelayTransicionMinecraft>();
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "LevelYoutubev2")
        {
            isPlatform = true;
        }
        AplicarModoLobbySiCorresponde(duranteSpawn: false);
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("IsOwner: " + IsOwner);
        Debug.Log("Target: " + target);
        Debug.Log("Rigidbody: " + rb);
        rb.isKinematic = !IsOwner;

        if (playerCamera != null) playerCamera.enabled = IsOwner;
        if (playerListener != null) playerListener.enabled = IsOwner;

        Debug.Log(
            "[Online] JUGADOR_SPAWN owner=" + OwnerClientId +
            " isOwner=" + IsOwner +
            " isServer=" + IsServer +
            " pos=" + transform.position);

        // During spawn, set the transform directly (NGO picks it up). Later
        // corrections must use NetworkTransform.Teleport or it snaps back.
        AplicarModoLobbySiCorresponde(duranteSpawn: true);

        if (IsOwner)
        {
            RefrescarCamara();

            if (JuegoOnlineEscena.HayPuntoAparicion)
                ColocarEnAparicion(JuegoOnlineEscena.PuntoAparicion, OwnerClientId, duranteSpawn: true);
        }

        networkSpeed.OnValueChanged += OnSpeedChanged;
        networkJump.OnValueChanged += OnJumpChanged;

        UpdateAnimations();

    }

    private bool EnLobby => SceneManager.GetActiveScene().name == "pruebaLobby";

    private void AplicarModoLobbySiCorresponde(bool duranteSpawn)
    {
        if (!EnLobby) return;

        mover = false;
        DesactivarSyncRedLobby();

        Vector3 escalaLobby = new Vector3(1.8f, 1.8f, 1.8f);
        AplicarPoseLocal(transform.position, transform.rotation, escalaLobby);
    }

    private void SalirModoLobbySiCorresponde()
    {
        if (EnLobby || mover) return;

        hayAnclaLobby = false;
        mover = true;

        AplicarPoseLocal(transform.position, transform.rotation, startSize);
        ReactivarSyncRed();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = !ControlaEsteCliente;
        }

        // Push the restored pose into NetworkTransform so peers stay aligned.
        if (transformRed != null && IsSpawned && IsOwner && transformRed.enabled)
            transformRed.Teleport(transform.position, transform.rotation, transform.localScale);
    }

    private void DesactivarSyncRedLobby()
    {
        if (redLobbyDesactivada) return;

        // Same approach as RedTiktok: while we own placement, NetworkTransform
        // must not run or it keeps restoring the prefab pose (-2, 1.74, 0).
        if (transformRed != null)
            transformRed.enabled = false;

        if (cuerposRed != null)
        {
            for (int i = 0; i < cuerposRed.Length; i++)
            {
                if (cuerposRed[i] != null)
                    cuerposRed[i].enabled = false;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        redLobbyDesactivada = true;
    }

    private void ReactivarSyncRed()
    {
        if (!redLobbyDesactivada) return;

        if (cuerposRed != null && cuerposRed.Length > 0 && cuerposRed[0] != null)
            cuerposRed[0].enabled = true;

        if (transformRed != null)
            transformRed.enabled = true;

        redLobbyDesactivada = false;
    }

    private void AplicarPoseLocal(Vector3 posicion, Quaternion rotacion, Vector3 escala)
    {
        transform.SetPositionAndRotation(posicion, rotacion);
        transform.localScale = escala;

        if (rb == null) return;

        rb.position = posicion;
        rb.rotation = rotacion;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    public override void OnNetworkDespawn()
    {
        networkSpeed.OnValueChanged -= OnSpeedChanged;
        networkJump.OnValueChanged -= OnJumpChanged;
    }

    public void RefrescarCamara()
    {
        if (!ControlaEsteCliente) return;

        if (playerCamera != null)
        {
            target = playerCamera.transform;
            return;
        }

        if (Camera.main != null)
            target = Camera.main.transform;
    }

    public void ColocarEnAparicion(Vector3 posicionBase, ulong ownerId, bool duranteSpawn = false)
    {
        if (!ControlaEsteCliente) return;

        if (EnLobby)
            AplicarModoLobbySiCorresponde(duranteSpawn);

        int indice = (int)(ownerId % 4u);
        float elevacion = AlturaPies() + 0.05f;
        Vector3 posicion = posicionBase + new Vector3(indice * 2.5f, elevacion, 0f);
        Vector3 escala = EnLobby ? new Vector3(1.8f, 1.8f, 1.8f) : transform.localScale;

        if (EnLobby)
        {
            DesactivarSyncRedLobby();
            AplicarPoseLocal(posicion, transform.rotation, escala);
            anclaLobby = posicion;
            hayAnclaLobby = true;
        }
        else if (!duranteSpawn && transformRed != null && IsSpawned && IsOwner && transformRed.enabled)
        {
            transformRed.Teleport(posicion, transform.rotation, escala);
            if (rb != null)
            {
                rb.position = posicion;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            AplicarPoseLocal(posicion, transform.rotation, escala);
        }

        Debug.Log(
            "[Online] COLOCADO objetivo=" + posicion +
            " actual=" + transform.position +
            " nt=" + (transformRed != null && transformRed.enabled) +
            " lobby=" + EnLobby);
    }

    private float AlturaPies()
    {
        CapsuleCollider capsula = GetComponent<CapsuleCollider>();
        float escalaY = Mathf.Abs(transform.localScale.y);
        if (EnLobby)
            escalaY = 1.8f;

        if (capsula != null)
            return capsula.height * 0.5f * escalaY;

        return escalaY * 0.5f;
    }

    void Update()
    {
        SalirModoLobbySiCorresponde();
        if (SceneManager.GetActiveScene().name == "LevelYoutubev2" && !isPlatform)
        {
            isPlatform = true;
        }
        else if (SceneManager.GetActiveScene().name != "LevelYoutubev2")
        {
            isPlatform = false;
        }
        if (!ControlaEsteCliente) return;
        if (PauseManager.IsPaused) return;

        movement = LeerMovimiento();

        float speed = movement.magnitude;
        if (IsSpawned) networkSpeed.Value = speed;

        ActualizarSuelo();

        if (canJump && ControlesJuego.QuiereSaltar())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
            if (IsSpawned) networkJump.Value = true;
        }

        UpdateAnimations();
    }

    private Vector2 LeerMovimiento()
    {
        Vector2 input = ControlesJuego.LeerMovimiento(velocity);
        if (ControlesJuego.QuiereCorrer())
            input *= runMultiplier;
        return input;
    }

    private void ActualizarSuelo()
    {
        // Restore jump without requiring only the "Platform" tag (fixes A appearing to "do nothing").
        float radio = 0.28f;
        float dist = 0.55f;
        Vector3 origen = transform.position + Vector3.up * 0.1f;
        if (!Physics.SphereCast(origen, radio, Vector3.down, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            return;
        if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            return;

        if (!canJump)
            canJump = true;
        if (IsSpawned && networkJump.Value)
            networkJump.Value = false;
    }

    void FixedUpdate()
    {
        if (EnLobby)
        {
            DesactivarSyncRedLobby();

            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Keep forcing the lobby pose; without NetworkTransform disabled,
            // NGO restores the Capsule prefab pose (-2, 1.74, 0).
            if (hayAnclaLobby)
                AplicarPoseLocal(anclaLobby, transform.rotation, new Vector3(1.8f, 1.8f, 1.8f));

            return;
        }

        if (!ControlaEsteCliente) return;
        if (PauseManager.IsPaused) return;

        if (target == null) RefrescarCamara();
        if (target == null) return;
        if (!mover) return;

        Vector3 forward = target.forward;
        Vector3 right = target.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * movement.y + right * movement.x;
        if (direction.sqrMagnitude > 1f) direction.Normalize();

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        Vector3 targetVelocity = direction * maxSpeed;

        float currentAcceleration = direction.sqrMagnitude > 0 ? acceleration : friction;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            currentAcceleration * Time.fixedDeltaTime
        );

        if (isPlatform)
        {
            horizontalVelocity.z = 0f;
        }

        rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
        if (IsSpawned) networkSpeed.Value = horizontalVelocity.magnitude;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float currentSpeed = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        ).magnitude;

        animator.SetFloat("speed", currentSpeed);
        animator.SetBool("isJumping", networkJump.Value);
    }
    private void OnSpeedChanged(float previous, float current)
    {
        UpdateAnimations();
    }
    private void OnJumpChanged(bool previous, bool current)
    {
        UpdateAnimations();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ControlaEsteCliente) return;
        if (collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;
            networkJump.Value = false;

            UpdateAnimations();
        }
    }
    public void AsignarAnimator(Animator nuevo)
    {
        animator = nuevo;
    }
    public void NoMover()
    {
        mover = false;
    }
}
