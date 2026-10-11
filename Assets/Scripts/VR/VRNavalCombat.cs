using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Atardecer, domo de cielo, cubierta del jugador y silueta de los barcos enemigos.
/// Lo llama VRGameManager al iniciar EscenaVR. No usa mouse ni teclado.
/// </summary>
public class VRNavalCombat : MonoBehaviour
{
    [Header("Ambiente")]
    [SerializeField] private int cantidadIslas = 7;
    [SerializeField] private Color colorSol = new Color(1f, 0.58f, 0.32f);
    [SerializeField] private float intensidadSol = 1.2f;

    [Header("Madera y velas")]
    [SerializeField] private Color madera = new Color(0.4f, 0.25f, 0.13f);
    [SerializeField] private Color maderaClara = new Color(0.62f, 0.42f, 0.22f);
    [SerializeField] private Color velaJugador = new Color(0.93f, 0.88f, 0.74f);
    [SerializeField] private Color velaEnemiga = new Color(0.45f, 0.1f, 0.12f);

    private float sombraAnterior = 40f;
    private bool sombraGuardada;

    public IslandSpawner CrearIslas(int cantidad)
    {
        cantidadIslas = cantidad;
        GameObject raiz = new GameObject("Archipielago");
        IslandSpawner spawner = raiz.AddComponent<IslandSpawner>();
        spawner.Generar(cantidadIslas);
        return spawner;
    }

    public void AplicarAtardecer()
    {
        sombraAnterior = QualitySettings.shadowDistance;
        sombraGuardada = true;
        QualitySettings.shadowDistance = 36f;

        Light sol = BuscarSol();
        if (sol != null)
        {
            sol.color = colorSol;
            sol.intensity = intensidadSol;
            sol.shadows = LightShadows.Soft;
            sol.transform.rotation = Quaternion.Euler(16f, -42f, 0f);
            CrearSolVisual(-sol.transform.forward);
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.95f, 0.58f, 0.38f);
        RenderSettings.fogStartDistance = 42f;
        RenderSettings.fogEndDistance = 185f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.78f, 0.52f, 0.4f);
        RenderSettings.skybox = null;

        CrearDomo();

