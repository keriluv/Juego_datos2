using UnityEngine;

public class hoverConfig : MonoBehaviour
{
    private Vector3 target;
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        target = rect.position;
    }

    void Update()
    {
        rect.position = Vector3.Lerp(
            rect.position,
            target,
            Time.deltaTime * 10f
        );
    }

    public void MoveTo(RectTransform y)
    {
        target.y = y.position.y;
    }
}