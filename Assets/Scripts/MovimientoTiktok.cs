using UnityEngine;

public class MovimientoTiktok : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Ajustes de Movimiento y Fisica UI")]
    public float velocidadHorizontal = 400f;
    public float gravedadPersonalizada = 2000f;

    [Tooltip("Altura del salto en unidades de la escena (aprox. pixeles)")]
    public float alturaSalto = 200f;

    private bool enSuelo = false;
    private bool saltoPedido = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // la gravedad la controlamos por codigo
        rb.interpolation = RigidbodyInterpolation.Interpolate; // movimiento mas suave
    }

    void Update()
    {
        // La entrada se lee en Update para no perder pulsaciones
        if (Input.GetKeyDown(KeyCode.Space))
        {
            saltoPedido = true;
        }
    }

    void FixedUpdate()
    {
        float vx = Input.GetAxisRaw("Horizontal") * velocidadHorizontal;
        float vy = rb.linearVelocity.y;

        if (enSuelo && saltoPedido)
        {
            // Velocidad inicial necesaria para alcanzar la altura deseada
            vy = Mathf.Sqrt(2f * gravedadPersonalizada * alturaSalto);
        }
        else
        {
            vy -= gravedadPersonalizada * Time.fixedDeltaTime;
        }

        rb.linearVelocity = new Vector3(vx, vy, 0f);

        saltoPedido = false;
        enSuelo = false; // se vuelve a marcar si hay contacto en este paso de fisica
    }

    void OnCollisionEnter(Collision collision)
    {
        RevisarSuelo(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        RevisarSuelo(collision);
    }

    private void RevisarSuelo(Collision collision)
    {
        // Es suelo si algun punto de contacto empuja hacia arriba.
        // Asi funciona con cualquier plataforma, sin depender de nombres.
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                enSuelo = true;
                return;
            }
        }
    }
}