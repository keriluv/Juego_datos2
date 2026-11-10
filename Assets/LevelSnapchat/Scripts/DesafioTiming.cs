using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesafioTiming : DesafioSnap
{
    private const float AnchoBarra = 360f;
    private const int Objetivo = 3;

    private Image zona;
    private RectTransform marcador;
    private TextMeshProUGUI textoAciertos;
    private TextMeshProUGUI textoFeedback;

    private float posicionMarcador;
    private float direccion = 1f;
    private float velocidad = 220f;
    private float zonaInicio;
    private float zonaAncho;
    private float cooldown;
    private int aciertos;
    private Coroutine destello;

    private static readonly Color Verde = new Color(0.13f, 0.77f, 0.37f, 1f);
    private static readonly Color Rojo = new Color(0.91f, 0.15f, 0.17f, 1f);
    private static readonly Color Amarillo = new Color(1f, 0.86f, 0.2f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.76f, 0.82f, 1f);

    protected override void Construir()
    {
        UtilidadesDesafio.ReiniciarEntrada();

        Image fondo = gameObject.AddComponent<Image>();
        fondo.sprite = SpriteRedondeado;
        fondo.type = Image.Type.Sliced;
        fondo.color = new Color(0.04f, 0.12f, 0.18f, 0.95f);
        fondo.raycastTarget = false;

        TextMeshProUGUI titulo = UtilidadesDesafio.CrearTexto("Titulo", raiz, Fuente,
            "RITMO DE PLAYA", 30f, Color.white);
        UtilidadesDesafio.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(400f, 40f));

        TextMeshProUGUI subtitulo = UtilidadesDesafio.CrearTexto("Subtitulo", raiz, Fuente,
            "Golpea cuando el marcador este en la zona verde", 14f, Gris);
        UtilidadesDesafio.Colocar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(410f, 26f));

        textoAciertos = UtilidadesDesafio.CrearTexto("Aciertos", raiz, Fuente,
            "ACIERTOS 0/" + Objetivo, 18f, Amarillo);
        UtilidadesDesafio.Colocar(textoAciertos.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(240f, 26f));

        Image barra = UtilidadesDesafio.CrearPanel("Barra", raiz, SpriteRedondeado, new Color(0.08f, 0.08f, 0.13f, 1f));
        UtilidadesDesafio.Colocar(barra.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(AnchoBarra, 44f));

        zona = UtilidadesDesafio.CrearPanel("Zona", barra.transform, SpriteRedondeado, new Color(Verde.r, Verde.g, Verde.b, 0.85f));
        zona.rectTransform.anchorMin = new Vector2(0f, 0f);
        zona.rectTransform.anchorMax = new Vector2(0f, 1f);
        zona.rectTransform.pivot = new Vector2(0f, 0.5f);
        zona.rectTransform.anchoredPosition = Vector2.zero;
        zona.rectTransform.sizeDelta = new Vector2(0f, 0f);

        Image imgMarcador = UtilidadesDesafio.CrearPanel("Marcador", barra.transform, SpriteRedondeado, Color.white);
        marcador = imgMarcador.rectTransform;
        marcador.anchorMin = new Vector2(0f, 0.5f);
        marcador.anchorMax = new Vector2(0f, 0.5f);
        marcador.pivot = new Vector2(0.5f, 0.5f);
        marcador.sizeDelta = new Vector2(9f, 54f);

        textoFeedback = UtilidadesDesafio.CrearTexto("Feedback", raiz, Fuente, "", 22f, Color.white);
        UtilidadesDesafio.Colocar(textoFeedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -56f), new Vector2(400f, 34f));

        TextMeshProUGUI pista = UtilidadesDesafio.CrearTexto("Pista", raiz, Fuente,
            "Click o gatillo para golpear", 13f, Gris);
        UtilidadesDesafio.Colocar(pista.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(400f, 24f));

        ConfigurarRonda();
    }

    private void Update()
    {
        if (terminado) return;

        if (cooldown > 0f) cooldown -= Time.unscaledDeltaTime;

        posicionMarcador += direccion * velocidad * Time.unscaledDeltaTime;

        if (posicionMarcador >= AnchoBarra)
        {
            posicionMarcador = AnchoBarra;
            direccion = -1f;
        }
        else if (posicionMarcador <= 0f)
        {
            posicionMarcador = 0f;
            direccion = 1f;
        }

        AplicarMarcador();

        if (cooldown <= 0f && UtilidadesDesafio.GolpePresionado())
            Evaluar();
    }

    private void Evaluar()
    {
        bool acierto = posicionMarcador >= zonaInicio && posicionMarcador <= zonaInicio + zonaAncho;

        if (acierto)
        {
            aciertos++;
            textoAciertos.text = "ACIERTOS " + aciertos + "/" + Objetivo;
            textoFeedback.text = "PERFECTO!";
            textoFeedback.color = Verde;

            Destellar(Verde);

            if (aciertos >= Objetivo)
            {
                terminado = true;
                StartCoroutine(Final());
                return;
            }

            ConfigurarRonda();
        }
        else
        {
            textoFeedback.text = "FALLASTE";
            textoFeedback.color = Rojo;
            Destellar(Rojo);
            cooldown = 0.25f;
        }
    }

    private void ConfigurarRonda()
    {
        zonaAncho = AnchoBarra * Mathf.Lerp(0.42f, 0.24f, aciertos / (float)Objetivo);
        zonaInicio = Random.Range(0f, AnchoBarra - zonaAncho);
        velocidad = 220f + aciertos * 90f;
        posicionMarcador = 0f;
        direccion = 1f;

        zona.rectTransform.anchoredPosition = new Vector2(zonaInicio, 0f);
        zona.rectTransform.sizeDelta = new Vector2(zonaAncho, 0f);
        AplicarMarcador();
    }

    private void AplicarMarcador()
    {
        marcador.anchoredPosition = new Vector2(posicionMarcador, 0f);
    }

    private void Destellar(Color color)
    {
        if (destello != null) StopCoroutine(destello);
        destello = StartCoroutine(RutinaDestello(color));
    }

    private IEnumerator RutinaDestello(Color color)
    {
        zona.color = color;
        yield return new WaitForSecondsRealtime(0.2f);
        zona.color = new Color(Verde.r, Verde.g, Verde.b, 0.85f);
        destello = null;
    }

    private IEnumerator Final()
    {
        textoFeedback.text = "DESAFIO COMPLETADO";
        textoFeedback.color = Amarillo;
        yield return new WaitForSecondsRealtime(0.9f);
        Completar();
    }
}
