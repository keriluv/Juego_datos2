using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class GestorMinijuegos : MonoBehaviour
{
    // Un video + el minijuego que le toca
    [Serializable]
    public class Asignacion
    {
        public VideoClip video;
        public MinijuegoBase minijuego;
    }

    [Header("Paneles")]
    [SerializeField] GameObject panelMinijuego;   // el cuadro flotante
    [SerializeField] GameObject panelResultado;   // mensaje + botones (dentro del cuadro)

    [Header("Textos y botones")]
    [SerializeField] TMP_Text textoResultado;
    [SerializeField] TMP_Text textoProgreso;      // opcional: "Minijuegos: 1/3"
    [SerializeField] Button botonReintentar;
    [SerializeField] Button botonVolverFeed;

    [Header("Qué minijuego va con cada video")]
    [SerializeField] Asignacion[] asignaciones;

    [Header("Video del feed")]
    [SerializeField] VideoPlayer videoPlayer;     // el Panel_video; se pausa mientras juegas
    [SerializeField] int victoriasNecesarias = 3;

    [Header("Final del nivel")]
    [SerializeField] SecuenciaTiktokDorado secuenciaFinal;  // arranca al volver al feed tras completar los minijuegos
    public UnityEvent AlCompletarNivel;

    int victorias;
    int indiceActual = -1;
    bool nivelListo;
    bool secuenciaLanzada;
    MinijuegoBase actual;
    readonly HashSet<int> asignacionesSuperadas = new HashSet<int>();

    void Awake()
    {
        panelMinijuego.SetActive(false);
        botonReintentar.onClick.AddListener(Reintentar);
        botonVolverFeed.onClick.AddListener(VolverAlFeed);
        ActualizarProgreso();
    }

    // Conéctalo al botón de like. Mira qué video está sonando y abre SU minijuego.
    public void AlDarLike()
    {
        if (victorias >= victoriasNecesarias) return;   // ya completó los minijuegos
        if (panelMinijuego.activeSelf) return;          // ya hay un minijuego abierto

        int i = BuscarAsignacion();
        if (i < 0)
        {
            string nombre = (videoPlayer != null && videoPlayer.clip != null) ? videoPlayer.clip.name : "(sin clip)";
            Debug.LogWarning("GestorMinijuegos: el video " + nombre + " no tiene minijuego asignado.");
            return;
        }
        if (asignaciones[i].minijuego == null) return;
        if (asignacionesSuperadas.Contains(i)) return;   // ese video ya se superó

        indiceActual = i;
        actual = asignaciones[i].minijuego;
        Abrir();
    }

    int BuscarAsignacion()
    {
        if (videoPlayer == null || videoPlayer.clip == null || asignaciones == null) return -1;
        for (int i = 0; i < asignaciones.Length; i++)
            if (asignaciones[i].video == videoPlayer.clip) return i;
        return -1;
    }

    void Abrir()
    {
        if (videoPlayer != null) videoPlayer.Pause();

        panelMinijuego.SetActive(true);
        panelResultado.SetActive(false);

        foreach (var a in asignaciones)
            if (a.minijuego != null)
                a.minijuego.gameObject.SetActive(a.minijuego == actual);

        actual.AlTerminar -= AlTerminarMinijuego;
        actual.AlTerminar += AlTerminarMinijuego;
        actual.Iniciar();
    }

    void AlTerminarMinijuego(bool gano)
    {
        panelResultado.SetActive(true);
        botonReintentar.gameObject.SetActive(!gano);

        if (!gano)
        {
            textoResultado.text = "¡Perdiste!";
            return;
        }

        asignacionesSuperadas.Add(indiceActual);
        victorias++;
        ActualizarProgreso();

        if (victorias >= victoriasNecesarias)
        {
            nivelListo = true;
            textoResultado.text = "¡Minijuegos completados!";
        }
        else
        {
            textoResultado.text = "¡Bien hecho!";
        }
    }

    void Reintentar()
    {
        panelResultado.SetActive(false);
        actual.Iniciar();
    }

    void VolverAlFeed()
    {
        if (actual != null) actual.Detener();
        panelMinijuego.SetActive(false);
        if (videoPlayer != null) videoPlayer.Play();

        // al completar los minijuegos: empieza la secuencia del TikTok dorado
        if (nivelListo && !secuenciaLanzada)
        {
            secuenciaLanzada = true;
            if (AlCompletarNivel != null) AlCompletarNivel.Invoke();
            if (secuenciaFinal != null) secuenciaFinal.Iniciar();
        }
    }

    void ActualizarProgreso()
    {
        if (textoProgreso != null)
            textoProgreso.text = $"Minijuegos: {victorias}/{victoriasNecesarias}";
    }
}
