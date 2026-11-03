using UnityEngine;

/// <summary>
/// Flotación por cuatro puntos del casco. La altura sale de GerstnerWaves,
/// así el barco cabecea con la misma ola que se ve en el shader.
/// En el barco del jugador se congelan X/Z para que el rig VR no se desplace.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WaterFloatBehavior : MonoBehaviour
{
    [SerializeField] private float fuerza = 28f;
    [SerializeField] private float amortiguacion = 2.2f;
    [SerializeField] private bool anclarHorizontal;
    [SerializeField] private Vector3[] puntos =
    {
        new Vector3(-0.7f, 0.1f, 1.6f),
        new Vector3(0.7f, 0.1f, 1.6f),
        new Vector3(-0.7f, 0.1f, -1.6f),
        new Vector3(0.7f, 0.1f, -1.6f)
    };

    private Rigidbody cuerpo;

    public void DefinirPuntos(Vector3[] nuevos)
    {
        if (nuevos != null && nuevos.Length > 0)
            puntos = nuevos;
    }

    public void AnclarEnElSitio(bool anclar)
    {
        anclarHorizontal = anclar;
        if (cuerpo != null)
            AplicarRestricciones();
    }

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        cuerpo.useGravity = true;
        cuerpo.mass = 12f;
        cuerpo.linearDamping = 0.35f;
        cuerpo.angularDamping = 2.4f;
        cuerpo.interpolation = RigidbodyInterpolation.Interpolate;
        AplicarRestricciones();
    }

    private void AplicarRestricciones()
    {
        cuerpo.constraints = anclarHorizontal
            ? RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
            : RigidbodyConstraints.None;
    }

    private void FixedUpdate()
    {
        float tiempo = Time.time;
        for (int i = 0; i < puntos.Length; i++)
        {
            Vector3 mundo = transform.TransformPoint(puntos[i]);
            float altura = GerstnerWaves.Altura(mundo.x, mundo.z, tiempo);
            float diferencia = altura - mundo.y + 0.15f;
            if (diferencia > 0f)
                cuerpo.AddForceAtPosition(Vector3.up * diferencia * fuerza, mundo, ForceMode.Acceleration);

            Vector3 velocidad = cuerpo.GetPointVelocity(mundo);
            cuerpo.AddForceAtPosition(-velocidad * amortiguacion * 0.12f, mundo, ForceMode.Acceleration);
        }
    }
}
