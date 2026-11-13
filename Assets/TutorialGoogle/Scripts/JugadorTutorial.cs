using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(RectTransform))]
public class JugadorTutorial : MonoBehaviour
{
    [Header("Referencias")]
    public RectTransform mundo;
    public Image sprite;

    [Header("Sprites del dino")]
    public Sprite[] spritesCarrera;
    public Sprite spriteSalto;
    public Sprite spriteAgachado;

    [Header("Movimiento")]
    public float velocidad = 520f;
    public float fuerzaSalto = 1100f;
    public float gravedad = 2000f;
    public float alturaSuelo = 48f;
    public Vector2 posicionInicial = new Vector2(960f, 48f);

    [Header("Controles")]
    public bool usarClick = true;
    public bool usarGatillo = true;
    public bool ignorarFocoDeTexto;
    public string nombreEjeGatillo = "Fire3";

    [Header("Golpe")]
    public float alcanceGolpe = 130f;
    public float duracionGolpe = 0.22f;
    public float esperaEntreGolpes = 0.35f;
    public bool golpearPlataformaActual = false;

    [Header("Limites")]
    public bool usarLimitesPersonalizados = false;
    public float limiteIzquierdo = 0f;
    public float limiteDerecho = 0f;

    [Header("Red")]
    public bool usarEntradaRemota;
    [HideInInspector] public float entradaHorizontal;
    [HideInInspector] public bool entradaSalto;
    [HideInInspector] public bool entradaGolpe;
    [HideInInspector] public bool entradaAgachado;

    public System.Action<PlataformaUI> alAterrizar;
    public System.Action<PlataformaUI> alGolpear;

    public bool EnSuelo => enSuelo;
    public PlataformaUI PlataformaActual => plataformaActual;

