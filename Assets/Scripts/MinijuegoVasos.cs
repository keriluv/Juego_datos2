using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Minijuego: ¿dónde está la bola?
// 1) Se levanta un vaso y muestras la bola. Luego el vaso baja y la tapa.
// 2) Los 3 vasos se intercambian de lugar durante "duracionMezcla" segundos.
// 3) El jugador hace clic en el vaso donde cree que está la bola.
// Ganas si aciertas, pierdes si fallas.
public class MinijuegoVasos : MinijuegoBase, IPointerDownHandler
{
    [Header("Elementos de la escena")]
    [SerializeField] RectTransform[] vasos;     // exactamente 3
    [SerializeField] RectTransform bola;
    [SerializeField] TMP_Text textoInfo;        // texto de arriba

    [Header("Velocidad y tiempos (puedes cambiarlos aquí)")]
    [SerializeField] float duracionMezcla = 10f;          // segundos que se revuelven los vasos
    [SerializeField] float duracionIntercambio = 0.35f;   // segundos que tarda cada cambio. MÁS BAJO = MÁS RÁPIDO
    [SerializeField] float pausaEntreIntercambios = 0.05f;
    [SerializeField] float tiempoMostrar = 1.5f;          // segundos que ves la bola antes de taparla

    [Header("Medidas")]
    [SerializeField] float separacion = 200f;       // distancia entre vasos
    [SerializeField] float alturaArco = 45f;        // cuánto "salta" un vaso al cambiar
    [SerializeField] float alturaLevantado = 110f;  // cuánto sube un vaso al destaparlo
    [SerializeField] float yBase = -10f;            // altura de la mesa
    [SerializeField] float desnivelBola = -45f;     // altura de la bola respecto al vaso

    enum Estado { Inactivo, Mostrando, Mezclando, Eligiendo, Revelando }
    Estado estado = Estado.Inactivo;

    readonly int[] vasoEnSlot = new int[3];   // qué vaso hay en cada lugar (0, 1, 2)
    readonly float[] xVaso = new float[3];
    readonly float[] alto = new float[3];
    readonly float[] altoObjetivo = new float[3];
    readonly float[] arco = new float[3];

    int vasoConBola;
    float tiempoEstado;
    float mezclaTranscurrida;
    bool intercambiando;
    int slotA, slotB, vasoA, vasoB;
    float tSwap;
    float pausa;
    bool acerto;

    const float VelocidadVertical = 900f;

    public override void Iniciar()
    {
        if (vasos == null || vasos.Length != 3 || bola == null)
        {
            Debug.LogError("MinijuegoVasos: asigna exactamente 3 vasos y la bola en el Inspector.");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            vasoEnSlot[i] = i;
            xVaso[i] = PosSlot(i);
            alto[i] = 0f;
            altoObjetivo[i] = 0f;
            arco[i] = 0f;

            var g = vasos[i].GetComponent<Graphic>();
            if (g != null) g.raycastTarget = false;
        }
        var gb = bola.GetComponent<Graphic>();
        if (gb != null) gb.raycastTarget = false;

        vasoConBola = Random.Range(0, 3);
        alto[vasoConBola] = alturaLevantado;     // empieza levantado para ver la bola
        altoObjetivo[vasoConBola] = alturaLevantado;

        mezclaTranscurrida = 0f;
        intercambiando = false;
        pausa = 0f;
        tiempoEstado = tiempoMostrar;
        estado = Estado.Mostrando;

        AplicarPosiciones();
        Texto("¡Mira dónde está la bola!");
    }

    public override void Detener()
    {
        estado = Estado.Inactivo;
    }

    void Update()
    {
        if (estado == Estado.Inactivo) return;
        float dt = Time.deltaTime;

        for (int c = 0; c < 3; c++)
            alto[c] = Mathf.MoveTowards(alto[c], altoObjetivo[c], VelocidadVertical * dt);

        switch (estado)
        {
            case Estado.Mostrando:
                tiempoEstado -= dt;
                if (tiempoEstado <= 0f)
                {
                    altoObjetivo[vasoConBola] = 0f;            // baja el vaso y tapa la bola
                    if (alto[vasoConBola] <= 0.5f) estado = Estado.Mezclando;
                }
                break;

            case Estado.Mezclando:
                mezclaTranscurrida += dt;
                if (intercambiando)
                {
                    tSwap += dt / Mathf.Max(0.05f, duracionIntercambio);
                    float t = Mathf.Clamp01(tSwap);
                    float suave = Mathf.SmoothStep(0f, 1f, t);
                    xVaso[vasoA] = Mathf.Lerp(PosSlot(slotA), PosSlot(slotB), suave);
                    xVaso[vasoB] = Mathf.Lerp(PosSlot(slotB), PosSlot(slotA), suave);
                    float salto = Mathf.Sin(Mathf.PI * t) * alturaArco;
                    arco[vasoA] = salto;
                    arco[vasoB] = -salto;

                    if (t >= 1f)
                    {
                        vasoEnSlot[slotA] = vasoB;
                        vasoEnSlot[slotB] = vasoA;
                        xVaso[vasoA] = PosSlot(slotB);
                        xVaso[vasoB] = PosSlot(slotA);
                        arco[vasoA] = 0f;
                        arco[vasoB] = 0f;
                        intercambiando = false;
                        pausa = pausaEntreIntercambios;
                    }
                }
                else
                {
                    if (mezclaTranscurrida >= duracionMezcla)
                    {
                        estado = Estado.Eligiendo;
                        Texto("¡Elige un vaso!");
                        break;
                    }
                    pausa -= dt;
                    if (pausa <= 0f) EmpezarIntercambio();
                }

                if (estado == Estado.Mezclando)
                    Texto("Sigue la bola: " + Mathf.CeilToInt(Mathf.Max(0f, duracionMezcla - mezclaTranscurrida)));
                break;

            case Estado.Revelando:
                tiempoEstado -= dt;
                if (tiempoEstado <= 0f)
                {
                    AplicarPosiciones();
                    Terminar(acerto);
                    return;
                }
                break;
        }

        AplicarPosiciones();
    }

    void EmpezarIntercambio()
    {
        slotA = Random.Range(0, 3);
        slotB = (slotA + Random.Range(1, 3)) % 3;
        vasoA = vasoEnSlot[slotA];
        vasoB = vasoEnSlot[slotB];
        tSwap = 0f;
        intercambiando = true;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (estado != Estado.Eligiendo) return;

        for (int c = 0; c < 3; c++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(vasos[c], e.position, e.pressEventCamera))
            {
                Elegir(c);
                return;
            }
        }
    }

    void Elegir(int vaso)
    {
        acerto = vaso == vasoConBola;
        altoObjetivo[vasoConBola] = alturaLevantado;   // se destapa la bola
        altoObjetivo[vaso] = alturaLevantado;
        Texto(acerto ? "¡Correcto!" : "¡Ahí no estaba!");
        estado = Estado.Revelando;
        tiempoEstado = 1.1f;
    }

    float PosSlot(int slot)
    {
        return (slot - 1) * separacion;
    }

    void AplicarPosiciones()
    {
        for (int c = 0; c < 3; c++)
            vasos[c].anchoredPosition = new Vector2(xVaso[c], yBase + alto[c] + arco[c]);

        // la bola acompaña a su vaso (siempre queda tapada mientras se mezclan)
        bola.anchoredPosition = new Vector2(xVaso[vasoConBola], yBase + desnivelBola + arco[vasoConBola]);
    }

    void Texto(string t)
    {
        if (textoInfo != null) textoInfo.text = t;
    }
}
