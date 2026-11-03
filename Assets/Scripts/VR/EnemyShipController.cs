using UnityEngine;

/// <summary>
/// Patrulla por delante del jugador y dispara por intervalos.
/// La flotación la resuelve WaterFloatBehavior; aquí solo se empuja en horizontal.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavalHull))]
public class EnemyShipController : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private NavalCannon canon;
    [SerializeField] private float radio = 14f;
    [SerializeField] private float velocidad = 3.2f;
    [SerializeField] private float intervalo = 3.4f;
    [SerializeField] private float rango = 32f;

    private Rigidbody cuerpo;
    private NavalHull casco;
    private float angulo;
    private float siguienteDisparo;

    public void Configurar(Transform blanco, NavalCannon canonDelBarco, float anguloInicial, float radioInicial)
    {
        objetivo = blanco;
        canon = canonDelBarco;
        angulo = anguloInicial;
        radio = radioInicial;
        siguienteDisparo = Time.time + Random.Range(1.2f, 2.6f);
    }

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        casco = GetComponent<NavalHull>();
    }

    private void FixedUpdate()
    {
        if (casco != null && casco.Hundido)
            return;

        Vector3 centro = objetivo != null ? objetivo.position : Vector3.zero;
        angulo += (velocidad / Mathf.Max(8f, radio)) * Time.fixedDeltaTime;
        Vector3 destino = new Vector3(centro.x + Mathf.Cos(angulo) * radio, cuerpo.position.y, centro.z + Mathf.Sin(angulo) * radio + 6f);
        Vector3 deseo = destino - cuerpo.position;
        deseo.y = 0f;
        if (deseo.sqrMagnitude > 0.2f)
        {
            Vector3 planar = deseo.normalized * velocidad;
            Vector3 velocidadActual = cuerpo.linearVelocity;
            cuerpo.linearVelocity = new Vector3(planar.x, velocidadActual.y, planar.z);
            Quaternion mirada = Quaternion.LookRotation(deseo.normalized, Vector3.up);
            cuerpo.MoveRotation(Quaternion.Slerp(cuerpo.rotation, mirada, 2.2f * Time.fixedDeltaTime));
        }
    }

    private void Update()
    {
        if (casco != null && casco.Hundido)
            return;
        if (canon == null || objetivo == null || Time.time < siguienteDisparo)
            return;

        float distancia = Vector3.Distance(transform.position, objetivo.position);
        if (distancia > rango)
            return;

        siguienteDisparo = Time.time + intervalo;
        Vector3 direccion = (objetivo.position + Vector3.up * 1.2f) - canon.Boca.position;
        canon.DispararHacia(direccion);
    }
}
