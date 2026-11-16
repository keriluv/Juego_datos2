using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Final del nivel.
// 1) Aparece el mensaje "Recoge El tiktok Dorado" (animado por AvisoDorado).
// 2) Cae lentamente del cielo el logo dorado.
// 3) El jugador lo recoge tocándolo con la cápsula (o haciéndole clic) y el nivel termina.
// Se activa solo desde el GestorMinijuegos cuando el jugador completa los minijuegos.
public class SecuenciaTiktokDorado : MonoBehaviour
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform tiktokDorado;   // la imagen del logo dorado (hija del Canvas)
    [SerializeField] TMP_Text textoAviso;          // el mensaje
    [SerializeField] Transform jugador;            // tu JugadorCapsula
    [SerializeField] RectTransform zonaCaida;      // opcional: por dónde cae (por ejemplo Panel_video)
    [SerializeField] GameObject panelFinal;        // opcional: pantalla de "Nivel completado"

    [Header("Mensaje")]
    [SerializeField] string mensaje = "Recoge El tiktok Dorado";
    [SerializeField] float duracionAviso = 2.5f;   // segundos que se ve el mensaje

    [Header("Caída y recogida")]
    [SerializeField] float velocidadCaida = 130f;  // más bajo = cae más lento
    [SerializeField] float radioRecoger = 90f;     // qué tan cerca debe estar la cápsula (en píxeles de pantalla)
    [SerializeField] float margenLateral = 80f;    // distancia mínima a los bordes de la zona de caída

    public UnityEvent AlRecoger;                   // para conectar lo que pasa después (siguiente nivel, etc.)

    enum Fase { Inactivo, Aviso, Cayendo, Recogido, Fin }
    Fase fase = Fase.Inactivo;

    RectTransform lienzo;
    Graphic gDorado;
    float t;

    public bool EnCurso { get { return fase != Fase.Inactivo; } }

    void Awake()
    {
        if (tiktokDorado != null)
        {
            gDorado = tiktokDorado.GetComponent<Graphic>();

            // se puede recoger también con clic o toque
            var boton = tiktokDorado.GetComponent<Button>();
            if (boton == null) boton = tiktokDorado.gameObject.AddComponent<Button>();
            boton.transition = Selectable.Transition.None;
            boton.targetGraphic = gDorado;
            boton.onClick.AddListener(Recoger);

            tiktokDorado.gameObject.SetActive(false);
        }
        if (textoAviso != null) textoAviso.gameObject.SetActive(false);
        if (panelFinal != null) panelFinal.SetActive(false);
    }

    // Lo llama el GestorMinijuegos
    public void Iniciar()
    {
        if (fase != Fase.Inactivo) return;
        if (tiktokDorado == null)
        {
            Debug.LogError("SecuenciaTiktokDorado: asigna el TiktokDorado en el Inspector.");
            return;
        }

        lienzo = tiktokDorado.parent as RectTransform;
        t = 0f;
        fase = Fase.Aviso;

        if (textoAviso != null)
        {
            textoAviso.text = mensaje;
            // Al activarse, el script AvisoDorado hace la animacion de entrada solo
            textoAviso.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        switch (fase)
        {
            case Fase.Aviso: Avisar(dt); break;
            case Fase.Cayendo: Caer(dt); break;
            case Fase.Recogido: Celebrar(dt); break;
        }
    }

    void Avisar(float dt)
    {
        t += dt;

        if (t >= duracionAviso)
        {
            OcultarAviso();
            tiktokDorado.gameObject.SetActive(true);
            Reaparecer();
            fase = Fase.Cayendo;
        }
    }

    // Oculta el mensaje con animacion (si tiene AvisoDorado) o de golpe (si no)
    void OcultarAviso()
    {
        if (textoAviso == null) return;

        var aviso = textoAviso.GetComponent<AvisoDorado>();
        if (aviso != null && textoAviso.gameObject.activeInHierarchy) aviso.Ocultar();
        else textoAviso.gameObject.SetActive(false);
    }

    // Coloca el logo arriba del todo, fuera de la pantalla, en un lugar al azar
    void Reaparecer()
    {
        float minX, maxX;
        RangoX(out minX, out maxX);

        float x = Random.Range(minX, maxX);
        float y = lienzo.rect.height / 2f + tiktokDorado.sizeDelta.y;
        tiktokDorado.anchoredPosition = new Vector2(x, y);
        tiktokDorado.localScale = Vector3.one;
        tiktokDorado.localRotation = Quaternion.identity;
        if (gDorado != null) gDorado.color = Color.white;
        t = 0f;
    }

    void RangoX(out float minX, out float maxX)
    {
        float mitad = lienzo.rect.width / 2f - margenLateral;
        minX = -mitad;
        maxX = mitad;

        if (zonaCaida != null)
        {
            var esquinas = new Vector3[4];
            zonaCaida.GetWorldCorners(esquinas);
            float a = lienzo.InverseTransformPoint(esquinas[0]).x;
            float b = lienzo.InverseTransformPoint(esquinas[2]).x;
            minX = Mathf.Max(minX, Mathf.Min(a, b) + margenLateral);
            maxX = Mathf.Min(maxX, Mathf.Max(a, b) - margenLateral);
        }
        if (minX > maxX) { minX = 0f; maxX = 0f; }
    }

    void Caer(float dt)
    {
        t += dt;

        Vector2 p = tiktokDorado.anchoredPosition;
        p.y -= velocidadCaida * dt;
        p.x += Mathf.Sin(t * 1.6f) * 18f * dt;          // pequeño vaivén
        tiktokDorado.anchoredPosition = p;
        tiktokDorado.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2f) * 8f);
        tiktokDorado.localScale = Vector3.one * (1f + 0.05f * Mathf.Sin(t * 5f));

        if (EstaCerca()) { Recoger(); return; }

        // si se cae sin que lo recojan, vuelve a caer desde arriba
        if (p.y < -lienzo.rect.height / 2f - tiktokDorado.sizeDelta.y) Reaparecer();
    }

    bool EstaCerca()
    {
        if (jugador == null) return false;
        return Vector2.Distance(PosPantalla(tiktokDorado), PosPantalla(jugador)) <= radioRecoger;
    }

    // Posición en pantalla de un objeto, sea de la interfaz o del mundo
    Vector2 PosPantalla(Transform tr)
    {
        var rt = tr as RectTransform;
        if (rt != null)
        {
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas = canvas.rootCanvas;
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return rt.position;
                Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                if (cam != null) return cam.WorldToScreenPoint(rt.position);
            }
        }
        Camera principal = Camera.main;
        if (principal != null) return principal.WorldToScreenPoint(tr.position);
        return tr.position;
    }

    // Público: también lo usa el clic sobre el logo
    public void Recoger()
    {
        if (fase != Fase.Cayendo) return;
        fase = Fase.Recogido;
        t = 0f;
    }

    // El logo gira, crece y se desvanece, y aparece el final
    void Celebrar(float dt)
    {
        t += dt;
        float k = Mathf.Clamp01(t / 0.6f);
        tiktokDorado.localScale = Vector3.one * Mathf.Lerp(1f, 2.2f, k);
        tiktokDorado.localRotation = Quaternion.Euler(0f, 0f, k * 360f);
        if (gDorado != null) gDorado.color = new Color(1f, 1f, 1f, 1f - k);

        if (k >= 1f)
        {
            tiktokDorado.gameObject.SetActive(false);
            fase = Fase.Fin;
            if (panelFinal != null) panelFinal.SetActive(true);
            if (AlRecoger != null) AlRecoger.Invoke();
        }
    }
}