    private RectTransform rect;
    private Vector2 posicion;
    private float velocidadY;
    private bool enSuelo;
    private float mirada = 1f;
    private float impacto;
    private PlataformaUI plataformaActual;
    private float temporizadorAnim;
    private int indiceFrameCarrera;
    private bool agachadoInput;
    private float tiempoGolpe;
    private float duracionGolpeActual = 0.22f;
    private float siguienteGolpe;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
    }

    private void Start()
    {
        posicion = posicionInicial;
        AplicarPosicion();
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        if (dt <= 0f) return;

        bool salto;
        bool golpe;

        if (usarEntradaRemota)
        {
            salto = entradaSalto;
            entradaSalto = false;

            golpe = entradaGolpe;
            entradaGolpe = false;

            agachadoInput = entradaAgachado;
        }
        else
        {
            salto = LeerSalto();
            LeerAgachado();
            golpe = LeerGolpe();
        }

        if (golpe)
            Golpear();

        if (enSuelo && salto)
        {
            velocidadY = fuerzaSalto;
            enSuelo = false;
            plataformaActual = null;
        }

        velocidadY -= gravedad * dt;

        float ancho = rect.rect.width;
        float limite = mundo != null ? mundo.rect.width : 1920f;

        float horizontal = usarEntradaRemota ? Mathf.Clamp(entradaHorizontal, -1f, 1f) : LeerHorizontal();
        posicion.x += horizontal * velocidad * dt;

        if (usarLimitesPersonalizados)
            posicion.x = Mathf.Clamp(posicion.x, limiteIzquierdo, Mathf.Max(limiteIzquierdo, limiteDerecho));
        else
            posicion.x = Mathf.Clamp(posicion.x, 0f, Mathf.Max(0f, limite - ancho));

        float anterior = posicion.y;
        posicion.y += velocidadY * dt;

        PlataformaUI aterrizada = null;
        float mejorArriba = float.NegativeInfinity;

        if (velocidadY <= 0f)
        {
            for (int i = 0; i < PlataformaUI.Todas.Count; i++)
            {
                PlataformaUI p = PlataformaUI.Todas[i];
                if (p == null || !p.isActiveAndEnabled) continue;

                Rect r = p.RectLocal(mundo);

                if (posicion.x + ancho <= r.xMin + 2f || posicion.x >= r.xMax - 2f) continue;

                float arriba = r.yMax;
                if (anterior >= arriba - 2f && posicion.y <= arriba && arriba > mejorArriba)
                {
                    mejorArriba = arriba;
                    aterrizada = p;
                }
            }
        }

        if (aterrizada != null)
        {
            posicion.y = mejorArriba;
            velocidadY = 0f;
            enSuelo = true;

            if (plataformaActual != aterrizada)
            {
                plataformaActual = aterrizada;
                impacto = 1.5f;
                alAterrizar?.Invoke(aterrizada);
            }
        }
        else if (posicion.y <= alturaSuelo && velocidadY <= 0f)
        {
            posicion.y = alturaSuelo;
            velocidadY = 0f;
            enSuelo = true;
            if (plataformaActual != null)
            {
                plataformaActual = null;
                impacto = 1.5f;
            }
        }
        else
        {
            enSuelo = false;
        }

        if (posicion.y < -260f)
        {
            posicion = posicionInicial;
            velocidadY = 0f;
            enSuelo = false;
            plataformaActual = null;
        }

        if (Mathf.Abs(horizontal) > 0.05f)
            mirada = horizontal > 0f ? 1f : -1f;

        AplicarPosicion();
        Animar(dt, horizontal);
    }

    public void IrA(RectTransform destino)
    {
        if (destino == null) return;

        Rect r = PlataformaUI.RectDe(mundo, destino);
        float ancho = rect.rect.width;
        float limite = mundo != null ? mundo.rect.width : 1920f;

        posicion = new Vector2(
            Mathf.Clamp(r.center.x - ancho * 0.5f, 0f, Mathf.Max(0f, limite - ancho)),
            r.yMax + 2f);

        velocidadY = 0f;
        enSuelo = false;
        plataformaActual = null;
        impacto = 1.5f;
        AplicarPosicion();
    }

    public void ColocarEn(Vector2 destino)
    {
        posicion = destino;
        velocidadY = 0f;
        enSuelo = false;
        plataformaActual = null;
        impacto = 1.5f;
        AplicarPosicion();
    }

    public void Saltar()
    {
        if (enSuelo && !ControlesBloqueadosPorTexto())
        {
            velocidadY = fuerzaSalto;
            enSuelo = false;
            plataformaActual = null;
        }
    }

    public void Golpear()
    {
        if (Time.unscaledTime < siguienteGolpe) return;

        siguienteGolpe = Time.unscaledTime + Mathf.Max(0.05f, esperaEntreGolpes);

        duracionGolpeActual = Mathf.Max(0.01f, duracionGolpe);
        tiempoGolpe = duracionGolpeActual;

        alGolpear?.Invoke(BuscarObjetivoGolpe());
    }

    private PlataformaUI BuscarObjetivoGolpe()
    {
        if (mundo == null) return null;

        float ancho = rect.rect.width;
        float alto = rect.rect.height;
        float x = mirada >= 0f ? posicion.x + ancho - 6f : posicion.x - alcanceGolpe + 6f;
        float anchoZona = Mathf.Max(1f, alcanceGolpe);

        if (golpearPlataformaActual)
        {
            x -= ancho * 0.75f;
            anchoZona += ancho * 0.75f;
        }

        float y = posicion.y - alto * 0.35f;

        Rect zona = new Rect(
            x,
            y,
            anchoZona,
            Mathf.Max(1f, alto * 1.35f));

        PlataformaUI mejor = null;
        float mejorDistancia = float.PositiveInfinity;

        for (int i = 0; i < PlataformaUI.Todas.Count; i++)
        {
            PlataformaUI p = PlataformaUI.Todas[i];
            if (p == null || !p.isActiveAndEnabled) continue;
            if (!golpearPlataformaActual && p == plataformaActual) continue;

            Rect r = p.RectLocal(mundo);
            if (!zona.Overlaps(r)) continue;

            float distancia = Mathf.Abs(r.center.x - zona.center.x) +
                              Mathf.Abs(r.center.y - zona.center.y) * 0.25f;

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = p;
            }
        }

        return mejor;
    }

    public void SetAgachar(bool valor)
    {
        agachadoInput = valor;
    }

    private void AplicarPosicion()
    {
        rect.anchoredPosition = posicion;
    }

    private void Animar(float dt, float horizontal)
    {
        if (sprite == null) return;

        impacto = Mathf.MoveTowards(impacto, 0f, dt * 5f);
        float k = impacto * impacto;

        float golpeK = 0f;
        if (tiempoGolpe > 0f)
        {
            tiempoGolpe = Mathf.Max(0f, tiempoGolpe - dt);
            float progreso = 1f - Mathf.Clamp01(tiempoGolpe / duracionGolpeActual);
            golpeK = Mathf.Sin(progreso * Mathf.PI);
        }

        float escalaX = 1f + 0.25f * k + 0.14f * golpeK;
        float escalaY = 1f - 0.30f * k - 0.10f * golpeK;
        float inclinacion = -8f * Mathf.Abs(horizontal) * mirada + mirada * 14f * golpeK;

        Sprite spriteActual = null;

        if (agachadoInput && enSuelo && spriteAgachado != null)
        {
            spriteActual = spriteAgachado;
            escalaX *= 0.9f;
            escalaY *= 0.85f;
        }
        else if (!enSuelo)
        {
            spriteActual = spriteSalto != null ? spriteSalto : (spritesCarrera != null && spritesCarrera.Length > 0 ? spritesCarrera[0] : null);
        }
        else
        {
            if (spritesCarrera != null && spritesCarrera.Length > 0)
            {
                temporizadorAnim += dt * 16f;
                if (temporizadorAnim >= 1f)
                {
                    temporizadorAnim = 0f;
                    indiceFrameCarrera = (indiceFrameCarrera + 1) % spritesCarrera.Length;
                }
                spriteActual = spritesCarrera[indiceFrameCarrera];
            }
        }

        if (spriteActual != null)
            sprite.sprite = spriteActual;

        sprite.rectTransform.localScale = new Vector3(mirada * escalaX, escalaY, 1f);
        sprite.rectTransform.localRotation = Quaternion.Euler(0f, 0f, inclinacion);
        sprite.rectTransform.anchoredPosition = new Vector2(mirada * 18f * golpeK, 0f);
    }

    private float LeerHorizontal()
    {
        float h = 0f;

        if (ControlesBloqueadosPorTexto()) return 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) h -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) h += 1f;
        }

        Gamepad mando = Gamepad.current;
        if (mando != null)
        {
            float eje = mando.leftStick.ReadValue().x;
            if (Mathf.Abs(eje) > 0.18f) h = eje;
            else if (mando.dpad.left.isPressed) h = -1f;
            else if (mando.dpad.right.isPressed) h = 1f;
        }
