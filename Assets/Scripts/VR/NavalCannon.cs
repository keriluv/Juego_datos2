using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Cañón montado. Grip apunta, Trigger dispara. El proyectil sale de la boca.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class NavalCannon : MonoBehaviour
{
    [SerializeField] private float dano = 28f;
    [SerializeField] private float velocidad = 22f;
    [SerializeField] private float enfriamiento = 1.05f;
    [SerializeField] private bool delJugador = true;

    public Transform Boca { get; private set; }
    public NavalVfxPool Efectos { get; set; }
    public event System.Action AlAgarrar;
    public event System.Action AlDisparar;
    public bool Agarrado => agarrado;

    private XRGrabInteractable agarre;
    private bool agarrado;
    private float siguiente;
    private Quaternion rotacionBase;

    public void DefinirJugador(bool jugador)
    {
        delJugador = jugador;
        if (agarre != null)
            agarre.enabled = jugador;
    }

    private void Awake()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;

        if (GetComponent<Collider>() == null)
        {
            BoxCollider caja = gameObject.AddComponent<BoxCollider>();
            caja.size = new Vector3(0.42f, 0.36f, 1.35f);
            caja.center = new Vector3(0f, 0.05f, 0.2f);
        }

        GameObject boca = new GameObject("Boca");
        boca.transform.SetParent(transform, false);
        boca.transform.localPosition = new Vector3(0f, 0.08f, 0.85f);
        Boca = boca.transform;
        rotacionBase = transform.localRotation;

        agarre = gameObject.AddComponent<XRGrabInteractable>();
        agarre.movementType = XRBaseInteractable.MovementType.Instantaneous;
        agarre.trackPosition = false;
        agarre.trackRotation = false;
        agarre.throwOnDetach = false;
        agarre.retainTransformParent = true;
        agarre.selectEntered.AddListener(_ =>
        {
            agarrado = true;
            AlAgarrar?.Invoke();
        });
        agarre.selectExited.AddListener(_ =>
        {
            agarrado = false;
            transform.localRotation = rotacionBase;
        });
        agarre.activated.AddListener(args =>
        {
            if (!agarrado)
                return;
            DispararHacia(Boca.forward);
            if (args.interactorObject is Component componente)
            {
                HapticImpulsePlayer haptico = componente.GetComponentInParent<HapticImpulsePlayer>();
                if (haptico != null)
                    haptico.SendHapticImpulse(0.65f, 0.1f);
            }
        });
        agarre.enabled = delJugador;
    }

    private void Update()
    {
        if (!delJugador || !agarrado || agarre.interactorsSelecting.Count == 0)
            return;

        Transform mano = agarre.interactorsSelecting[0].GetAttachTransform(agarre);
        Vector3 direccion = mano.position - transform.position;
        direccion.y = Mathf.Clamp(direccion.y, -0.45f, 0.45f);
        if (direccion.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.LookRotation(direccion.normalized, Vector3.up);
    }

    public void DispararHacia(Vector3 direccion)
    {
        if (Time.time < siguiente || direccion.sqrMagnitude < 0.01f)
            return;

        siguiente = Time.time + enfriamiento;
        AlDisparar?.Invoke();
        Quaternion rotacion = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        if (Efectos != null)
            Efectos.Disparo(Boca.position, rotacion);

        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "Proyectil";
        bala.transform.position = Boca.position;
        bala.transform.localScale = Vector3.one * 0.16f;
        bala.GetComponent<Renderer>().material = NavalCinematicDirector.Toon(new Color(0.15f, 0.15f, 0.16f));
        Rigidbody cuerpo = bala.AddComponent<Rigidbody>();
        cuerpo.useGravity = true;
        cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        cuerpo.linearVelocity = direccion.normalized * velocidad;
        ProyectilNaval proyectil = bala.AddComponent<ProyectilNaval>();
        proyectil.Preparar(dano, Efectos, transform.root);
        Destroy(bala, 4f);
    }
}

public class ProyectilNaval : MonoBehaviour
{
    private float dano;
    private NavalVfxPool efectos;
    private Transform dueno;

    public void Preparar(float danoDisparo, NavalVfxPool vfx, Transform origen)
    {
        dano = danoDisparo;
        efectos = vfx;
        dueno = origen;
    }

    private void Update()
    {
        float agua = GerstnerWaves.Altura(transform.position.x, transform.position.z, Time.time);
        if (transform.position.y > agua)
            return;

        if (efectos != null)
            efectos.ImpactoAgua(new Vector3(transform.position.x, agua, transform.position.z));
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision choque)
    {
        if (dueno != null && choque.transform.IsChildOf(dueno))
            return;

        NavalHull casco = choque.collider.GetComponentInParent<NavalHull>();
        if (casco != null)
        {
            casco.RecibirDanio(dano);
            if (efectos != null)
                efectos.Disparo(choque.contacts[0].point, Quaternion.LookRotation(Vector3.up));
        }
        else if (efectos != null)
        {
            efectos.ImpactoAgua(choque.contacts[0].point);
        }

        Destroy(gameObject);
    }
}
