using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Place on hole triggers in YouTube levels. Respawns only the owning player.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HoyoReaparicion : MonoBehaviour
{
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ReaparicionJugador reaparicion = other.GetComponentInParent<ReaparicionJugador>();
        if (reaparicion == null) return;

        NetworkObject red = other.GetComponentInParent<NetworkObject>();
        if (red != null && red.IsSpawned && !red.IsOwner) return;

        reaparicion.NotificarHoyo();
    }
}
