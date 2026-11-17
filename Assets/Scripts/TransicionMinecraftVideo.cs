using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays MinecraftUI/videoplayback.mp4 fullscreen, then loads LevelYoutubev3
/// through MultijugadorJuego.IrAEscena (host only when networked).
/// Synced via RelayTransicionMinecraft ClientRpc.
/// </summary>
public class TransicionMinecraftVideo : MonoBehaviour
{
    public static TransicionMinecraftVideo Instancia { get; private set; }

    private const string EscenaDestino = "LevelYoutubev3";
    private const string RutaVideo = "MinecraftUI/videoplayback";

    private bool enCurso;
    private VideoPlayer reproductor;
    private GameObject lienzo;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
        if (reproductor != null)
            reproductor.loopPointReached -= AlTerminarVideo;
    }

    public static void SolicitarDesdeCrack()
    {
        TransicionMinecraftVideo t = Instancia;
        if (t == null)
        {
            GameObject go = new GameObject("TransicionMinecraftVideo");
            DontDestroyOnLoad(go);
            t = go.AddComponent<TransicionMinecraftVideo>();
        }

        t.IniciarReproduccionLocal();
    }

    private void IniciarReproduccionLocal()
    {
        if (enCurso) return;
        enCurso = true;
        StartCoroutine(ReproducirYContinuar());
    }

    private IEnumerator ReproducirYContinuar()
    {
        if (PauseManager.IsPaused)
            PauseManager.Instancia.pause();

        CrearUI();

        VideoClip clip = Resources.Load<VideoClip>(RutaVideo);
        if (clip == null)
        {
            // Fallback: StreamingAssets / direct file under Assets is not in Resources.
            // Load via URL from the project MinecraftUI path when possible.
            string ruta = System.IO.Path.Combine(Application.dataPath, "MinecraftUI", "videoplayback.mp4");
            if (System.IO.File.Exists(ruta))
            {
                reproductor.source = VideoSource.Url;
                reproductor.url = "file:///" + ruta.Replace("\\", "/");
            }
            else
            {
                Debug.LogError("[TransicionMinecraft] No se encontró videoplayback.mp4");
                TerminarYCargar();
                yield break;
            }
        }
        else
        {
            reproductor.source = VideoSource.VideoClip;
            reproductor.clip = clip;
        }

        reproductor.playOnAwake = false;
        reproductor.isLooping = false;
        reproductor.skipOnDrop = true;
        reproductor.loopPointReached += AlTerminarVideo;
        reproductor.Prepare();

        while (!reproductor.isPrepared)
            yield return null;

        reproductor.Play();
    }

    private void AlTerminarVideo(VideoPlayer vp)
    {
        TerminarYCargar();
    }

    private void TerminarYCargar()
    {
        if (reproductor != null)
            reproductor.loopPointReached -= AlTerminarVideo;

        if (lienzo != null)
            Destroy(lienzo);

        // Entering v3 after crack — mark visit objective (not "level complete").
        GameProgressionManager.Asegurar().RegisterObjectiveCompleted("Entered_LevelYoutubev3");
        GameProgressionManager.Asegurar().RegisterMilestone("Entered_LevelYoutubev3");

        bool esHostOOffline =
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening ||
            NetworkManager.Singleton.IsServer;

        if (!esHostOOffline)
        {
            Debug.Log("[TransicionMinecraft] Cliente esperando carga del anfitrión.");
            return;
        }

        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.IrAEscena(EscenaDestino);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(EscenaDestino);
    }

    private void CrearUI()
    {
        lienzo = new GameObject("VideoMinecraftUI");
        DontDestroyOnLoad(lienzo);

        Canvas canvas = lienzo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9500;
        lienzo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        lienzo.AddComponent<GraphicRaycaster>();

        GameObject rawGo = new GameObject("Video");
        rawGo.transform.SetParent(lienzo.transform, false);
        RawImage raw = rawGo.AddComponent<RawImage>();
        raw.color = Color.white;

        RectTransform rect = raw.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RenderTexture rt = new RenderTexture(1920, 1080, 0);
        raw.texture = rt;

        reproductor = lienzo.AddComponent<VideoPlayer>();
        reproductor.renderMode = VideoRenderMode.RenderTexture;
        reproductor.targetTexture = rt;
        reproductor.audioOutputMode = VideoAudioOutputMode.AudioSource;

        AudioSource audio = lienzo.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        reproductor.SetTargetAudioSource(0, audio);
    }
}
