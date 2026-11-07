using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class introPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nombreEscena;
    [SerializeField] private CanvasGroup fadePanel;

    [SerializeField] private float duracionFade = 2f;

    void Start()
    {
        fadePanel.alpha = 0;
        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        StartCoroutine(FadeYCambiarEscena());
    }

    IEnumerator FadeYCambiarEscena()
    {
        float tiempo = 0;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0, 1, tiempo / duracionFade);
            yield return null;
        }

        fadePanel.alpha = 1;

        SceneManager.LoadScene(nombreEscena);
    }
}
