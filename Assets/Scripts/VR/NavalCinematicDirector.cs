using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Arma la batalla con la lectura de la cinemática: mar Gerstner, barcos blancos,
/// humo de cañón y atardecer. Se engancha solo al pulsar Play en EscenaVR.
/// </summary>
public class NavalCinematicDirector : MonoBehaviour
{
    [SerializeField] private GameObject controlDerecho;
    [SerializeField] private GameObject controlIzquierdo;

    private NavalVfxPool efectos;
    private Transform jugador;

    private void Start()
    {
        GameObject plano = GameObject.Find("Plane");
        if (plano != null)
            plano.SetActive(false);

        efectos = gameObject.AddComponent<NavalVfxPool>();
        AplicarLuz();
        AplicarVolumen();
        CrearCielo();
        CrearMar();
        CrearIslaLejana();
        jugador = CrearBarco(true, Vector3.zero, 1f);
        AsegurarMandos();
        CrearEnemigo(12f, 0.4f);
        CrearEnemigo(16f, 2.1f);
        CrearEnemigo(18f, 3.6f);
    }

    public static Material Toon(Color color)
    {
        Shader shader = Shader.Find("Juego/ToonNaval");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.enableInstancing = true;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        material.color = color;
        return material;
    }

    private void AplicarLuz()
    {
        Light[] luces = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light sol = null;
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i].type == LightType.Directional)
                sol = luces[i];
        }

        if (sol == null)
        {
            GameObject go = new GameObject("Sol");
            sol = go.AddComponent<Light>();
            sol.type = LightType.Directional;
        }

        sol.color = new Color(1f, 0.62f, 0.267f);
        sol.intensity = 1.5f;
        sol.shadows = LightShadows.Soft;
        sol.transform.rotation = Quaternion.Euler(16f, -28f, 0f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.85f, 0.55f, 0.38f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(1f, 0.62f, 0.32f);
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 120f;
        QualitySettings.shadowDistance = 42f;
    }

    private void AplicarVolumen()
    {
        Camera camara = Camera.main;
        if (camara != null)
        {
            UniversalAdditionalCameraData datos = camara.GetUniversalAdditionalCameraData();
            datos.renderPostProcessing = true;
            datos.requiresDepthTexture = true;
            camara.farClipPlane = 250f;
        }

        Volume volumen = gameObject.AddComponent<Volume>();
        volumen.isGlobal = true;
        volumen.priority = 20f;
        VolumeProfile perfil = ScriptableObject.CreateInstance<VolumeProfile>();
        volumen.profile = perfil;

        Bloom bloom = perfil.Add<Bloom>();
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(1.2f);

        Tonemapping mapa = perfil.Add<Tonemapping>();
        mapa.mode.Override(TonemappingMode.ACES);

        ColorAdjustments ajuste = perfil.Add<ColorAdjustments>();
        ajuste.postExposure.Override(0.12f);
        ajuste.contrast.Override(14f);
        ajuste.colorFilter.Override(new Color(1f, 0.93f, 0.82f));

        ShadowsMidtonesHighlights tonos = perfil.Add<ShadowsMidtonesHighlights>();
        tonos.shadows.Override(new Vector4(0.75f, 0.82f, 1.15f, 0.35f));
        tonos.highlights.Override(new Vector4(1.15f, 0.78f, 0.42f, 0.3f));

        Vignette vineta = perfil.Add<Vignette>();
        vineta.intensity.Override(0.16f);
        vineta.smoothness.Override(0.42f);

        DepthOfField foco = perfil.Add<DepthOfField>();
        foco.mode.Override(DepthOfFieldMode.Gaussian);
        foco.gaussianStart.Override(26f);
        foco.gaussianEnd.Override(80f);
        foco.gaussianMaxRadius.Override(0.55f);
    }

    private static void CrearCielo()
    {
        GameObject domo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(domo.GetComponent<Collider>());
        domo.name = "Cielo";
        domo.transform.localScale = Vector3.one * 90f;
        Shader shader = Shader.Find("Juego/CieloDorado");
        Renderer render = domo.GetComponent<Renderer>();
        render.sharedMaterial = shader != null ? new Material(shader) : Toon(new Color(1f, 0.55f, 0.25f));
        render.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void CrearMar()
    {
        GameObject mar = new GameObject("Mar");
        MeshFilter filtro = mar.AddComponent<MeshFilter>();
        MeshRenderer render = mar.AddComponent<MeshRenderer>();
        filtro.sharedMesh = Malla(64, 140f);
        Shader shader = Shader.Find("Juego/AguaGerstner");
        Material material = shader != null ? new Material(shader) : Toon(new Color(0.06f, 0.28f, 0.5f));
        material.enableInstancing = true;
        render.sharedMaterial = material;
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
    }

    private static void CrearIslaLejana()
    {
        GameObject isla = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(isla.GetComponent<Collider>());
        isla.name = "Isla";
        isla.transform.position = new Vector3(-38f, 0.2f, 34f);
        isla.transform.localScale = new Vector3(16f, 3.2f, 14f);
        isla.GetComponent<Renderer>().material = Toon(new Color(0.78f, 0.62f, 0.28f));

        GameObject pico = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pico.GetComponent<Collider>());
        pico.transform.position = new Vector3(-38f, 3.4f, 34f);
        pico.transform.localScale = new Vector3(7f, 4.5f, 6f);
        pico.GetComponent<Renderer>().material = Toon(new Color(0.55f, 0.62f, 0.28f));
    }

    private Transform CrearBarco(bool delJugador, Vector3 posicion, float escala)
    {
        GameObject raiz = new GameObject(delJugador ? "BarcoJugador" : "BarcoEnemigo");
        raiz.transform.position = posicion + Vector3.up * 0.35f;
        raiz.transform.localScale = Vector3.one * escala;

        Rigidbody cuerpo = raiz.AddComponent<Rigidbody>();
        cuerpo.mass = delJugador ? 18f : 10f;
        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(1.8f, 1.1f, 4.6f);
        caja.center = new Vector3(0f, 0.55f, 0f);

        NavalHull casco = raiz.AddComponent<NavalHull>();
        casco.Configurar(delJugador ? 140f : 70f, delJugador);
        WaterFloatBehavior flotacion = raiz.AddComponent<WaterFloatBehavior>();
        flotacion.AnclarEnElSitio(delJugador);

        Color blanco = new Color(0.93f, 0.94f, 0.95f);
        Color madera = new Color(0.55f, 0.32f, 0.16f);
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 0.32f, 0.1f), new Vector3(1.55f, 0.42f, 4.1f), blanco);
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 0.42f, 2.15f), new Vector3(0.7f, 0.28f, 0.7f), blanco);
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 0.58f, -0.15f), new Vector3(1.25f, 0.08f, 3.1f), madera);
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 1.05f, -0.35f), new Vector3(0.85f, 0.62f, 1.15f), blanco);
        Bloque(raiz.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.55f, -0.15f), new Vector3(0.18f, 0.28f, 0.18f), new Color(0.16f, 0.16f, 0.17f));
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 1.72f, -0.15f), new Vector3(0.22f, 0.06f, 0.22f), new Color(0.82f, 0.12f, 0.1f));
        Bloque(raiz.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.7f, 0.35f), new Vector3(0.06f, 0.7f, 0.06f), madera);
        Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0.16f, 2.25f, 0.35f), new Vector3(0.32f, 0.22f, 0.04f), new Color(0.82f, 0.12f, 0.1f));
        Baranda(raiz.transform, -0.72f);
        Baranda(raiz.transform, 0.72f);
        Farol(raiz.transform, new Vector3(-0.55f, 0.85f, 1.15f));
        Farol(raiz.transform, new Vector3(0.55f, 0.85f, 1.15f));

        GameObject popa = new GameObject("Popa");
        popa.transform.SetParent(raiz.transform, false);
        popa.transform.localPosition = new Vector3(0f, 0.2f, -2.15f);
        efectos.CrearEstelaSeguidora(popa.transform);

        NavalCannon canon = MontarCanon(raiz.transform, new Vector3(0.95f, 0.85f, 0.7f), delJugador);
        if (!delJugador)
        {
            EnemyShipController ia = raiz.AddComponent<EnemyShipController>();
            ia.Configurar(jugador, canon, Random.Range(0f, 6f), Random.Range(11f, 18f));
        }
        else
        {
            MontarCanon(raiz.transform, new Vector3(-0.95f, 0.85f, 0.7f), true);
            GameObject cubierta = new GameObject("Cubierta");
            cubierta.transform.SetParent(raiz.transform, false);
            cubierta.transform.localPosition = new Vector3(0f, 0.7f, -0.4f);
            GameObject origen = GameObject.Find("XR Origin (VR)");
            if (origen != null)
            {
                origen.transform.SetParent(cubierta.transform, false);
                origen.transform.localPosition = Vector3.zero;
                origen.transform.localRotation = Quaternion.identity;
            }
        }

        return raiz.transform;
    }

    private void CrearEnemigo(float distancia, float angulo)
    {
        Vector3 posicion = new Vector3(Mathf.Cos(angulo) * distancia, 0f, Mathf.Sin(angulo) * distancia + 4f);
        CrearBarco(false, posicion, 0.85f);
    }

    private NavalCannon MontarCanon(Transform barco, Vector3 local, bool delJugador)
    {
        GameObject raiz = new GameObject(delJugador ? "Canon" : "CanonEnemigo");
        raiz.transform.SetParent(barco, false);
        raiz.transform.localPosition = local;
        Bloque(raiz.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.05f, 0.25f), new Vector3(0.16f, 0.42f, 0.16f), new Color(0.12f, 0.12f, 0.13f));
        raiz.transform.GetChild(0).localRotation = Quaternion.Euler(90f, 0f, 0f);
        if (delJugador)
            Bloque(raiz.transform, PrimitiveType.Cube, new Vector3(0f, 0.22f, -0.25f), new Vector3(0.46f, 0.08f, 0.08f), new Color(0.5f, 0.3f, 0.14f));

        NavalCannon canon = raiz.AddComponent<NavalCannon>();
        canon.Efectos = efectos;
        canon.DefinirJugador(delJugador);
        return canon;
    }

    private void AsegurarMandos()
    {
        GameObject offset = GameObject.Find("Camera Offset");
        if (offset == null || offset.transform.childCount > 1)
            return;

        if (controlDerecho != null)
            Instantiate(controlDerecho, offset.transform);
        if (controlIzquierdo != null)
            Instantiate(controlIzquierdo, offset.transform);
    }

    private static void Baranda(Transform padre, float x)
    {
        Color madera = new Color(0.62f, 0.38f, 0.16f);
        Bloque(padre, PrimitiveType.Cube, new Vector3(x, 1.05f, 0.15f), new Vector3(0.05f, 0.05f, 3.2f), madera);
        for (int i = 0; i < 5; i++)
            Bloque(padre, PrimitiveType.Cube, new Vector3(x, 0.82f, -1.2f + i * 0.7f), new Vector3(0.05f, 0.38f, 0.05f), madera);
    }

    private static void Farol(Transform padre, Vector3 local)
    {
        GameObject farol = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(farol.GetComponent<Collider>());
        farol.transform.SetParent(padre, false);
        farol.transform.localPosition = local;
        farol.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material material = shader != null ? new Material(shader) : Toon(new Color(1f, 0.78f, 0.3f));
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", new Color(1f, 0.78f, 0.3f));
        farol.GetComponent<Renderer>().material = material;
    }

    private static void Bloque(Transform padre, PrimitiveType tipo, Vector3 local, Vector3 escala, Color color)
    {
        GameObject pieza = GameObject.CreatePrimitive(tipo);
        Destroy(pieza.GetComponent<Collider>());
        pieza.transform.SetParent(padre, false);
        pieza.transform.localPosition = local;
        pieza.transform.localScale = escala;
        pieza.GetComponent<Renderer>().material = Toon(color);
    }

    private static Mesh Malla(int divisiones, float tamano)
    {
        Mesh malla = new Mesh();
        malla.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        int lado = divisiones + 1;
        Vector3[] vertices = new Vector3[lado * lado];
        float paso = tamano / divisiones;
        float mitad = tamano * 0.5f;
        for (int z = 0; z < lado; z++)
        {
            for (int x = 0; x < lado; x++)
                vertices[z * lado + x] = new Vector3(x * paso - mitad, 0f, z * paso - mitad);
        }

        int[] triangulos = new int[divisiones * divisiones * 6];
        int t = 0;
        for (int z = 0; z < divisiones; z++)
        {
            for (int x = 0; x < divisiones; x++)
            {
                int i = z * lado + x;
                triangulos[t++] = i;
                triangulos[t++] = i + lado;
                triangulos[t++] = i + 1;
                triangulos[t++] = i + 1;
                triangulos[t++] = i + lado;
                triangulos[t++] = i + lado + 1;
            }
        }

        malla.vertices = vertices;
        malla.triangles = triangulos;
        malla.RecalculateNormals();
        return malla;
    }
}
