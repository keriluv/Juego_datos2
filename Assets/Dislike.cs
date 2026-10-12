using UnityEngine;

public class Dislike : MonoBehaviour
{
    [SerializeField] private LikeDislike likedislike;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            likedislike.likedislike(-1);
        }
    }
}
