using UnityEngine;

/// <summary>
/// Barco rival. Navega en círculo alrededor del jugador y dispara con su cañón.
/// </summary>
public class BarcoEnemigo : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private CanonPirata canon;
    [SerializeField] private float radio = 22f;
    [SerializeField] private float velocidad = 6f;
    [SerializeField] private float altura = 0.35f;

    private float angulo;
    private BarcoSalud salud;

    public void Configurar(Transform blanco, CanonPirata canonDelBarco, float radioOrbita, float anguloInicial)
    {
        objetivo = blanco;
        canon = canonDelBarco;
        radio = radioOrbita;
        angulo = anguloInicial;
        salud = GetComponent<BarcoSalud>();
    }

    private void Update()
    {
        if (salud != null && salud.EstaHundido)
            return;

        angulo += (velocidad / Mathf.Max(4f, radio)) * Time.deltaTime;
        Vector3 centro = objetivo != null ? objetivo.position : Vector3.zero;
        Vector3 destino = new Vector3(centro.x + Mathf.Cos(angulo) * radio, altura, centro.z + Mathf.Sin(angulo) * radio);
        transform.position = destino;

        Vector3 hacia = centro - transform.position;
        hacia.y = 0f;
        if (hacia.sqrMagnitude > 0.2f)
        {
            Quaternion mirada = Quaternion.LookRotation(hacia.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, mirada, 2.5f * Time.deltaTime);
        }

        if (canon != null && objetivo != null)
            canon.IntentarDisparoAutomatico(objetivo);
    }
}
