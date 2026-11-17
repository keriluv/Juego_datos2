using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// OpenXR 1.16.x registers interaction type "DPad" as a device layout. Input System
/// layout names are case-insensitive, so that overwrites the real "Dpad" control used
/// by Xbox/XInput and causes:
/// "Cannot instantiate device layout 'Dpad' as child of '/XInputControllerWindows'".
/// OpenXR 1.17+ renames it to XRDPad; on 1.16.1 (Unity 6000.4 default) we restore Dpad.
/// </summary>
public static class FixOpenXRDpadLayout
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RestaurarAlArrancarSubsistema()
    {
        Restaurar();
        InputSystem.onLayoutChange += AlCambiarLayout;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RestaurarAntesDeEscena()
    {
        Restaurar();
    }

    private static void AlCambiarLayout(string name, InputControlLayoutChange change)
    {
        if (change != InputControlLayoutChange.Added && change != InputControlLayoutChange.Replaced)
            return;
        if (!string.Equals(name, "Dpad", System.StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(name, "DPad", System.StringComparison.OrdinalIgnoreCase))
            return;

        // OpenXR may re-register after us — put the control layout back.
        Restaurar();
    }

    private static void Restaurar()
    {
        try
        {
            // Drop the conflicting OpenXR device layout name if present.
            InputSystem.RemoveLayout("DPad");
            InputSystem.RegisterLayout<DpadControl>("Dpad");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[FixOpenXRDpadLayout] No se pudo restaurar layout Dpad: " + e.Message);
        }
    }
}
