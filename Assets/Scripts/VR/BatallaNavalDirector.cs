using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Construye la batalla de barcos dentro de EscenaVR al pulsar Play.
/// Usa el XR Origin que ya está en la escena y los prefabs asignados en el Inspector.
/// </summary>
public class BatallaNavalDirector : MonoBehaviour
{
    [Header("Assets del proyecto")]
    [SerializeField] private GameObject modeloBarco;
    [SerializeField] private GameObject efectoExplosion;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private GameObject controlDerecho;
    [SerializeField] private GameObject controlIzquierdo;

    [Header("Batalla")]
    [SerializeField] private string escenaAlSalir = "MainMenu";
    [SerializeField] private int cantidadEnemigos = 3;
    [SerializeField] private float vidaJugador = 120f;
    [SerializeField] private float vidaEnemigo = 90f;
    [SerializeField] private float danoJugador = 34f;
    [SerializeField] private float danoEnemigo = 14f;

    private readonly List<CanonPirata> canonesJugador = new List<CanonPirata>();
    private readonly List<BarcoSalud> enemigos = new List<BarcoSalud>();
    private BarcoSalud barcoJugador;
    private Text textoEstado;
    private Image barraVida;
    private int indiceCanon;
    private bool terminada;

    private void Start()
    {
        DesactivarPlanoSuelto();
        CrearMar();
        AsegurarControles();

        Transform barco = CrearBarcoJugador();
        CrearHud(barco);
        for (int i = 0; i < cantidadEnemigos; i++)
            CrearEnemigo(i);

        ActualizarTexto();
    }

    private void Update()
    {
        if (terminada || barcoJugador == null)
            return;

        bool sinCasco = !XRSettings.isDeviceActive;
        if (sinCasco && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            DispararConRaton();

        if (sinCasco && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            VolverAlMenu();

        ActualizarTexto();
    }

    public void VolverAlMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(escenaAlSalir))
            SceneManager.LoadScene(escenaAlSalir);
    }

    private void DispararConRaton()
    {
        if (canonesJugador.Count == 0 || Camera.main == null)
            return;

        CanonPirata canon = canonesJugador[indiceCanon % canonesJugador.Count];
        indiceCanon++;
        canon.DispararHacia(Camera.main.transform.forward);
    }

    private void DesactivarPlanoSuelto()
    {
        GameObject plano = GameObject.Find("Plane");
        if (plano != null)
            plano.SetActive(false);
    }

