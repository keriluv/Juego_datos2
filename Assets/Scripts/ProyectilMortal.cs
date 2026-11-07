using UnityEngine;
using UnityEngine.SceneManagement;

public class ProyectilMortal : MonoBehaviour
{
    [Header("Configuración de Velocidad")]
    public float velocidad = 70f; // ¡Puedes cambiar este número en el Inspector a tu gusto!
    public float tiempoVida = 5f;

    private Vector3 direccionAtaque;

    void Start()
    {
        Destroy(gameObject, tiempoVida);

        ControlJugador jugador = FindAnyObjectByType<ControlJugador>();

        if (jugador != null)
        {
            direccionAtaque = (jugador.transform.position - transform.position).normalized;
            direccionAtaque.z = 0;
        }
        else
        {
            direccionAtaque = Vector3.left;
        }
    }

    void Update()
    {
        transform.position += direccionAtaque * velocidad * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<ControlJugador>() != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}