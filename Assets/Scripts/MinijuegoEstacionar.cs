using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: estacionar justo en el momento.
// Tu carro se mueve de lado a lado abajo. Arriba hay una fila de carros estacionados con UN hueco libre.
// Pulsa ESPACIO, la flecha ↑ o haz clic cuando tu carro esté justo debajo del hueco para meterte.
// Si no estás bien alineado, chocas con los carros de al lado y pierdes una vida.
// Ganas si estacionas "aciertosNecesarios" veces. Pierdes si se acaban las vidas o el tiempo.
public class MinijuegoEstacionar : MinijuegoBase, IPointerDownHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform autoJugador;     // tu carro (mira hacia arriba)
    [SerializeField] RectTransform prefabCarro;     // prefab con una Image (los carros estacionados)
    [SerializeField] TMP_Text textoCuenta;          // número grande al centro (3, 2, 1...)
    [SerializeField] TMP_Text textoInfo;            // texto de arriba

    [Header("Imágenes de los carros estacionados (opcional)")]
    [SerializeField] Sprite[] spritesEstacionados;

    [Header("Reglas")]
    [SerializeField] int aciertosNecesarios = 3;
    [SerializeField] int fallosPermitidos = 1;      // 1 = puedes fallar una vez, el segundo fallo pierde
    [SerializeField] float tiempoLimite = 15f;
    [SerializeField] float cuentaRegresiva = 3f;    // segundos de cuenta antes de empezar (0 = sin cuenta)
    [SerializeField] string textoFinalCuenta = "0"; // lo último que se muestra (puedes poner ¡YA!)

    [Header("Velocidad y dificultad")]
    [SerializeField] float velocidadCarro = 300f;   // velocidad del carro de lado a lado
    [SerializeField] int espacios = 8;              // MÁS espacios = hueco más angosto = MÁS difícil
    [SerializeField] float holgura = 0f;            // píxeles extra de permiso (positivo = más fácil)
    [SerializeField] float velocidadEntrada = 900f; // qué tan rápido se mete al hueco

    [Header("Posiciones")]
    [SerializeField] float margenSuperior = 100f;   // distancia de la fila de carros al borde de arriba
    [SerializeField] float margenInferior = 100f;   // distancia de tu carril al borde de abajo

    enum Fase { Inactivo, Cuenta, Moviendo, Entrando, Pausa, Volviendo }
    Fase fase = Fase.Inactivo;

    RectTransform area;
    readonly List<RectTransform> fila = new List<RectTransform>();
    readonly List<RectTransform> decoracion = new List<RectTransform>();

    int aciertos, fallos, espacioLibre = -1;
    float tiempo, cuenta, direccion, pausaT, yFila, yCarril;
    bool pulsado, intentoAcertado;
    Vector2 destino, posChoque;

    float Paso { get { return area.rect.width / Mathf.Max(2, espacios); } }
    float PosX(int i) { return -area.rect.width / 2f + Paso * (i + 0.5f); }
    float LimiteX() { return area.rect.width / 2f - autoJugador.sizeDelta.x / 2f - 6f; }

    // Qué tan lejos del centro del hueco puedes estar y aun así entrar sin tocar a los de al lado
    float Tolerancia()
    {
        float t = (2f * Paso - prefabCarro.sizeDelta.x - autoJugador.sizeDelta.x) / 2f + holgura;
        return Mathf.Max(4f, t);
    }

    void Awake()
    {
        area = (RectTransform)transform;
    }

    public override void Iniciar()
    {
        Detener();
        aciertos = 0;
        fallos = 0;
        espacioLibre = -1;
        tiempo = tiempoLimite;
        yFila = area.rect.height / 2f - margenSuperior;
        yCarril = -area.rect.height / 2f + margenInferior;

        var g = autoJugador.GetComponent<Graphic>();
        if (g != null) g.raycastTarget = false;

        CrearLineas();
        NuevaRonda();

        cuenta = cuentaRegresiva;
        fase = cuentaRegresiva > 0f ? Fase.Cuenta : Fase.Moviendo;
        if (fase == Fase.Cuenta) MostrarCuenta(Mathf.CeilToInt(cuenta).ToString());
        ActualizarTexto();
    }

    public override void Detener()
    {
        fase = Fase.Inactivo;
        DestruirFila();
        foreach (var d in decoracion) if (d != null) Destroy(d.gameObject);
        decoracion.Clear();
        OcultarCuenta();
    }

    void Update()
    {
        if (fase == Fase.Inactivo) return;
        float dt = Time.deltaTime;

        // cuenta regresiva
        if (fase == Fase.Cuenta)
        {
            cuenta -= dt;
            if (cuenta > 0f)
            {
                MostrarCuenta(Mathf.CeilToInt(cuenta).ToString());
            }
            else
            {
                MostrarCuenta(textoFinalCuenta);
                if (cuenta <= -0.5f)
                {
                    OcultarCuenta();
                    fase = Fase.Moviendo;
                }
            }
            pulsado = false;
            return;
        }

        tiempo -= dt;
        if (tiempo <= 0f)
        {
            tiempo = 0f;
            ActualizarTexto();
            Terminar(false);
            return;
        }

        switch (fase)
        {
            case Fase.Moviendo: Mover(dt); break;
            case Fase.Entrando: Entrar(dt); break;
            case Fase.Pausa: EsperarPausa(dt); break;
            case Fase.Volviendo: Volver(dt); break;
        }

        pulsado = false;
        ActualizarTexto();
    }

    void Mover(float dt)
    {
        Vector2 p = autoJugador.anchoredPosition;
        float lim = LimiteX();
        p.x += direccion * velocidadCarro * dt;
        if (p.x > lim) { p.x = lim; direccion = -1f; }
        else if (p.x < -lim) { p.x = -lim; direccion = 1f; }
        autoJugador.anchoredPosition = p;

        if (pulsado || TeclaPresionada()) Intentar();
    }

    void Intentar()
    {
        float xLibre = PosX(espacioLibre);
        float dx = Mathf.Abs(autoJugador.anchoredPosition.x - xLibre);
        intentoAcertado = dx <= Tolerancia();

        if (intentoAcertado)
        {
            destino = new Vector2(xLibre, yFila);
        }
        else
        {
            float yChoque = yFila - (prefabCarro.sizeDelta.y + autoJugador.sizeDelta.y) / 2f;
            destino = new Vector2(autoJugador.anchoredPosition.x, yChoque);
        }
        fase = Fase.Entrando;
    }

    void Entrar(float dt)
    {
        autoJugador.anchoredPosition = Vector2.MoveTowards(autoJugador.anchoredPosition, destino, velocidadEntrada * dt);
        if (Vector2.Distance(autoJugador.anchoredPosition, destino) < 0.5f)
        {
            posChoque = autoJugador.anchoredPosition;
            pausaT = intentoAcertado ? 0.5f : 0.6f;
            fase = Fase.Pausa;
        }
    }

    void EsperarPausa(float dt)
    {
        pausaT -= dt;

        // temblor al chocar
        if (!intentoAcertado)
        {
            float temblor = Mathf.Sin(Time.time * 70f) * 6f * Mathf.Clamp01(pausaT * 2f);
            autoJugador.anchoredPosition = posChoque + new Vector2(temblor, 0f);
        }

        if (pausaT > 0f) return;

        if (intentoAcertado)
        {
            aciertos++;
            if (aciertos >= aciertosNecesarios) { Terminar(true); return; }
            NuevaRonda();
            fase = Fase.Moviendo;
        }
        else
        {
            fallos++;
            if (fallos > fallosPermitidos) { Terminar(false); return; }
            autoJugador.anchoredPosition = posChoque;
            fase = Fase.Volviendo;
        }
    }

    void Volver(float dt)
    {
        Vector2 meta = new Vector2(autoJugador.anchoredPosition.x, yCarril);
        autoJugador.anchoredPosition = Vector2.MoveTowards(autoJugador.anchoredPosition, meta, velocidadEntrada * 0.6f * dt);
        if (Mathf.Abs(autoJugador.anchoredPosition.y - yCarril) < 0.5f)
        {
            direccion = Random.value < 0.5f ? -1f : 1f;
            fase = Fase.Moviendo;
        }
    }

    // Arma la fila de carros con un hueco libre en un lugar al azar
    void NuevaRonda()
    {
        DestruirFila();

        int anterior = espacioLibre;
        do { espacioLibre = Random.Range(0, espacios); }
        while (espacios > 1 && espacioLibre == anterior);

        for (int i = 0; i < espacios; i++)
        {
            if (i == espacioLibre) continue;

            RectTransform rt = Instantiate(prefabCarro, area);
            rt.anchoredPosition = new Vector2(PosX(i), yFila);

            Image img = rt.GetComponent<Image>();
            img.raycastTarget = false;
            if (spritesEstacionados != null && spritesEstacionados.Length > 0)
                img.sprite = spritesEstacionados[Random.Range(0, spritesEstacionados.Length)];
            else
                img.color = new Color(Random.value, Random.value, Random.value);

            fila.Add(rt);
        }

        // marca verde suave en el hueco libre
        var marca = NuevaMarca(PosX(espacioLibre), yFila, Paso - 8f, prefabCarro.sizeDelta.y + 16f,
                               new Color(0.3f, 1f, 0.4f, 0.22f));
        fila.Add(marca);

        // tu carro empieza en un lado, al azar
        float lim = LimiteX();
        bool izquierda = Random.value < 0.5f;
        direccion = izquierda ? 1f : -1f;
        autoJugador.anchoredPosition = new Vector2(izquierda ? -lim : lim, yCarril);
    }

    void CrearLineas()
    {
        for (int i = 0; i <= espacios; i++)
        {
            float x = -area.rect.width / 2f + Paso * i;
            var m = NuevaMarca(x, yFila, 4f, prefabCarro.sizeDelta.y + 30f, new Color(1f, 1f, 1f, 0.5f));
            decoracion.Add(m);
        }
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
        return rt;
    }

    void DestruirFila()
    {
        foreach (var f in fila) if (f != null) Destroy(f.gameObject);
        fila.Clear();
    }

    // Clic o toque dentro de la zona de juego
    public void OnPointerDown(PointerEventData e)
    {
        pulsado = true;
    }

    // ESPACIO o flecha arriba. Funciona con el Input nuevo o el clásico de Unity.
    bool TeclaPresionada()
    {
#if ENABLE_INPUT_SYSTEM
        var t = UnityEngine.InputSystem.Keyboard.current;
        return t != null && (t.spaceKey.wasPressedThisFrame || t.upArrowKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }

    void MostrarCuenta(string t)
    {
        if (textoCuenta == null) return;
        textoCuenta.gameObject.SetActive(true);
        textoCuenta.text = t;
    }

    void OcultarCuenta()
    {
        if (textoCuenta != null) textoCuenta.gameObject.SetActive(false);
    }

    void ActualizarTexto()
    {
        if (textoInfo == null) return;
        int vidas = Mathf.Max(0, fallosPermitidos + 1 - fallos);
        textoInfo.text = $"Tiempo: {Mathf.CeilToInt(Mathf.Max(0f, tiempo))}    Estacionados: {aciertos}/{aciertosNecesarios}    Vidas: {vidas}";
    }
}
