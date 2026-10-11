using System.Collections;
using UnityEngine;

/// <summary>
/// Barco enemigo. Se acerca a una distancia aleatoria, rodea al jugador y dispara por intervalos.
/// </summary>
public class EnemyShipAI : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private VRShipCannon canon;
    [SerializeField] private float distanciaMinima = 14f;
    [SerializeField] private float distanciaMaxima = 28f;
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float intervaloMinimo = 2.4f;
    [SerializeField] private float intervaloMaximo = 4.2f;
    [SerializeField] private float rangoDisparo = 36f;

    private float angulo;
    private float radio;
    private float altura = 0.35f;
    private BarcoSalud salud;
    private Transform barraVida;

    public void Configurar(Transform blanco, VRShipCannon canonDelBarco, float anguloInicial, int oleada)
    {
        objetivo = blanco;
        canon = canonDelBarco;
        angulo = anguloInicial;
        radio = Random.Range(distanciaMinima, distanciaMaxima);
        velocidad = 4.5f + oleada * 0.35f;
        intervaloMinimo = Mathf.Max(1.1f, 2.6f - oleada * 0.15f);
        intervaloMaximo = intervaloMinimo + 1.4f;
        salud = GetComponent<BarcoSalud>();
        CrearBarraVida();
        StartCoroutine(DispararEnIntervalos());
    }

    private void Update()
    {
        if (salud != null && salud.EstaHundido)
            return;

        Vector3 centro = objetivo != null ? objetivo.position : Vector3.zero;
        float distancia = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(centro.x, 0f, centro.z));
        if (distancia > radio + 1f)
            radio = Mathf.MoveTowards(radio, distanciaMinima + 2f, Time.deltaTime * 1.5f);

        angulo += (velocidad / Mathf.Max(6f, radio)) * Time.deltaTime;
        Vector3 destino = new Vector3(centro.x + Mathf.Cos(angulo) * radio, altura, centro.z + Mathf.Sin(angulo) * radio);
        transform.position = Vector3.Lerp(transform.position, destino, 2f * Time.deltaTime);

        Vector3 hacia = centro - transform.position;
        hacia.y = 0f;
        if (hacia.sqrMagnitude > 0.2f)
        {
            Quaternion mirada = Quaternion.LookRotation(hacia.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, mirada, 2f * Time.deltaTime);
        }

        if (barraVida != null && salud != null)
        {
            float proporcion = salud.Vida / salud.VidaMaxima;
            barraVida.localScale = new Vector3(Mathf.Max(0.05f, 2.4f * proporcion), 0.12f, 0.12f);
        }
    }

    private IEnumerator DispararEnIntervalos()
    {
        yield return new WaitForSeconds(Random.Range(0.6f, 1.8f));

        while (salud == null || !salud.EstaHundido)
        {
            yield return new WaitForSeconds(Random.Range(intervaloMinimo, intervaloMaximo));
            if (canon == null || objetivo == null || salud.EstaHundido)
                continue;

            float distancia = Vector3.Distance(transform.position, objetivo.position);
            if (distancia > rangoDisparo)
                continue;

            Vector3 origen = canon.Boca != null ? canon.Boca.position : transform.position;
            canon.DispararHacia(objetivo.position + Vector3.up * 1.2f - origen);
        }
    }

    private void CrearBarraVida()
    {
        GameObject fondo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(fondo.GetComponent<Collider>());
        fondo.name = "FondoVida";
        fondo.transform.SetParent(transform, false);
        fondo.transform.localPosition = new Vector3(0f, 5.5f, 0f);
        fondo.transform.localScale = new Vector3(2.6f, 0.18f, 0.18f);
        fondo.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.15f, 0.05f, 0.05f));

        GameObject barra = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(barra.GetComponent<Collider>());
        barra.name = "BarraVida";
        barra.transform.SetParent(transform, false);
        barra.transform.localPosition = new Vector3(0f, 5.62f, 0f);
        barra.transform.localScale = new Vector3(2.4f, 0.12f, 0.12f);
        barra.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.85f, 0.2f, 0.15f));
        barraVida = barra.transform;
    }
}
