using UnityEngine;
using UnityEngine.UI;

public class LikeDislike : MonoBehaviour
{
    [SerializeField] private Sprite liked;
    [SerializeField] private Sprite disliked;
    [SerializeField] private Sprite none;
    [SerializeField] private Image image;
 
    private bool isLiked=false;
    private bool isDisliked=false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image.sprite = none;
    }

    // Update is called once per frame
    

    public bool getLiked()
    {
        return isLiked;
    }
    public bool getDisliekd()
    {
        return isDisliked;
    }
    public void setLiked(bool liked)
    {
        this.isLiked = liked;
    }
    public void setDisliked(bool disliked)
    {
        this.isDisliked = disliked;
    }

    public void likedislike(int a)
    {
        if (a == 1)
        {
            isLiked = !isLiked;
            if (isDisliked && isLiked)
            {
                isDisliked = false;
            }
        }
        else
        {
            isDisliked = !isDisliked;
            if (isDisliked && isLiked)
            {
                isLiked= false;
            }
        }

        if (isLiked)
        {
            image.sprite = liked;
            return;
        }
        if (isDisliked)
        {
            image.sprite = disliked;
            return;
        }
        image.sprite = none;
    }
}
