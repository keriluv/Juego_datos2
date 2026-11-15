
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsety = 1f;
    [SerializeField] private float VerticalPadding = 200f;

    private Vector3 smoothVelocity;
    [SerializeField] private float smoothTime = 1f;

    private void Start()
    {
        BuscarJugadorLocal();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            BuscarJugadorLocal();
            return;
        }

        Vector3 destino = player.position;
        destino.y = offsety;
        destino.z = transform.position.z;

        float posicionPantalla =
            Camera.main.WorldToScreenPoint(player.position).y;

        if (posicionPantalla > (Screen.height / 2 - VerticalPadding) ||
            posicionPantalla < (-Screen.height / 2 + VerticalPadding))
        {
            destino.y = player.position.y;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            destino,
            ref smoothVelocity,
            smoothTime
        );
    }

    private void BuscarJugadorLocal()
    {
        MovePlayer1[] barcos =
            FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude);

        foreach (MovePlayer1 barco in barcos)
        {
            if (barco.IsSpawned && barco.IsOwner)
            {
                player = barco.transform;
                return;
            }
        }

        JugadorRed[] jugadores =
            FindObjectsByType<JugadorRed>(FindObjectsInactive.Exclude);

        foreach (JugadorRed jugador in jugadores)
        {
            if (jugador.IsSpawned && jugador.IsOwner)
            {
                player = jugador.transform;
                return;
            }
        }
    }
}
