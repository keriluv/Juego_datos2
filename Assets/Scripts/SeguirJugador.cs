using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    public Transform jugador;
    public Vector3 offset = new Vector3(0f, 3f, -8f);
    public float velocidadSuavizado = 5f;

    void LateUpdate()
    {
        if (jugador != null)
        {
            Vector3 posicionDeseada = jugador.position + offset;
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, velocidadSuavizado * Time.deltaTime);
            transform.position = posicionSuavizada;
        }
    }
}