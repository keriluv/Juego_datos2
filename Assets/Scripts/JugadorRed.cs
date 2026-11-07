using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Jugador del lobby online.
/// El dueño del objeto simula el movimiento y NetworkTransform lo replica
/// al resto de jugadores. Los jugadores remotos se vuelven cinematicos.
///
/// Controles: WASD / flechas para moverse. Espacio o flecha arriba para saltar.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class JugadorRed : NetworkBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 6f;
    [SerializeField] private float fuerzaSalto = 8f;
    [SerializeField] private float tiempoCoyote = 0.12f;
    [SerializeField] private float tiempoBufferSalto = 0.12f;

    [Header("Aspecto")]
    [SerializeField] private TextMesh etiqueta;

    private static readonly Color[] Colores =
    {
        new Color(0.20f, 0.82f, 1.00f), // azul
        new Color(1.00f, 0.55f, 0.15f), // naranja
        new Color(0.40f, 1.00f, 0.45f), // verde
        new Color(1.00f, 0.30f, 0.75f), // rosa
    };

    private Rigidbody cuerpo;
    private float ultimoSuelo = -100f;
    private float ultimoSaltoPulsado = -100f;
    private bool preparado;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (cuerpo == null) cuerpo = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            cuerpo.isKinematic = false;
            cuerpo.interpolation = RigidbodyInterpolation.Interpolate;
            ColocarEnPuntoDeAparicion();
        }
        else
        {
            cuerpo.isKinematic = true;
            cuerpo.interpolation = RigidbodyInterpolation.None;
        }

        Pintar();
        ActualizarEtiqueta();
        preparado = true;

        if (IsOwner && CamaraLobby.Instancia != null)
            CamaraLobby.Instancia.Seguir(transform);

        Debug.Log("[Jugador] Spawn cliente " + OwnerClientId + " (owner=" + IsOwner + ")");
    }

    private void ColocarEnPuntoDeAparicion()
    {
        int indice = (int)(OwnerClientId % 4u);
        Vector3 posicion = new Vector3(-6f + 4f * indice, 2.5f, 0f);

        transform.position = posicion;
        cuerpo.position = posicion;
        cuerpo.rotation = Quaternion.identity;
        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
    }

    private void Pintar()
    {
        Color color = Colores[(int)(OwnerClientId % (uint)Colores.Length)];

        foreach (Renderer render in GetComponentsInChildren<Renderer>())
        {
            // La etiqueta usa el material de la fuente: no lo tocamos.
            if (etiqueta != null && render.transform == etiqueta.transform) continue;

            Material material = render.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }
    }

    private void ActualizarEtiqueta()
    {
        if (etiqueta == null) return;
        etiqueta.text = "Jugador " + (OwnerClientId + 1) + (IsOwner ? " (tu)" : "");
        etiqueta.color = Colores[(int)(OwnerClientId % (uint)Colores.Length)];
    }

    private void Update()
    {
        if (!IsOwner || !preparado) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            ultimoSaltoPulsado = Time.time;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !preparado) return;

        float horizontal = 0f;
        float adelante = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.W)) adelante += 1f;
        if (Input.GetKey(KeyCode.S)) adelante -= 1f;

        Vector3 direccion = new Vector3(horizontal, 0f, adelante);
        if (direccion.sqrMagnitude > 1f) direccion.Normalize();

        Vector3 velocidadActual = cuerpo.linearVelocity;
        velocidadActual.x = direccion.x * velocidad;
        velocidadActual.z = direccion.z * velocidad;

        bool enSuelo = Time.time - ultimoSuelo <= tiempoCoyote;
        bool saltoReciente = Time.time - ultimoSaltoPulsado <= tiempoBufferSalto;

        if (enSuelo && saltoReciente)
        {
            velocidadActual.y = fuerzaSalto;
            ultimoSaltoPulsado = -100f;
            ultimoSuelo = -100f;
        }

        cuerpo.linearVelocity = velocidadActual;

        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion objetivo = Quaternion.LookRotation(direccion, Vector3.up);
            cuerpo.MoveRotation(Quaternion.Slerp(cuerpo.rotation, objetivo, 12f * Time.fixedDeltaTime));
        }
    }

    private void OnCollisionStay(Collision colision)
    {
        if (!IsOwner) return;

        for (int i = 0; i < colision.contactCount; i++)
        {
            if (colision.GetContact(i).normal.y > 0.5f)
            {
                ultimoSuelo = Time.time;
                return;
            }
        }
    }
}
