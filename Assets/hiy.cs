using UnityEngine;

public class hit : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsetX = 2f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 target = player.position;
        target.x = player.position.x + offsetX;

        transform.position = target;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("colision");
        if (other.CompareTag("Hit"))
        {
            StruckObject struckObject = other.GetComponent<StruckObject>();
            struckObject.OnHit();
        }
    }
}
