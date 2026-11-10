using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModoPruebaCapsula : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Crear()
    {
        if (SceneManager.GetActiveScene().name != "LevelSnapchat") return;
        if (GameObject.Find("ModoPruebaCapsula") != null) return;

        GameObject objeto = new GameObject("ModoPruebaCapsula");
        objeto.AddComponent<ModoPruebaCapsula>();
        DontDestroyOnLoad(objeto);
    }

    private Camera camaraJuego;
    private Camera camaraCapsula;
    private RenderTexture textura;
    private RawImage imagen;
    private Canvas lienzoCapsula;

    private bool interfazVisible = true;
    private bool capsulaVisible = true;

    private void Start()
    {
        if (GameObject.Find("CamaraCapsula") != null || GameObject.Find("CanvasCapsula") != null)
        {
            Destroy(gameObject);
            return;
        }

        camaraJuego = Camera.main;

        if (camaraJuego == null)
        {
            Debug.LogWarning("ModoPruebaCapsula: no hay ninguna camara con el tag MainCamera.");
            return;
        }

        CrearCamara();
        CrearLienzo();
    }

    private void CrearCamara()
    {
        GameObject objeto = new GameObject("CamaraCapsula");
        objeto.transform.SetParent(transform, false);

        camaraCapsula = objeto.AddComponent<Camera>();
        camaraCapsula.CopyFrom(camaraJuego);
        camaraCapsula.enabled = true;

        // Fondo transparente: solo se vera el mundo, no el color de la camara.
        camaraCapsula.clearFlags = CameraClearFlags.SolidColor;
        camaraCapsula.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camaraCapsula.depth = camaraJuego.depth + 1;
        camaraCapsula.allowHDR = false;
        camaraCapsula.allowMSAA = false;

        CrearTextura();
    }

    private void CrearTextura()
    {
        int ancho = Mathf.Max(1, Screen.width);
        int alto = Mathf.Max(1, Screen.height);

        if (textura != null)
        {
            camaraCapsula.targetTexture = null;
            textura.Release();
            Destroy(textura);
        }

        textura = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32);
        textura.name = "RT_Capsula";
        textura.Create();
        camaraCapsula.targetTexture = textura;

        if (imagen != null) imagen.texture = textura;
    }

    private void CrearLienzo()
    {
        GameObject objeto = new GameObject("CanvasCapsula", typeof(RectTransform));
        objeto.transform.SetParent(transform, false);

        lienzoCapsula = objeto.AddComponent<Canvas>();
        lienzoCapsula.renderMode = RenderMode.ScreenSpaceOverlay;
        lienzoCapsula.sortingOrder = 1000;

        GameObject imagenObjeto = new GameObject("Mundo", typeof(RectTransform));
        imagenObjeto.transform.SetParent(objeto.transform, false);

        RectTransform rect = imagenObjeto.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        imagen = imagenObjeto.AddComponent<RawImage>();
        imagen.texture = textura;
        imagen.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (camaraJuego == null || camaraCapsula == null || textura == null) return;

        if (textura.width != Screen.width || textura.height != Screen.height)
            CrearTextura();

        camaraCapsula.transform.SetPositionAndRotation(
            camaraJuego.transform.position,
            camaraJuego.transform.rotation);

        camaraCapsula.fieldOfView = camaraJuego.fieldOfView;
        camaraCapsula.orthographic = camaraJuego.orthographic;
        camaraCapsula.orthographicSize = camaraJuego.orthographicSize;
        camaraCapsula.nearClipPlane = camaraJuego.nearClipPlane;
        camaraCapsula.farClipPlane = camaraJuego.farClipPlane;
        camaraCapsula.aspect = camaraJuego.aspect;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) AlternarInterfaz();
        if (Input.GetKeyDown(KeyCode.F2)) AlternarCapsula();
    }

    private void AlternarInterfaz()
    {
        interfazVisible = !interfazVisible;

        Canvas[] lienzos = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        int cambiados = 0;

        foreach (Canvas lienzo in lienzos)
        {
            if (lienzo == lienzoCapsula) continue;
            if (lienzo.renderMode != RenderMode.ScreenSpaceOverlay) continue;

            lienzo.enabled = interfazVisible;

            GraphicRaycaster raycaster = lienzo.GetComponent<GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = interfazVisible;

            cambiados++;
        }

        // Si la app esta oculta, la camara principal ya muestra el mundo.
        if (camaraCapsula != null)
            camaraCapsula.enabled = interfazVisible && capsulaVisible;

        Debug.Log(interfazVisible
            ? "UI visible (" + cambiados + " canvas). F1 para ocultarla."
            : "UI oculta (" + cambiados + " canvas). F1 para volver.");
    }

    private void AlternarCapsula()
    {
        capsulaVisible = !capsulaVisible;

        if (lienzoCapsula != null)
            lienzoCapsula.enabled = capsulaVisible;

        if (camaraCapsula != null)
            camaraCapsula.enabled = capsulaVisible && interfazVisible;

        Debug.Log(capsulaVisible
            ? "Capsula siempre visible: ACTIVADA. F2 para desactivar."
            : "Capsula siempre visible: DESACTIVADA. F2 para activar.");
    }
}
