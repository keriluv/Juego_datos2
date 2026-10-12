using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Vida de un barco. Al llegar a cero se hunde y avisa al director de la batalla.
/// </summary>
public class BarcoSalud : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private int equipo = 1;
    [SerializeField] private bool esJugador;

    public float Vida { get; private set; }
    public float VidaMaxima => vidaMaxima;
    public int Equipo => equipo;
    public bool EsJugador => esJugador;
    public bool EstaHundido { get; private set; }

    public event Action<BarcoSalud> AlHundirse;
    public event Action<BarcoSalud, float> AlRecibirDanio;

    private void Awake()
    {
        Vida = vidaMaxima;
    }

    public void Configurar(float vida, int equipoBarco, bool jugador)
    {
        vidaMaxima = Mathf.Max(1f, vida);
        Vida = vidaMaxima;
        equipo = equipoBarco;
        esJugador = jugador;
    }

    public void RecibirDanio(float cantidad, int equipoAtacante)
    {
        if (EstaHundido || cantidad <= 0f || equipoAtacante == equipo)
            return;

        Vida = Mathf.Max(0f, Vida - cantidad);
        AlRecibirDanio?.Invoke(this, cantidad);

        if (Vida <= 0f)
            StartCoroutine(Hundir());
    }

    private IEnumerator Hundir()
    {
        if (EstaHundido)
            yield break;

        EstaHundido = true;

        foreach (var colisionador in GetComponentsInChildren<Collider>())
            colisionador.enabled = false;

        AlHundirse?.Invoke(this);

        Vector3 inicio = transform.position;
        Quaternion rotacionInicial = transform.rotation;
        float tiempo = 0f;
        const float duracion = 2.4f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float avance = Mathf.Clamp01(tiempo / duracion);
            transform.position = inicio + Vector3.down * (avance * 4f);
            transform.rotation = rotacionInicial * Quaternion.Euler(avance * 28f, 0f, avance * 18f);
            yield return null;
        }

        if (!esJugador)
            gameObject.SetActive(false);
    }
}
