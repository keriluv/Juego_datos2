using UnityEngine;

/// <summary>
/// Agua estilizada con desplazamiento UV y olas en el shader, más un balanceo corto del barco.
/// Arrastra la textura Stylize Water al campo Textura Agua. Si queda vacío, se genera una de reserva.
/// Va en el objeto del mar.
/// </summary>
public class OceanMover : MonoBehaviour
{
    [SerializeField] private Texture2D texturaAgua;
    [SerializeField] private Transform barcoJugador;
    [SerializeField] private float alturaOla = 0.55f;
    [SerializeField] private float velocidadOla = 0.8f;
    [SerializeField] private Vector2 scroll = new Vector2(0.035f, 0.018f);
    [SerializeField] private float balanceoGrados = 2.1f;
    [SerializeField] private float velocidadBalanceo = 0.2f;

    private Quaternion rotacionBase;

    public void Configurar(Texture2D textura, Transform barco)
    {
        if (textura != null)
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
        float balance = Mathf.Cos(tiempo * 0.73f) * balanceoGrados * 0.5f;
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

        Material material = new Material(shader);
        Texture textura = texturaAgua != null ? texturaAgua : CrearTexturaEstilizada();
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", textura);
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", textura);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", new Color(0.2f, 0.46f, 0.58f, 1f));
        if (material.HasProperty("_ColorProfundo"))
            material.SetColor("_ColorProfundo", new Color(0.04f, 0.1f, 0.24f, 1f));
        if (material.HasProperty("_Scroll"))
            material.SetVector("_Scroll", new Vector4(scroll.x, scroll.y, 0f, 0f));
        if (material.HasProperty("_WaveHeight"))
            material.SetFloat("_WaveHeight", alturaOla);
        if (material.HasProperty("_WaveSpeed"))
            material.SetFloat("_WaveSpeed", velocidadOla);

        render.material = material;
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        render.receiveShadows = false;
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
                float ruido = Mathf.PerlinNoise(bloqueX * 0.37f, bloqueY * 0.37f);
                Color profundo = new Color(0.04f, 0.18f, 0.38f);
                Color claro = new Color(0.22f, 0.62f, 0.7f);
                Color espuma = new Color(0.86f, 0.9f, 0.82f);
                Color color = Color.Lerp(profundo, claro, ruido);
                if (ruido > 0.78f)
                    color = Color.Lerp(color, espuma, 0.65f);
                textura.SetPixel(x, y, color);
            }
        }

        textura.Apply(false, true);
        return textura;
    }
}
