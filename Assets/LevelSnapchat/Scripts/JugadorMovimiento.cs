using UnityEngine;

public class JugadorMovimiento : MonoBehaviour
{
    [Tooltip("Velocidad normal en unidades por segundo.")]
    public float velocidad = 5f;

    [Tooltip("Velocidad al mantener Shift.")]
    public float velocidadCorrer = 9f;

    [Tooltip("Velocidad de giro hacia la direccion de movimiento.")]
    public float giro = 12f;

    private void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

        Vector3 direccion = new Vector3(horizontal, 0f, vertical);
        if (direccion.sqrMagnitude > 1f) direccion.Normalize();

        float actual = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            ? velocidadCorrer
            : velocidad;

        transform.position += direccion * actual * Time.deltaTime;

        if (direccion.sqrMagnitude > 0.001f)
        {
            Quaternion objetivo = Quaternion.LookRotation(direccion, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, giro * Time.deltaTime);
        }
    }
}