#endif

        return Mathf.Clamp(h, -1f, 1f);
    }

    private bool LeerSalto()
    {
        bool salto = false;

        if (ControlesBloqueadosPorTexto()) return false;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            salto = true;
        }

#if ENABLE_INPUT_SYSTEM
        if (!salto)
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null &&
                (teclado.spaceKey.wasPressedThisFrame ||
                 teclado.wKey.wasPressedThisFrame ||
                 teclado.upArrowKey.wasPressedThisFrame))
            {
                salto = true;
            }

            Gamepad mando = Gamepad.current;
            if (mando != null && (mando.buttonSouth.wasPressedThisFrame || mando.buttonNorth.wasPressedThisFrame))
                salto = true;
        }
#endif

        return salto;
    }

    private bool LeerGolpe()
    {
        if (Time.unscaledTime < siguienteGolpe) return false;

        bool escribiendo = ControlesBloqueadosPorTexto();

        if (usarClick)
        {
            bool click = Input.GetMouseButtonDown(0);

#if ENABLE_INPUT_SYSTEM
            Mouse raton = Mouse.current;
            if (raton != null && raton.leftButton.wasPressedThisFrame)
                click = true;
#endif

            if (click)
                return true;
        }

        if (usarGatillo && !escribiendo)
        {
            float gatillo = Input.GetAxis(nombreEjeGatillo);
            if (gatillo > 0.5f)
                return true;

#if ENABLE_INPUT_SYSTEM
            Gamepad mando = Gamepad.current;
            if (mando != null &&
                (mando.rightTrigger.wasPressedThisFrame || mando.leftTrigger.wasPressedThisFrame))
                return true;
#endif
        }

        return false;
    }

    private void LeerAgachado()
    {
        agachadoInput = false;
        if (ControlesBloqueadosPorTexto()) return;

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            agachadoInput = true;

#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado != null && (teclado.downArrowKey.isPressed || teclado.sKey.isPressed))
            agachadoInput = true;
#endif
    }

    private bool ControlesBloqueadosPorTexto()
    {
        return !ignorarFocoDeTexto && BuscadorGoogle.Escribiendo();
    }
}
