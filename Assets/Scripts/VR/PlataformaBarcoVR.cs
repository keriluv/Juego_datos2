using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Mantiene al jugador pegado a la cubierta.
/// El barco se mece poco y una viñeta oscurece los bordes si el movimiento sube,
/// para reducir el mareo. No hay locomoción artificial.
/// </summary>
public class PlataformaBarcoVR : MonoBehaviour
{
    [SerializeField] private Transform anclaCubierta;
    [SerializeField] private Transform origenXR;
    [SerializeField] private float balanceoGrados = 2.8f;
    [SerializeField] private float velocidadBalanceo = 0.28f;

    private Transform camara;
    private TrackedPoseDriver seguimiento;
    private Image[] bordesVineta;
    private bool usarRaton;
    private float yaw;
    private float pitch;

    public void Configurar(Transform ancla, Transform origen)
    {
        anclaCubierta = ancla;
        origenXR = origen;
    }

    private void Start()
    {
        if (origenXR == null)
        {
            GameObject encontrado = GameObject.Find("XR Origin (VR)");
            if (encontrado != null)
                origenXR = encontrado.transform;
        }

        if (origenXR != null && anclaCubierta != null)
        {
            origenXR.SetParent(anclaCubierta, false);
            origenXR.localPosition = Vector3.zero;
            origenXR.localRotation = Quaternion.identity;
        }

        Camera camaraPrincipal = Camera.main;
        if (camaraPrincipal != null)
        {
            camara = camaraPrincipal.transform;
            seguimiento = camara.GetComponent<TrackedPoseDriver>();
        }

        usarRaton = !XRSettings.isDeviceActive;
        if (usarRaton && seguimiento != null)
            seguimiento.enabled = false;

        CrearVineta();
    }

    private void Update()
    {
        float tiempo = Time.time * velocidadBalanceo;
        float cabeceo = Mathf.Sin(tiempo) * balanceoGrados;
        float balance = Mathf.Cos(tiempo * 0.75f) * balanceoGrados * 0.55f;
        transform.localRotation = Quaternion.Euler(cabeceo, 0f, balance);

        if (usarRaton && camara != null && Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * 0.08f;
            pitch = Mathf.Clamp(pitch - delta.y * 0.08f, -35f, 40f);
            camara.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        float intensidad = Mathf.Clamp01((Mathf.Abs(cabeceo) + Mathf.Abs(balance)) / 8f);
        if (bordesVineta == null)
            return;

        byte alfa = (byte)(intensidad * 150f);
        foreach (Image borde in bordesVineta)
        {
            Color color = borde.color;
            color.a = alfa / 255f;
            borde.color = color;
        }
    }

    private void CrearVineta()
    {
        if (camara == null)
            return;

        GameObject lienzoGo = new GameObject("VinetaConfort");
        lienzoGo.transform.SetParent(camara, false);
        lienzoGo.transform.localPosition = new Vector3(0f, 0f, 0.35f);
        lienzoGo.transform.localRotation = Quaternion.identity;
        lienzoGo.transform.localScale = Vector3.one * 0.0016f;

        Canvas lienzo = lienzoGo.AddComponent<Canvas>();
        lienzo.renderMode = RenderMode.WorldSpace;
        RectTransform rect = lienzoGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 260f);

        bordesVineta = new Image[4];
        bordesVineta[0] = CrearBorde(rect, new Vector2(0f, 115f), new Vector2(420f, 70f));
        bordesVineta[1] = CrearBorde(rect, new Vector2(0f, -115f), new Vector2(420f, 70f));
        bordesVineta[2] = CrearBorde(rect, new Vector2(-190f, 0f), new Vector2(50f, 260f));
        bordesVineta[3] = CrearBorde(rect, new Vector2(190f, 0f), new Vector2(50f, 260f));
    }

    private static Image CrearBorde(RectTransform padre, Vector2 posicion, Vector2 tamano)
    {
        GameObject borde = new GameObject("Borde");
        borde.transform.SetParent(padre, false);
        Image imagen = borde.AddComponent<Image>();
        imagen.color = new Color(0f, 0f, 0f, 0f);
        RectTransform rect = borde.GetComponent<RectTransform>();
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
        return imagen;
    }
}
