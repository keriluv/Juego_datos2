using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class ControladorFeed : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] listaDeVideos;
    
    [Header("UI Botones (Imágenes)")]
    public Image imagenLike;
    public Image imagenGuardar;
    
    [Header("Colores de Estado")]
    public Color colorLikeActivo = Color.red;
    public Color colorLikeInactivo = Color.white;
    public Color colorGuardarActivo = Color.yellow;
    public Color colorGuardarInactivo = Color.white;

    private int indiceActual = 0;
    private bool[] estadoLikes;
    private bool[] estadoGuardados;

    void Start()
    {
        if (listaDeVideos.Length > 0)
        {
            // Inicializamos los arreglos según la cantidad de vídeos
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

        // Actualizamos los colores visuales de los botones para este vídeo en específico
        ActualizarEstadoVisualUI();
    }

    public void SiguienteVideo()
    {
        if (listaDeVideos.Length == 0) return;

        indiceActual = (indiceActual + 1) % listaDeVideos.Length;
        ReproducirVideo(indiceActual);
    }

    public void VideoAnterior()
    {
        if (listaDeVideos.Length == 0) return;

        indiceActual = (indiceActual - 1 + listaDeVideos.Length) % listaDeVideos.Length;
        ReproducirVideo(indiceActual);
    }

    // Método para conectar al evento OnClick() del botón de Like
    public void ToggleLike()
    {
        if (listaDeVideos.Length == 0) return;

        estadoLikes[indiceActual] = !estadoLikes[indiceActual];
        ActualizarEstadoVisualUI();
    }

    // Método para conectar al evento OnClick() del botón de Guardar
    public void ToggleGuardar()
    {
        if (listaDeVideos.Length == 0) return;

        estadoGuardados[indiceActual] = !estadoGuardados[indiceActual];
        ActualizarEstadoVisualUI();
    }

    private void ActualizarEstadoVisualUI()
    {
        // Cambia el color del botón de Like según el vídeo actual
        if (imagenLike != null)
        {
            imagenLike.color = estadoLikes[indiceActual] ? colorLikeActivo : colorLikeInactivo;
        }

        // Cambia el color del botón de Guardar según el vídeo actual
        if (imagenGuardar != null)
        {
            imagenGuardar.color = estadoGuardados[indiceActual] ? colorGuardarActivo : colorGuardarInactivo;
        }
    }
}