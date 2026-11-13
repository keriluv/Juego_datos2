using UnityEngine;

public class MovimientoTiktok : MonoBehaviour
{
    private Rigidbody rb;
    
    [Header("Ajustes de Movimiento y Fisica UI")]
    public float velocidadHorizontal = 500f; 
    public float fuerzaSalto = 700f;         
    public float gravedadPersonalizada = 2000f; // Caída rápida sin lag ni efecto lunar
    
    private float velocidadVertical = 0f;
    private bool enSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Desactivamos la gravedad nativa de Unity para controlar la caída por código
        rb.useGravity = false;
    }

    void Update()
    {
        // 1. Movimiento horizontal con flechas o A/D
        float movimientoH = Input.GetAxisRaw("Horizontal");
        float vx = movimientoH * velocidadHorizontal;

        // 2. Control de salto y caída
        if (enSuelo)
        {
            velocidadVertical = 0f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocidadVertical = fuerzaSalto;
                enSuelo = false;
            }
        }
        else
        {
            // Aplica la gravedad personalizada progresiva hacia abajo
            velocidadVertical -= gravedadPersonalizada * Time.deltaTime;
        }

        // 3. Aplicar velocidad final al Rigidbody
        rb.linearVelocity = new Vector3(vx, velocidadVertical, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Detecta el suelo y los bloques sólidos por su nombre para evitar errores de Tags
        string nombreColision = collision.gameObject.name;
        
        if (nombreColision.Contains("Borde") || 
            nombreColision.Contains("Pared") || 
            nombreColision.Contains("Tangible"))
        {
            enSuelo = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Si sale de la superficie de un bloque, entra en estado de aire
        string nombreColision = collision.gameObject.name;

        if (nombreColision.Contains("Borde") || 
            nombreColision.Contains("Pared") || 
            nombreColision.Contains("Tangible"))
        {
            enSuelo = false;
        }
    }
}