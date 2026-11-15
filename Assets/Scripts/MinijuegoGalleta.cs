using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: romper la galleta.
// 1) Cuenta regresiva (3, 2, 1, ¡YA!) sin que cuenten los clics.
// 2) Tienes "duracion" segundos para hacer "clicsNecesarios" clics.
// Cada clic agrieta más la galleta. Si llegas a la meta, se rompe y ganas.
// Si se acaba el tiempo antes, pierdes.
public class MinijuegoGalleta : MinijuegoBase, IPointerDownHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] Image galleta;
    [SerializeField] TMP_Text textoCuenta;   // número grande en el centro
    [SerializeField] TMP_Text textoInfo;     // texto de arriba (tiempo y clics)

    [Header("Imágenes de la galleta")]
    [SerializeField] Sprite[] etapas;        // de entera a muy agrietada, en orden
    [SerializeField] Sprite galletaRota;

    [Header("Ajustes (puedes cambiarlos aquí)")]
    [SerializeField] int clicsNecesarios = 30;
    [SerializeField] float duracion = 6f;
    [SerializeField] float cuentaRegresiva = 3f;

    enum Estado { Inactivo, Cuenta, Jugando, Fin }
    Estado estado = Estado.Inactivo;

    float tiempo;
    float cuenta;
    float mostrarYa;
    float esperaFin;
    float golpe;
    int clics;

    public override void Iniciar()
    {
        clics = 0;
        tiempo = duracion;
        cuenta = cuentaRegresiva;
        mostrarYa = 0f;
        golpe = 0f;
        estado = Estado.Cuenta;

        galleta.raycastTarget = false;   // los clics los recibe toda la zona de juego
        galleta.rectTransform.localScale = Vector3.one;
        MostrarEtapa();

        textoCuenta.gameObject.SetActive(true);
        textoCuenta.text = Mathf.CeilToInt(cuenta).ToString();
        ActualizarTexto();
    }

    public override void Detener()
    {
        estado = Estado.Inactivo;
        if (textoCuenta != null) textoCuenta.gameObject.SetActive(false);
        if (galleta != null) galleta.rectTransform.localScale = Vector3.one;
    }

    void Update()
    {
        if (estado == Estado.Inactivo) return;

        // pequeño "salto" de la galleta cada vez que le das clic
        golpe = Mathf.MoveTowards(golpe, 0f, Time.deltaTime * 6f);
        galleta.rectTransform.localScale = Vector3.one * (1f + 0.12f * golpe);

        switch (estado)
        {
            case Estado.Cuenta:
                cuenta -= Time.deltaTime;
                if (cuenta <= 0f)
                {
                    estado = Estado.Jugando;
                    textoCuenta.text = "¡YA!";
                    mostrarYa = 0.6f;
                }
                else
                {
                    textoCuenta.text = Mathf.CeilToInt(cuenta).ToString();
                }
                break;

            case Estado.Jugando:
                tiempo -= Time.deltaTime;

                if (mostrarYa > 0f)
                {
                    mostrarYa -= Time.deltaTime;
                    if (mostrarYa <= 0f) textoCuenta.gameObject.SetActive(false);
                }

                if (tiempo <= 0f)
                {
                    tiempo = 0f;
                    ActualizarTexto();
                    Terminar(false);
                    return;
                }
                ActualizarTexto();
                break;

            case Estado.Fin:
                esperaFin -= Time.deltaTime;
                if (esperaFin <= 0f) Terminar(true);
                break;
        }
    }

    // Cada clic (o toque) dentro de la zona de juego cuenta
    public void OnPointerDown(PointerEventData e)
    {
        if (estado != Estado.Jugando) return;

        clics++;
        golpe = 1f;
        MostrarEtapa();
        ActualizarTexto();

        if (clics >= clicsNecesarios)
        {
            if (galletaRota != null) galleta.sprite = galletaRota;
            golpe = 1.5f;
            estado = Estado.Fin;
            esperaFin = 0.8f;    // deja ver la galleta rota un momento antes del mensaje
        }
    }

    void MostrarEtapa()
    {
        if (etapas == null || etapas.Length == 0) return;
        float progreso = Mathf.Clamp01((float)clics / Mathf.Max(1, clicsNecesarios));
        int i = Mathf.Min(etapas.Length - 1, Mathf.FloorToInt(progreso * etapas.Length));
        galleta.sprite = etapas[i];
    }

    void ActualizarTexto()
    {
        if (textoInfo == null) return;
        textoInfo.text = $"Tiempo: {Mathf.CeilToInt(Mathf.Max(0f, tiempo))}    Clics: {clics}/{clicsNecesarios}";
    }
}
