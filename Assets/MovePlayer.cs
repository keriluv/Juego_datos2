using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private Transform Camera;

    [SerializeField] private float Speed = 10f;
    private Vector2 moveInput;
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    void Update()
    {
        Vector3 forward = Camera.forward;
        Vector3 right = Camera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveInput.y + right * moveInput.x;

        if (movement.magnitude > 0.1f)
        {
            transform.position += movement.normalized * Speed * Time.deltaTime;

            transform.rotation = Quaternion.LookRotation(movement);
        }
    }
}

