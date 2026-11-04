using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Enciende OpenXR después de que la escena ya existe.
/// En el editor no llama al runtime si el casco no está abierto: esa llamada
/// es síncrona y deja Play congelado antes de que corran los scripts.
/// </summary>
public static class ArranqueXR
{
    public static IEnumerator IniciarSiHayCasco()
    {
        yield return null;

        XRGeneralSettings ajustes = XRGeneralSettings.Instance;
        XRManagerSettings gestor = ajustes != null ? ajustes.Manager : null;
        if (gestor == null || gestor.activeLoader != null)
            yield break;

        if (!ConvieneIniciar())
        {
            Debug.Log("EscenaVR: no hay un runtime de VR abierto. La batalla se ve en la ventana Game. Abre el casco y pulsa Play otra vez para usar los mandos.");
            yield break;
        }

        yield return gestor.InitializeLoader();
        if (gestor.activeLoader != null)
            gestor.StartSubsystems();
    }

    private static bool ConvieneIniciar()
    {
#if UNITY_EDITOR
        return ProcesoActivo("vrserver")
            || ProcesoActivo("vrcompositor")
            || ProcesoActivo("OVRServer_x64")
            || ProcesoActivo("VirtualDesktop.Streamer");
#else
        return true;
#endif
    }

#if UNITY_EDITOR
    private static bool ProcesoActivo(string nombre)
    {
        try
        {
            return System.Diagnostics.Process.GetProcessesByName(nombre).Length > 0;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
#endif
}
