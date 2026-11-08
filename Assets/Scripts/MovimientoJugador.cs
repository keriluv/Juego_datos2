using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento lateral")]
    public float velocidadMovimiento = 5f;

    [Header("Salto")]
    [Tooltip("Fuerza aplicada al saltar")]
    public float fuerzaSalto = 7f;

    private Rigidbody rb;
    private bool enSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movimiento horizontal (A/D o Flechas izquierda/derecha)
        float movimientoH = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector3(movimientoH * velocidadMovimiento, rb.linearVelocity.y, 0f);

        // Salto (Barra espaciadora)
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Detecta si está tocando el suelo para permitir saltar de nuevo
        enSuelo = true;
    }
}