using UnityEngine;

public class CamaraLobby : MonoBehaviour
{
    public static CamaraLobby Instancia { get; private set; }

    [SerializeField] private Vector3 desplazamiento = new Vector3(0f, 5.5f, -9f);
    [SerializeField] private float suavizado = 8f;
    [SerializeField] private Vector3 puntoMirada = new Vector3(0f, 0.8f, 0f);

    private Transform objetivo;

    private void Awake()
    {
        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    public void Seguir(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }

    private void LateUpdate()
    {
        if (objetivo == null) BuscarJugadorLocal();
        if (objetivo == null) return;

        Vector3 destino = objetivo.position + desplazamiento;
        float factor = 1f - Mathf.Exp(-suavizado * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, destino, factor);

        Vector3 mirada = (objetivo.position + puntoMirada) - transform.position;
        if (mirada.sqrMagnitude > 0.001f)
        {
            Quaternion rotacion = Quaternion.LookRotation(mirada, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacion, factor);
        }
    }

    private void BuscarJugadorLocal()
    {
        MovePlayer1[] barcos = FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude);

        foreach (MovePlayer1 barco in barcos)
        {
            if (barco.IsSpawned && barco.IsOwner)
            {
                objetivo = barco.transform;
                return;
            }
        }

        JugadorRed[] jugadores = FindObjectsByType<JugadorRed>(FindObjectsInactive.Exclude);

        foreach (JugadorRed jugador in jugadores)
        {
            if (jugador.IsSpawned && jugador.IsOwner)
            {
                objetivo = jugador.transform;
                return;
            }
        }
    }
}
