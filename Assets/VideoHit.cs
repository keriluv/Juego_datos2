using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class VideoHit : StruckObject
{
    [SerializeField] private VideoPlayer vp;

    [SerializeField] private UnityEngine.UI.Image screen;
    [SerializeField] private Sprite pausedScreen;
    [SerializeField] private Sprite resumedScreen;

    [SerializeField] private UnityEngine.UI.Image pause;
    [SerializeField] private UnityEngine.UI.Image resume;
    private bool isPaused = false;

    [SerializeField] private float duracion = 0.5f;
    [SerializeField] float escalaMaxima = 1.15f;
    [SerializeField] private float escalaInicial;
    [SerializeField] private float escalaFinal;
    private Vector3 escalaOriginal;

    private void Start()
    {
        escalaOriginal = pause.transform.localScale;
    }
    public override void OnHit()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            StartCoroutine(animationIcon(pause));
            vp.Pause();
            screen.sprite = pausedScreen;
        }
        else
        {
            StartCoroutine(animationIcon(resume));
            vp.Play();
            screen.sprite = resumedScreen;
        }
    }

    private IEnumerator animationIcon(UnityEngine.UI.Image image)
    {
        float tiempo = 0f;

        while (tiempo < duracion / 2f)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / (duracion / 2f);

            float escala = Mathf.Lerp(escalaInicial, escalaMaxima, progreso);
            image.transform.localScale = escalaOriginal * escala;

            Color color = image.color;
            color.a = Mathf.Lerp(0f, 1f, progreso);
            image.color = color;

            yield return null;
        }

        tiempo = 0f;

        while (tiempo < duracion / 2f)
        {
            tiempo += Time.deltaTime;

            float progreso = tiempo / (duracion / 2f);

            float escala = Mathf.Lerp(escalaMaxima, escalaFinal, progreso);
            image.transform.localScale = escalaOriginal * escala;

            Color color = image.color;
            color.a = Mathf.Lerp(1f, 0f, progreso);
            image.color = color;

            yield return null;
        }

        image.transform.localScale = escalaOriginal * escalaFinal;

        Color final = image.color;
        final.a = 0f;
        image.color = final;
    }
}