    private void CrearMar()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.55f, 0.72f, 0.82f);
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 160f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.62f, 0.7f);

        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.45f, 0.68f, 0.86f);
            Camera.main.farClipPlane = 250f;
        }

        GameObject mar = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mar.name = "Mar";
        mar.transform.position = Vector3.zero;
        mar.transform.localScale = new Vector3(40f, 1f, 40f);
        mar.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.05f, 0.28f, 0.45f));
    }

    private void AsegurarControles()
    {
        bool yaHayInteraccion = FindAnyObjectByType<XRRayInteractor>() != null
            || FindAnyObjectByType<NearFarInteractor>() != null;
        if (yaHayInteraccion)
            return;

        Transform padre = GameObject.Find("Camera Offset") != null
            ? GameObject.Find("Camera Offset").transform
            : null;

        if (controlDerecho != null)
            Instantiate(controlDerecho, padre);
        if (controlIzquierdo != null)
            Instantiate(controlIzquierdo, padre);
    }

    private Transform CrearBarcoJugador()
    {
        GameObject raiz = new GameObject("BarcoJugador");
        raiz.transform.position = new Vector3(0f, 0.15f, 0f);
        ColocarModelo(raiz.transform, modeloBarco, 9f, new Color(0.35f, 0.22f, 0.12f));

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.2f, 1.4f, 8f);
        caja.center = new Vector3(0f, 0.7f, 0f);

        barcoJugador = raiz.AddComponent<BarcoSalud>();
        barcoJugador.Configurar(vidaJugador, 0, true);
        barcoJugador.AlHundirse += _ => Terminar(false);

        GameObject cubierta = new GameObject("Cubierta");
        cubierta.transform.SetParent(raiz.transform, false);
        cubierta.transform.localPosition = new Vector3(0f, 1.35f, -0.6f);

        PlataformaBarcoVR plataforma = raiz.AddComponent<PlataformaBarcoVR>();
        GameObject origen = GameObject.Find("XR Origin (VR)");
        plataforma.Configurar(cubierta.transform, origen != null ? origen.transform : null);

        canonesJugador.Add(CrearCanon(raiz.transform, barcoJugador, new Vector3(-1.35f, 1.15f, 0.8f), false, danoJugador));
        canonesJugador.Add(CrearCanon(raiz.transform, barcoJugador, new Vector3(1.35f, 1.15f, 0.8f), false, danoJugador));
        return raiz.transform;
    }

    private void CrearEnemigo(int indice)
    {
        GameObject raiz = new GameObject("BarcoEnemigo " + (indice + 1));
        ColocarModelo(raiz.transform, modeloBarco, 8f, new Color(0.45f, 0.08f, 0.08f));

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.2f, 1.4f, 8f);
        caja.center = new Vector3(0f, 0.7f, 0f);

        BarcoSalud salud = raiz.AddComponent<BarcoSalud>();
        salud.Configurar(vidaEnemigo, 1, false);
        salud.AlHundirse += AlHundirEnemigo;
        enemigos.Add(salud);

        CanonPirata canon = CrearCanon(raiz.transform, salud, new Vector3(0f, 1.2f, 3.2f), true, danoEnemigo);
        BarcoEnemigo piloto = raiz.AddComponent<BarcoEnemigo>();
        float radio = 16f + indice * 6f;
        piloto.Configurar(barcoJugador.transform, canon, radio, indice * 2.1f);
    }

    private void AlHundirEnemigo(BarcoSalud barco)
    {
        enemigos.Remove(barco);
        if (enemigos.Count == 0)
            Terminar(true);
    }

    private CanonPirata CrearCanon(Transform barco, BarcoSalud dueno, Vector3 local, bool automatico, float dano)
    {
        GameObject raiz = new GameObject(automatico ? "CanonEnemigo" : "CanonJugador");
        raiz.transform.SetParent(barco, false);
        raiz.transform.localPosition = local;

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(0.4f, 0.4f, 1.5f);
        caja.center = new Vector3(0f, 0f, 0.2f);
        raiz.AddComponent<Rigidbody>();

        GameObject tubo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(tubo.GetComponent<Collider>());
        tubo.transform.SetParent(raiz.transform, false);
        tubo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        tubo.transform.localPosition = new Vector3(0f, 0f, 0.25f);
        tubo.transform.localScale = new Vector3(0.28f, 0.65f, 0.28f);
        tubo.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.12f, 0.12f, 0.13f));

        CanonPirata canon = raiz.AddComponent<CanonPirata>();
        canon.Configurar(dueno, automatico, dano, null, efectoExplosion, sonidoDisparo, sonidoImpacto);
        return canon;
    }

    private void ColocarModelo(Transform padre, GameObject modelo, float largoDeseado, Color colorRespaldo)
    {
        GameObject visual = modelo != null
            ? Instantiate(modelo, padre)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);

        visual.name = "Casco";
        visual.transform.SetParent(padre, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        Collider colisionDelModelo = visual.GetComponent<Collider>();
        if (colisionDelModelo != null)
            Destroy(colisionDelModelo);

        Renderer[] renders = visual.GetComponentsInChildren<Renderer>();
        if (renders.Length == 0)
        {
            visual.transform.localScale = new Vector3(2.4f, 0.8f, 6.5f);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Renderer propio = visual.GetComponent<Renderer>();
            if (propio != null)
                propio.material = MaterialBarco.Crear(colorRespaldo);
            return;
        }

        Bounds limites = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++)
            limites.Encapsulate(renders[i].bounds);

        float largo = Mathf.Max(limites.size.x, limites.size.z);
        if (largo > 0.01f)
            visual.transform.localScale = Vector3.one * (largoDeseado / largo);

        renders = visual.GetComponentsInChildren<Renderer>();
        limites = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++)
            limites.Encapsulate(renders[i].bounds);

        Vector3 centroLocal = padre.InverseTransformPoint(limites.center);
        visual.transform.localPosition -= new Vector3(centroLocal.x, limites.min.y - padre.position.y, centroLocal.z);
    }

    private void CrearHud(Transform barco)
    {
        GameObject lienzoGo = new GameObject("HudBatalla");
        lienzoGo.transform.SetParent(barco, false);
        lienzoGo.transform.localPosition = new Vector3(0f, 2.3f, 2.4f);
        lienzoGo.transform.localRotation = Quaternion.identity;
        lienzoGo.transform.localScale = Vector3.one * 0.01f;

        Canvas lienzo = lienzoGo.AddComponent<Canvas>();
        lienzo.renderMode = RenderMode.WorldSpace;
        RectTransform rect = lienzoGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 140f);

        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(lienzoGo.transform, false);
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = new Color(0.05f, 0.08f, 0.12f, 0.72f);
        RectTransform fondoRect = fondo.GetComponent<RectTransform>();
        fondoRect.anchorMin = Vector2.zero;
        fondoRect.anchorMax = Vector2.one;
        fondoRect.offsetMin = Vector2.zero;
        fondoRect.offsetMax = Vector2.zero;

        GameObject barraGo = new GameObject("BarraVida");
        barraGo.transform.SetParent(lienzoGo.transform, false);
        barraVida = barraGo.AddComponent<Image>();
        barraVida.color = new Color(0.2f, 0.75f, 0.35f);
        RectTransform barraRect = barraGo.GetComponent<RectTransform>();
        barraRect.anchorMin = new Vector2(0.08f, 0.62f);
        barraRect.anchorMax = new Vector2(0.92f, 0.82f);
        barraRect.offsetMin = Vector2.zero;
        barraRect.offsetMax = Vector2.zero;

        GameObject textoGo = new GameObject("Texto");
        textoGo.transform.SetParent(lienzoGo.transform, false);
        textoEstado = textoGo.AddComponent<Text>();
        textoEstado.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoEstado.fontSize = 22;
        textoEstado.alignment = TextAnchor.MiddleCenter;
        textoEstado.color = Color.white;
        textoEstado.horizontalOverflow = HorizontalWrapMode.Wrap;
        textoEstado.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0.05f, 0.05f);
        textoRect.anchorMax = new Vector2(0.95f, 0.58f);
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;
    }

    private void ActualizarTexto()
    {
        if (textoEstado == null || barcoJugador == null)
            return;

        float proporcion = barcoJugador.Vida / barcoJugador.VidaMaxima;
        if (barraVida != null)
        {
            RectTransform barra = barraVida.rectTransform;
            barra.anchorMax = new Vector2(Mathf.Lerp(0.08f, 0.92f, proporcion), 0.82f);
            barraVida.color = Color.Lerp(new Color(0.75f, 0.15f, 0.12f), new Color(0.2f, 0.75f, 0.35f), proporcion);
        }

        string ayuda = XRSettings.isDeviceActive
            ? "Grip agarra el cañón. Trigger dispara."
            : "Clic izquierdo dispara. Esc vuelve al menú.";

        textoEstado.text = "Casco " + Mathf.CeilToInt(barcoJugador.Vida) + "    Barcos enemigos " + enemigos.Count + "\n" + ayuda;
    }

    private void Terminar(bool victoria)
    {
        if (terminada)
            return;

        terminada = true;
        if (textoEstado != null)
        {
            textoEstado.text = victoria
                ? "Victoria. La flota enemiga se hundió."
                : "Derrota. Tu barco se fue a pique.";
        }

        Invoke(nameof(VolverAlMenu), 4f);
    }
}
