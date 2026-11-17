using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Cañón de cubierta. El jugador agarra el manubrio con Grip y dispara con Trigger.
/// El cañón permanece montado en el barco.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class VRShipCannon : MonoBehaviour
{
    [SerializeField] private Transform boca;
    [SerializeField] private Transform manubrio;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private GameObject efectoImpactoBarco;
    [SerializeField] private GameObject efectoImpactoAgua;
    [SerializeField] private float fuerza = 24f;
    [SerializeField] private float dano = 34f;
    [SerializeField] private float enfriamiento = 0.9f;
    [SerializeField] private float amplitudHaptica = 0.7f;
    [SerializeField] private float duracionHaptica = 0.12f;
    [SerializeField] private bool controladoPorJugador = true;

    public Transform Boca => boca;
    public BarcoSalud Barco { get; private set; }

    private XRGrabInteractable agarre;
    private IXRSelectInteractor interactor;
    private bool agarrado;
    private float proximoDisparo;
    private Vector3 posicionLocal;
    private Quaternion rotacionLocal;
    private AudioSource audioFuente;
    private InputAction gatilloIzquierdo;
    private InputAction gatilloDerecho;
    private bool izquierdoPresionado;
    private bool derechoPresionado;
    private bool accionesBuscadas;

    private void Awake()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;

        if (boca == null)
        {
            GameObject punto = new GameObject("Boca");
            punto.transform.SetParent(transform, false);
            punto.transform.localPosition = new Vector3(0f, 0.05f, 1.35f);
            boca = punto.transform;
        }

        agarre = GetComponent<XRGrabInteractable>();
        if (agarre == null)
            agarre = gameObject.AddComponent<XRGrabInteractable>();

        agarre.movementType = XRBaseInteractable.MovementType.Instantaneous;
        agarre.trackPosition = false;
        agarre.trackRotation = false;
        agarre.throwOnDetach = false;
        agarre.retainTransformParent = true;
        agarre.selectEntered.AddListener(AlAgarrar);
        agarre.selectExited.AddListener(AlSoltar);
        agarre.activated.AddListener(AlDispararGatillo);

        audioFuente = gameObject.AddComponent<AudioSource>();
        audioFuente.spatialBlend = 1f;
        audioFuente.minDistance = 1.5f;
        audioFuente.maxDistance = 40f;
        audioFuente.playOnAwake = false;

        posicionLocal = transform.localPosition;
        rotacionLocal = transform.localRotation;
    }

    private void OnDestroy()
    {
        if (agarre == null)
            return;

        agarre.selectEntered.RemoveListener(AlAgarrar);
        agarre.selectExited.RemoveListener(AlSoltar);
        agarre.activated.RemoveListener(AlDispararGatillo);
    }

    public void Configurar(BarcoSalud barco, bool delJugador, float danoDisparo, AudioClip disparo, AudioClip impacto, GameObject fuego, GameObject agua)
    {
        Barco = barco;
        controladoPorJugador = delJugador;
        dano = danoDisparo;
        sonidoDisparo = disparo;
        sonidoImpacto = impacto;
        efectoImpactoBarco = fuego;
        efectoImpactoAgua = agua;
        if (agarre != null)
            agarre.enabled = delJugador;
    }

    private void Update()
    {
        transform.localPosition = posicionLocal;

        if (!agarrado || interactor == null)
        {
            if (!controladoPorJugador)
                return;

            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionLocal, 6f * Time.deltaTime);
            if (GatilloPresionado())
            {
                Vector3 proa = transform.parent != null ? transform.parent.forward : transform.forward;
                DispararHacia(proa);
            }
            return;
        }

        Transform mano = interactor.GetAttachTransform(agarre);
        Vector3 direccion = mano.position - transform.position;
        direccion.y = Mathf.Clamp(direccion.y, -0.35f, 0.55f);
        if (direccion.sqrMagnitude > 0.05f)
            transform.rotation = Quaternion.LookRotation(direccion.normalized, Vector3.up);
    }

    public void DispararHacia(Vector3 direccionMundo)
    {
        direccionMundo.y = Mathf.Clamp(direccionMundo.y, -0.15f, 0.35f);
        if (direccionMundo.sqrMagnitude < 0.01f)
            return;

        transform.rotation = Quaternion.LookRotation(direccionMundo.normalized, Vector3.up);
        Disparar(null);
    }

    private bool GatilloPresionado()
    {
        if (!accionesBuscadas)
            BuscarGatillos();

        return Flanco(gatilloIzquierdo, ref izquierdoPresionado) || Flanco(gatilloDerecho, ref derechoPresionado);
    }

    private void BuscarGatillos()
    {
        accionesBuscadas = true;
        InputActionAsset[] activos = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        for (int i = 0; i < activos.Length; i++)
        {
            if (activos[i] == null || activos[i].name != "XRI Default Input Actions")
                continue;

            gatilloIzquierdo = activos[i].FindAction("XRI Left Interaction/Activate Value", false);
            gatilloDerecho = activos[i].FindAction("XRI Right Interaction/Activate Value", false);
            Habilitar(gatilloIzquierdo);
            Habilitar(gatilloDerecho);
            return;
        }
    }

    private static void Habilitar(InputAction accion)
    {
        if (accion == null)
            return;
        if (accion.actionMap != null && !accion.actionMap.enabled)
            accion.actionMap.Enable();
        if (!accion.enabled)
            accion.Enable();
    }

    private static bool Flanco(InputAction accion, ref bool estabaPresionado)
    {
        bool presionado = accion != null && accion.ReadValue<float>() > 0.55f;
        bool flanco = presionado && !estabaPresionado;
        estabaPresionado = presionado;
        return flanco;
    }

    private void AlAgarrar(SelectEnterEventArgs args)
    {
        agarrado = true;
        interactor = args.interactorObject;
        Vibrar(interactor, 0.25f, 0.05f);
    }

    private void AlSoltar(SelectExitEventArgs args)
    {
        agarrado = false;
        interactor = null;
    }

    private void AlDispararGatillo(ActivateEventArgs args)
    {
        if (!agarrado)
            return;

        Disparar(args.interactorObject);
    }

    private void Disparar(IXRInteractor origenHaptica)
    {
        if (Time.time < proximoDisparo || (Barco != null && Barco.EstaHundido))
            return;

        proximoDisparo = Time.time + enfriamiento;

        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "Cannonball_Prefab";
        bala.transform.position = boca.position;
        bala.transform.localScale = Vector3.one * 0.28f;
        bala.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.12f, 0.12f, 0.13f));

        Rigidbody cuerpo = bala.AddComponent<Rigidbody>();
        cuerpo.mass = 5f;
        cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        cuerpo.useGravity = true;

        ProyectilCanon proyectil = bala.AddComponent<ProyectilCanon>();
        int equipo = Barco != null ? Barco.Equipo : 0;
        GameObject dueno = Barco != null ? Barco.gameObject : null;
        proyectil.Lanzar(boca.forward * fuerza + Vector3.up * 3f, equipo, dueno, dano, efectoImpactoBarco, sonidoImpacto);
        proyectil.DefinirEfectoAgua(efectoImpactoAgua);

        if (sonidoDisparo != null)
            audioFuente.PlayOneShot(sonidoDisparo);

        if (origenHaptica != null)
            Vibrar(origenHaptica, amplitudHaptica, duracionHaptica);
    }

    private static void Vibrar(IXRInteractor interactor, float amplitud, float duracion)
    {
        Component componente = interactor as Component;
        if (componente == null)
            return;

        HapticImpulsePlayer haptico = componente.GetComponentInParent<HapticImpulsePlayer>();
        if (haptico != null)
            haptico.SendHapticImpulse(amplitud, duracion);
    }
}
