using System.Collections.Generic;
using UnityEngine;

// Congela a la cápsula mientras el cuadro del minijuego esté abierto.
// No hay que tocar los scripts de movimiento: este script los apaga cuando se abre
// el minijuego y los vuelve a encender cuando se cierra ("Volver al feed").
// Mientras está congelada, la cápsula no camina, no salta y no se mueve de ninguna forma.
[DefaultExecutionOrder(-50)]
public class CongelarJugador : MonoBehaviour
{
    [SerializeField] GameObject jugador;           // tu JugadorCapsula
    [SerializeField] GameObject panelMinijuego;    // el PanelMinijuego (el cuadro del minijuego)
    [SerializeField] MonoBehaviour[] scriptsExtra; // opcional: otros scripts que también deban apagarse

    readonly List<MonoBehaviour> apagados = new List<MonoBehaviour>();
    bool congelado;

    Rigidbody2D rb2d;
    Rigidbody rb3d;
    bool rb2dSimulaba;
    bool rb3dEraCinematico;

    void Update()
    {
        if (jugador == null || panelMinijuego == null) return;

        bool abierto = panelMinijuego.activeInHierarchy;
        if (abierto && !congelado) Congelar();
        else if (!abierto && congelado) Descongelar();
    }

    void Congelar()
    {
        congelado = true;
        apagados.Clear();

        // apaga los scripts propios del jugador (movimiento, salto, etc.)
        foreach (var m in jugador.GetComponentsInChildren<MonoBehaviour>(false))
        {
            if (m == null || m == this || !m.enabled) continue;
            if (!EsScriptPropio(m)) continue;

            m.enabled = false;
            apagados.Add(m);
        }

        // scripts extra que hayas indicado en el Inspector
        if (scriptsExtra != null)
        {
            foreach (var m in scriptsExtra)
            {
                if (m == null || m == this || !m.enabled) continue;
                m.enabled = false;
                apagados.Add(m);
            }
        }

        // detiene la física para que no siga deslizándose ni cayendo
        rb2d = jugador.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2dSimulaba = rb2d.simulated;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.simulated = false;
        }

        rb3d = jugador.GetComponent<Rigidbody>();
        if (rb3d != null)
        {
            rb3dEraCinematico = rb3d.isKinematic;
            if (!rb3d.isKinematic)
            {
                rb3d.linearVelocity = Vector3.zero;
                rb3d.angularVelocity = Vector3.zero;
            }
            rb3d.isKinematic = true;
        }
    }

    void Descongelar()
    {
        congelado = false;

        foreach (var m in apagados)
            if (m != null) m.enabled = true;
        apagados.Clear();

        if (rb2d != null) rb2d.simulated = rb2dSimulaba;
        if (rb3d != null) rb3d.isKinematic = rb3dEraCinematico;
    }

    // Solo apaga scripts hechos por ti (no imágenes, textos ni componentes de Unity)
    static bool EsScriptPropio(MonoBehaviour m)
    {
        System.Type tipo = m.GetType();
        return tipo.Assembly.GetName().Name == "Assembly-CSharp" || tipo.Name == "PlayerInput";
    }
}
