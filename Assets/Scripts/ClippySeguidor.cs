using UnityEngine;
using TMPro;

public class ClippySeguidor : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform jugador;             
    public float velocidadSuavizado = 4f;   
    public Vector3 offset = new Vector3(1.2f, 1f, 0f); 

    [Header("Tiempos Iniciales y Retrasos")]
    public float tiempoEsperaAparicion = 2f; 
    public float retrasoMensaje = 0.5f;
    public float retrasoRespuestaJugador = 0.8f; 

    [Header("Interfaz (UI)")]
    public GameObject panelDialogo;       
    public TextMeshProUGUI textoDialogo;    

    [Header("Villano Chromer")]
    public ChromerVillano scriptChromer;
    public GameObject objetoChromer;

    private bool estaActivo = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 posicionObjetivo;
    
    private Rigidbody rbJugador;
    private ControlJugador scriptControl;

    private enum EstadoTutorial { EsperandoMovimiento, ProcesandoMovimiento, EsperandoSalto, ProcesandoSalto, SecuenciaChromer, Finalizado }
    private EstadoTutorial estadoActual = EstadoTutorial.EsperandoMovimiento;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (panelDialogo != null) panelDialogo.SetActive(false);
        if (objetoChromer != null) objetoChromer.SetActive(false); // Chromer oculto al inicio

        if (jugador != null)
        {
            rbJugador = jugador.GetComponent<Rigidbody>();
            scriptControl = jugador.GetComponent<ControlJugador>();
        }

        BloquearTodo(true);
        Invoke("AparecerClippy", tiempoEsperaAparicion);
    }

    void AparecerClippy()
    {
        estaActivo = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true; 

        if (panelDialogo != null)
        {
            Invoke("MostrarPrimerMensaje", retrasoMensaje);
        }
    }

    void MostrarPrimerMensaje()
    {
        if (panelDialogo != null) panelDialogo.SetActive(true);

        if (textoDialogo != null)
        {
            textoDialogo.text = "Hola, eres nuevo por aqui? Bienvenido a Navify";
        }

        Invoke("MostrarSegundoMensaje", 4f);
    }

    void MostrarSegundoMensaje()
    {
        if (textoDialogo != null)
        {
            textoDialogo.text = "No te quedes ahi parado, Usa A y D para moverte a los lados";
        }

        if (scriptControl != null)
        {
            scriptControl.movimientoHabilitado = true;
        }

        if (rbJugador != null)
        {
            rbJugador.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        estadoActual = EstadoTutorial.EsperandoMovimiento;
    }

    void Update()
    {
        if (!estaActivo || jugador == null) return;

        posicionObjetivo = jugador.position + offset;
        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, velocidadSuavizado * Time.deltaTime);

        if (scriptControl != null)
        {
            if (estadoActual == EstadoTutorial.EsperandoMovimiento && scriptControl.yaSeMovio)
            {
                estadoActual = EstadoTutorial.ProcesandoMovimiento;
                Invoke("SecuenciaMensajeSalto", retrasoRespuestaJugador);
            }
            else if (estadoActual == EstadoTutorial.EsperandoSalto && scriptControl.yaSalto)
            {
                estadoActual = EstadoTutorial.ProcesandoSalto;
                Invoke("SecuenciaMensajeFinal", retrasoRespuestaJugador);
            }
        }
    }

    void SecuenciaMensajeSalto()
    {
        if (textoDialogo != null)
        {
            textoDialogo.text = "Muy bien, hecho. Ahora que tal si probamos algo diferente, presiona espacio para saltar";
        }

        if (scriptControl != null)
        {
            scriptControl.saltoHabilitado = true;
        }

        estadoActual = EstadoTutorial.EsperandoSalto;
    }

    void SecuenciaMensajeFinal()
    {
        if (textoDialogo != null)
        {
            textoDialogo.text = "WOW, aprendes rapido";
        }

        estadoActual = EstadoTutorial.SecuenciaChromer;
        Invoke("MensajeDesafio", 3.5f);
    }

    void MensajeDesafio()
    {
        if (textoDialogo != null)
        {
            textoDialogo.text = "Creo que ya estas listo para un pequeño desafio...";
        }

        Invoke("LlamarAChromer", 3.5f);
    }

    void LlamarAChromer()
    {
        if (textoDialogo != null)
        {
            textoDialogo.text = "CHROMEEEEER, VEN AQUI";
        }

        Invoke("AparecerChromerEvent", 2f);
    }

    void AparecerChromerEvent()
    {
        if (objetoChromer != null) 
        {
            objetoChromer.SetActive(true);
        }

        if (textoDialogo != null)
        {
            textoDialogo.text = "¡Esquiva sus ataques!";
        }

        if (scriptChromer != null)
        {
            // Llama a la nueva rutina de animación y combate de Chromer
            scriptChromer.IniciarAparicionYEspecial();
        }

        Invoke("OcultarDialogoFinal", 2.5f);
    }

    void OcultarDialogoFinal()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);
    }

    void BloquearTodo(bool bloquear)
    {
        if (scriptControl != null)
        {
            scriptControl.movimientoHabilitado = !bloquear;
            scriptControl.saltoHabilitado = !bloquear;
        }

        if (rbJugador != null && bloquear)
        {
            rbJugador.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}