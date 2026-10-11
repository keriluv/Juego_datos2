using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Cañón montado en el barco.
/// Grip lo selecciona, el mando lo apunta y el Trigger dispara.
/// Sigue anclado al barco para no marear al jugador.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CanonPirata : MonoBehaviour
{
    [SerializeField] private GameObject prefabBala;
    [SerializeField] private GameObject efectoExplosion;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private Transform boca;
    [SerializeField] private float fuerza = 22f;
    [SerializeField] private float dano = 30f;
    [SerializeField] private float enfriamiento = 0.85f;
    [SerializeField] private bool automatico;

    public BarcoSalud Barco { get; private set; }

    private XRGrabInteractable agarre;
    private IXRSelectInteractor interactor;
    private bool agarrado;
    private float proximoDisparo;
    private Vector3 posicionLocalReposo;
    private Quaternion rotacionLocalReposo;
    private AudioSource audioFuente;

    private void Awake()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;

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
        agarre.activated.AddListener(AlActivar);

        if (boca == null)
        {
            GameObject punto = new GameObject("Boca");
            punto.transform.SetParent(transform, false);
            punto.transform.localPosition = new Vector3(0f, 0.15f, 1.1f);
            boca = punto.transform;
        }

        audioFuente = GetComponent<AudioSource>();
        if (audioFuente == null)
            audioFuente = gameObject.AddComponent<AudioSource>();
        audioFuente.playOnAwake = false;
        audioFuente.spatialBlend = 1f;

        posicionLocalReposo = transform.localPosition;
        rotacionLocalReposo = transform.localRotation;
    }

    private void OnDestroy()
    {
        if (agarre == null)
            return;

        agarre.selectEntered.RemoveListener(AlAgarrar);
        agarre.selectExited.RemoveListener(AlSoltar);
        agarre.activated.RemoveListener(AlActivar);
    }

    public void Configurar(BarcoSalud barco, bool esAutomatico, float danoDisparo, GameObject bala, GameObject explosion, AudioClip disparo, AudioClip impacto)
    {
        Barco = barco;
        automatico = esAutomatico;
        dano = danoDisparo;
        prefabBala = bala;
        efectoExplosion = explosion;
        sonidoDisparo = disparo;
        sonidoImpacto = impacto;
        agarre.enabled = !esAutomatico;
    }

    private void Update()
    {
        transform.localPosition = posicionLocalReposo;

        if (agarrado && interactor != null)
        {
            Transform mira = interactor.GetAttachTransform(agarre);
            Vector3 direccion = mira.forward;
            direccion.y = Mathf.Clamp(direccion.y, -0.25f, 0.4f);
            if (direccion.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionLocalReposo, 8f * Time.deltaTime);
        }
    }

    public void IntentarDisparoAutomatico(Transform objetivo)
    {
        if (!automatico || objetivo == null || Barco == null || Barco.EstaHundido)
            return;

        Vector3 hacia = objetivo.position - boca.position;
        hacia.y = 0f;
        if (hacia.sqrMagnitude < 1f || hacia.sqrMagnitude > 40f * 40f)
            return;

        transform.rotation = Quaternion.LookRotation(hacia.normalized, Vector3.up);
        Disparar();
    }

    private void AlAgarrar(SelectEnterEventArgs args)
    {
        agarrado = true;
        interactor = args.interactorObject;
    }

    private void AlSoltar(SelectExitEventArgs args)
    {
        agarrado = false;
        interactor = null;
    }

    private void AlActivar(ActivateEventArgs args)
    {
        Disparar();
    }

    public void DispararHacia(Vector3 direccionMundo)
    {
        direccionMundo.y = Mathf.Clamp(direccionMundo.y, -0.2f, 0.35f);
        if (direccionMundo.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direccionMundo.normalized, Vector3.up);
        Disparar();
    }

    public void Disparar()
    {
        if (Time.time < proximoDisparo || (Barco != null && Barco.EstaHundido))
            return;

        proximoDisparo = Time.time + enfriamiento;

        GameObject bala = prefabBala != null
            ? Instantiate(prefabBala, boca.position, boca.rotation)
            : CrearBalaBasica();

        if (bala.GetComponent<Collider>() == null)
        {
            SphereCollider esfera = bala.AddComponent<SphereCollider>();
            esfera.radius = 0.18f;
        }

        Rigidbody cuerpo = bala.GetComponent<Rigidbody>();
        if (cuerpo == null)
            cuerpo = bala.AddComponent<Rigidbody>();
        cuerpo.mass = 4f;
        cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        cuerpo.useGravity = true;

        ProyectilCanon proyectil = bala.GetComponent<ProyectilCanon>();
        if (proyectil == null)
            proyectil = bala.AddComponent<ProyectilCanon>();

        int equipo = Barco != null ? Barco.Equipo : 0;
        GameObject dueno = Barco != null ? Barco.gameObject : null;
        proyectil.Lanzar(boca.forward * fuerza + Vector3.up * 2.5f, equipo, dueno, dano, efectoExplosion, sonidoImpacto);

        if (sonidoDisparo != null)
            audioFuente.PlayOneShot(sonidoDisparo);
    }

    private GameObject CrearBalaBasica()
    {
        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "Bala";
        bala.transform.position = boca.position;
        bala.transform.localScale = Vector3.one * 0.28f;
        Renderer render = bala.GetComponent<Renderer>();
        render.material = MaterialBarco.Crear(new Color(0.15f, 0.15f, 0.16f));
        return bala;
    }
}
