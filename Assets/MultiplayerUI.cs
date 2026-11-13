using System.Collections;
using TMPro;
using UnityEngine;

public class MultiplayerUI : MonoBehaviour
{
    private bool multiplayerPopUp = false;
    private bool crearSala = false;
    private bool unirSala = false;
    private bool codeShown = false;

    [SerializeField] private GameObject mPopUp;
    [SerializeField] private GameObject black1;

    [SerializeField] private GameObject mCrear;
    [SerializeField] private GameObject black2;

    [SerializeField] private GameObject mUnir;

    [SerializeField] private TextMeshProUGUI code;
    [SerializeField] private TextMeshProUGUI roomCode;

    [SerializeField] private float duration = 0.25f;
    [SerializeField] private GameObject panelJugadores;
    private AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.7f, 1.1f),
        new Keyframe(1f, 1f)
    );

    [SerializeField] private GameObject panelSkins;
    [SerializeField] private GameObject black3;
    [SerializeField] private RectTransform posFinal;
    [SerializeField] private float duracionMovimiento = 0.5f;
    private Vector3 localposStart;
    private Vector3 localAngleStart;
    private bool charSelector = false;

    private MultijugadorJuego multiplayer;

    void Start()
    {
        ResetearEscalaYDesactivar(mPopUp);
        ResetearEscalaYDesactivar(mCrear);
        ResetearEscalaYDesactivar(mUnir);
        roomCode.gameObject.SetActive(false);

        multiplayer = GameObject.FindAnyObjectByType<MultijugadorJuego>();
        localposStart = panelSkins.GetComponent<RectTransform>().localPosition;
        localAngleStart = panelSkins.GetComponent<RectTransform>().localEulerAngles;
    }

    private void Update()
    {
        if (codeShown)
        {
            roomCode.text = multiplayer.CodigoActual;
        }
        if (panelJugadores != null)
        {
            panelJugadores.SetActive(multiplayer != null && multiplayer.EnPartida);
        }
    }

    public void multiplayerUI()
    {
        multiplayerPopUp = !multiplayerPopUp;
        black1.SetActive(multiplayerPopUp);
        ManejarPanel(mPopUp, multiplayerPopUp);
    }

    public void crearsalaUI()
    {
        crearSala = !crearSala;
        black2.SetActive(crearSala);
        unirSala = false;

        ManejarPanel(mCrear, crearSala);
        AnimarCierreInmediato(mUnir);
    }

    public void unirSalaUI()
    {
        unirSala = !unirSala;
        black2.SetActive(unirSala);
        crearSala = false;

        ManejarPanel(mUnir, unirSala);
        AnimarCierreInmediato(mCrear);
    }

    public void crearPartida()
    {
        crearSala = false;
        unirSala = false;
        AnimarCierreInmediato(mCrear);
        AnimarCierreInmediato(mUnir);
        black1.SetActive(false);
        black2.SetActive(false);
        multiplayer.CrearPartida();
        roomCode.gameObject.SetActive(true);
        codeShown = true;
    }

    public void iniciarPartida()
    {
        StartCoroutine(multiplayer.CargarEscenaCuandoListo());
    }

    public void unirseCodigo()
    {
        string codigo = code.text;
        codigo = (codigo ?? "").Replace("\u200B", "").Replace("\uFEFF", "").Replace("\u2060", "").Replace("\u00A0", "").Trim().ToUpperInvariant();
        multiplayer.UnirseConCodigo(codigo);
    }
    public void elegirSkin(int indice)
    {
        if (SkinJugador.Local != null)
            SkinJugador.Local.CambiarSkin(indice);
    }

    public void skinSiguiente()
    {
        if (SkinJugador.Local != null) SkinJugador.Local.SiguienteSkin();
    }

    public void skinAnterior()
    {
        if (SkinJugador.Local != null) SkinJugador.Local.AnteriorSkin();
    }

    private void ManejarPanel(GameObject panel, bool abrir)
    {
        StopAllCoroutines();
        if (abrir)
        {
            panel.SetActive(true);
            StartCoroutine(AnimarEscala(panel, Vector3.one, duration));
        }
        else
        {
            StartCoroutine(AnimarCierre(panel, duration));
        }
    }

    private void AnimarCierreInmediato(GameObject panel)
    {
        if (panel.activeSelf)
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(false);
        }
    }

    private IEnumerator AnimarEscala(GameObject panel, Vector3 targetScale, float duracionAnim)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector3 initialScale = rect.localScale;
        float elapsed = 0f;

        while (elapsed < duracionAnim)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duracionAnim);
            float curveValue = bounceCurve.Evaluate(t);

            rect.localScale = Vector3.LerpUnclamped(initialScale, targetScale, curveValue);
            yield return null;
        }

        rect.localScale = targetScale;
    }

    private IEnumerator AnimarCierre(GameObject panel, float duracionAnim)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector3 initialScale = rect.localScale;
        float elapsed = 0f;

        while (elapsed < duracionAnim)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duracionAnim);

            rect.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }

        rect.localScale = Vector3.zero;
        panel.SetActive(false);
    }

    private void ResetearEscalaYDesactivar(GameObject panel)
    {
        if (panel != null)
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(false);
        }
    }
    public void MoverPanelPersonajes()
    {
        charSelector = !charSelector;
        black3.SetActive(charSelector);
        if (charSelector)
        {
            StopCoroutine(nameof(AnimarMovimientoPanel));
            StartCoroutine(AnimarMovimientoPanel());
        }
        else
        {
            StopCoroutine(nameof(CerrarPanel));
            StartCoroutine(CerrarPanel());
        }
    }

    private IEnumerator AnimarMovimientoPanel()
    {
        RectTransform rect = panelSkins.GetComponent<RectTransform>();

        Vector3 posicionInicial = rect.localPosition;
        Vector3 rotacionInicial = rect.localEulerAngles;

        float tiempo = 0f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionMovimiento);

            rect.localPosition = Vector3.Lerp(
                posicionInicial,
                posFinal.localPosition,
                t
            );

            rect.localRotation = Quaternion.Lerp(
                Quaternion.Euler(rotacionInicial),
                Quaternion.Euler(posFinal.localEulerAngles),
                t
            );

            yield return null;
        }

        rect.localPosition = posFinal.localPosition;
        rect.localRotation = Quaternion.Euler(posFinal.localEulerAngles);
    }
    private IEnumerator CerrarPanel()
    {
        RectTransform rect = panelSkins.GetComponent<RectTransform>();

        Vector3 posicionInicial = rect.localPosition;
        Vector3 rotacionInicial = rect.localEulerAngles;

        float tiempo = 0f;

        while (tiempo < duracionMovimiento)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionMovimiento);

            rect.localPosition = Vector3.Lerp(
                posicionInicial,
                localposStart,
                t
            );

            rect.localRotation = Quaternion.Lerp(
                Quaternion.Euler(rotacionInicial),
                Quaternion.Euler(localAngleStart),
                t
            );

            yield return null;
        }

        rect.localPosition = localposStart;
        rect.localRotation = Quaternion.Euler(localAngleStart);
    }
}