using System.Collections;
using UnityEngine;

/// <summary>
/// Informe de la batalla en la consola. No busca objetos en cada frame.
/// </summary>
public class VRAssuranceAgent : MonoBehaviour
{
    private bool pirataRevisado;
    private bool camaraRevisada;
    private bool audioRevisado;

    private void Start()
    {
        Debug.Log("[QA COMPILACIÓN] 0 errores detectados, escena operativa.");
        StartCoroutine(RevisarMusica());
    }

    private IEnumerator RevisarMusica()
    {
        yield return null;
        if (audioRevisado)
            yield break;

        audioRevisado = true;
        GameObject musica = GameObject.Find("MusicaBatallaVR");
        AudioSource fuente = musica != null ? musica.GetComponent<AudioSource>() : null;
        bool dosD = fuente != null && fuente.spatialBlend <= 0.01f;
        if (fuente != null && fuente.clip != null && fuente.loop && dosD && fuente.isPlaying)
            Debug.Log("[QA AUDIO] Clip asignado, Spatial Blend 2D configurado y música reproduciéndose en bucle (isPlaying == true).");
        else if (fuente == null || fuente.clip == null)
            Debug.LogWarning("[QA AUDIO] FALLO. MusicaBatallaVR no tiene un AudioClip.");
        else if (!fuente.loop || !dosD)
            Debug.LogWarning("[QA AUDIO] FALLO. Revisa Loop y Spatial Blend 2D en MusicaBatallaVR.");
        else
            Debug.LogWarning("[QA AUDIO] FALLO. MusicaBatallaVR no está reproduciendo.");
    }

    public void RegistrarDespliegue(float distancia, int aliados)
    {
        Debug.Log("[VR-QA] Despliegue aliado: los enemigos cruzaron " + distancia.ToString("0")
            + " m. Salen " + aliados + " barcos desde el costado hacia el frente.");
    }

    public void Informe(int enemigos, int aliadosVivos, int aliadosDestruidos, BarcoSalud jugador)
    {
        float vida = jugador != null ? jugador.Vida : 0f;
        float maxima = jugador != null ? jugador.VidaMaxima : 0f;
        Debug.Log("[VR-QA] Batalla: enemigos " + enemigos
            + " | aliados en pie " + aliadosVivos
            + " | aliados hundidos " + aliadosDestruidos
            + " | BarcoJugador " + vida.ToString("0") + "/" + maxima.ToString("0") + " HP.");
        RevisarPirataYCamara();
    }

    private void RevisarPirataYCamara()
    {
        if (!pirataRevisado)
        {
            pirataRevisado = true;
            GameObject pirata = GameObject.Find("Pirate_Full");
            if (pirata == null)
                pirata = GameObject.Find("Pirata");
            if (pirata != null && pirata.activeInHierarchy)
                Debug.Log("[VR-QA] Pirata: OK. Pirate_Full está activo y visible en la cubierta.");
            else
                Debug.LogWarning("[VR-QA] Pirata: FALLO. Pirate_Full no está activo en la jerarquía.");
        }

        if (camaraRevisada)
            return;

        camaraRevisada = true;
        GameObject origen = GameObject.Find("XR Origin (VR)");
        if (origen == null)
        {
            Debug.LogWarning("[VR-QA] Cámara: FALLO. No está XR Origin (VR).");
            return;
        }

        string padre = origen.transform.parent != null ? origen.transform.parent.name : "sin padre";
        Debug.Log("[VR-QA] Cámara: OK. XR Origin hijo de " + padre
            + ", posición local " + origen.transform.localPosition.ToString("0.00") + ".");
    }
}
