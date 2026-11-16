using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: esquivar tráfico.
// Arrastra el mouse (o el dedo) de lado a lado para mover tu carro por la carretera.
// Ganas si aguantas "duracion" segundos sin chocar. Pierdes si chocas con otro carro.
public class MinijuegoEsquivar : MinijuegoBase, IPointerDownHandler, IDragHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform autoJugador;
    [SerializeField] RectTransform prefabTrafico;   // prefab con una Image
    [SerializeField] TMP_Text textoTiempo;

    [Header("Imágenes del tráfico (si lo dejas vacío usa colores al azar)")]
    [SerializeField] Sprite[] spritesTrafico;

    [Header("Ajustes")]
    [SerializeField] float duracion = 12f;
    [SerializeField] float velocidad = 320f;          // velocidad de la carretera
    [SerializeField] float intervaloTrafico = 0.75f;  // segundos entre carro y carro
    [SerializeField] float velocidadLateral = 650f;   // qué tan rápido gira tu carro
    [SerializeField] int carriles = 4;
    [SerializeField, Range(0.5f, 1f)] float tamanoChoque = 0.8f; // 1 = choca con todo el dibujo

    RectTransform area;
    readonly List<RectTransform> trafico = new List<RectTransform>();
    readonly List<RectTransform> marcasMoviles = new List<RectTransform>();
    readonly List<RectTransform> decoracion = new List<RectTransform>();

    bool activo;
    float tiempo;
    float temporizador;
    float objetivoX;
    float largoCiclo;

    void Awake()
    {
        area = (RectTransform)transform;
    }

    public override void Iniciar()
    {
        Detener();
        tiempo = duracion;
        temporizador = 0.3f;
        objetivoX = 0f;

        var grafico = autoJugador.GetComponent<Graphic>();
        if (grafico != null) grafico.raycastTarget = false;
        autoJugador.anchoredPosition = new Vector2(0f, -area.rect.height / 2f + autoJugador.sizeDelta.y * 0.8f);

        CrearCarretera();
        activo = true;
        ActualizarTexto();
    }

    public override void Detener()
    {
        activo = false;
        foreach (var t in trafico) if (t != null) Destroy(t.gameObject);
        foreach (var d in decoracion) if (d != null) Destroy(d.gameObject);
        trafico.Clear();
        decoracion.Clear();
        marcasMoviles.Clear();
    }

    void Update()
    {
        if (!activo) return;
        float dt = Time.deltaTime;

        tiempo -= dt;
        temporizador -= dt;
        if (temporizador <= 0f)
        {
            CrearAuto();
            temporizador = intervaloTrafico;
        }

        // el carro del jugador se mueve hacia donde apuntas, con velocidad limitada
        Vector2 pos = autoJugador.anchoredPosition;
        pos.x = Mathf.MoveTowards(pos.x, objetivoX, velocidadLateral * dt);
        autoJugador.anchoredPosition = pos;

        float limite = -area.rect.height / 2f - 60f;

        // rayas de la carretera
        foreach (var m in marcasMoviles)
        {
            Vector2 p = m.anchoredPosition;
            p.y -= velocidad * dt;
            if (p.y < limite) p.y += largoCiclo;
            m.anchoredPosition = p;
        }

        // tráfico
        for (int i = trafico.Count - 1; i >= 0; i--)
        {
            var t = trafico[i];
            t.anchoredPosition += Vector2.down * velocidad * 0.85f * dt;

            if (Choca(t, autoJugador)) { Terminar(false); return; }

            if (t.anchoredPosition.y < limite - t.sizeDelta.y)
            {
                Destroy(t.gameObject);
                trafico.RemoveAt(i);
            }
        }

        if (tiempo <= 0f)
        {
            tiempo = 0f;
            ActualizarTexto();
            Terminar(true);
            return;
        }
        ActualizarTexto();
    }

    void CrearAuto()
    {
        RectTransform rt = Instantiate(prefabTrafico, area);
        float anchoCarril = area.rect.width / carriles;
        int c = Random.Range(0, carriles);
        float x = -area.rect.width / 2f + anchoCarril * (c + 0.5f);
        rt.anchoredPosition = new Vector2(x, area.rect.height / 2f + rt.sizeDelta.y);

        Image img = rt.GetComponent<Image>();
        img.raycastTarget = false;
        if (spritesTrafico != null && spritesTrafico.Length > 0)
            img.sprite = spritesTrafico[Random.Range(0, spritesTrafico.Length)];
        else
            img.color = new Color(Random.value, Random.value, Random.value);

        trafico.Add(rt);
    }

    void CrearCarretera()
    {
        float w = area.rect.width;
        float h = area.rect.height;
        float largo = 50f, hueco = 40f, paso = largo + hueco;
        int cantidad = Mathf.CeilToInt(h / paso) + 2;
        largoCiclo = cantidad * paso;
        float anchoCarril = w / carriles;

        // líneas discontinuas entre carriles
        for (int c = 1; c < carriles; c++)
        {
            float x = -w / 2f + anchoCarril * c;
            for (int i = 0; i < cantidad; i++)
            {
                var m = NuevaMarca(x, h / 2f + largo - i * paso, 10f, largo, new Color(1f, 1f, 1f, 0.8f));
                marcasMoviles.Add(m);
            }
        }

        // bordes de la carretera (fijos)
        NuevaMarca(-w / 2f + 8f, 0f, 8f, h, new Color(1f, 0.85f, 0.2f, 1f));
        NuevaMarca(w / 2f - 8f, 0f, 8f, h, new Color(1f, 0.85f, 0.2f, 1f));
    }

    RectTransform NuevaMarca(float x, float y, float ancho, float alto, Color color)
    {
        var go = new GameObject("Marca", typeof(RectTransform), typeof(Image));
        go.layer = gameObject.layer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(area, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(ancho, alto);
        rt.anchoredPosition = new Vector2(x, y);

        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        rt.SetAsFirstSibling(); // siempre detrás de los carros
        decoracion.Add(rt);
        return rt;
    }

    bool Choca(RectTransform a, RectTransform b)
    {
        Vector2 d = a.anchoredPosition - b.anchoredPosition;
        Vector2 m = (a.sizeDelta + b.sizeDelta) / 2f * tamanoChoque;
        return Mathf.Abs(d.x) < m.x && Mathf.Abs(d.y) < m.y;
    }

    void ActualizarTexto()
    {
        if (textoTiempo != null)
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(Mathf.Max(0f, tiempo))}";
    }

    // Mover con mouse / dedo
    public void OnPointerDown(PointerEventData e) { Apuntar(e); }
    public void OnDrag(PointerEventData e) { Apuntar(e); }

    void Apuntar(PointerEventData e)
    {
        if (!activo) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                area, e.position, e.pressEventCamera, out Vector2 local))
        {
            float mitad = area.rect.width / 2f - autoJugador.sizeDelta.x / 2f;
            objetivoX = Mathf.Clamp(local.x, -mitad, mitad);
        }
    }
}
