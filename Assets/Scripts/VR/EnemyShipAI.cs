using System.Collections;
using UnityEngine;

/// <summary>
/// Barco enemigo. Se acerca a una distancia aleatoria, rodea al jugador y dispara por intervalos.
/// </summary>
public class EnemyShipAI : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private VRShipCannon canon;
    [SerializeField] private float stoppingDistance = 26f;
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

    private bool esAliado;
    private bool enLinea;
    private Vector3 puntoReunion;
    private float proximaBusqueda;

    public void Configurar(Transform blanco, VRShipCannon canonDelBarco, float anguloInicial, int oleada)
    {
        objetivo = blanco;
        canon = canonDelBarco;
        stoppingDistance = 26f;
        velocidad = 4.5f + oleada * 0.35f;
        intervaloMinimo = Mathf.Max(1.1f, 2.6f - oleada * 0.15f);
        intervaloMaximo = intervaloMinimo + 1.4f;
        salud = GetComponent<BarcoSalud>();
        CrearBarraVida(new Color(0.85f, 0.2f, 0.15f));
        StartCoroutine(DispararEnIntervalos());
    }

    public void ConfigurarAliado(Transform jugador, VRShipCannon canonDelBarco, Vector3 reunion, float velocidadSalida)
    {
        esAliado = true;
        if (jugador == null)
            return;
        objetivo = null;
        canon = canonDelBarco;
        puntoReunion = reunion;
        stoppingDistance = 22f;
        rangoDisparo = 42f;
        velocidad = Mathf.Max(3f, velocidadSalida);
        intervaloMinimo = 2.1f;
        intervaloMaximo = 3.4f;
        salud = GetComponent<BarcoSalud>();
        CrearBarraVida(new Color(0.25f, 0.55f, 0.9f));
        StartCoroutine(DispararEnIntervalos());
    }

    private void Update()
    {
        if (salud != null && salud.EstaHundido)
            return;

        if (esAliado && !enLinea)
        {
            AvanzarHacia(puntoReunion, 5f);
            if (DistanciaPlana(puntoReunion) < 5f)
                enLinea = true;
            ActualizarBarra();
            return;
        }

        if (esAliado)
            ElegirEnemigo();

        if (objetivo == null)
            return;

        AvanzarHacia(objetivo.position, stoppingDistance);
        ActualizarBarra();
    }

    private void AvanzarHacia(Vector3 destino, float distanciaDeParada)
    {
        Vector3 hacia = destino - transform.position;
        hacia.y = 0f;
        float distancia = hacia.magnitude;
        if (distancia <= 0.5f)
            return;

        Vector3 direccion = hacia / distancia;
        if (distancia > distanciaDeParada)
            transform.position += direccion * velocidad * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion, Vector3.up), 3f * Time.deltaTime);
    }

    private float DistanciaPlana(Vector3 destino)
    {
        Vector3 delta = destino - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private void ElegirEnemigo()
    {
        if (Time.time < proximaBusqueda && objetivo != null)
            return;

        proximaBusqueda = Time.time + 0.7f;
        BarcoSalud[] cascos = FindObjectsByType<BarcoSalud>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float mejor = float.MaxValue;
        Transform elegido = null;
        for (int i = 0; i < cascos.Length; i++)
        {
            BarcoSalud casco = cascos[i];
            if (casco == null || casco.EstaHundido || casco.EsJugador || casco.Equipo == 0)
                continue;

            float distancia = DistanciaPlana(casco.transform.position);
            if (distancia < mejor)
            {
                mejor = distancia;
                elegido = casco.transform;
            }
        }

        if (elegido != null)
            objetivo = elegido;
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

    private void ActualizarBarra()
    {
        if (barraVida == null || salud == null || salud.VidaMaxima <= 0f)
            return;

        float proporcion = salud.Vida / salud.VidaMaxima;
        barraVida.localScale = new Vector3(Mathf.Max(0.04f, 1.4f * proporcion), 0.08f, 0.08f);
    }

    private void CrearBarraVida(Color color)
    {
        GameObject fondo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(fondo.GetComponent<Collider>());
        fondo.name = "FondoVida";
        fondo.transform.SetParent(transform, false);
        fondo.transform.localPosition = new Vector3(0f, 8.2f, 0f);
        fondo.transform.localScale = new Vector3(1.6f, 0.1f, 0.08f);
        fondo.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.15f, 0.05f, 0.05f));

        GameObject barra = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(barra.GetComponent<Collider>());
        barra.name = "BarraVida";
        barra.transform.SetParent(transform, false);
        barra.transform.localPosition = new Vector3(0f, 8.28f, 0f);
        barra.transform.localScale = new Vector3(1.4f, 0.08f, 0.08f);
        barra.GetComponent<Renderer>().material = MaterialBarco.Crear(color);
        barraVida = barra.transform;
    }
}
