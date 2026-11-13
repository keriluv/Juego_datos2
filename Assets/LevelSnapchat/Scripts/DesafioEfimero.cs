using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioEfimero : DesafioSnap
{
    private const int TotalRondas = 3;
    private const int Columnas = 3;
    private const float LadoCelda = 74f;
    private const float Separacion = 12f;

    private readonly Image[] celdas = new Image[9];
    private readonly bool[] patron = new bool[9];
    private readonly bool[] seleccion = new bool[9];

    private TextMeshProUGUI textoEstado;
    private TextMeshProUGUI textoFeedback;
    private TextMeshProUGUI textoRonda;
    private Button botonEnviar;

    private int ronda;
    private int vidas;
    private float tiempoVista;
    private float tiempoEntrada;
    private bool mostrando;
    private bool entrada;
    private bool castigo;

    private static readonly Color CeldaApagada = new Color(0.1f, 0.13f, 0.2f, 1f);
    private static readonly Color CeldaPatron = new Color(1f, 0.84f, 0.2f, 1f);
    private static readonly Color CeldaElegida = new Color(0.25f, 0.62f, 0.95f, 1f);
    private static readonly Color Verde = new Color(0.2f, 0.85f, 0.45f, 1f);
    private static readonly Color Rojo = new Color(0.95f, 0.25f, 0.28f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        Image fondo = gameObject.AddComponent<Image>();
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.06f, 0.05f, 0.11f, 0.97f);
        fondo.raycastTarget = false;

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "SNAP EFIMERO", 30f, Color.white);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(400f, 38f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Memorizalo antes de que se borre y repitelo", 13f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(410f, 22f));

        textoEstado = UtilidadesDesafio.CrearTexto("Estado", raiz, Fuente,
            "PREPARATE", 20f, Color.white);
        UtilidadesDesafio.Colocar(textoEstado.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(410f, 26f));

        CrearRejilla();

        textoFeedback = UtilidadesDesafio.CrearTexto("Feedback", raiz, Fuente, "", 18f, Color.white);
        UtilidadesDesafio.Colocar(textoFeedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -135f), new Vector2(410f, 26f));

        botonEnviar = UtilidadesDesafio.CrearBoton("Enviar", raiz, SpriteRedondeado, Fuente,
            "ENVIAR", new Color(0.25f, 0.62f, 0.95f, 1f), new Vector2(0f, -182f), new Vector2(200f, 52f), 20f);
        botonEnviar.onClick.AddListener(Comprobar);

        textoRonda = UtilidadesDesafio.CrearTexto("Ronda", raiz, Fuente,
            "SNAP 1/" + TotalRondas, 13f, Gris);
        UtilidadesDesafio.Colocar(textoRonda.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(400f, 20f));

        vidas = 2;
        ronda = 0;
        IniciarRonda();
    }

    private void CrearRejilla()
    {
        RectTransform rejilla = UtilidadesDesafio.CrearRect("Rejilla", raiz);
        UtilidadesDesafio.Colocar(rejilla, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(246f, 246f));

        for (int i = 0; i < celdas.Length; i++)
        {
            int columna = i % Columnas;
            int fila = i / Columnas;

            Image celda = UtilidadesDesafio.CrearPanel("Celda_" + i, rejilla, SpriteRedondeado, CeldaApagada);
            celda.raycastTarget = true;

            RectTransform rt = celda.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(LadoCelda, LadoCelda);
            rt.anchoredPosition = new Vector2(
                -86f + columna * (LadoCelda + Separacion),
                86f - fila * (LadoCelda + Separacion));

            Button boton = celda.gameObject.AddComponent<Button>();
            boton.targetGraphic = celda;
            boton.transition = Selectable.Transition.None;

            int indice = i;
            boton.onClick.AddListener(() => Alternar(indice));

            celdas[i] = celda;
        }
    }

    private void IniciarRonda()
    {
        mostrando = true;
        entrada = false;
        castigo = false;

        botonEnviar.interactable = false;
        textoFeedback.text = "";

        int cantidad = 3 + ronda;
        GenerarPatron(cantidad);

        tiempoVista = Mathf.Lerp(2.4f, 1.4f, ronda / (float)TotalRondas);
        tiempoEntrada = Mathf.Lerp(10f, 7f, ronda / (float)TotalRondas);

        textoRonda.text = "SNAP " + (ronda + 1) + "/" + TotalRondas;
        textoEstado.text = "MIRA...";
        textoEstado.color = new Color(1f, 0.84f, 0.2f, 1f);

        PintarPatron(true);
    }

    private void GenerarPatron(int cantidad)
    {
        for (int i = 0; i < patron.Length; i++)
        {
            patron[i] = false;
            seleccion[i] = false;
        }

        int puestos = 0;

        while (puestos < cantidad)
        {
            int indice = Random.Range(0, patron.Length);
            if (patron[indice]) continue;
            patron[indice] = true;
            puestos++;
        }
    }

    private void PintarPatron(bool visible)
    {
        for (int i = 0; i < celdas.Length; i++)
        {
            if (visible && patron[i]) celdas[i].color = CeldaPatron;
            else celdas[i].color = CeldaApagada;
        }
    }

    private void Update()
    {
        if (castigo || terminado) return;

        if (mostrando)
        {
            tiempoVista -= Time.unscaledDeltaTime;

            if (tiempoVista <= 0f)
            {
                mostrando = false;
                entrada = true;
                PintarPatron(false);
                botonEnviar.interactable = true;
                textoEstado.text = "SE BORRO! REPITELO";
                textoEstado.color = new Color(0.25f, 0.62f, 0.95f, 1f);
                UtilidadesDesafio.Sonido("sfx_desaparece", 0.7f);
            }

            return;
        }

        if (!entrada) return;

        tiempoEntrada -= Time.unscaledDeltaTime;

        textoEstado.text = "REPITELO " + Mathf.Max(0, Mathf.CeilToInt(tiempoEntrada)) + "s";
        textoEstado.color = tiempoEntrada <= 3f ? Rojo : new Color(0.25f, 0.62f, 0.95f, 1f);

        if (tiempoEntrada <= 0f)
            StartCoroutine(Fallar("SE TE ACABO EL TIEMPO"));
    }

    private void Alternar(int indice)
    {
        if (!entrada || castigo) return;

        seleccion[indice] = !seleccion[indice];
        celdas[indice].color = seleccion[indice] ? CeldaElegida : CeldaApagada;
        UtilidadesDesafio.Sonido("sfx_lente", 0.35f);
    }

    private void Comprobar()
    {
        if (!entrada || castigo) return;

        entrada = false;
        botonEnviar.interactable = false;

        for (int i = 0; i < patron.Length; i++)
        {
            if (patron[i] != seleccion[i])
            {
                StartCoroutine(Fallar("NO ERA ESE"));
                return;
            }
        }

        StartCoroutine(Acertar());
    }

    private IEnumerator Acertar()
    {
        textoFeedback.text = "SNAP ENVIADO";
        textoFeedback.color = Verde;
        UtilidadesDesafio.Sonido("sfx_acierto");
        UtilidadesDesafio.Sacudir(raiz, 5f, 0.15f);

        yield return new WaitForSecondsRealtime(0.7f);

        ronda++;

        if (ronda >= TotalRondas)
        {
            terminado = true;
            textoEstado.text = "DESAFIO COMPLETADO";
            textoEstado.color = Verde;
            UtilidadesDesafio.Sonido("sfx_ronda");
            yield return new WaitForSecondsRealtime(0.9f);
            Completar();
            yield break;
        }

        IniciarRonda();
    }

    private IEnumerator Fallar(string motivo)
    {
        castigo = true;
        entrada = false;
        botonEnviar.interactable = false;

        textoFeedback.text = motivo;
        textoFeedback.color = Rojo;
        UtilidadesDesafio.Sonido("sfx_fallo");
        UtilidadesDesafio.Sacudir(raiz, 16f, 0.25f);

        for (int i = 0; i < celdas.Length; i++)
            celdas[i].color = CeldaApagada;

        yield return new WaitForSecondsRealtime(0.5f);
        PintarPatron(true);

        yield return new WaitForSecondsRealtime(0.7f);

        vidas--;

        if (vidas <= 0)
        {
            vidas = 2;
            ronda = 0;
            textoEstado.text = "SNAP PERDIDO";
            textoEstado.color = Rojo;
            yield return new WaitForSecondsRealtime(0.8f);
        }

        castigo = false;
        IniciarRonda();
    }
}
