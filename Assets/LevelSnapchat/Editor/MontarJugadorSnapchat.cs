using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MontarJugadorSnapchat
{
    private const string RutaEscena = "Assets/LevelSnapchat/Scenes/LevelSnapchat.unity";
    private const string RutaSprite = "Assets/TutorialGoogle/Sprites/JugadorTutorial.png";
    private const string RutaFantasma = "Assets/LevelSnapchat/Sprites/fantasma.png";
    private const string NombreVentana = "VentanaSnap";
    private const string NombreMundo = "MundoSnap";
    private const string NombreJugador = "JugadorSnap";
    private const string NombreSecuencia = "SecuenciaSnap";
    private const string NombreGestor = "GestorDesafios";

    static MontarJugadorSnapchat()
    {
        EditorApplication.delayCall += AplicarSiProcede;
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += AplicarSiProcede;
        };
    }

    [MenuItem("Tools/Snapchat/Montar jugador del tutorial")]
    public static void AplicarMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene escena = SceneManager.GetActiveScene();
        if (escena.path != RutaEscena)
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);

        if (Aplicar(escena))
        {
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.Refresh();
            Debug.Log("SNAP_JUGADOR_OK: jugador del tutorial montado en LevelSnapchat.");
        }
        else
        {
            Debug.Log("SNAP_JUGADOR: la escena ya tiene el jugador montado.");
        }
    }

    private static void AplicarSiProcede()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (BuildPipeline.isBuildingPlayer) return;
        if (!File.Exists(RutaEscena)) return;

        try
        {
            string texto = File.ReadAllText(RutaEscena);
            if (texto.Contains(NombreVentana) && texto.Contains(NombreGestor)) return;
        }
        catch
        {
            return;
        }

        Scene escena = SceneManager.GetSceneByPath(RutaEscena);
        bool abierta = escena.IsValid() && escena.isLoaded;
        bool aditiva = false;

        if (!abierta)
        {
            escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Additive);
            aditiva = true;
        }

        if (!escena.IsValid()) return;

        bool cambiada = Aplicar(escena);

        if (cambiada)
        {
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("SNAP_JUGADOR_OK: jugador del tutorial montado automáticamente.");
        }

        if (aditiva)
            EditorSceneManager.CloseScene(escena, true);

        if (cambiada)
            AssetDatabase.Refresh();
    }

    private static bool Aplicar(Scene escena)
    {
        if (!escena.IsValid()) return false;

        SnapWebApp snap = BuscarComponente<SnapWebApp>(escena);
        if (snap == null)
        {
            Debug.LogWarning("SNAP_JUGADOR: no encontré SnapWebApp en LevelSnapchat.");
            return false;
        }

        Canvas canvas = snap.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("SNAP_JUGADOR: no encontré el Canvas principal.");
            return false;
        }

        bool cambiada = false;
        Transform canvasT = canvas.transform;
        RectTransform ventana = AsegurarVentana(canvasT, ref cambiada);
        Transform raizUI = ventana != null ? (Transform)ventana : canvasT;

        Transform sidebar = raizUI.Find("Sidebar");
        Transform navHistorias = sidebar != null ? sidebar.Find("Nav_Historias") : null;
        Transform lista = sidebar != null ? sidebar.Find("Lista_Chats/Viewport/Content") : null;
        Transform main = raizUI.Find("Main");
        Transform panelHistorias = main != null ? main.Find("Body/Historias_Panel") : null;
        Transform primeraHistoria = panelHistorias != null ? panelHistorias.Find("Historia_PLAYA") : null;

        if (navHistorias == null || lista == null || main == null || panelHistorias == null || primeraHistoria == null)
        {
            Debug.LogWarning("SNAP_JUGADOR: falta Sidebar, Nav_Historias, Lista_Chats, Main o Historias_Panel.");
            return false;
        }

        Transform inicio = lista.Find("Conv_Jesus");
        if (inicio == null && lista.childCount > 0) inicio = lista.GetChild(lista.childCount - 1);

        RectTransform mundo = raizUI.Find(NombreMundo) as RectTransform;
        if (mundo == null)
        {
            mundo = CrearMundo(raizUI);
            cambiada = true;
        }

        JugadorTutorial jugador = mundo.GetComponentInChildren<JugadorTutorial>(true);
        if (jugador == null)
        {
            RectTransform jugadorRT = CrearJugador(mundo, inicio);
            jugador = jugadorRT.GetComponent<JugadorTutorial>();
            cambiada = true;
        }

        for (int i = 0; i < lista.childCount; i++)
        {
            Transform fila = lista.GetChild(i);
            if (!fila.name.StartsWith("Conv_")) continue;

            bool nueva;
            AsegurarPlataforma(fila, "Contacto " + fila.name.Replace("Conv_", ""), out nueva);
            if (nueva) cambiada = true;
        }

        bool nuevaBoton;
        PlataformaUI plataformaBoton = AsegurarPlataforma(navHistorias, "Historias", out nuevaBoton);
        if (nuevaBoton) cambiada = true;

        GestorDesafiosSnap gestor = AsegurarGestor(canvasT, ref cambiada);
        RectTransform[] historias = new RectTransform[3];
        historias[0] = panelHistorias.Find("Historia_PLAYA") as RectTransform;
        historias[1] = panelHistorias.Find("Historia_FUEGO") as RectTransform;
        historias[2] = panelHistorias.Find("Historia_CIUDAD") as RectTransform;

        if (gestor != null)
        {
            gestor.ventana = ventana;
            gestor.panelPrincipal = main as RectTransform;
            gestor.historias = historias;

            Sprite spriteFantasma = AssetDatabase.LoadAssetAtPath<Sprite>(RutaFantasma);
            if (spriteFantasma == null)
                Debug.LogError("SNAP_JUGADOR: no se pudo cargar " + RutaFantasma + ".");
            else if (gestor.spriteFantasma != spriteFantasma)
            {
                gestor.spriteFantasma = spriteFantasma;
                EditorUtility.SetDirty(gestor);
                cambiada = true;
            }
        }

        SecuenciaHistoriasSnap secuencia = AsegurarSecuencia(raizUI, ref cambiada);

        if (secuencia != null)
        {
            secuencia.jugador = jugador;
            secuencia.plataformaBoton = plataformaBoton;
            secuencia.botonHistorias = navHistorias.GetComponent<Button>();
            secuencia.snap = snap;
            secuencia.mundo = mundo;
            secuencia.primeraHistoria = historias[0];
            secuencia.contactoInicial = (RectTransform)inicio;
            secuencia.gestor = gestor;
        }

        DesactivarJugadorAntiguo(escena);

        return cambiada;
    }

    private static RectTransform AsegurarVentana(Transform canvasT, ref bool cambiada)
    {
        Transform existente = canvasT.Find(NombreVentana);
        RectTransform ventana;

        if (existente == null)
        {
            GameObject go = new GameObject(NombreVentana, typeof(RectTransform));
            go.layer = 5;

            ventana = go.GetComponent<RectTransform>();
            ventana.SetParent(canvasT, false);
            ventana.anchorMin = Vector2.zero;
            ventana.anchorMax = Vector2.one;
            ventana.pivot = Vector2.zero;
            ventana.anchoredPosition = Vector2.zero;
            ventana.sizeDelta = Vector2.zero;
            ventana.localScale = Vector3.one;

            cambiada = true;
        }
        else
        {
            ventana = (RectTransform)existente;
        }

        List<Transform> hijos = new List<Transform>();

        for (int i = 0; i < canvasT.childCount; i++)
        {
            Transform hijo = canvasT.GetChild(i);
            if (hijo == ventana) continue;
            if (hijo.name == NombreGestor) continue;
            hijos.Add(hijo);
        }

        for (int i = 0; i < hijos.Count; i++)
        {
            hijos[i].SetParent(ventana, false);
            cambiada = true;
        }

        ventana.SetAsFirstSibling();
        return ventana;
    }

    private static GestorDesafiosSnap AsegurarGestor(Transform canvasT, ref bool cambiada)
    {
        Transform existente = canvasT.Find(NombreGestor);
        GestorDesafiosSnap gestor;

        if (existente == null)
        {
            GameObject go = new GameObject(NombreGestor);
            go.transform.SetParent(canvasT, false);
            gestor = go.AddComponent<GestorDesafiosSnap>();
            cambiada = true;
        }
        else
        {
            gestor = existente.GetComponent<GestorDesafiosSnap>();

            if (gestor == null)
            {
                gestor = existente.gameObject.AddComponent<GestorDesafiosSnap>();
                cambiada = true;
            }
        }

        return gestor;
    }

    private static SecuenciaHistoriasSnap AsegurarSecuencia(Transform padre, ref bool cambiada)
    {
        Transform existente = padre.Find(NombreSecuencia);
        SecuenciaHistoriasSnap secuencia;

        if (existente == null)
        {
            GameObject go = new GameObject(NombreSecuencia, typeof(RectTransform), typeof(SecuenciaHistoriasSnap));
            go.layer = 5;
            go.transform.SetParent(padre, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            secuencia = go.GetComponent<SecuenciaHistoriasSnap>();
            cambiada = true;
        }
        else
        {
            secuencia = existente.GetComponent<SecuenciaHistoriasSnap>();

            if (secuencia == null)
            {
                secuencia = existente.gameObject.AddComponent<SecuenciaHistoriasSnap>();
                cambiada = true;
            }
        }

        return secuencia;
    }

    private static RectTransform CrearMundo(Transform padre)
    {
        GameObject go = new GameObject(NombreMundo, typeof(RectTransform));
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();

        return rt;
    }

    private static RectTransform CrearJugador(RectTransform mundo, Transform inicio)
    {
        Vector2 posicion = new Vector2(60f, 256f);

        if (inicio != null)
        {
            Canvas.ForceUpdateCanvases();
            Rect r = PlataformaUI.RectDe(mundo, (RectTransform)inicio);

            if (r.width > 50f && r.height > 10f &&
                r.center.x > 0f && r.center.x < 600f &&
                r.center.y > 0f && r.center.y < 1080f)
                posicion = new Vector2(r.center.x - 36f, r.yMax + 2f);
        }

        GameObject go = new GameObject(NombreJugador, typeof(RectTransform));
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(mundo, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.sizeDelta = new Vector2(72f, 96f);
        rt.anchoredPosition = posicion;

        GameObject spriteGO = new GameObject("Sprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        spriteGO.layer = 5;

        RectTransform spriteRT = spriteGO.GetComponent<RectTransform>();
        spriteRT.SetParent(rt, false);
        spriteRT.anchorMin = Vector2.zero;
        spriteRT.anchorMax = Vector2.one;
        spriteRT.pivot = new Vector2(0.5f, 0.5f);
        spriteRT.anchoredPosition = Vector2.zero;
        spriteRT.sizeDelta = Vector2.zero;

        Image sprite = spriteGO.GetComponent<Image>();
        sprite.sprite = CargarSprite(RutaSprite);
        sprite.color = Color.white;
        sprite.preserveAspect = true;
        sprite.raycastTarget = false;

        JugadorTutorial jugador = go.AddComponent<JugadorTutorial>();
        jugador.mundo = mundo;
        jugador.sprite = sprite;
        jugador.velocidad = 620f;
        jugador.fuerzaSalto = 1200f;
        jugador.gravedad = 2300f;
        jugador.alturaSuelo = 48f;
        jugador.posicionInicial = posicion;
        jugador.usarClick = true;
        jugador.usarGatillo = true;
        jugador.nombreEjeGatillo = "Fire3";
        jugador.alcanceGolpe = 130f;
        jugador.duracionGolpe = 0.22f;
        jugador.esperaEntreGolpes = 0.35f;
        jugador.golpearPlataformaActual = true;

        return rt;
    }

    private static PlataformaUI AsegurarPlataforma(Transform t, string etiqueta, out bool nueva)
    {
        nueva = false;
        if (t == null) return null;

        PlataformaUI plataforma = t.GetComponent<PlataformaUI>();

        if (plataforma == null)
        {
            plataforma = t.gameObject.AddComponent<PlataformaUI>();
            nueva = true;
        }

        plataforma.etiqueta = etiqueta;
        plataforma.esExtension = false;
        plataforma.indiceExtension = 0;

        return plataforma;
    }

    private static void DesactivarJugadorAntiguo(Scene escena)
    {
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.name != "Jugador") continue;
            if (raiz.GetComponent<JugadorMovimiento>() == null) continue;
            raiz.SetActive(false);
        }
    }

    private static T BuscarComponente<T>(Scene escena) where T : Component
    {
        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            T componente = raiz.GetComponentInChildren<T>(true);
            if (componente != null) return componente;
        }

        return null;
    }

    private static Sprite CargarSprite(string ruta)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);

        if (s == null)
        {
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
            s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }

        if (s == null) Debug.LogWarning("SNAP_JUGADOR: falta el sprite " + ruta);
        return s;
    }
}
