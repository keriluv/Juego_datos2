using System.Collections;
using UnityEngine;

public class Wall :  StruckObject
{
    [SerializeField] private GameObject explosionParticle;
    [SerializeField] private GameObject EndWall;

    [SerializeField] private Rigidbody player;

    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float duration = 0.5f;

    private bool isHit = false;

    [SerializeField] private ObjectivesManager om;
    void Start()
    {
        explosionParticle.SetActive(false);
        EndWall.SetActive(false);
    }


    public override void OnHit()
    {
        if (!isHit)
        {
            isHit = true;
 
            Vector3 direction = (player.position - transform.position).normalized;
            om.UpdateObjective();

            player.AddForce(new Vector3(-explosionForce,0,0), ForceMode.Impulse);

            StartCoroutine(ActivateEndWall());
            

        }
    }
    private IEnumerator ActivateEndWall()
    {
        explosionParticle.SetActive (true);
        yield return new WaitForSeconds(duration);

        EndWall.SetActive(true);
        gameObject.SetActive(false);

    }
}
