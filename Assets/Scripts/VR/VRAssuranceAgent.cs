using UnityEngine;

/// <summary>
/// Informe de la batalla en la consola. No busca objetos en cada frame.
/// </summary>
public class VRAssuranceAgent : MonoBehaviour
{
    private bool pirataRevisado;
    private bool camaraRevisada;

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
