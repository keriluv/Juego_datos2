using System.Collections;
using UnityEngine;

public class crackMinecraft : StruckObject
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private ObjectivesManager om;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        explosion.SetActive(false);
    }

    public override void OnHit()
    {
        om.UpdateObjective();

        StartCoroutine(animationEffect());
    }

    private IEnumerator animationEffect()
    {
        explosion.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        explosion.SetActive(false);

        Debug.Log("Ir a video minecraft");
    }
   
}
