using UnityEngine;
using UnityEngine.InputSystem;

public class scrip : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 10f;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.W)) 
        {
            rb.AddForce(new Vector3(0, 0, 1));
        }
    }
}
