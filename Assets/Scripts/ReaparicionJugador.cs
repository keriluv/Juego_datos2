using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owner-only respawn when falling below a Y threshold or hitting a HoyoReaparicion.
/// Uses JuegoOnlineEscena spawn / MovePlayer1.ColocarEnAparicion.
/// </summary>
[RequireComponent(typeof(MovePlayer1))]
public class ReaparicionJugador : NetworkBehaviour
{
    [SerializeField] private float umbralY = -25f;
    [SerializeField] private float cooldown = 1.25f;

    private MovePlayer1 movimiento;
    private float proximoRespawn;
    private bool reapareciendo;

    private void Awake()
    {
        movimiento = GetComponent<MovePlayer1>();
        AjustarUmbralPorEscena();
    }

    private void AjustarUmbralPorEscena()
    {
        string escena = SceneManager.GetActiveScene().name;
        if (escena == "LevelYoutubev2" || escena == "LevelYoutubev3" || escena == "LevelYoutube")
            umbralY = -15f;
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner) return;
        if (PauseManager.IsPaused) return;
        if (reapareciendo) return;
        if (Time.time < proximoRespawn) return;

        if (transform.position.y < umbralY)
            Reaparecer();
    }

    public void NotificarHoyo()
    {
        if (!IsSpawned || !IsOwner) return;
        if (PauseManager.IsPaused) return;
        if (reapareciendo) return;
        if (Time.time < proximoRespawn) return;
        Reaparecer();
    }

    private void Reaparecer()
    {
        reapareciendo = true;
        proximoRespawn = Time.time + cooldown;

        Vector3 punto = JuegoOnlineEscena.HayPuntoAparicion
            ? JuegoOnlineEscena.PuntoAparicion
            : BuscarSpawnEscena();

        if (movimiento != null)
            movimiento.ColocarEnAparicion(punto, OwnerClientId);
        else
            transform.position = punto + Vector3.up;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        reapareciendo = false;
    }

    private static Vector3 BuscarSpawnEscena()
    {
        Spawn spawn = Object.FindAnyObjectByType<Spawn>();
        if (spawn != null) return spawn.transform.position;

        GameObject marcado = GameObject.Find("Spawnpoint");
        if (marcado != null) return marcado.transform.position;

        return Vector3.up * 2f;
    }
}
