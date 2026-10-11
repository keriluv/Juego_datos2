using UnityEngine;

/// <summary>
/// Mueve la textura y las olas del mar, y balancea el barco del jugador con poca amplitud.
/// Asigna aquí la textura Stylize Water de LowlyPoly.
/// </summary>
public class OceanWaterController : MonoBehaviour
{
    [SerializeField] private Texture2D texturaAgua;
    [SerializeField] private Transform barcoJugador;
    [SerializeField] private float alturaOla = 0.42f;
    [SerializeField] private float velocidadOla = 0.85f;
    [SerializeField] private float balanceoGrados = 2.4f;
    [SerializeField] private float velocidadBalanceo = 0.22f;

    private Material materialAgua;
    private Quaternion rotacionBase;

    public void Configurar(Texture2D textura, Transform barco)
    {
        texturaAgua = textura;
        barcoJugador = barco;
        AplicarMaterial();
        if (barcoJugador != null)
            rotacionBase = barcoJugador.localRotation;
    }

    private void Awake()
    {
        AplicarMaterial();
    }

    private void Update()
    {
        if (barcoJugador == null)
            return;

        float tiempo = Time.time * velocidadBalanceo;
        float cabeceo = Mathf.Sin(tiempo) * balanceoGrados;
        float balance = Mathf.Cos(tiempo * 0.7f) * balanceoGrados * 0.55f;
        barcoJugador.localRotation = rotacionBase * Quaternion.Euler(cabeceo, 0f, balance);
    }

    private void AplicarMaterial()
    {
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render == null)
            return;

        Shader shader = Shader.Find("Juego/AguaEstilizada");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");

        materialAgua = new Material(shader);
        Texture textura = texturaAgua != null ? texturaAgua : CrearTexturaEstilizada();
        if (materialAgua.HasProperty("_MainTex"))
            materialAgua.SetTexture("_MainTex", textura);
        if (materialAgua.HasProperty("_BaseMap"))
            materialAgua.SetTexture("_BaseMap", textura);
        materialAgua.SetFloat("_WaveHeight", alturaOla);
        materialAgua.SetFloat("_WaveSpeed", velocidadOla);
        render.material = materialAgua;
    }

    private static Texture2D CrearTexturaEstilizada()
    {
        const int tamano = 128;
        Texture2D textura = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Repeat;
        textura.filterMode = FilterMode.Point;

        for (int y = 0; y < tamano; y++)
        {
            for (int x = 0; x < tamano; x++)
            {
                int bloqueX = x / 8;
                int bloqueY = y / 8;
                float ruido = Mathf.PerlinNoise(bloqueX * 0.35f, bloqueY * 0.35f);
                Color claro = new Color(0.25f, 0.72f, 0.78f);
                Color profundo = new Color(0.03f, 0.22f, 0.42f);
                Color espuma = new Color(0.85f, 0.95f, 0.95f);
                Color color = Color.Lerp(profundo, claro, ruido);
                if (ruido > 0.72f)
                    color = Color.Lerp(color, espuma, 0.55f);
                textura.SetPixel(x, y, color);
            }
        }

        textura.Apply();
        return textura;
    }
}
