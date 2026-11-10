using UnityEngine;

public class JugadorPlataformas2D : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad horizontal en unidades por segundo.")]
    public float velocidad = 4.5f;

    [Tooltip("Impulso del salto.")]
    public float fuerzaSalto = 9.5f;

    [Tooltip("Cuanto mas rapido cae para que el salto no se sienta flotante.")]
    public float multiplicadorCaida = 2.2f;

    [Header("Suelo")]
    [Tooltip("Punto en los pies para detectar el suelo.")]
    public Transform pies;

    [Tooltip("Radio de deteccion del suelo.")]
    public float radioPies = 0.12f;

    private Rigidbody2D cuerpo;
    private bool saltoPedido;
    private float coyote;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            saltoPedido = true;
        }
    }

    private void FixedUpdate()
    {
        bool enSuelo = ComprobarSuelo();

        // Un pelin de coyote time para que el salto perdone fallos de timing.
        if (enSuelo) coyote = 0.12f;
        else coyote -= Time.fixedDeltaTime;

        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        Vector2 velocidadActual = cuerpo.linearVelocity;
        velocidadActual.x = horizontal * velocidad;

        if (saltoPedido && coyote > 0f)
        {
            velocidadActual.y = fuerzaSalto;
            coyote = 0f;
        }

        saltoPedido = false;

        // Caida mas rapida.
        if (velocidadActual.y < 0f)
            velocidadActual.y += Physics2D.gravity.y * (multiplicadorCaida - 1f) * Time.fixedDeltaTime;

        cuerpo.linearVelocity = velocidadActual;
    }

    private bool ComprobarSuelo()
    {
        Vector3 punto = pies != null
            ? pies.position
            : transform.position + Vector3.down * 0.45f;

        Collider2D[] choques = Physics2D.OverlapCircleAll(punto, radioPies);

        foreach (Collider2D choque in choques)
        {
            if (choque.transform == transform) continue;
            if (choque.transform.IsChildOf(transform)) continue;
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 punto = pies != null
            ? pies.position
            : transform.position + Vector3.down * 0.45f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(punto, radioPies);
    }
}
