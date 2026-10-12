using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer1 : MonoBehaviour
{
    [SerializeField] private float velocity=1f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float friction = 5f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private Transform target;

    [SerializeField] private bool isPlatform = false;

    private Rigidbody rb;
    private bool canJump = true;
    private Vector2 movement = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
        movement = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) movement.y += velocity;
        if (Keyboard.current.sKey.isPressed) movement.y -= velocity;
        if (Keyboard.current.aKey.isPressed) movement.x -= velocity;
        if (Keyboard.current.dKey.isPressed) movement.x += velocity;

        if (canJump && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
        }
    }

    void FixedUpdate()
    {
        Vector3 forward = target.forward;
        Vector3 right = target.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * movement.y + right * movement.x;
        if (direction.sqrMagnitude > 1f) direction.Normalize();

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
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;
        }
    }
}