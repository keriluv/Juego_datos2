using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private string nextLevel="SampleScene";
    public void LoadLevel0()
    {
        SceneManager.LoadScene(nextLevel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadLevel0();
        }
    }
}
