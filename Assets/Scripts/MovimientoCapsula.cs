using UnityEngine;

public class MovimientoCapsula : MonoBehaviour
{
    public float velocidad = 5f;

    [Header("Configuración de Salto")]
    public float fuerzaSalto = 7f; // Ajusta este valor desde el Inspector
    
    private Rigidbody rb;
    private bool enElSuelo = true;
    private bool solicitarSalto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Detectar si presionas Espacio estando en el suelo
        if (Input.GetKeyDown(KeyCode.Space) && enElSuelo)
        {
            solicitarSalto = true;
        }
    }

    void FixedUpdate()
    {
        // Movimiento original por fuerza
        float movimientoAdelante = Input.GetAxis("Vertical");
        float movimientoLateral = Input.GetAxis("Horizontal");

        Vector3 fuerza = new Vector3(movimientoLateral, 0.0f, movimientoAdelante) * velocidad;
        rb.AddForce(fuerza);

        // Aplicar la fuerza del salto dentro de FixedUpdate (para físicas precisas)
        if (solicitarSalto)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enElSuelo = false;
            solicitarSalto = false;
        }
    }

    // Detectar cuando vuelve a tocar cualquier objeto sólido (suelo/obstáculos)
    private void OnCollisionEnter(Collision collision)
    {
        enElSuelo = true;
    }
}