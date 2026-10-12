using UnityEngine;

public class AparicionJugador : MonoBehaviour
{
    private Vector3 escalaOriginal;
    private float duracionAnimacion = 0.4f; // Tiempo que tarda en crecer (en segundos)
    private float tiempo = 0f;
    private bool estaApareciendo = true;

    void Start()
    {
        // Guardamos el tamaño real que tiene la cápsula
        escalaOriginal = transform.localScale;
        
        // Al iniciar el juego, hacemos que la cápsula empiece en tamaño cero (oculta)
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (estaApareciendo)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracionAnimacion;

            // Hace crecer suavemente la cápsula desde 0 hasta su tamaño original
            transform.localScale = Vector3.Lerp(Vector3.zero, escalaOriginal, progreso);

            // Detenemos la animación cuando llega al 100%
            if (progreso >= 1f)
            {
                transform.localScale = escalaOriginal;
                estaApareciendo = false;
            }
        }
    }
}