using System;
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
    [SerializeField] private GameObject modeloJugador;
    [SerializeField] private GameObject modeloPirata;
    [SerializeField] private Texture2D texturaAgua;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoImpacto;
    [SerializeField] private GameObject controlDerecho;
    [SerializeField] private GameObject controlIzquierdo;

    [Header("Oleadas")]
    [SerializeField] private string escenaAlSalir = "MainMenu";
    [Tooltip("Cuántos barcos enemigos salen a la vez, en abanico frente a la proa.")]
    [SerializeField] private int barcosPorOleada = 10;
    [Tooltip("Distancia a la que un enemigo activa la salida de la flota aliada.")]
    [SerializeField] private float radioDespliegue = 82f;
    [Tooltip("Barcos aliados que salen del costado al cruzar ese radio.")]
    [SerializeField] private int barcosAliados = 5;
    [Tooltip("Velocidad con la que los aliados salen de la base y entran al frente.")]
    [SerializeField] private float velocidadDespliegue = 7.5f;
    [SerializeField] private float vidaJugador = 140f;
    [SerializeField] private float vidaBaseEnemigo = 80f;

    private readonly List<BarcoSalud> enemigosVivos = new List<BarcoSalud>();
    private readonly List<BarcoSalud> aliadosVivos = new List<BarcoSalud>();
    private BarcoSalud barcoJugador;
    private Text textoCubierta;
    private Image rellenoVida;
    private GameObject efectoFuego;
    private GameObject efectoAgua;
    private IslandSpawner islas;
    private VRNavalCombat naval;
    private VRAssuranceAgent agente;
    private int oleada = 1;
    private int aliadosDestruidos;
    private bool terminada;
    private bool aliadosDesplegados;

    private void Start()
    {
        Paso("plano", DesactivarPlanoViejo);
        Paso("combate", () => naval = gameObject.AddComponent<VRNavalCombat>());
        Paso("atardecer", () => { if (naval != null) naval.AplicarAtardecer(); });
        Paso("fuego", () => efectoFuego = CrearEfectoFuego());
        Paso("agua", () => efectoAgua = CrearEfectoAgua());
        Paso("mar", CrearMar);
        Paso("islas", () => { if (naval != null) islas = naval.CrearIslas(7); });
        Paso("controles", AsegurarControlesVR);

        Transform barco = null;
        Paso("barco", () => barco = CrearBarcoJugador());
        Paso("cubierta", () => { if (naval != null && barco != null && modeloJugador == null) naval.VestirCubierta(barco); });
        Paso("tabla", () => { if (barco != null) CrearTablaCubierta(barco); });
        Paso("entorno", AgruparEntorno);

        StartCoroutine(ArranqueXR.IniciarSiHayCasco());
        agente = gameObject.AddComponent<VRAssuranceAgent>();
        StartCoroutine(LanzarOleada());
        StartCoroutine(VigilarProximidad());
        Debug.Log("EscenaVR: batalla lista. Entorno, BarcoJugador y oleada ya están en la jerarquía.");
    }

    private static void Paso(string nombre, System.Action accion)
    {
        try
        {
            accion();
        }
        catch (Exception excepcion)
        {
            Debug.LogError("EscenaVR se detuvo al crear " + nombre + ".");
            Debug.LogException(excepcion);
        }
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
        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.4f, 1.5f, 8.5f);
        caja.center = new Vector3(0f, 0.8f, 0f);

        Vector3 cubiertaLocal = new Vector3(0f, 1.45f, -1.1f);
        Vector3 canonIzquierda = new Vector3(-1.45f, 1.2f, 1.1f);
        Vector3 canonDerecha = new Vector3(1.45f, 1.2f, 1.1f);
        if (modeloJugador != null)
            ColocarBarcoEstilizado(raiz.transform, caja, ref cubiertaLocal, ref canonIzquierda, ref canonDerecha);
        else
            ColocarCasco(raiz.transform, 10f, new Color(0.32f, 0.2f, 0.1f));

        barcoJugador = raiz.AddComponent<BarcoSalud>();
        barcoJugador.Configurar(vidaJugador, 0, true);
        ShipHealth vida = raiz.AddComponent<ShipHealth>();
        vida.Configurar(barcoJugador);
        barcoJugador.AlRecibirDanio += (_, __) =>
        {
            HapticsUtility.SendHapticImpulse(0.85f, 0.18f, HapticsUtility.Controller.Both);
            Instanciar(efectoFuego, raiz.transform.position + Vector3.up * 1.2f);
        };
        barcoJugador.AlHundirse += _ => Terminar(false);

        GameObject cubierta = new GameObject("Cubierta");
        cubierta.transform.SetParent(raiz.transform, false);
        cubierta.transform.localPosition = cubiertaLocal;
        Vector3 vista = VistaEnCubierta(raiz.transform, cubierta.transform, out Vector3 puestoPirata);
        if (modeloPirata != null)
            ColocarPirata(cubierta.transform, puestoPirata);

        GameObject origen = GameObject.Find("XR Origin (VR)");
        if (origen != null)
        {
            origen.transform.SetParent(cubierta.transform, false);
            origen.transform.localPosition = vista;
            origen.transform.localRotation = Quaternion.identity;
        }

        OceanMover agua = FindAnyObjectByType<OceanMover>();
        if (agua != null)
            agua.Configurar(texturaAgua, raiz.transform);

        CrearCanonJugador(raiz.transform, canonIzquierda);
        CrearCanonJugador(raiz.transform, canonDerecha);
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
        if (barcoJugador == null)
        {
            Debug.LogError("EscenaVR: el barco del jugador no quedó creado, así que no salen enemigos.");
            yield break;
        }

        while (!terminada)
        {
            int cantidad = Mathf.Max(1, barcosPorOleada);
            for (int i = 0; i < cantidad; i++)
                CrearEnemigo(i, cantidad);

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

    private void CrearEnemigo(int indice, int total)
    {
        AsegurarModeloHolandes();

        GameObject raiz = new GameObject("Enemigo oleada " + oleada);
        Vector3 proa = barcoJugador.transform.forward;
        proa.y = 0f;
        if (proa.sqrMagnitude < 0.01f)
            proa = Vector3.forward;
        proa.Normalize();
        float arco = 86f;
        float angulo = total <= 1 ? 0f : Mathf.Lerp(-arco * 0.5f, arco * 0.5f, indice / (total - 1f));
        Vector3 direccion = Quaternion.AngleAxis(angulo, Vector3.up) * proa;
        float distancia = 125f + Mathf.Abs(angulo) * 0.15f + (oleada - 1) * 18f;
        raiz.transform.position = barcoJugador.transform.position + direccion * distancia;
        raiz.transform.rotation = Quaternion.LookRotation(-proa, Vector3.up);

        ColocarCasco(raiz.transform, 8.5f, new Color(0.28f, 0.1f, 0.1f));
        Transform cascoVisual = raiz.transform.Find("Casco");
        if (cascoVisual != null)
        {
            cascoVisual.localRotation = Quaternion.Euler(0f, 90f, 0f);
            cascoVisual.name = "dutch-ship-medium";
            PintarUrp(cascoVisual.gameObject);
        }

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.2f, 2.2f, 8.5f);
        caja.center = new Vector3(0f, 1.1f, 0f);

        BarcoSalud salud = raiz.AddComponent<BarcoSalud>();
        salud.Configurar(100f, 1, false);
        ShipHealth vida = raiz.AddComponent<ShipHealth>();
        vida.Configurar(salud);
        salud.AlHundirse += AlHundirEnemigo;
        salud.AlRecibirDanio += (_, __) => Instanciar(efectoFuego, raiz.transform.position + Vector3.up * 2f);
        enemigosVivos.Add(salud);

        VRShipCannon canon = ConstruirCanon(raiz.transform, new Vector3(0f, 1.6f, 3.2f), false);
        canon.Configurar(salud, false, 16f, sonidoDisparo, sonidoImpacto, efectoFuego, efectoAgua);

        EnemyShipAI ia = raiz.AddComponent<EnemyShipAI>();
        ia.Configurar(barcoJugador.transform, canon, 0f, oleada);
    }

    private void AsegurarModeloHolandes()
    {
        if (modeloBarco != null && modeloBarco.name.IndexOf("dutch", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return;

#if UNITY_EDITOR
        GameObject dutch = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/recurso de pirata/dutch-ship-medium/dutch-ship-medium.obj");
        if (dutch != null)
            modeloBarco = dutch;
#endif
    }

    private void AlHundirEnemigo(BarcoSalud barco)
    {
        enemigosVivos.Remove(barco);
        RefrescarTabla();
    }

    private IEnumerator VigilarProximidad()
    {
        float proximoInforme = 0f;
        while (!terminada)
        {
            if (!aliadosDesplegados && barcoJugador != null)
            {
                float cerca = DistanciaDelEnemigoMasCercano();
                if (cerca > 0.5f && cerca <= radioDespliegue)
                    DesplegarAliados(cerca);
            }

            if (Time.time >= proximoInforme)
            {
                proximoInforme = Time.time + 3f;
                if (agente != null)
                    agente.Informe(enemigosVivos.Count, aliadosVivos.Count, aliadosDestruidos, barcoJugador);
            }

            yield return new WaitForSeconds(0.4f);
        }
    }

    private float DistanciaDelEnemigoMasCercano()
    {
        float mejor = float.MaxValue;
        Vector3 origen = barcoJugador.transform.position;
        for (int i = enemigosVivos.Count - 1; i >= 0; i--)
        {
            BarcoSalud enemigo = enemigosVivos[i];
            if (enemigo == null || enemigo.EstaHundido)
                continue;
            float distancia = Vector3.Distance(origen, enemigo.transform.position);
            if (distancia < mejor)
                mejor = distancia;
        }

        return mejor == float.MaxValue ? 0f : mejor;
    }

    private void DesplegarAliados(float distancia)
    {
        aliadosDesplegados = true;
        int total = Mathf.Max(1, barcosAliados);
        for (int i = 0; i < total; i++)
            CrearAliado(i, total);

        if (agente != null)
            agente.RegistrarDespliegue(distancia, total);
    }

    private void CrearAliado(int indice, int total)
    {
        AsegurarModeloHolandes();
        GameObject raiz = new GameObject("Aliado " + (indice + 1));
        Vector3 proa = barcoJugador.transform.forward;
        proa.y = 0f;
        if (proa.sqrMagnitude < 0.01f)
            proa = Vector3.forward;
        proa.Normalize();
        Vector3 costado = Vector3.Cross(Vector3.up, proa);
        float fila = indice - (total - 1) * 0.5f;
        raiz.transform.position = barcoJugador.transform.position - proa * 18f + costado * (34f + fila * 14f);
        raiz.transform.rotation = Quaternion.LookRotation(proa, Vector3.up);

        ColocarCasco(raiz.transform, 8f, new Color(0.12f, 0.22f, 0.38f));
        Transform cascoVisual = raiz.transform.Find("Casco");
        if (cascoVisual != null)
        {
            cascoVisual.localRotation = Quaternion.Euler(0f, 90f, 0f);
            cascoVisual.name = "dutch-ship-medium";
            PintarUrp(cascoVisual.gameObject);
        }

        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.size = new Vector3(3.2f, 2.2f, 8.5f);
        caja.center = new Vector3(0f, 1.1f, 0f);

        BarcoSalud salud = raiz.AddComponent<BarcoSalud>();
        salud.Configurar(100f, 0, false);
        ShipHealth vida = raiz.AddComponent<ShipHealth>();
        vida.Configurar(salud);
        salud.AlHundirse += AlHundirAliado;
        salud.AlRecibirDanio += (_, __) => Instanciar(efectoFuego, raiz.transform.position + Vector3.up * 2f);
        aliadosVivos.Add(salud);

        VRShipCannon canon = ConstruirCanon(raiz.transform, new Vector3(0f, 1.6f, 3.2f), false);
        canon.Configurar(salud, false, 16f, sonidoDisparo, sonidoImpacto, efectoFuego, efectoAgua);

        Vector3 reunion = barcoJugador.transform.position + proa * 36f + costado * (fila * 12f);
        EnemyShipAI ia = raiz.AddComponent<EnemyShipAI>();
        ia.ConfigurarAliado(barcoJugador.transform, canon, reunion, velocidadDespliegue);
    }

    private void AlHundirAliado(BarcoSalud barco)
    {
        aliadosVivos.Remove(barco);
        aliadosDestruidos++;
        if (agente != null && barcoJugador != null)
            agente.Informe(enemigosVivos.Count, aliadosVivos.Count, aliadosDestruidos, barcoJugador);
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

    private void ColocarBarcoEstilizado(Transform raiz, BoxCollider caja, ref Vector3 cubiertaLocal, ref Vector3 canonIzquierda, ref Vector3 canonDerecha)
    {
        GameObject visual = Instantiate(modeloJugador, raiz);
        visual.name = "ModeloJugador";
        visual.transform.localRotation = Quaternion.identity;
        foreach (Collider colision in visual.GetComponentsInChildren<Collider>())
            Destroy(colision);
        PintarUrp(visual);

        Renderer[] piezas = visual.GetComponentsInChildren<Renderer>();
        if (piezas.Length == 0)
            return;

        Bounds medida = Encapsular(piezas);
        float largo = Mathf.Max(medida.size.x, medida.size.z);
        if (largo > 0.01f)
            visual.transform.localScale *= 10f / largo;

        medida = Encapsular(piezas);
        visual.transform.position += new Vector3(raiz.position.x - medida.center.x, -medida.min.y, raiz.position.z - medida.center.z);

        Bounds casco = Encapsular(piezas);
        caja.center = raiz.InverseTransformPoint(casco.center);
        caja.size = new Vector3(casco.size.x, Mathf.Clamp(casco.size.y, 1f, 3f), casco.size.z);

        Transform suelo = null;
        foreach (Transform hijo in visual.GetComponentsInChildren<Transform>())
        {
            if (hijo.name.Contains("FloorMid"))
            {
                suelo = hijo;
                break;
            }
        }

        Renderer tabla = suelo != null ? suelo.GetComponent<Renderer>() : null;
        if (tabla == null)
            return;

        Bounds superficie = tabla.bounds;
        cubiertaLocal = raiz.InverseTransformPoint(new Vector3(superficie.center.x, superficie.max.y + 0.05f, superficie.center.z - superficie.size.z * 0.2f));
        float margen = Mathf.Max(0.4f, superficie.size.x * 0.28f);
        float avance = superficie.size.z * 0.22f;
        float altura = superficie.max.y + 0.15f;
        canonIzquierda = raiz.InverseTransformPoint(new Vector3(superficie.center.x - margen, altura, superficie.center.z + avance));
        canonDerecha = raiz.InverseTransformPoint(new Vector3(superficie.center.x + margen, altura, superficie.center.z + avance));
    }

    private void ColocarPirata(Transform cubierta, Vector3 puesto)
    {
        GameObject pirata = Instantiate(modeloPirata, cubierta);
        pirata.name = "Pirate_Full";
        pirata.SetActive(true);
        foreach (Camera camara in pirata.GetComponentsInChildren<Camera>(true))
            camara.enabled = false;
        foreach (AudioListener oido in pirata.GetComponentsInChildren<AudioListener>(true))
            oido.enabled = false;
        foreach (Light luz in pirata.GetComponentsInChildren<Light>(true))
            luz.enabled = false;
        Animator animador = pirata.GetComponentInChildren<Animator>();
        if (animador != null)
            animador.enabled = false;
        PintarUrp(pirata);

        Renderer[] piezas = pirata.GetComponentsInChildren<Renderer>();
        if (piezas.Length == 0)
            return;

        Bounds cuerpo = Encapsular(piezas);
        if (cuerpo.size.y > 0.01f)
            pirata.transform.localScale *= 1.7f / cuerpo.size.y;
        cuerpo = Encapsular(piezas);
        Vector3 pies = cubierta.InverseTransformPoint(new Vector3(cuerpo.center.x, cuerpo.min.y, cuerpo.center.z));
        pirata.transform.localPosition += new Vector3(puesto.x - pies.x, -pies.y, puesto.z - pies.z);
        pirata.transform.localRotation = Quaternion.identity;
        StartCoroutine(ReposoPirata(pirata.transform));
    }

    private IEnumerator ReposoPirata(Transform pirata)
    {
        Transform pecho = BuscarHueso(pirata, "spine_02");
        Transform cabeza = BuscarHueso(pirata, "head");
        Transform brazoIzq = BuscarHueso(pirata, "upperarm_l");
        Transform brazoDer = BuscarHueso(pirata, "upperarm_r");
        Quaternion basePecho = Rotacion(pecho);
        Quaternion baseCabeza = Rotacion(cabeza);
        PoseTierraAVista(pirata, cabeza, brazoIzq, brazoDer);
        Quaternion baseBrazoIzq = Rotacion(brazoIzq);
        Quaternion baseBrazoDer = Rotacion(brazoDer);
        float tiempo = 0f;

        while (pirata != null)
        {
            tiempo += Time.deltaTime;
            float respirar = Mathf.Sin(tiempo * 1.4f);
            float balanceo = Mathf.Sin(tiempo * 0.55f);
            Girar(pecho, basePecho, Vector3.right, respirar * 2.5f);
            Girar(cabeza, baseCabeza, Vector3.up, Mathf.Sin(tiempo * 0.32f) * 6f);
            Girar(brazoIzq, baseBrazoIzq, Vector3.right, balanceo * 1.5f);
            Girar(brazoDer, baseBrazoDer, Vector3.right, -balanceo * 1.5f);
            yield return null;
        }
    }

    private static Vector3 VistaEnCubierta(Transform barco, Transform cubierta, out Vector3 puestoPirata)
    {
        Renderer palo = RendererDe(barco, "StylShip_MastMid");
        Renderer tabla = RendererDe(barco, "StylShip_FloorMid");
        Renderer paloProa = RendererDe(barco, "StylShip_MastFront");
        Renderer pisoProa = RendererDe(barco, "StylShip_FloorFront");
        if (palo == null || tabla == null || paloProa == null || pisoProa == null)
        {
            puestoPirata = new Vector3(0.3f, 0.28f, 3.53f);
            return new Vector3(0.45f, 0f, -0.8f);
        }

        Vector3 centroPalo = palo.bounds.center;
        Vector3 vista = cubierta.InverseTransformPoint(new Vector3(centroPalo.x + 0.45f, cubierta.position.y, tabla.bounds.min.z + 0.08f));
        vista.y = 0f;
        Vector3 centroProa = paloProa.bounds.center;
        puestoPirata = cubierta.InverseTransformPoint(new Vector3(centroProa.x + 0.3f, pisoProa.bounds.max.y - 0.06f, centroProa.z + 0.72f));
        return vista;
    }

    private static Renderer RendererDe(Transform barco, string nombre)
    {
        Transform pieza = BuscarHueso(barco, nombre);
        return pieza != null ? pieza.GetComponent<Renderer>() : null;
    }

    private static void PoseTierraAVista(Transform pirata, Transform cabeza, Transform brazoIzq, Transform brazoDer)
    {
        Transform antebrazoIzq = BuscarHueso(pirata, "lowerarm_l");
        Transform antebrazoDer = BuscarHueso(pirata, "lowerarm_r");
        Transform manoIzq = BuscarHueso(pirata, "hand_l");
        Transform manoDer = BuscarHueso(pirata, "hand_r");
        if (cabeza == null || brazoIzq == null || brazoDer == null || antebrazoIzq == null || antebrazoDer == null || manoIzq == null || manoDer == null)
            return;

        Vector3 senal = pirata.TransformDirection(new Vector3(0.3f, 0.35f, 0.9f).normalized);
        Apuntar(brazoDer, antebrazoDer.position, senal);
        Apuntar(antebrazoDer, manoDer.position, senal);

        float largoBrazo = Vector3.Distance(brazoIzq.position, antebrazoIzq.position);
        float largoAntebrazo = Vector3.Distance(antebrazoIzq.position, manoIzq.position);
        Vector3 visera = cabeza.position + pirata.forward * 0.16f + pirata.up * 0.12f - pirata.right * 0.04f;
        Vector3 hombro = brazoIzq.position;
        Vector3 eje = (visera - hombro).normalized;
        float distancia = Mathf.Min(Vector3.Distance(hombro, visera), (largoBrazo + largoAntebrazo) * 0.999f);
        Vector3 polo = Vector3.ProjectOnPlane(-pirata.right - pirata.up * 0.3f, eje).normalized;
        float coseno = (largoBrazo * largoBrazo + distancia * distancia - largoAntebrazo * largoAntebrazo) / (2f * largoBrazo * distancia);
        float angulo = Mathf.Acos(Mathf.Clamp(coseno, -1f, 1f));
        Vector3 codo = hombro + (eje * Mathf.Cos(angulo) + polo * Mathf.Sin(angulo)) * largoBrazo;
        Apuntar(brazoIzq, antebrazoIzq.position, codo - hombro);
        Apuntar(antebrazoIzq, manoIzq.position, visera - antebrazoIzq.position);
    }

    private static void Apuntar(Transform hueso, Vector3 hijo, Vector3 direccion)
    {
        hueso.rotation = Quaternion.FromToRotation(hijo - hueso.position, direccion) * hueso.rotation;
    }

    private static Quaternion Rotacion(Transform hueso)
    {
        return hueso != null ? hueso.localRotation : Quaternion.identity;
    }

    private static void Girar(Transform hueso, Quaternion baseRot, Vector3 ejeMundo, float grados)
    {
        if (hueso == null)
            return;

        Vector3 eje = hueso.parent != null ? hueso.parent.InverseTransformDirection(ejeMundo) : ejeMundo;
        hueso.localRotation = Quaternion.AngleAxis(grados, eje) * baseRot;
    }

    private static Transform BuscarHueso(Transform raiz, string nombre)
    {
        foreach (Transform hueso in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (hueso.name == nombre)
                return hueso;
        }

        return null;
    }

    private static Bounds Encapsular(Renderer[] piezas)
    {
        Bounds limites = piezas[0].bounds;
        for (int i = 1; i < piezas.Length; i++)
            limites.Encapsulate(piezas[i].bounds);
        return limites;
    }

    private static void PintarUrp(GameObject visual)
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
        if (urp == null)
            return;

        foreach (Renderer render in visual.GetComponentsInChildren<Renderer>(true))
        {
            Material[] originales = render.sharedMaterials;
            Material[] copia = new Material[originales.Length];
            for (int i = 0; i < originales.Length; i++)
            {
                Material fuente = originales[i];
                if (fuente != null && fuente.shader != null && fuente.shader.name.Contains("Universal"))
                {
                    copia[i] = fuente;
                    continue;
                }

                Material nuevo = new Material(urp);
                if (fuente != null && fuente.HasProperty("_MainTex"))
                    nuevo.SetTexture("_BaseMap", fuente.GetTexture("_MainTex"));
                if (fuente != null && fuente.HasProperty("_Color"))
                    nuevo.SetColor("_BaseColor", fuente.color);
                copia[i] = nuevo;
            }

            render.materials = copia;
        }
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
