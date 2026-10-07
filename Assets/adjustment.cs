using UnityEngine;

[RequireComponent(typeof(Camera))]
public class adjustment : MonoBehaviour
{
    public float targetAspectWidth = 16f;
    public float targetAspectHeight = 9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateAspectRatio();
    }

    public void UpdateAspectRatio()
    {
        // Relación objetivo y relación actual de la pantalla
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // Escala relativa
        float scaleHeight = windowAspect / targetAspect;

        // Si la pantalla es más alta que 16:9 (por ejemplo, 16:10) -> Barras negras arriba y abajo
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        // Si la pantalla es más ancha (por ejemplo, 21:9) -> Barras negras a los lados
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}