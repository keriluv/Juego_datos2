using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private string nextLevel="SampleScene";
    [SerializeField] private GameObject player;
    public void LoadLevel0()
    {
        SceneManager.LoadScene(nextLevel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (nextLevel == "EscenaXR")
            {
                player.SetActive(true);
                return;
            }
            LoadLevel0();
        }
    }
}
