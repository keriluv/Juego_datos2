using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ajustes globales (brillo, sensibilidad, musica, ambiente/efectos).
/// Persisten con PlayerPrefs y se aplican en todas las escenas.
/// La UI del menu (y luego la pausa) solo llama a los setters.
/// </summary>
public class AjustesJuego : MonoBehaviour
{
    public static AjustesJuego Instancia { get; private set; }

    private const string ClaveBrillo = "ajustes.brillo";
    private const string ClaveSensibilidad = "ajustes.sensibilidad";
    private const string ClaveMusica = "ajustes.musica";
    private const string ClaveAmbiente = "ajustes.ambiente";

    [Range(0f, 1f)] public float brillo = 1f;
    [Range(0f, 1f)] public float sensibilidad = 0.5f;
    [Range(0f, 1f)] public float volumenMusica = 1f;
    [Range(0f, 1f)] public float volumenAmbiente = 1f;

    /// <summary>Multiplicador listo para camaras (aprox. 0.2 .. 4).</summary>
    public float MultiplicadorSensibilidad => Mathf.Lerp(0.2f, 4f, sensibilidad);

    public static event System.Action AlCambiarAjustes;

    private Canvas lienzoBrillo;
    private Image panelBrillo;
    private readonly Dictionary<int, float> volumenBase = new Dictionary<int, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        if (Instancia != null) return;

        GameObject objeto = new GameObject("AjustesJuego");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<AjustesJuego>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
        Cargar();
        CrearOverlayBrillo();
        AplicarTodo();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Keep base volumes for DontDestroyOnLoad sources; drop destroyed ones.
        AudioSource[] vivos = FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        HashSet<int> idsVivos = new HashSet<int>();
        for (int i = 0; i < vivos.Length; i++)
        {
            if (vivos[i] != null)
                idsVivos.Add(vivos[i].GetInstanceID());
        }

        List<int> muertos = new List<int>();
        foreach (KeyValuePair<int, float> par in volumenBase)
        {
            if (!idsVivos.Contains(par.Key))
                muertos.Add(par.Key);
        }

        for (int i = 0; i < muertos.Count; i++)
            volumenBase.Remove(muertos[i]);

        AplicarTodo();
    }

    public void Cargar()
    {
        brillo = PlayerPrefs.GetFloat(ClaveBrillo, 1f);
        sensibilidad = PlayerPrefs.GetFloat(ClaveSensibilidad, 0.5f);
        volumenMusica = PlayerPrefs.GetFloat(ClaveMusica, 1f);
        volumenAmbiente = PlayerPrefs.GetFloat(ClaveAmbiente, 1f);
    }

    public void Guardar()
    {
        PlayerPrefs.SetFloat(ClaveBrillo, brillo);
        PlayerPrefs.SetFloat(ClaveSensibilidad, sensibilidad);
        PlayerPrefs.SetFloat(ClaveMusica, volumenMusica);
        PlayerPrefs.SetFloat(ClaveAmbiente, volumenAmbiente);
        PlayerPrefs.Save();
    }

    public void EstablecerBrillo(float valor)
    {
        brillo = Mathf.Clamp01(valor);
        AplicarBrillo();
        Guardar();
        AlCambiarAjustes?.Invoke();
    }

    public void EstablecerSensibilidad(float valor)
    {
        sensibilidad = Mathf.Clamp01(valor);
        Guardar();
        AlCambiarAjustes?.Invoke();
    }

    public void EstablecerMusica(float valor)
    {
        volumenMusica = Mathf.Clamp01(valor);
        AplicarAudio();
        Guardar();
        AlCambiarAjustes?.Invoke();
    }

    public void EstablecerAmbiente(float valor)
    {
        volumenAmbiente = Mathf.Clamp01(valor);
        AplicarAudio();
        Guardar();
        AlCambiarAjustes?.Invoke();
    }

    public void AplicarTodo()
    {
        AplicarBrillo();
        AplicarAudio();
        AlCambiarAjustes?.Invoke();
    }

    private void CrearOverlayBrillo()
    {
        if (lienzoBrillo != null) return;

        GameObject raiz = new GameObject("OverlayBrillo");
        raiz.transform.SetParent(transform, false);

        lienzoBrillo = raiz.AddComponent<Canvas>();
        lienzoBrillo.renderMode = RenderMode.ScreenSpaceOverlay;
        lienzoBrillo.sortingOrder = 9999;

        CanvasScaler escalador = raiz.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);

        raiz.AddComponent<GraphicRaycaster>().enabled = false;

        GameObject panel = new GameObject("PanelBrillo");
        panel.transform.SetParent(raiz.transform, false);
        panelBrillo = panel.AddComponent<Image>();
        panelBrillo.color = new Color(0f, 0f, 0f, 0f);
        panelBrillo.raycastTarget = false;

        RectTransform rect = panelBrillo.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void AplicarBrillo()
    {
        if (panelBrillo == null) CrearOverlayBrillo();
        // 1 = pantalla clara, 0 = casi negra
        float oscuridad = 1f - brillo;
        panelBrillo.color = new Color(0f, 0f, 0f, Mathf.Clamp01(oscuridad));
    }

    private void AplicarAudio()
    {
        AudioSource[] fuentes = FindObjectsByType<AudioSource>(FindObjectsInactive.Include);

        for (int i = 0; i < fuentes.Length; i++)
        {
            AudioSource fuente = fuentes[i];
            if (fuente == null) continue;

            int id = fuente.GetInstanceID();
            if (!volumenBase.ContainsKey(id))
                volumenBase[id] = fuente.volume;

            float baseVol = volumenBase[id];
            CanalAudio canal = fuente.GetComponent<CanalAudio>();

            if (canal != null)
            {
                fuente.volume = baseVol * FactorCanal(canal.tipo);
                continue;
            }

            // Sin marcador: looping ≈ musica, el resto ≈ ambiente/efectos.
            fuente.volume = baseVol * (fuente.loop ? volumenMusica : volumenAmbiente);
        }
    }

    private float FactorCanal(CanalAudio.Tipo tipo)
    {
        return tipo == CanalAudio.Tipo.Musica ? volumenMusica : volumenAmbiente;
    }

    /// <summary>Volumen para PlayOneShot / SFX puntuales.</summary>
    public static float FactorEfectos()
    {
        return Instancia != null ? Instancia.volumenAmbiente : 1f;
    }

    public static float FactorMusica()
    {
        return Instancia != null ? Instancia.volumenMusica : 1f;
    }
}
