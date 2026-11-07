using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    public Transform jugador;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float velocidadSuave = 5f;

    [Header("Límites de la Cámara")]
    public float minX = -4f;
    public float maxX = 4f;
    public float minY = -2f;
    public float maxY = 2f;

    void LateUpdate()
    {
        if (jugador != null)
        {
            Vector3 posicionDeseada = jugador.position + offset;

            // Restringe la posición X e Y de la cámara dentro de los límites
            float xLimitada = Mathf.Clamp(posicionDeseada.x, minX, maxX);
            float yLimitada = Mathf.Clamp(posicionDeseada.y, minY, maxY);

            Vector3 posicionFinal = new Vector3(xLimitada, yLimitada, posicionDeseada.z);

            transform.position = Vector3.Lerp(transform.position, posicionFinal, velocidadSuave * Time.deltaTime);
        }
    }
}