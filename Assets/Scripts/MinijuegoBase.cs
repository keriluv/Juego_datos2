using System;
using UnityEngine;

// Todos los minijuegos heredan de esta clase.
// Así el gestor puede manejar cualquier minijuego igual, sin saber cuál es.
public abstract class MinijuegoBase : MonoBehaviour
{
    // true = el jugador ganó, false = perdió
    public event Action<bool> AlTerminar;

    public abstract void Iniciar();
    public abstract void Detener();

    protected void Terminar(bool gano)
    {
        Detener();
        AlTerminar?.Invoke(gano);
    }
}
