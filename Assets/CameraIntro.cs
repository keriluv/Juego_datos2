using System.Collections;
using UnityEngine;

public class CameraIntro : MonoBehaviour
{
    [SerializeField] private RectTransform sceneContent;

    [SerializeField] private float escalaInicial = 2f;
    [SerializeField] private float escalaFinal = 1f;
    [SerializeField] private float duracion = 2f;

    private void Start()
    {
        StartCoroutine(ZoomOut());
    }

    private IEnumerator ZoomOut()
    {
        float tiempo = 0f;

        // Empieza MUY cerca
        sceneContent.localScale = Vector3.one * escalaInicial;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);

            // Suavizado
            progreso = 1f - Mathf.Pow(1f - progreso, 3f);

            sceneContent.localScale = Vector3.Lerp(
                Vector3.one * escalaInicial,
                Vector3.one * escalaFinal,
                progreso
            );

            yield return null;
        }

        sceneContent.localScale = Vector3.one * escalaFinal;
    }
}

