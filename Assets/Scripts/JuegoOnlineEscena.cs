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

        // Creating a room while already inside pruebaLobby does not reload the
        // scene, so force a prepare pass as soon as the NetworkManager exists.
        if (SceneManager.GetActiveScene().name == "pruebaLobby")
        {
            StopAllCoroutines();
            StartCoroutine(EsperarRed());
        }
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();

        if (escena.name == "MainMenu")
        {
            HayPuntoAparicion = false;
            return;
        }

        if (escena.name == "LevelSnapchat" || escena.name == "EscenaTiktok")
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
            if (escena == "pruebaLobby")
            {
                // Cinematic lobby cam uses nearClip=10; keep a fixed frame, but
                // clamp near clip so a standing player at spawn is not shaved away.
                if (camara.nearClipPlane > 1f)
                    camara.nearClipPlane = 0.3f;
            }
            else if (escena == "LevelYoutubev2")
            {
            }
            else if (escena == "LevelYoutube")
            {
                camara.GetComponent<FollowPlayer>().enabled = true;
            }
            else if (escena == "LevelSelector" || escena == "LevelYoutubev3")
            {
                camara.GetComponent<BoatCamera>().enabled = true;
            }
            else { 
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

                // Re-apply a few times: NetworkTransform / NetworkRigidbody can
                // overwrite a single position write right after spawn.
                for (int i = 0; i < 8; i++)
                {
                    yield return new WaitForFixedUpdate();
                    yield return null;

                    if (HayPuntoAparicion)
                        local.ColocarEnAparicion(PuntoAparicion, local.OwnerClientId);
                }

                Debug.Log("[Online] LOCAL_COLOCADO owner=" + local.OwnerClientId + " pos=" + local.transform.position);
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

