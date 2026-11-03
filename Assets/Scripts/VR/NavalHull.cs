using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Casco con vida. Al llegar a cero se hunde sin tirar el rig de VR.
/// </summary>
public class NavalHull : MonoBehaviour
{
    [SerializeField] private float vida = 90f;
    [SerializeField] private bool esJugador;

    public float Vida => vida;
    public float VidaMaxima { get; private set; }
    public bool Hundido { get; private set; }
    public bool EsJugador => esJugador;
    public bool EsPractica { get; set; }
    public event Action<NavalHull> AlHundirse;

    public void Configurar(float vidaInicial, bool jugador)
    {
        vida = vidaInicial;
        VidaMaxima = vidaInicial;
        esJugador = jugador;
    }

    public void RecibirDanio(float cantidad)
    {
        if (Hundido)
            return;

        vida -= cantidad;
        if (esJugador)
            HapticsUtility.SendHapticImpulse(0.85f, 0.16f, HapticsUtility.Controller.Both);
        if (vida > 0f)
            return;

        vida = 0f;
        Hundido = true;
        AlHundirse?.Invoke(this);
        StartCoroutine(Hundir());
    }

    private IEnumerator Hundir()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        if (cuerpo != null)
        {
            cuerpo.useGravity = true;
            cuerpo.constraints = RigidbodyConstraints.None;
        }

        float t = 0f;
        Quaternion giro = transform.rotation * Quaternion.Euler(18f, 0f, 28f);
        while (t < 1f)
        {
            t += Time.deltaTime / 3.2f;
            transform.position += Vector3.down * Time.deltaTime * 0.8f;
            transform.rotation = Quaternion.Slerp(transform.rotation, giro, Time.deltaTime);
            yield return null;
        }

        if (!esJugador)
            Destroy(gameObject, 1.5f);
    }
}
