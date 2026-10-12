using UnityEngine;
using UnityEngine.UI;

public class Subscribe : MonoBehaviour
{
    [SerializeField] private Sprite subscribe;
    [SerializeField] private Sprite subscribed;

    [SerializeField] private Image image;
    private bool isSubscribed=false;
    void Start()
    {
        image.sprite = subscribe;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isSubscribed = !isSubscribed;
        }
        if (isSubscribed)
        {
            image.sprite = subscribed;
        }
        else
        {
            image.sprite = subscribe;
        }
    }


}
