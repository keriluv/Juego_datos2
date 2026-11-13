using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControladorFeed : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] listaDeVideos;

    [Header("Datos de cada video (mismo orden que la lista de videos)")]
    public string[] usuarios;
    [TextArea] public string[] descripciones;
    public Sprite[] fotosDePerfil; // NUEVO

    [Header("UI Usuario")]
    public TMP_Text textoUsuario;
    public TMP_Text textoDescripcion;
    public Image imagenPerfil; // NUEVO: arrastra aquí "Foto"

    [Header("UI Botones (Imágenes)")]
    public Image imagenLike;
    public Image imagenGuardar;

    [Header("Colores de Estado")]
    public Color colorLikeActivo = Color.red;
    public Color colorLikeInactivo = Color.white;
    public Color colorGuardarActivo = Color.yellow;
    public Color colorGuardarInactivo = Color.white;

    [Header("Animación de deslizar")]
    public RectTransform panelVideo;
    public float distanciaDeslizar = 600f;
    public float duracionSalida = 0.15f;
    public float duracionEntrada = 0.2f;

    private int indiceActual = 0;
    private bool[] estadoLikes;
    private bool[] estadoGuardados;
    private bool animando = false;
    private Vector2 posicionOriginal;

    void Start()
    {
        if (panelVideo != null) posicionOriginal = panelVideo.anchoredPosition;

        if (listaDeVideos.Length > 0)
        {
            estadoLikes = new bool[listaDeVideos.Length];
            estadoGuardados = new bool[listaDeVideos.Length];

            ReproducirVideo(indiceActual);
        }
    }

    public void ReproducirVideo(int indice)
    {
        if (listaDeVideos.Length == 0) return;

        videoPlayer.clip = listaDeVideos[indice];
        videoPlayer.Play();

        ActualizarEstadoVisualUI();
        ActualizarDatosUsuario(indice);
    }

    public void SiguienteVideo()
    {
        if (listaDeVideos.Length == 0 || animando) return;

        int nuevo = (indiceActual + 1) % listaDeVideos.Length;
        StartCoroutine(CambiarVideoConAnimacion(nuevo, 1));
    }

    public void VideoAnterior()
    {
        if (listaDeVideos.Length == 0 || animando) return;

        int nuevo = (indiceActual - 1 + listaDeVideos.Length) % listaDeVideos.Length;
        StartCoroutine(CambiarVideoConAnimacion(nuevo, -1));
    }

    private IEnumerator CambiarVideoConAnimacion(int nuevoIndice, int direccion)
    {
        if (panelVideo == null)
        {
            indiceActual = nuevoIndice;
            ReproducirVideo(indiceActual);
            yield break;
        }

        animando = true;

        Vector2 destinoSalida = posicionOriginal + new Vector2(0, distanciaDeslizar * direccion);
        yield return MoverPanel(posicionOriginal, destinoSalida, duracionSalida);

        indiceActual = nuevoIndice;
        ReproducirVideo(indiceActual);

        Vector2 inicioEntrada = posicionOriginal - new Vector2(0, distanciaDeslizar * direccion);
        yield return MoverPanel(inicioEntrada, posicionOriginal, duracionEntrada);

        panelVideo.anchoredPosition = posicionOriginal;
        animando = false;
    }

    private IEnumerator MoverPanel(Vector2 desde, Vector2 hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            t = Mathf.SmoothStep(0f, 1f, t);
            panelVideo.anchoredPosition = Vector2.Lerp(desde, hasta, t);
            yield return null;
        }
        panelVideo.anchoredPosition = hasta;
    }

    public void ToggleLike()
    {
        if (listaDeVideos.Length == 0) return;

        estadoLikes[indiceActual] = !estadoLikes[indiceActual];
        ActualizarEstadoVisualUI();
    }

    public void ToggleGuardar()
    {
        if (listaDeVideos.Length == 0) return;

        estadoGuardados[indiceActual] = !estadoGuardados[indiceActual];
        ActualizarEstadoVisualUI();
    }

    private void ActualizarEstadoVisualUI()
    {
        if (imagenLike != null)
        {
            imagenLike.color = estadoLikes[indiceActual] ? colorLikeActivo : colorLikeInactivo;
        }

        if (imagenGuardar != null)
        {
            imagenGuardar.color = estadoGuardados[indiceActual] ? colorGuardarActivo : colorGuardarInactivo;
        }
    }

    private void ActualizarDatosUsuario(int indice)
    {
        if (textoUsuario != null)
        {
            textoUsuario.text = (usuarios != null && indice < usuarios.Length) ? usuarios[indice] : "";
        }

        if (textoDescripcion != null)
        {
            textoDescripcion.text = (descripciones != null && indice < descripciones.Length) ? descripciones[indice] : "";
        }

        // NUEVO: cambia la foto de perfil según el video
        if (imagenPerfil != null)
        {
            if (fotosDePerfil != null && indice < fotosDePerfil.Length && fotosDePerfil[indice] != null)
            {
                imagenPerfil.sprite = fotosDePerfil[indice];
                imagenPerfil.enabled = true;
            }
            else
            {
                imagenPerfil.enabled = false; // si no hay foto para ese video, se oculta
            }
        }
    }
}