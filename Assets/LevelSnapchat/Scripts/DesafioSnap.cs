using TMPro;
using System.Collections.Generic;
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
    protected JugadorTutorial jugador2;
    protected RectTransform jugador2RT;
    protected JugadorTutorial[] jugadores;
    protected RectTransform[] jugadoresRT;
    protected VinetaHalloween vineta;
    protected bool online;
    protected bool esAnfitrion;
    protected RedSnapchat red;
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
        jugador2 = contexto.jugador2;
        jugador2RT = contexto.jugador2RT;
        jugadores = contexto.jugadores;
        jugadoresRT = contexto.jugadoresRT;
        vineta = contexto.vineta;
        online = contexto.online;
        esAnfitrion = contexto.esAnfitrion;
        red = contexto.red;

        if (jugadores == null)
        {
            List<JugadorTutorial> lista = new List<JugadorTutorial>();

            if (jugador != null) lista.Add(jugador);
            if (jugador2 != null) lista.Add(jugador2);

            jugadores = lista.ToArray();
        }

        if (jugadoresRT == null || jugadoresRT.Length != jugadores.Length)
        {
            jugadoresRT = new RectTransform[jugadores.Length];

            for (int i = 0; i < jugadores.Length; i++)
                jugadoresRT[i] = jugadores[i] != null ? (RectTransform)jugadores[i].transform : null;
        }

        Construir();
    }

    protected abstract void Construir();

    protected void Completar()
    {
        if (completado) return;
        if (online && !esAnfitrion) return;
        completado = true;
        Completado?.Invoke();
    }
}
