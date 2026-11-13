using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlTikTok : MonoBehaviour
{
    public Image iconoLike;
    public Image iconoGuardar;

    private bool liked = false;
    private bool guardado = false;

    public void ToggleLike()
    {
        liked = !liked;
        if (iconoLike != null)
        {
            iconoLike.color = liked ? Color.red : Color.white;
            StartCoroutine(AnimarBototon(iconoLike.transform));
        }
    }

    public void ToggleGuardar()
    {
        guardado = !guardado;
        if (iconoGuardar != null)
        {
            iconoGuardar.color = guardado ? Color.yellow : Color.white;
            StartCoroutine(AnimarBototon(iconoGuardar.transform));
        }
    }

    private IEnumerator AnimarBototon(Transform botonTransform)
    {
        Vector3 escalaOriginal = Vector3.one;
        Vector3 escalaGrande = Vector3.one * 1.3f;

        // Crecer rápido
        float tiempo = 0f;
        while (tiempo < 0.1f)
        {
            tiempo += Time.deltaTime;
            botonTransform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, tiempo / 0.1f);
            yield return null;
        }

        // Volver al tamaño normal
        tiempo = 0f;
        while (tiempo < 0.1f)
        {
            tiempo += Time.deltaTime;
            botonTransform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, tiempo / 0.1f);
            yield return null;
        }

        botonTransform.localScale = escalaOriginal;
    }
}