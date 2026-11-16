using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: meter la llave en la cerradura.
// La llave sube y baja a la izquierda. Pulsa ESPACIO, la flecha ↑ o haz clic cuando esté
// a la altura de la ranura de la puerta. Si acertas, la llave entra, gira y la puerta se abre.
// Si fallas, la llave rebota contra la puerta y pierdes un intento (y la llave va más rápido).
public class MinijuegoLlave : MinijuegoBase, IPointerDownHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform llave;        // la llave (mira hacia la derecha)
    [SerializeField] RectTransform puerta;       // la puerta con la cerradura (Pivot X = 1)
    [SerializeField] RectTransform resplandor;   // luz detrás de la puerta (opcional)
    [SerializeField] TMP_Text textoCuenta;       // número grande al centro (3, 2, 1...)
    [SerializeField] TMP_Text textoInfo;         // texto de arriba

    [Header("Reglas")]
    [SerializeField] int fallosPermitidos = 2;      // 2 = puedes fallar dos veces, el tercer fallo pierde
    [SerializeField] float tiempoLimite = 15f;
    [SerializeField] float cuentaRegresiva = 3f;    // segundos de cuenta antes de empezar (0 = sin cuenta)
    [SerializeField] string textoFinalCuenta = "0"; // lo último que se muestra (puedes poner ¡YA!)

    [Header("Velocidad y dificultad")]
    [SerializeField] float velocidadLlave = 240f;   // qué tan rápido sube y baja la llave
    [SerializeField] float aumentoPorFallo = 0.25f; // la llave va 25% más rápido tras cada fallo
    [SerializeField] float tolerancia = 16f;        // píxeles de margen respecto a la ranura (más = más fácil)
    [SerializeField] float velocidadInsercion = 900f;

    [Header("Posiciones")]
    [SerializeField] float margenIzquierdo = 30f;   // distancia de la llave al borde izquierdo
    [SerializeField] float recorridoVertical = 160f;// la llave sube y baja entre -este valor y +este valor
    [SerializeField] float profundidad = 75f;       // cuánto entra la punta en la puerta
    [SerializeField] float alturaRanura = 0f;       // ajuste fino de la altura de la ranura

    [Header("Animaciones")]
    [SerializeField] float duracionGiro = 0.6f;     // segundos que tarda en girar la llave
    [SerializeField] float duracionAbrir = 0.8f;    // segundos que tarda en abrirse la puerta

    enum Fase { Inactivo, Cuenta, Moviendo, Insertando, Rebote, Volviendo, Girando, Abriendo, Final }
    Fase fase = Fase.Inactivo;

    RectTransform area;
    Graphic gLlave, gPuerta, gResplandor;

    int fallos;
    float tiempo, cuenta, faseT, velMult, yLlave, dirY, xInicio;
    bool pulsado, acierto;
    Vector2 destino;
    string mensaje = "";

    void Awake()
    {
        area = (RectTransform)transform;
    }

    float IzqPuerta()
    {
        return puerta.anchoredPosition.x - puerta.pivot.x * puerta.sizeDelta.x;
    }

    float YRanura()
    {
        return puerta.anchoredPosition.y + (0.5f - puerta.pivot.y) * puerta.sizeDelta.y + alturaRanura;
    }

    public override void Iniciar()
    {
        if (llave == null || puerta == null)
        {
            Debug.LogError("MinijuegoLlave: asigna la Llave y la Puerta en el Inspector.");
            return;
        }
        Detener();

        gLlave = llave.GetComponent<Graphic>();
        gPuerta = puerta.GetComponent<Graphic>();
        gResplandor = resplandor != null ? resplandor.GetComponent<Graphic>() : null;

        if (gLlave != null) { gLlave.raycastTarget = false; gLlave.color = Color.white; }
        if (gPuerta != null) gPuerta.raycastTarget = false;
        if (gResplandor != null) { gResplandor.raycastTarget = false; gResplandor.color = new Color(1f, 1f, 1f, 0f); }
        if (resplandor != null) resplandor.localScale = Vector3.one * 0.8f;

        puerta.localScale = Vector3.one;
        llave.localScale = Vector3.one;

        fallos = 0;
        velMult = 1f;
        tiempo = tiempoLimite;
        mensaje = "";
        acierto = false;

        xInicio = -area.rect.width / 2f + margenIzquierdo + llave.sizeDelta.x / 2f;
        yLlave = Random.Range(-recorridoVertical, recorridoVertical);
        dirY = Random.value < 0.5f ? -1f : 1f;
        llave.anchoredPosition = new Vector2(xInicio, yLlave);

        cuenta = cuentaRegresiva;
        fase = cuentaRegresiva > 0f ? Fase.Cuenta : Fase.Moviendo;
        if (fase == Fase.Cuenta) MostrarCuenta(Mathf.CeilToInt(cuenta).ToString());
        ActualizarTexto();
    }

    public override void Detener()
    {
        fase = Fase.Inactivo;
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

        // el tiempo corre mientras juegas, pero se detiene cuando ya acertaste
        bool corre = fase == Fase.Moviendo || fase == Fase.Volviendo || fase == Fase.Rebote
                     || (fase == Fase.Insertando && !acierto);
        if (corre)
        {
            tiempo -= dt;
            if (tiempo <= 0f)
            {
                tiempo = 0f;
                ActualizarTexto();
                Terminar(false);
                return;
            }
        }

        switch (fase)
        {
            case Fase.Moviendo: Mover(dt); break;
            case Fase.Insertando: Insertar(dt); break;
            case Fase.Rebote: Rebotar(dt); break;
            case Fase.Volviendo: Volver(dt); break;
            case Fase.Girando: Girar(dt); break;
            case Fase.Abriendo: AbrirPuerta(dt); break;
            case Fase.Final: EsperarFinal(dt); break;
        }

        pulsado = false;
        ActualizarTexto();
    }

    void Mover(float dt)
    {
        yLlave += dirY * velocidadLlave * velMult * dt;
        if (yLlave > recorridoVertical) { yLlave = recorridoVertical; dirY = -1f; }
        else if (yLlave < -recorridoVertical) { yLlave = -recorridoVertical; dirY = 1f; }
        llave.anchoredPosition = new Vector2(xInicio, yLlave);

        if (pulsado || TeclaPresionada()) Intentar();
    }

    void Intentar()
    {
        acierto = Mathf.Abs(yLlave - YRanura()) <= tolerancia;
        float izq = IzqPuerta();
        float mitad = llave.sizeDelta.x / 2f;

        if (acierto)
            destino = new Vector2(izq + profundidad - mitad, YRanura());   // entra en la ranura
        else
            destino = new Vector2(izq + 4f - mitad, yLlave);               // choca con la puerta

        fase = Fase.Insertando;
    }

    void Insertar(float dt)
    {
        llave.anchoredPosition = Vector2.MoveTowards(llave.anchoredPosition, destino, velocidadInsercion * dt);
        if (Vector2.Distance(llave.anchoredPosition, destino) < 0.5f)
        {
            faseT = 0f;
            if (acierto)
            {
                mensaje = "¡Entró!";
                fase = Fase.Girando;
            }
            else
            {
                fase = Fase.Rebote;
            }
        }
    }

    // Fallo: la llave tiembla contra la puerta y vuelve atrás
    void Rebotar(float dt)
    {
        faseT += dt;
        float k = Mathf.Clamp01(1f - faseT / 0.4f);
        llave.anchoredPosition = destino + new Vector2(Mathf.Sin(Time.time * 70f) * 7f * k, 0f);

        if (faseT >= 0.4f)
        {
            fallos++;
            if (fallos > fallosPermitidos) { Terminar(false); return; }
            velMult += aumentoPorFallo;
            llave.anchoredPosition = destino;
            fase = Fase.Volviendo;
        }
    }

    void Volver(float dt)
    {
        Vector2 meta = new Vector2(xInicio, llave.anchoredPosition.y);
        llave.anchoredPosition = Vector2.MoveTowards(llave.anchoredPosition, meta, velocidadInsercion * 0.7f * dt);
        if (Mathf.Abs(llave.anchoredPosition.x - xInicio) < 0.5f)
        {
            yLlave = llave.anchoredPosition.y;
            dirY = Random.value < 0.5f ? -1f : 1f;
            fase = Fase.Moviendo;
        }
    }

    // Acierto 1: la llave gira (se ve cada vez más "de canto", como cuando gira sobre su eje)
    void Girar(float dt)
    {
        faseT += dt / Mathf.Max(0.05f, duracionGiro);
        float t = Mathf.Clamp01(faseT);
        float escalaY = Mathf.Max(0.12f, Mathf.Cos(t * Mathf.PI * 0.5f));
        llave.localScale = new Vector3(1f, escalaY, 1f);

        if (t >= 1f)
        {
            faseT = 0f;
            mensaje = "¡Click!";
            fase = Fase.Abriendo;
        }
    }

    // Acierto 2: la puerta se abre (gira sobre su bisagra) y aparece la luz detrás
    void AbrirPuerta(float dt)
    {
        faseT += dt / Mathf.Max(0.05f, duracionAbrir);
        float t = Mathf.Clamp01(faseT);
        float k = Mathf.SmoothStep(0f, 1f, t);

        puerta.localScale = new Vector3(Mathf.Max(0.001f, 1f - k), 1f, 1f);

        // la llave se queda pegada al borde de la puerta y se desvanece
        llave.anchoredPosition = new Vector2(destino.x + k * puerta.sizeDelta.x, destino.y);
        if (gLlave != null) gLlave.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(t * 2.5f));

        if (resplandor != null)
        {
            resplandor.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.15f, k);
            if (gResplandor != null) gResplandor.color = new Color(1f, 1f, 1f, Mathf.Clamp01(t * 1.5f));
        }

        if (t >= 1f)
        {
            faseT = 0f;
            mensaje = "¡Abierto!";
            fase = Fase.Final;
        }
    }

    void EsperarFinal(float dt)
    {
        faseT += dt;
        if (resplandor != null)
            resplandor.localScale = Vector3.one * (1.15f + 0.04f * Mathf.Sin(Time.time * 8f));
        if (faseT >= 0.8f) Terminar(true);
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
        if (!string.IsNullOrEmpty(mensaje))
        {
            textoInfo.text = mensaje;
            return;
        }
        int intentos = Mathf.Max(0, fallosPermitidos + 1 - fallos);
        textoInfo.text = $"Tiempo: {Mathf.CeilToInt(Mathf.Max(0f, tiempo))}    Intentos: {intentos}";
    }
}
