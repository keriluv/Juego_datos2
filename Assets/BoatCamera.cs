using UnityEngine;
using UnityEngine.InputSystem;

public class BoatCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float distance = 8f;
    [SerializeField] private float height = 3f;

    [SerializeField] private float mouseSensitivity = 2f;

    private float rotationX = 20f;
    private float rotationY = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue();

        rotationY += mouse.x * mouseSensitivity;
        rotationX -= mouse.y * mouseSensitivity;

        rotationX = Mathf.Clamp(rotationX, -10f, 60f);
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        transform.position = target.position + offset;

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}