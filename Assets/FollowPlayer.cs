using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsety = 1f;

    [SerializeField] private float VerticalPadding = 200f;

    private Vector3 smoothVelocity;
    [SerializeField] private float smoothTime = 1f;

    private void LateUpdate()
    {
        Vector3 destino = player.position;
        destino.y = offsety;
        destino.z = transform.position.z;

        if (Camera.main.WorldToScreenPoint(player.position).y > (Screen.height / 2 - VerticalPadding))
        {
            destino.y = player.position.y;
        }
        if (Camera.main.WorldToScreenPoint(player.position).y < (-Screen.height / 2 + VerticalPadding))
        {
            destino.y = player.position.y;
        }
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref smoothVelocity, smoothTime);
    }

}
