using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JuegoOnlineEscena : MonoBehaviour
{
    public static Vector3 PuntoAparicion { get; private set; }
    public static bool HayPuntoAparicion { get; private set; }

    private static JuegoOnlineEscena instancia;

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
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();

        if (escena.name == "MainMenu" || escena.name == "pruebaLobby")
        {
            HayPuntoAparicion = false;
            return;
        }

        NetworkManager red = NetworkManager.Singleton;
        if (red == null || !red.IsListening) return;

        StartCoroutine(PrepararEscena());
    }

    private IEnumerator PrepararEscena()
    {
        yield return null;

        bool puntoGuardado = false;

        foreach (MovePlayer1 controlador in FindObjectsByType<MovePlayer1>(FindObjectsInactive.Exclude))
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
            if (camara.GetComponent<CamaraLobby>() == null)
                camara.gameObject.AddComponent<CamaraLobby>();
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
