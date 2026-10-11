using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Arma la batalla en EscenaVR: mar, barco del jugador, cañones VR y oleadas de enemigos.
/// El marcador vive en una tabla de madera sobre la cubierta, no en la pantalla.
/// </summary>
public class VRGameManager : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private GameObject modeloBarco;
    [SerializeField] private Texture2D texturaAgua;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private GameObject controlDerecho;
    [SerializeField] private GameObject controlIzquierdo;

    [Header("Oleadas")]
    [SerializeField] private string escenaAlSalir = "MainMenu";
    [SerializeField] private int enemigosIniciales = 2;
    [SerializeField] private float vidaJugador = 140f;
    [SerializeField] private float vidaBaseEnemigo = 80f;

    private readonly List<BarcoSalud> enemigosVivos = new List<BarcoSalud>();
    private BarcoSalud barcoJugador;
    private Text textoCubierta;
    private Image rellenoVida;
    private GameObject efectoFuego;
    private GameObject efectoAgua;
    private IslandSpawner islas;
    private VRNavalCombat naval;
    private int oleada = 1;
    private bool terminada;

    private void Start()
    {
        DesactivarPlanoViejo();
        naval = gameObject.AddComponent<VRNavalCombat>();
        naval.AplicarAtardecer();
        efectoFuego = CrearEfectoFuego();
        efectoAgua = CrearEfectoAgua();
        CrearMar();
        islas = naval.CrearIslas(7);
        AsegurarControlesVR();
        Transform barco = CrearBarcoJugador();
        naval.VestirCubierta(barco);
        CrearTablaCubierta(barco);
        AgruparEntorno();
        StartCoroutine(LanzarOleada());
    }

    public void VolverAlMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(escenaAlSalir))
            SceneManager.LoadScene(escenaAlSalir);
    }

    private static void AgruparEntorno()
    {
        GameObject entorno = new GameObject("Entorno");
        MoverA(entorno.transform, "Mar");
        MoverA(entorno.transform, "Cielo");
        MoverA(entorno.transform, "Sol");
        MoverA(entorno.transform, "Archipielago");
    }

    private static void MoverA(Transform padre, string nombre)
    {
        GameObject objeto = GameObject.Find(nombre);
        if (objeto != null)
            objeto.transform.SetParent(padre, true);
    }

    private void DesactivarPlanoViejo()
    {
        GameObject plano = GameObject.Find("Plane");
        if (plano != null)
            plano.SetActive(false);
    }

    private void CrearMar()
    {
        GameObject mar = new GameObject("Mar");
        MeshFilter filtro = mar.AddComponent<MeshFilter>();
        mar.AddComponent<MeshRenderer>();
        MeshCollider colision = mar.AddComponent<MeshCollider>();
        filtro.mesh = CrearMallaMar(56, 180f);
        colision.sharedMesh = filtro.mesh;

        OceanMover agua = mar.AddComponent<OceanMover>();
        agua.Configurar(texturaAgua, null);
    }

    private static Mesh CrearMallaMar(int divisiones, float tamano)
    {
        Mesh malla = new Mesh();
        malla.name = "Mar";
        int lado = divisiones + 1;
        Vector3[] vertices = new Vector3[lado * lado];
        Vector2[] uvs = new Vector2[vertices.Length];
        float paso = tamano / divisiones;
        float mitad = tamano * 0.5f;

        for (int z = 0; z < lado; z++)
        {
            for (int x = 0; x < lado; x++)
            {
                int indice = z * lado + x;
                vertices[indice] = new Vector3(x * paso - mitad, 0f, z * paso - mitad);
                uvs[indice] = new Vector2((float)x / divisiones, (float)z / divisiones) * 12f;
            }
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
        malla.uv = uvs;
        malla.triangles = triangulos;
        malla.RecalculateNormals();
        return malla;
    }

    private void AsegurarControlesVR()
    {
        Transform padre = GameObject.Find("Camera Offset") != null
            ? GameObject.Find("Camera Offset").transform
            : null;

        if (padre == null)
            return;

        if (padre.childCount > 1)
            return;

        if (controlDerecho != null)
            Instantiate(controlDerecho, padre);
        if (controlIzquierdo != null)
            Instantiate(controlIzquierdo, padre);
    }

    private Transform CrearBarcoJugador()
    {
        GameObject raiz = new GameObject("BarcoJugador");
        raiz.transform.position = new Vector3(0f, 0.2f, 0f);
        ColocarCasco(raiz.transform, 10f, new Color(0.32f, 0.2f, 0.1f));

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.4f, 1.5f, 8.5f);
        caja.center = new Vector3(0f, 0.8f, 0f);

        barcoJugador = raiz.AddComponent<BarcoSalud>();
        barcoJugador.Configurar(vidaJugador, 0, true);
        barcoJugador.AlRecibirDanio += (_, __) =>
        {
            HapticsUtility.SendHapticImpulse(0.85f, 0.18f, HapticsUtility.Controller.Both);
            Instanciar(efectoFuego, raiz.transform.position + Vector3.up * 1.2f);
        };
        barcoJugador.AlHundirse += _ => Terminar(false);

        GameObject cubierta = new GameObject("Cubierta");
        cubierta.transform.SetParent(raiz.transform, false);
        cubierta.transform.localPosition = new Vector3(0f, 1.45f, -1.1f);

        GameObject origen = GameObject.Find("XR Origin (VR)");
        if (origen != null)
        {
            origen.transform.SetParent(cubierta.transform, false);
            origen.transform.localPosition = Vector3.zero;
            origen.transform.localRotation = Quaternion.identity;
        }

        OceanMover agua = FindAnyObjectByType<OceanMover>();
        if (agua != null)
            agua.Configurar(texturaAgua, raiz.transform);

        CrearCanonJugador(raiz.transform, new Vector3(-1.45f, 1.2f, 1.1f));
        CrearCanonJugador(raiz.transform, new Vector3(1.45f, 1.2f, 1.1f));
        return raiz.transform;
    }

    private void CrearCanonJugador(Transform barco, Vector3 local)
    {
        VRShipCannon canon = ConstruirCanon(barco, local, true);
        canon.Configurar(barcoJugador, true, 34f, sonidoDisparo, sonidoImpacto, efectoFuego, efectoAgua);
    }

    private IEnumerator LanzarOleada()
    {
        yield return new WaitForSeconds(1.2f);
        while (!terminada)
        {
            int cantidad = Mathf.Min(enemigosIniciales + oleada - 1, 5);
            for (int i = 0; i < cantidad; i++)
                CrearEnemigo(i);

            RefrescarTabla();
            while (enemigosVivos.Count > 0 && !terminada)
                yield return null;

            if (terminada)
                yield break;

            oleada++;
            if (oleada > 5)
            {
                Terminar(true);
                yield break;
            }

            RefrescarTabla();
            yield return new WaitForSeconds(3.5f);
        }
    }

    private void CrearEnemigo(int indice)
    {
        GameObject raiz = new GameObject("Enemigo oleada " + oleada);
        if (islas != null)
            raiz.transform.position = islas.PuntoDeAtaque(indice + oleada * 3);
        ColocarCasco(raiz.transform, 8.5f, new Color(0.28f, 0.1f, 0.1f));

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.2f, 1.4f, 8f);
        caja.center = new Vector3(0f, 0.7f, 0f);

        BarcoSalud salud = raiz.AddComponent<BarcoSalud>();
        salud.Configurar(vidaBaseEnemigo + (oleada - 1) * 18f, 1, false);
        salud.AlHundirse += AlHundirEnemigo;
        salud.AlRecibirDanio += (_, __) => Instanciar(efectoFuego, raiz.transform.position + Vector3.up);
        enemigosVivos.Add(salud);

        VRShipCannon canon = ConstruirCanon(raiz.transform, new Vector3(0f, 1.25f, 3.4f), false);
        canon.Configurar(salud, false, 16f, sonidoDisparo, sonidoImpacto, efectoFuego, efectoAgua);

        if (naval != null)
            naval.VestirEnemigo(raiz.transform, indice);

        float angulo = islas != null ? islas.AnguloDeIsla(indice + oleada * 3) : indice * 1.7f + oleada;
        EnemyShipAI ia = raiz.AddComponent<EnemyShipAI>();
        ia.Configurar(barcoJugador.transform, canon, angulo, oleada);
    }

    private void AlHundirEnemigo(BarcoSalud barco)
    {
        enemigosVivos.Remove(barco);
        RefrescarTabla();
    }

    private VRShipCannon ConstruirCanon(Transform barco, Vector3 local, bool delJugador)
    {
        GameObject raiz = new GameObject(delJugador ? "CanonJugador" : "CanonEnemigo");
        raiz.transform.SetParent(barco, false);
        raiz.transform.localPosition = local;

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(0.55f, 0.45f, 1.7f);
        caja.center = new Vector3(0f, 0.1f, 0.15f);
        raiz.AddComponent<Rigidbody>();

        GameObject tubo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(tubo.GetComponent<Collider>());
        tubo.transform.SetParent(raiz.transform, false);
        tubo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        tubo.transform.localPosition = new Vector3(0f, 0.05f, 0.35f);
        tubo.transform.localScale = new Vector3(0.26f, 0.7f, 0.26f);
        tubo.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.1f, 0.1f, 0.11f));

        if (delJugador)
        {
            GameObject manubrio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(manubrio.GetComponent<Collider>());
            manubrio.name = "Manubrio";
            manubrio.transform.SetParent(raiz.transform, false);
            manubrio.transform.localPosition = new Vector3(0f, 0.28f, -0.35f);
            manubrio.transform.localScale = new Vector3(0.55f, 0.12f, 0.12f);
            manubrio.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.55f, 0.32f, 0.14f));
        }

        return raiz.AddComponent<VRShipCannon>();
    }

    private void ColocarCasco(Transform padre, float largo, Color colorRespaldo)
    {
        GameObject visual = modeloBarco != null
            ? Instantiate(modeloBarco, padre)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);

        visual.name = "Casco";
        visual.transform.SetParent(padre, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        Collider colision = visual.GetComponent<Collider>();
        if (colision != null)
            Destroy(colision);

        Renderer[] renders = visual.GetComponentsInChildren<Renderer>();
        if (renders.Length == 0)
        {
            visual.transform.localScale = new Vector3(2.6f, 0.8f, 7f);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            return;
        }

        Bounds limites = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++)
            limites.Encapsulate(renders[i].bounds);

        float medida = Mathf.Max(limites.size.x, limites.size.z);
        if (medida > 0.01f)
            visual.transform.localScale *= largo / medida;

        if (modeloBarco == null && visual.GetComponent<Renderer>() != null)
            visual.GetComponent<Renderer>().material = MaterialBarco.Crear(colorRespaldo);
    }

    private void CrearTablaCubierta(Transform barco)
    {
        GameObject tabla = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(tabla.GetComponent<Collider>());
        tabla.name = "TablaCubierta";
        tabla.transform.SetParent(barco, false);
        tabla.transform.localPosition = new Vector3(0f, 1.15f, 1.55f);
        tabla.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        tabla.transform.localScale = new Vector3(1.4f, 0.9f, 0.08f);
        tabla.GetComponent<Renderer>().material = MaterialBarco.Crear(new Color(0.38f, 0.24f, 0.12f));

        GameObject lienzoGo = new GameObject("Marcador");
        lienzoGo.transform.SetParent(tabla.transform, false);
        lienzoGo.transform.localPosition = new Vector3(0f, 0f, -0.6f);
        lienzoGo.transform.localRotation = Quaternion.identity;
        lienzoGo.transform.localScale = Vector3.one * 0.008f;

        Canvas lienzo = lienzoGo.AddComponent<Canvas>();
        lienzo.renderMode = RenderMode.WorldSpace;
        RectTransform rect = lienzoGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160f, 90f);

        GameObject relleno = new GameObject("Vida");
        relleno.transform.SetParent(lienzoGo.transform, false);
        rellenoVida = relleno.AddComponent<Image>();
        rellenoVida.color = new Color(0.2f, 0.72f, 0.32f);
        RectTransform vidaRect = relleno.GetComponent<RectTransform>();
        vidaRect.anchorMin = new Vector2(0.08f, 0.62f);
        vidaRect.anchorMax = new Vector2(0.92f, 0.82f);
        vidaRect.offsetMin = Vector2.zero;
        vidaRect.offsetMax = Vector2.zero;

        GameObject textoGo = new GameObject("Texto");
        textoGo.transform.SetParent(lienzoGo.transform, false);
        textoCubierta = textoGo.AddComponent<Text>();
        textoCubierta.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoCubierta.fontSize = 18;
        textoCubierta.alignment = TextAnchor.MiddleCenter;
        textoCubierta.color = new Color(0.95f, 0.9f, 0.75f);
        RectTransform textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0.05f, 0.05f);
        textoRect.anchorMax = new Vector2(0.95f, 0.55f);
        textoRect.offsetMin = Vector2.zero;
        textoRect.offsetMax = Vector2.zero;

        barcoJugador.AlRecibirDanio += (_, __) => RefrescarTabla();
        RefrescarTabla();
    }

    private void RefrescarTabla()
    {
        if (textoCubierta == null || barcoJugador == null)
            return;

        float proporcion = barcoJugador.Vida / barcoJugador.VidaMaxima;
        if (rellenoVida != null)
        {
            rellenoVida.rectTransform.anchorMax = new Vector2(Mathf.Lerp(0.08f, 0.92f, proporcion), 0.82f);
            rellenoVida.color = Color.Lerp(new Color(0.7f, 0.15f, 0.1f), new Color(0.2f, 0.72f, 0.32f), proporcion);
        }

        textoCubierta.text = "Oleada " + oleada + "\nEnemigos " + enemigosVivos.Count;
    }

    private void Terminar(bool victoria)
    {
        if (terminada)
            return;

        terminada = true;
        if (textoCubierta != null)
            textoCubierta.text = victoria ? "Victoria" : "Barco hundido";

        Invoke(nameof(VolverAlMenu), 5f);
    }

    private static void Instanciar(GameObject efecto, Vector3 punto)
    {
        if (efecto == null)
            return;

        GameObject instancia = Instantiate(efecto, punto, Quaternion.identity);
        instancia.SetActive(true);
        ParticleSystem sistema = instancia.GetComponent<ParticleSystem>();
        if (sistema != null)
            sistema.Play(true);
        Destroy(instancia, 4f);
    }

    private static GameObject CrearEfectoFuego()
    {
        GameObject raiz = new GameObject("FuegoHumo");
        raiz.SetActive(false);
        ParticleSystem fuego = raiz.AddComponent<ParticleSystem>();
        ConfigurarParticulas(fuego, new Color(1f, 0.45f, 0.1f), 0.45f, 3.5f, 18);
        ParticleSystem humo = new GameObject("Humo").AddComponent<ParticleSystem>();
        humo.transform.SetParent(raiz.transform, false);
        humo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        ConfigurarParticulas(humo, new Color(0.25f, 0.25f, 0.25f, 0.7f), 1.1f, 1.6f, 10);
        raiz.SetActive(false);
        return raiz;
    }

    private static GameObject CrearEfectoAgua()
    {
        GameObject raiz = new GameObject("Salpicadura");
        ParticleSystem sistema = raiz.AddComponent<ParticleSystem>();
        ConfigurarParticulas(sistema, new Color(0.75f, 0.9f, 1f), 0.35f, 5f, 22);
        var shape = sistema.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        raiz.SetActive(false);
        return raiz;
    }

    private static void ConfigurarParticulas(ParticleSystem sistema, Color color, float vida, float velocidad, short cantidad)
    {
        sistema.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var principal = sistema.main;
        principal.playOnAwake = false;
        principal.loop = false;
        principal.duration = 0.4f;
        principal.startLifetime = vida;
        principal.startSpeed = velocidad;
        principal.startSize = 0.28f;
        principal.startColor = color;
        principal.gravityModifier = 0.6f;

        var emision = sistema.emission;
        emision.SetBursts(new[] { new ParticleSystem.Burst(0f, cantidad) });

        ParticleSystemRenderer render = sistema.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader != null)
            render.material = new Material(shader);
    }
}
