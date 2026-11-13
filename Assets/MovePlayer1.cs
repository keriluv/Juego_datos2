using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MovePlayer1 : NetworkBehaviour
{
    [SerializeField] private float velocity = 1f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float friction = 5f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform target;

    [SerializeField] private bool isPlatform = false;

    [Header("Red (opcionales: solo si la cámara está dentro del prefab)")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerListener;

    private Rigidbody rb;
    private bool canJump = true;
    private Vector2 movement = Vector2.zero;

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
        Debug.Log("PLAYER: Awake ejecutado");

        rb = GetComponent<Rigidbody>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "LevelYoutubev2")
        {
            isPlatform = true;
        }
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

        if (IsOwner)
        {
            RefrescarCamara();

            if (JuegoOnlineEscena.HayPuntoAparicion)
                ColocarEnAparicion(JuegoOnlineEscena.PuntoAparicion, OwnerClientId);
        }

        networkSpeed.OnValueChanged += OnSpeedChanged;
        networkJump.OnValueChanged += OnJumpChanged;

        UpdateAnimations();

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

    public void ColocarEnAparicion(Vector3 posicionBase, ulong ownerId)
    {
        if (!ControlaEsteCliente) return;

        int indice = (int)(ownerId % 4u);
        Vector3 posicion = posicionBase + new Vector3(indice * 2.5f, 0.5f, 0f);

        transform.position = posicion;
        rb.position = posicion;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (!ControlaEsteCliente) return;
        if (Keyboard.current == null) return;

        movement = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) movement.y += velocity;
        if (Keyboard.current.sKey.isPressed) movement.y -= velocity;
        if (Keyboard.current.aKey.isPressed) movement.x -= velocity;
        if (Keyboard.current.dKey.isPressed) movement.x += velocity;


        float speed = movement.magnitude;

        networkSpeed.Value = speed;

        if (canJump && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
 
            networkJump.Value = true;
        }

        UpdateAnimations();

    }

    void FixedUpdate()
    {
        if (!ControlaEsteCliente) return;

        if (target == null) RefrescarCamara();
        if (target == null) return;


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
        networkSpeed.Value = horizontalVelocity.magnitude;
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
}
