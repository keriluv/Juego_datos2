using UnityEngine;

public class ControlJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 6f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public KeyCode teclaSalto = KeyCode.Space;

    private Rigidbody rb;
    private bool enSuelo = true;

    // Estados de control para el tutorial
    [HideInInspector] public bool movimientoHabilitado = false;
    [HideInInspector] public bool saltoHabilitado = false;
    [HideInInspector] public bool yaSeMovio = false;
    [HideInInspector] public bool yaSalto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Movimiento horizontal (solo si está habilitado por Clippy)
        float movimientoH = 0f;
        if (movimientoHabilitado)
        {
            movimientoH = Input.GetAxis("Horizontal");
            
            // Detectar si el jugador presionó A o D por primera vez
            agentDetectarMovimiento(movimientoH);
        }

        rb.linearVelocity = new Vector3(movimientoH * velocidadMovimiento, rb.linearVelocity.y, rb.linearVelocity.z);

        // Lógica de Salto (solo si el salto está habilitado)
        if (saltoHabilitado && Input.GetKeyDown(teclaSalto) && enSuelo)
        {
            if (!yaSalto)
            {
                yaSalto = true; // Marca que ya saltó para avisarle a Clippy
            }

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    void agentDetectarMovimiento(float valorH)
    {
        if (Mathf.Abs(valorH) > 0.1f && !yaSeMovio)
        {
            yaSeMovio = true; // Marca que ya se movió para avisarle a Clippy
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        enSuelo = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        enSuelo = false;
    }
}