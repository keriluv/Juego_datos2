using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Lives on the player prefab so crack hits can sync the cinematic over NGO.
/// </summary>
public class RelayTransicionMinecraft : NetworkBehaviour
{
    public static RelayTransicionMinecraft Local { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) Local = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Local == this) Local = null;
    }

    public void PedirTransicion()
    {
        if (!IsSpawned)
        {
            TransicionMinecraftVideo.SolicitarDesdeCrack();
            return;
        }

        if (IsServer)
            ReproducirClientRpc();
        else
            PedirServerRpc();
    }

    [ServerRpc]
    private void PedirServerRpc()
    {
        ReproducirClientRpc();
    }

    [ClientRpc]
    private void ReproducirClientRpc()
    {
        TransicionMinecraftVideo.SolicitarDesdeCrack();
    }
}
