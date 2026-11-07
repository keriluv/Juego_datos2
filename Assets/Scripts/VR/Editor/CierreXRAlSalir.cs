using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Suelta el loader de OpenXR al salir de Play. Si se queda activo, el editor
/// no vuelve a entrar en Play hasta reiniciarlo.
/// </summary>
[InitializeOnLoad]
public static class CierreXRAlSalir
{
    static CierreXRAlSalir()
    {
        EditorApplication.playModeStateChanged += AlCambiar;
    }

    private static void AlCambiar(PlayModeStateChange estado)
    {
        if (estado != PlayModeStateChange.ExitingPlayMode)
            return;

        XRManagerSettings gestor = XRGeneralSettings.Instance != null
            ? XRGeneralSettings.Instance.Manager
            : null;
        if (gestor == null || gestor.activeLoader == null)
            return;

        gestor.StopSubsystems();
        gestor.DeinitializeLoader();
    }
}