        if (Camera.main == null)
            return;

        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0.16f, 0.26f, 0.48f);
        Camera.main.farClipPlane = 320f;
    }

    public void VestirCubierta(Transform barco)
    {
        Transform decoracion = Carpeta(barco, "DecoracionCubierta");
        Suelo(decoracion, new Vector3(2.5f, 0.08f, 6.4f), new Vector3(0f, 1.32f, 0.15f));
        Barandas(decoracion);
        Barriles(decoracion);
        Timon(decoracion, new Vector3(0f, 1.55f, -2.55f));
        Mastil(decoracion, new Vector3(0f, 1.35f, 0.35f), velaJugador, 4.4f);
        Cureña(decoracion, new Vector3(-1.45f, 1.05f, 1.1f));
        Cureña(decoracion, new Vector3(1.45f, 1.05f, 1.1f));
        Farol(decoracion, new Vector3(-1.15f, 1.85f, 2.7f));
        Farol(decoracion, new Vector3(1.15f, 1.85f, 2.7f));
        Estela(barco, new Vector3(0f, 0.15f, 4.1f));
    }

    public void VestirEnemigo(Transform barco, int variante)
    {
        Transform decoracion = Carpeta(barco, "DecoracionEnemigo");
        Mastil(decoracion, new Vector3(0f, 1.15f, -0.2f), velaEnemiga, 3.6f);
        Primitiva(decoracion, PrimitiveType.Cube, "Cabina", new Vector3(0f, 1.55f, -1.7f), Quaternion.identity, new Vector3(1.3f, 0.7f, 1.1f), new Color(0.22f, 0.08f, 0.08f));
        Primitiva(decoracion, PrimitiveType.Cube, "Bandera", new Vector3(0f, 4.55f, -0.2f), Quaternion.identity, new Vector3(0.7f, 0.38f, 0.05f), new Color(0.75f, 0.12f, 0.1f));
        Estela(barco, new Vector3(0f, 0.05f, 3.4f));
    }

    private void OnDestroy()
    {
        if (sombraGuardada)
            QualitySettings.shadowDistance = sombraAnterior;
    }

    private void CrearDomo()
    {
        GameObject domo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        domo.name = "Cielo";
        Collider colision = domo.GetComponent<Collider>();
        if (colision != null)
            Destroy(colision);

        domo.transform.position = Vector3.zero;
        domo.transform.localScale = Vector3.one * 90f;

        Shader shader = Shader.Find("Juego/CieloAtardecer");
        Renderer render = domo.GetComponent<Renderer>();
        if (shader != null)
            render.sharedMaterial = new Material(shader);
        else
            render.sharedMaterial = MaterialBarco.CrearSinLuz(new Color(0.95f, 0.55f, 0.32f));

        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
    }

    private static void CrearSolVisual(Vector3 direccion)
    {
        GameObject sol = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sol.name = "Sol";
        Collider colision = sol.GetComponent<Collider>();
        if (colision != null)
            Destroy(colision);

        sol.transform.position = direccion.normalized * 70f + Vector3.up * 8f;
        sol.transform.localScale = Vector3.one * 9f;
        Renderer render = sol.GetComponent<Renderer>();
        render.sharedMaterial = MaterialBarco.CrearSinLuz(new Color(1f, 0.82f, 0.45f));
        render.shadowCastingMode = ShadowCastingMode.Off;
        render.receiveShadows = false;
    }

    private void Barandas(Transform padre)
    {
        for (int lado = -1; lado <= 1; lado += 2)
        {
            Primitiva(padre, PrimitiveType.Cube, "Pasamanos", new Vector3(lado * 1.55f, 1.95f, 0.2f), Quaternion.identity, new Vector3(0.06f, 0.06f, 5.6f), maderaClara);
            for (int i = 0; i < 6; i++)
            {
                float z = -2.3f + i * 1.05f;
                Primitiva(padre, PrimitiveType.Cube, "Poste", new Vector3(lado * 1.55f, 1.62f, z), Quaternion.identity, new Vector3(0.07f, 0.55f, 0.07f), madera);
            }
        }
    }

    private void Barriles(Transform padre)
    {
        Vector3[] puestos = { new Vector3(-0.85f, 1.55f, -1.7f), new Vector3(0.85f, 1.55f, -1.85f), new Vector3(0.9f, 1.55f, 2.15f) };
        for (int i = 0; i < puestos.Length; i++)
        {
            Primitiva(padre, PrimitiveType.Cylinder, "Barril", puestos[i], Quaternion.identity, new Vector3(0.42f, 0.28f, 0.42f), madera);
            Primitiva(padre, PrimitiveType.Cylinder, "Aro", puestos[i] + Vector3.up * 0.12f, Quaternion.identity, new Vector3(0.46f, 0.03f, 0.46f), new Color(0.15f, 0.15f, 0.16f));
        }
    }

    private void Timon(Transform padre, Vector3 local)
    {
        Primitiva(padre, PrimitiveType.Cylinder, "ColumnaTimon", local + Vector3.down * 0.25f, Quaternion.identity, new Vector3(0.08f, 0.28f, 0.08f), madera);
        for (int i = 0; i < 8; i++)
        {
            float angulo = i * 45f * Mathf.Deg2Rad;
            Vector3 puesto = local + new Vector3(Mathf.Cos(angulo) * 0.38f, Mathf.Sin(angulo) * 0.38f, 0f);
            Primitiva(padre, PrimitiveType.Cube, "AroTimon", puesto, Quaternion.Euler(0f, 0f, -i * 45f), new Vector3(0.28f, 0.06f, 0.06f), maderaClara);
        }
    }

    private void Mastil(Transform padre, Vector3 local, Color vela, float alto)
    {
        Primitiva(padre, PrimitiveType.Cylinder, "Mastil", local + Vector3.up * (alto * 0.5f), Quaternion.identity, new Vector3(0.1f, alto * 0.5f, 0.1f), madera);
        Primitiva(padre, PrimitiveType.Cube, "Vela", local + new Vector3(0.15f, alto * 0.62f, 0f), Quaternion.identity, new Vector3(0.06f, alto * 0.42f, 1.7f), vela);
    }

    private void Cureña(Transform padre, Vector3 local)
    {
        Primitiva(padre, PrimitiveType.Cube, "Cureña", local, Quaternion.identity, new Vector3(0.7f, 0.22f, 1.15f), madera);
        Primitiva(padre, PrimitiveType.Cylinder, "Rueda", local + new Vector3(-0.32f, -0.12f, 0.35f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.18f, 0.05f, 0.18f), new Color(0.18f, 0.12f, 0.08f));
        Primitiva(padre, PrimitiveType.Cylinder, "Rueda", local + new Vector3(0.32f, -0.12f, 0.35f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.18f, 0.05f, 0.18f), new Color(0.18f, 0.12f, 0.08f));
    }

    private void Farol(Transform padre, Vector3 local)
    {
        Transform farol = Primitiva(padre, PrimitiveType.Cube, "Farol", local, Quaternion.identity, new Vector3(0.16f, 0.22f, 0.16f), new Color(1f, 0.72f, 0.28f));
        farol.GetComponent<Renderer>().sharedMaterial = MaterialBarco.CrearSinLuz(new Color(1f, 0.72f, 0.28f));
    }

    private static void Suelo(Transform padre, Vector3 escala, Vector3 local)
    {
        Primitiva(padre, PrimitiveType.Cube, "Tablas", local, Quaternion.identity, escala, new Color(0.48f, 0.31f, 0.16f));
    }

    private static void Estela(Transform barco, Vector3 local)
    {
        GameObject estela = new GameObject("Estela");
        estela.transform.SetParent(barco, false);
        estela.transform.localPosition = local;
        ParticleSystem sistema = estela.AddComponent<ParticleSystem>();
        sistema.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var principal = sistema.main;
        principal.loop = true;
        principal.playOnAwake = true;
        principal.startLifetime = 1.1f;
        principal.startSpeed = 0.35f;
        principal.startSize = 0.28f;
        principal.startColor = new Color(0.9f, 0.93f, 0.9f, 0.65f);
        principal.maxParticles = 16;
        var emision = sistema.emission;
        emision.rateOverTime = 7f;
        var forma = sistema.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = new Vector3(1.2f, 0.05f, 0.2f);
        ParticleSystemRenderer render = sistema.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader != null)
            render.material = new Material(shader);
        sistema.Play();
    }

    private static Transform Carpeta(Transform padre, string nombre)
    {
        GameObject carpeta = new GameObject(nombre);
        carpeta.transform.SetParent(padre, false);
        return carpeta.transform;
    }

    private static Transform Primitiva(Transform padre, PrimitiveType tipo, string nombre, Vector3 local, Quaternion rotacion, Vector3 escala, Color color)
    {
        GameObject pieza = GameObject.CreatePrimitive(tipo);
        pieza.name = nombre;
        Collider colision = pieza.GetComponent<Collider>();
        if (colision != null)
            Destroy(colision);
        pieza.transform.SetParent(padre, false);
        pieza.transform.localPosition = local;
        pieza.transform.localRotation = rotacion;
        pieza.transform.localScale = escala;
        Renderer render = pieza.GetComponent<Renderer>();
        render.sharedMaterial = MaterialBarco.Crear(color);
        render.shadowCastingMode = ShadowCastingMode.Off;
        return pieza.transform;
    }

    private static Light BuscarSol()
    {
        Light[] luces = FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i].type == LightType.Directional)
                return luces[i];
        }

        return null;
    }
}
