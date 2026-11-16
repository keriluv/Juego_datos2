using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetSelected(bool selected)
    {
        img.sprite = selected ? selectedSprite : normalSprite;
    }
}
