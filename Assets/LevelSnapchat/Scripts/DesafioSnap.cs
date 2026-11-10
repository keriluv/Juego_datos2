using TMPro;
using UnityEngine;

public abstract class DesafioSnap : MonoBehaviour
{
    public System.Action Completado;

    protected SnapWebApp snap;
    protected RectTransform raiz;
    protected RectTransform historia;
    protected RectTransform mundo;
    protected JugadorTutorial jugador;
    protected RectTransform jugadorRT;
    protected VinetaHalloween vineta;
    protected bool terminado;
    private bool completado;

    protected Sprite SpriteRedondeado => snap != null ? snap.spriteRedondeado : null;
    protected TMP_FontAsset Fuente => snap != null ? snap.fuente : null;

    public void Iniciar(ContextoDesafio contexto)
    {
        snap = contexto.snap;
        raiz = contexto.raiz;
        historia = contexto.historia;
        mundo = contexto.mundo;
        jugador = contexto.jugador;
        jugadorRT = contexto.jugadorRT;
        vineta = contexto.vineta;

        Construir();
    }

    protected abstract void Construir();

    protected void Completar()
    {
        if (completado) return;
        completado = true;
        Completado?.Invoke();
    }
}
