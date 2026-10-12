using UnityEngine;

public class Like : MonoBehaviour
{
    [SerializeField] private LikeDislike likedislike;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            likedislike.likedislike(1);
        }
    }
}
