using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [SerializeField] private GameObject hit;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float showHit = 0.2f;

    private bool hitEnabled = false;
    private float time = 0f;

    void Start()
    {
        hit.SetActive(false);
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !hitEnabled)
        {
            hitEnabled = true;
            hit.SetActive(true);
            time = 0f;
        }

        if (hitEnabled)
        {
            time += Time.fixedDeltaTime;

            if (time >= cooldown)
            {
                time = 0f;
                hitEnabled = false;
            }
            if (time>=showHit)
            {
                hit.SetActive(false);
            }
        }
    }
}
