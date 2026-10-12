using UnityEngine;

public class MoveObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject[] points;
    private int i=0;
    private GameObject target;
    private Vector3 destino;
    [SerializeField] private Vector3 velocity = new Vector3(1,1,1);

    void Start()
    {
        destino = points[0].transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocity, 1f);

    }
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Colisioné con: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Point"))
        {    
            i= i + 1;
            if(i>= points.Length)
            {
                i = 0;
            }

            target = points[i];
            destino = target.transform.position;

        }
    }
}
