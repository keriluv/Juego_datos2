using UnityEngine;

public class ZonaOscuridad : MonoBehaviour
{
    [Tooltip("Vineta que se activa. Si se deja vacia, usa la instancia activa.")]
    public VinetaHalloween vineta;

    [Tooltip("Tag del jugador.")]
    public string tagJugador = "Player";

    [Tooltip("Si esta activado, solo reacciona al tag indicado.")]
    public bool soloJugador = true;

    private void Reset()
    {
        Collider2D colisionador2D = GetComponent<Collider2D>();
        if (colisionador2D != null) colisionador2D.isTrigger = true;

        Collider colisionador3D = GetComponent<Collider>();
        if (colisionador3D != null) colisionador3D.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!EsJugador(otro)) return;
        Activar(true);
    }

    private void OnTriggerExit2D(Collider2D otro)
    {
        if (!EsJugador(otro)) return;
        Activar(false);
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (!EsJugador(otro)) return;
        Activar(true);
    }

    private void OnTriggerExit(Collider otro)
    {
        if (!EsJugador(otro)) return;
        Activar(false);
    }

    private void Activar(bool entrar)
    {
        VinetaHalloween destino = vineta != null ? vineta : VinetaHalloween.Instancia;
        if (destino == null) return;

        if (entrar) destino.Entrar();
        else destino.Salir();
    }

    private bool EsJugador(Component otro)
    {
        if (!soloJugador) return true;
        return otro.CompareTag(tagJugador);
    }

    private void OnDrawGizmos()
    {
        Bounds limites;
        Collider2D colisionador2D = GetComponent<Collider2D>();
        Collider colisionador3D = GetComponent<Collider>();

        if (colisionador2D != null) limites = colisionador2D.bounds;
        else if (colisionador3D != null) limites = colisionador3D.bounds;
        else return;

        Gizmos.color = new Color(0.55f, 0.2f, 0.8f, 0.18f);
        Gizmos.DrawCube(limites.center, limites.size);
        Gizmos.color = new Color(0.55f, 0.2f, 0.8f, 1f);
        Gizmos.DrawWireCube(limites.center, limites.size);
    }
}
