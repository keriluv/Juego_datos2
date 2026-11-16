using System;
using System.Collections;
using UnityEngine;

public class ObjetoColeccionable : MonoBehaviour
{
    [Header("Aparicion")]
    public float retrasoInicial = 1f;
    public float duracionAparicion = 0.6f;

    [Header("Animacion continua")]
    public float velocidadGiro = 90f;
    public float amplitudFlotar = 15f;
    public float velocidadFlotar = 2f;

    // Otros scripts (por ejemplo el de los enemigos) pueden suscribirse a esto
    public static event Action OnRecogido;

    private Vector3 escalaFinal;
    private Vector3 posicionBase;
    private bool activo = false;
    private bool recogido = false;

    void Start()
    {
        escalaFinal = transform.localScale;
        posicionBase = transform.position;
        transform.localScale = Vector3.zero; // empieza invisible
        StartCoroutine(Aparecer());
    }

    void Update()
    {
        // Gira sobre si misma
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f, Space.World);

        // Flota arriba y abajo
        if (activo && !recogido)
        {
            float offset = Mathf.Sin(Time.time * velocidadFlotar) * amplitudFlotar;
            transform.position = posicionBase + Vector3.up * offset;
        }
    }

    private IEnumerator Aparecer()
    {
        yield return new WaitForSeconds(retrasoInicial);

        float t = 0f;
        while (t < duracionAparicion)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracionAparicion);

            // Crece con un pequeño "rebote" al final
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float s = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);

            transform.localScale = escalaFinal * s;
            yield return null;
        }

        transform.localScale = escalaFinal;
        activo = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!activo || recogido) return;

        // Solo reacciona al jugador
        if (other.GetComponentInParent<MovimientoTiktok>() == null) return;

        recogido = true;
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        float duracion = 0.25f;
        float t = 0f;
        Vector3 escalaInicio = transform.localScale;

        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaInicio, Vector3.zero, t / duracion);
            yield return null;
        }

        Debug.Log("Objeto recogido: ahora aparecerian los enemigos");
        OnRecogido?.Invoke();
        gameObject.SetActive(false);
    }
}