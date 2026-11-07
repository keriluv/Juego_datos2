using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    [SerializeField] private GameObject hit;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float showHit = 0.2f;

    [SerializeField] private Animator animator;

    private bool hitEnabled = false;
    private float time = 0f;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        hit.SetActive(false);
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !hitEnabled)
        {
            hitEnabled = true;
            hit.SetActive(true);
            if (animator != null) { animator.SetBool("isHit", true); }
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
                if (animator != null) { animator.SetBool("isHit", false); }
                hit.SetActive(false);
            }
        }
    }
}
