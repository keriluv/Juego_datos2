using UnityEngine;
using UnityEngine.InputSystem;

public class BoatCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float distance = 8f;
    [SerializeField] private float height = 3f;

    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float stickSensitivity = 120f;
    [SerializeField] private float deadzone = 0.2f;

    private float rotationX = 20f;
    private float rotationY = 0f;

    private float SensibilidadActual
    {
        get
        {
            float multi = AjustesJuego.Instancia != null
                ? AjustesJuego.Instancia.MultiplicadorSensibilidad
                : 1f;
            return mouseSensitivity * multi;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        BuscarJugadorLocal();
    }

    void Update()
    {
        if (PauseManager.IsPaused) return;
        if (target == null) BuscarJugadorLocal();

        float sens = SensibilidadActual;
        Vector2 look = Vector2.zero;

        if (Mouse.current != null)
            look += Mouse.current.delta.ReadValue() * sens;

        Vector2 stick = ControlesJuego.LeerMiradaStick(deadzone);
        if (stick.sqrMagnitude > 0f)
        {
            float multi = AjustesJuego.Instancia != null
                ? AjustesJuego.Instancia.MultiplicadorSensibilidad
                : 1f;
            look += stick * stickSensitivity * multi * Time.deltaTime;
        }

        rotationY += look.x;
        rotationX -= look.y;
        rotationX = Mathf.Clamp(rotationX, -10f, 60f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void BuscarJugadorLocal()
    {
        MovePlayer1[] barcos =
            FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude);

        foreach (MovePlayer1 barco in barcos)
        {
            if (barco.IsSpawned && barco.IsOwner)
            {
                target = barco.transform;
                return;
            }
        }

        JugadorRed[] jugadores =
            FindObjectsByType<JugadorRed>(FindObjectsInactive.Exclude);

        foreach (JugadorRed jugador in jugadores)
        {
            if (jugador.IsSpawned && jugador.IsOwner)
            {
                target = jugador.transform;
                return;
            }
        }
    }
}
