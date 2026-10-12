using UnityEngine;
using System.Collections;
using TMPro;

public class ChromerVillano : MonoBehaviour
{
    [Header("Ataque")]
    public GameObject prefabProyectil;
    public Transform puntoDisparo;
    public int totalProyectiles = 12;
    public float intervaloDisparo = 0.8f; // Disparos más seguidos

    [Header("Movimiento Libre y Amplio")]
    public float velocidadGiro = 2.5f;     
    public float radioHorizontal = 4.5f; // Mucho más amplio a los lados
    public float radioVertical = 3.0f;   // Mucho más amplio arriba y abajo
    
    private Vector3 posicionCentral;
    private float contadorTiempo = 0f;
    private bool estaAtacando = false;

    [Header("Mensajes de Victoria/Derrota")]
    public TextMeshProUGUI textoDialogoClippy;
    public GameObject panelDialogoClippy;

    private int proyectilesLanzados = 0;

    void Start()
    {
        posicionCentral = transform.position;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (estaAtacando)
        {
            contadorTiempo += Time.deltaTime * velocidadGiro;

            // Movimiento fluido en 'Infinito / Figura 8' que recorre gran parte de la pantalla
            float x = posicionCentral.x + Mathf.Cos(contadorTiempo) * radioHorizontal;
            float y = posicionCentral.y + Mathf.Sin(contadorTiempo * 2f) * radioVertical;

            transform.position = new Vector3(x, y, posicionCentral.z);
        }
    }

    public void IniciarAparicionYEspecial()
    {
        posicionCentral = transform.position;
        StartCoroutine(RutinaAparicionYCombate());
    }

    IEnumerator RutinaAparicionYCombate()
    {
        // Animación de aparición
        float tiempoAnim = 0f;
        float duracionAnim = 1.0f;

        while (tiempoAnim < duracionAnim)
        {
            tiempoAnim += Time.deltaTime;
            float progreso = tiempoAnim / duracionAnim;
            transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, progreso);
            yield return null;
        }
        transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.5f);

        estaAtacando = true;
        StartCoroutine(RutinaAtaque());
    }

    IEnumerator RutinaAtaque()
    {
        while (proyectilesLanzados < totalProyectiles)
        {
            LanzarProyectil();
            proyectilesLanzados++;
            yield return new WaitForSeconds(intervaloDisparo);
        }

        // Fin del ataque / Victoria
        estaAtacando = false;
        yield return new WaitForSeconds(1f);
        if (panelDialogoClippy != null) panelDialogoClippy.SetActive(true);
        if (textoDialogoClippy != null)
        {
            textoDialogoClippy.text = "¡LO LOGRASTE! Sobreviviste a Chromer. ¡Has ganado!";
        }
    }

    void LanzarProyectil()
    {
        if (prefabProyectil != null)
        {
            Vector3 pos = puntoDisparo != null ? puntoDisparo.position : transform.position;
            Instantiate(prefabProyectil, pos, Quaternion.identity);
        }
    }
}