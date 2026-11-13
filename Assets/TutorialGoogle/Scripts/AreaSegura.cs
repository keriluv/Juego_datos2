using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AreaSegura : MonoBehaviour
{
    private RectTransform rect;
    private Rect ultima;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Aplicar();
    }

    private void Update()
    {
        if (Screen.safeArea != ultima)
            Aplicar();
    }

    private void Aplicar()
    {
        ultima = Screen.safeArea;

        Vector2 min = ultima.position;
        Vector2 max = ultima.position + ultima.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
