using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: caen objetos desde arriba. Arrastra la cesta con el mouse (o el dedo).
// Ganas si atrapas "objetivoPuntos" piezas correctas antes de que acabe el tiempo.
// Pierdes si atrapas una pieza incorrecta o se acaba el tiempo.
public class MinijuegoAtrapar : MinijuegoBase, IPointerDownHandler, IDragHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform cesta;
    [SerializeField] RectTransform prefabObjeto;   // prefab con una Image
    [SerializeField] TMP_Text textoInfo;           // opcional

    [Header("Imágenes (si las dejas vacías se usan colores)")]
    [SerializeField] Sprite[] spritesCorrectos;
    [SerializeField] Sprite[] spritesIncorrectos;

    [Header("Ajustes")]
    [SerializeField] float duracion = 12f;
    [SerializeField] float velocidadCaida = 350f;
    [SerializeField] float intervaloSpawn = 0.7f;
    [SerializeField] int objetivoPuntos = 5;
    [SerializeField, Range(0f, 1f)] float probabilidadCorrecto = 0.65f;

    class Caida { public RectTransform rt; public bool correcto; }

    RectTransform area;
    readonly List<Caida> objetos = new List<Caida>();
    bool activo;
    float tiempo;
    float temporizadorSpawn;
    int puntos;

    void Awake()
    {
        area = (RectTransform)transform;
    }

    public override void Iniciar()
    {
        Detener();
        puntos = 0;
        tiempo = duracion;
        temporizadorSpawn = 0f;

        var grafico = cesta.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;
        cesta.anchoredPosition = new Vector2(0f, -area.rect.height / 2f + cesta.sizeDelta.y);

        activo = true;
        ActualizarTexto();
    }

    public override void Detener()
    {
        activo = false;
        foreach (var o in objetos)
            if (o.rt != null) Destroy(o.rt.gameObject);
        objetos.Clear();
    }

    void Update()
    {
        if (!activo) return;

        tiempo -= Time.deltaTime;
        temporizadorSpawn -= Time.deltaTime;
        if (temporizadorSpawn <= 0f)
        {
            Crear();
            temporizadorSpawn = intervaloSpawn;
        }

        float limiteInferior = -area.rect.height / 2f - 60f;

        for (int i = objetos.Count - 1; i >= 0; i--)
        {
            var o = objetos[i];
            o.rt.anchoredPosition += Vector2.down * velocidadCaida * Time.deltaTime;

            if (Toca(o.rt, cesta))
            {
                Destroy(o.rt.gameObject);
                objetos.RemoveAt(i);

                if (!o.correcto) { Terminar(false); return; }

                puntos++;
                if (puntos >= objetivoPuntos) { Terminar(true); return; }
            }
            else if (o.rt.anchoredPosition.y < limiteInferior)
            {
                Destroy(o.rt.gameObject);
                objetos.RemoveAt(i);
            }
        }

        ActualizarTexto();
        if (tiempo <= 0f) Terminar(false);
    }

    void Crear()
    {
        bool correcto = Random.value < probabilidadCorrecto;

        RectTransform rt = Instantiate(prefabObjeto, area);
        float mitad = area.rect.width / 2f - rt.sizeDelta.x / 2f;
        rt.anchoredPosition = new Vector2(
            Random.Range(-mitad, mitad),
            area.rect.height / 2f + rt.sizeDelta.y);

        Image img = rt.GetComponent<Image>();
        img.raycastTarget = false;

        Sprite[] lista = correcto ? spritesCorrectos : spritesIncorrectos;
        if (lista != null && lista.Length > 0)
            img.sprite = lista[Random.Range(0, lista.Length)];
        else
            img.color = correcto ? Color.green : Color.red;

        objetos.Add(new Caida { rt = rt, correcto = correcto });
    }

    // Choque simple entre dos rectángulos (ambos con pivote y anclas al centro)
    bool Toca(RectTransform a, RectTransform b)
    {
        Vector2 d = a.anchoredPosition - b.anchoredPosition;
        Vector2 m = (a.sizeDelta + b.sizeDelta) / 2f;
        return Mathf.Abs(d.x) < m.x && Mathf.Abs(d.y) < m.y;
    }

    void ActualizarTexto()
    {
        if (textoInfo == null) return;
        textoInfo.text = $"Piezas: {puntos}/{objetivoPuntos}    Tiempo: {Mathf.CeilToInt(Mathf.Max(0f, tiempo))}";
    }

    // Mover la cesta con el mouse / dedo
    public void OnPointerDown(PointerEventData e) { Mover(e); }
    public void OnDrag(PointerEventData e) { Mover(e); }

    void Mover(PointerEventData e)
    {
        if (!activo) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                area, e.position, e.pressEventCamera, out Vector2 local))
        {
            float mitad = area.rect.width / 2f - cesta.sizeDelta.x / 2f;
            cesta.anchoredPosition = new Vector2(Mathf.Clamp(local.x, -mitad, mitad), cesta.anchoredPosition.y);
        }
    }
}
