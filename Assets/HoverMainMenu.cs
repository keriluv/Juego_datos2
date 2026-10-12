using UnityEngine;
using UnityEngine.UI;

public class HoverMainMenu : MonoBehaviour
{
    [SerializeField] private Sprite playButton;
    [SerializeField] private Sprite configButton;
    [SerializeField] private Sprite creditsButton;
    [SerializeField] private Sprite noneButton;

    [SerializeField] private Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        image.sprite = noneButton;
    }

    public void ShowHover(int a)
    {
        switch (a)
        {
            case 0:
                image.sprite = playButton;
                break;
            case 1:
                image.sprite = configButton;
                break;
            case 2:
                image.sprite = creditsButton;
                break;
        }

    }
    public void HideHover()
    {
        image.sprite = noneButton;
    }
}
