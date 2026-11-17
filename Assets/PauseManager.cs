using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instancia { get; private set; }
    public static bool IsPaused => Instancia != null && Instancia.isPaused;

    private bool isPaused;
    private PauseButton selectedPauseButton;

    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject xboxMenu;
    [SerializeField] private GameObject configMenu;
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private GameObject invMenu;
    [SerializeField] private GameObject exitMenu;
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject inv;
    [SerializeField] private GameObject hover;

    private RectTransform target;
    private bool usandoFallback;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AsegurarInstancia()
    {
        if (Instancia != null) return;
        // Scene-placed PauseManager (LevelSelector) wins when present.
        PauseManager existente = Object.FindAnyObjectByType<PauseManager>();
        if (existente != null) return;

        GameObject go = new GameObject("PauseManagerRuntime");
        go.AddComponent<PauseManager>();
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            // Prefer the LevelSelector-authored pause UI over the runtime fallback.
            if (Instancia.usandoFallback && pauseScreen != null)
            {
                Destroy(Instancia.gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        if (Instancia == this) Instancia = null;
        if (isPaused)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Start()
    {
        if (pauseScreen == null)
            CrearFallbackPause();

        SetActivo(pauseScreen, false);
        SetActivo(map, false);
        SetActivo(inv, false);
        AsegurarEventSystem();
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Never leave pause stuck when entering menu/lobby/UI levels.
        if (isPaused || !EscenaPermitePausa(escena.name))
        {
            if (isPaused)
            {
                isPaused = false;
                SetActivo(pauseScreen, false);
            }
            Time.timeScale = 1f;
        }

        // Always restore the right cursor for this scene (UI levels must not stay locked
        // after leaving LevelSelector boat / 3D pause).
        AplicarCursorParaEscena(escena.name, forzarDespausado: true);

        AsegurarEventSystem();
    }

    private void Update()
    {
        ActualizarHover();

        if (DebeAlternarPausa())
            pause();

        if (!isPaused) return;

        if (ControlesJuego.QuiereCancelar())
            CambiarxboxMenu();
        else if (ControlesJuego.QuiereInventario())
            CambiarinvMenu();
    }

    /// <summary>
    /// Pause is gameplay-only. Main Menu and multiplayer lobby must ignore Escape/Start.
    /// </summary>
    public static bool EscenaPermitePausa(string nombreEscena)
    {
        if (string.IsNullOrEmpty(nombreEscena)) return false;
        if (nombreEscena == "MainMenu") return false;
        if (nombreEscena == "pruebaLobby") return false;
        return true;
    }

    /// <summary>
    /// Phone/UI minigames need a free visible cursor so Button/EventSystem clicks work.
    /// </summary>
    public static bool EscenaUsaCursorLibre(string nombreEscena)
    {
        if (string.IsNullOrEmpty(nombreEscena)) return true;
        if (nombreEscena == "MainMenu") return true;
        if (nombreEscena == "pruebaLobby") return true;
        if (nombreEscena == "LevelSnapchat") return true;
        if (nombreEscena == "EscenaTiktok") return true;
        if (nombreEscena == "MapaNiveles3D") return true;
        if (nombreEscena == "TutorialGoogle") return true;
        return false;
    }

    private void AplicarCursorParaEscena(string nombreEscena, bool forzarDespausado)
    {
        bool libre = EscenaUsaCursorLibre(nombreEscena) || (!forzarDespausado && isPaused);
        if (forzarDespausado)
            libre = EscenaUsaCursorLibre(nombreEscena);

        if (libre)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private bool DebeAlternarPausa()
    {
        if (!EscenaPermitePausa(SceneManager.GetActiveScene().name))
            return false;

        return ControlesJuego.QuierePausar();
    }

    private void ActualizarHover()
    {
        if (target == null || hover == null) return;

        RectTransform hoverRect = hover.GetComponent<RectTransform>();
        if (hoverRect == null) return;

        Vector3 destino = target.position + new Vector3(0f, -40f, 0f);
        hoverRect.position = Vector3.Lerp(
            hoverRect.position,
            destino,
            Time.unscaledDeltaTime * 10f);
    }

    public void pause()
    {
        if (!EscenaPermitePausa(SceneManager.GetActiveScene().name))
        {
            if (isPaused)
            {
                isPaused = false;
                SetActivo(pauseScreen, false);
                Time.timeScale = 1f;
            }
            return;
        }

        if (pauseScreen == null)
            CrearFallbackPause();

        isPaused = !isPaused;
        SetActivo(pauseScreen, isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        string escena = SceneManager.GetActiveScene().name;
        if (isPaused || EscenaUsaCursorLibre(escena))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (isPaused)
            AsegurarEventSystem();
    }

    public void CambiarxboxMenu()
    {
        if (usandoFallback) return;
        SetActivo(xboxMenu, true);
        SetActivo(configMenu, false);
        SetActivo(mapMenu, false);
        SetActivo(exitMenu, false);
        SetActivo(invMenu, false);
        SetActivo(map, false);
        SetActivo(inv, false);
    }

    public void CambiarconfigMenu()
    {
        if (usandoFallback) return;
        SetActivo(configMenu, true);
        SetActivo(xboxMenu, false);
        SetActivo(mapMenu, false);
        SetActivo(exitMenu, false);
        SetActivo(invMenu, false);
        SetActivo(map, false);
        SetActivo(inv, false);
    }

    public void CambiarmapMenu()
    {
        if (usandoFallback) return;
        SetActivo(map, true);
        SetActivo(mapMenu, true);
        SetActivo(exitMenu, false);
        SetActivo(invMenu, false);
        SetActivo(configMenu, false);
        SetActivo(xboxMenu, false);
        SetActivo(inv, false);
    }

    public void CambiarinvMenu()
    {
        if (usandoFallback) return;
        SetActivo(invMenu, true);
        SetActivo(configMenu, false);
        SetActivo(exitMenu, false);
        SetActivo(mapMenu, false);
        SetActivo(xboxMenu, false);
        SetActivo(map, false);
        SetActivo(inv, true);
    }

    public void CambiarexitMenu()
    {
        if (usandoFallback) return;
        SetActivo(exitMenu, true);
        SetActivo(xboxMenu, false);
        SetActivo(mapMenu, false);
        SetActivo(configMenu, false);
        SetActivo(invMenu, false);
        SetActivo(map, false);
        SetActivo(inv, false);
    }

    public void ExitServer()
    {
        if (isPaused) pause();
        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.SalirDePartida();
    }

    public void goToSelector()
    {
        if (isPaused) pause();
        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.IrAEscena("LevelSelector");
    }

    public void CerrarJuego()
    {
        Application.Quit();
    }

    public void SelectedButton(RectTransform valor)
    {
        if (valor == null) return;

        target = valor;

        Button btn = valor.GetComponent<Button>();
        if (btn != null)
            btn.Select();

        PauseButton button = valor.GetComponent<PauseButton>();
        if (button != null)
            changeSprite(button);
    }

    public void changeSprite(PauseButton button)
    {
        if (button == null) return;

        if (selectedPauseButton != null)
            selectedPauseButton.SetSelected(false);

        selectedPauseButton = button;
        selectedPauseButton.SetSelected(true);
    }

    private static void SetActivo(GameObject go, bool activo)
    {
        if (go != null) go.SetActive(activo);
    }

    private void AsegurarEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    private void CrearFallbackPause()
    {
        usandoFallback = true;

        GameObject canvasGo = new GameObject("PauseFallbackCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9000;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("PauseScreen");
        panel.transform.SetParent(canvasGo.transform, false);
        Image fondo = panel.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.72f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CrearBotonFallback(panel.transform, "Continuar", new Vector2(0f, 40f), () => { if (isPaused) pause(); });
        CrearBotonFallback(panel.transform, "Level Selector", new Vector2(0f, -20f), goToSelector);
        CrearBotonFallback(panel.transform, "Salir partida", new Vector2(0f, -80f), ExitServer);

        pauseScreen = panel;
        panel.SetActive(false);
    }

    private void CrearBotonFallback(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion)
    {
        GameObject go = new GameObject(texto);
        go.transform.SetParent(padre, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280f, 48f);
        rect.anchoredPosition = pos;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(accion);

        GameObject labelGo = new GameObject("Text");
        labelGo.transform.SetParent(go.transform, false);
        Text label = labelGo.AddComponent<Text>();
        label.text = texto;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 22;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
}
