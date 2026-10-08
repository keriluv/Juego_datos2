using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button playButton;
    [SerializeField] private string LevelSelector;

    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject configScreen;
    private bool isConfig=false;
    void Start()
    {
        menuScreen.SetActive(true);
        configScreen.SetActive(false);
    }

    // Update is called once per frame
    public void goToNextScene()
    {
        SceneManager.LoadScene(LevelSelector);
    }
    public void openConfig()
    {
        if (!isConfig)
        {
            isConfig = true;
            configScreen.SetActive(true);
            menuScreen.SetActive(false);
        }
        else
        {
            isConfig = false;
            configScreen.SetActive(false);
            menuScreen.SetActive(true);
        }
        }
}
