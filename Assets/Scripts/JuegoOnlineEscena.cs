using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10000)]
public class JuegoOnlineEscena : MonoBehaviour
{
    public static Vector3 PuntoAparicion { get; private set; }
    public static bool HayPuntoAparicion { get; private set; }

    private static JuegoOnlineEscena instancia;
    private NetworkManager redActual;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        if (instancia != null) return;

        GameObject objeto = new GameObject("JuegoOnlineEscena");
        DontDestroyOnLoad(objeto);
        instancia = objeto.AddComponent<JuegoOnlineEscena>();
    }

    private void Update()
    {
        if (MultijugadorJuego.Instancia != null)
        {
            Debug.Log(
                "[ESCENA] Singleton=" + (NetworkManager.Singleton != null) +
                " | Red=" + (MultijugadorJuego.Instancia.Red != null)
            );
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
        MultijugadorJuego.AlCrearRed += RecibirRed;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        MultijugadorJuego.AlCrearRed -= RecibirRed;
    }

    private void RecibirRed(NetworkManager nuevaRed)
    {
        redActual = nuevaRed;

        Debug.Log("[Online] RED RECIBIDA: " + (redActual != null));
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();

        if (escena.name == "MainMenu")
        {
            HayPuntoAparicion = false;
            return;
        }

        StartCoroutine(EsperarRed());
    }

    private IEnumerator EsperarRed()
    {
        while (redActual == null || !redActual.IsListening)
            yield return null;

        yield return StartCoroutine(PrepararEscena());
    }

    private IEnumerator PrepararEscena()
    {
        yield return null;

        bool puntoGuardado = false;

        foreach (Spawn controlador in FindObjectsByType<Spawn>(FindObjectsInactive.Exclude))
        {
            if (EsJugadorDeRed(controlador.transform)) continue;

            if (!puntoGuardado)
            {
                PuntoAparicion = controlador.transform.position;
                HayPuntoAparicion = true;
                puntoGuardado = true;
            }

            controlador.gameObject.SetActive(false);
        }

        foreach (MovePlayer controlador in FindObjectsByType<MovePlayer>(FindObjectsInactive.Exclude))
        {
            if (EsJugadorDeRed(controlador.transform)) continue;

            if (!puntoGuardado)
            {
                PuntoAparicion = controlador.transform.position;
                HayPuntoAparicion = true;
                puntoGuardado = true;
            }

            controlador.gameObject.SetActive(false);
        }

        foreach (ControlJugador controlador in FindObjectsByType<ControlJugador>(FindObjectsInactive.Exclude))
        {
            if (EsJugadorDeRed(controlador.transform)) continue;

            if (!puntoGuardado)
            {
                PuntoAparicion = controlador.transform.position;
                HayPuntoAparicion = true;
                puntoGuardado = true;
            }

            controlador.gameObject.SetActive(false);
        }

        Desactivar<FollowPlayer>();
        Desactivar<BoatCamera>();
        Desactivar<CameraIntro>();
        Desactivar<SeguirJugador>();

        Desactivar<part2Youtube>();
        Desactivar<NewMonoBehaviourScript>();
        Desactivar<ProyectilMortal>();

        Camera camara = Camera.main;

        if (camara == null)
        {
            foreach (Camera candidata in FindObjectsByType<Camera>(FindObjectsInactive.Exclude))
            {
                if (EsJugadorDeRed(candidata.transform)) continue;

                camara = candidata;
                break;
            }
        }

        if (camara != null && !EsJugadorDeRed(camara.transform))
        {
            string escena = SceneManager.GetActiveScene().name;

            if (escena == "LevelYoutube" || escena == "LevelYoutubev2")
            {
                camara.GetComponent<FollowPlayer>().enabled = true;
            }
            else
            {
                if (camara.GetComponent<CamaraLobby>() == null)
                    camara.gameObject.AddComponent<CamaraLobby>();
            }
        }

        StartCoroutine(ColocarJugadorLocal());

        int jugadoresRed = 0;
        foreach (MovePlayer1 jugador in FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude))
        {
            if (jugador.IsSpawned) jugadoresRed++;
        }

        Debug.Log(
            "[Online] ESCENA_PREPARADA=" + SceneManager.GetActiveScene().name +
            " punto=" + PuntoAparicion +
            " hayPunto=" + HayPuntoAparicion +
            " jugadoresRed=" + jugadoresRed +
            " camara=" + (camara != null ? camara.name : "ninguna"));
    }

    private static void Desactivar<T>() where T : Behaviour
    {
        foreach (T componente in FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            if (EsJugadorDeRed(componente.transform)) continue;
            componente.enabled = false;
        }
    }

    private IEnumerator ColocarJugadorLocal()
    {
        float limite = Time.realtimeSinceStartup + 10f;

        while (Time.realtimeSinceStartup < limite)
        {
            MovePlayer1 local = BuscarJugadorLocal();

            if (local != null)
            {
                if (HayPuntoAparicion)
                    local.ColocarEnAparicion(PuntoAparicion, local.OwnerClientId);

                local.RefrescarCamara();

                Debug.Log(
                    "[Online] LOCAL_COLOCADO owner=" + local.OwnerClientId +
                    " pos=" + local.transform.position +
                    " camara=" + (Camera.main != null ? Camera.main.name : "ninguna"));

                yield break;
            }

            yield return null;
        }

        Debug.LogWarning("[Online] NO_SE_ENCONTRO_JUGADOR_LOCAL en 10s");
    }

    private static MovePlayer1 BuscarJugadorLocal()
    {
        foreach (MovePlayer1 jugador in FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude))
        {
            if (jugador.IsSpawned && jugador.IsOwner)
                return jugador;
        }

        return null;
    }

    private static bool EsJugadorDeRed(Transform transform)
    {
        return transform.GetComponentInParent<NetworkObject>() != null;
    }
}

