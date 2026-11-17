using UnityEngine;

/// <summary>
/// Vida que leen las balas. Delega en BarcoSalud para hundir el casco al llegar a cero.
/// </summary>
public class ShipHealth : MonoBehaviour
{
    private BarcoSalud casco;

    public float CurrentHealth => casco != null ? casco.Vida : 0f;
    public float MaxHealth => casco != null ? casco.VidaMaxima : 100f;

    public void Configurar(BarcoSalud saludDelCasco)
    {
        casco = saludDelCasco;
    }

    public void TakeDamage(float cantidad)
    {
        if (casco == null)
            casco = GetComponent<BarcoSalud>();
        if (casco == null || casco.EstaHundido)
            return;

        int equipoAtacante = casco.EsJugador ? 1 : 0;
        casco.RecibirDanio(cantidad, equipoAtacante);
    }
}
