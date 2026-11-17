using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Va en el prefab del jugador (el "Capsule" que carga MultijugadorJuego).
/// Cada skin es un hijo del prefab (Yoshi, Spiderman...). Solo una queda activa.
/// La skin elegida se replica con un NetworkVariable.
/// Una skin marcada como "esBarco" no se puede elegir: se activa automaticamente
/// en la escena indicada en "escenaBarco" (por defecto LevelSelector).
/// </summary>
public class SkinJugador : NetworkBehaviour
{
    [Serializable]
    public class SkinData
    {
        public string nombre;          // "Yoshi", "Spiderman", "Barco"
        public GameObject modelo;      // hijo del prefab
        public Sprite icono;           // opcional, para la UI
        public bool esBarco;           // true = solo se usa en escenaBarco, no es elegible
    }

    [SerializeField] private SkinData[] skins;
    [SerializeField] private string escenaBarco = "LevelSelector";

    private readonly NetworkVariable<int> skinIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public static readonly List<SkinJugador> Todos = new List<SkinJugador>();
    public static SkinJugador Local { get; private set; }

    public static event System.Action AlCambiarLista;

    public int IndiceSkin => skinIndex.Value;
    public SkinData[] Skins => skins;

    // Cantidad de skins que el jugador puede elegir (sin contar el barco)
    public int CantidadSkins
    {
        get
        {
            int n = 0;
            if (skins == null) return 0;
            foreach (SkinData s in skins) if (!s.esBarco) n++;
            return n;
        }
    }

    public string NombreSkin
    {
        get
        {
            int i = IndiceElegido();
            return i >= 0 ? skins[i].nombre : "?";
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!Todos.Contains(this)) Todos.Add(this);
        if (IsOwner) Local = this;

        PrepararSkins();

        skinIndex.OnValueChanged += AlCambiarSkin;
        SceneManager.sceneLoaded += AlCargarEscena;

        AplicarSkin();
        AlCambiarLista?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        skinIndex.OnValueChanged -= AlCambiarSkin;
        SceneManager.sceneLoaded -= AlCargarEscena;
        Todos.Remove(this);
        if (Local == this) Local = null;
        AlCambiarLista?.Invoke();
    }

    /// <summary>
    /// Los modelos de skin son solo visuales. Si traen Rigidbody, colliders, camara o
    /// root motion, empujan al jugador o le roban la camara (y el teletransporte al spawn falla).
    /// </summary>
    private void PrepararSkins()
    {
        if (skins == null) return;

        foreach (SkinData skin in skins)
        {
            if (skin.modelo == null) continue;

            foreach (Rigidbody r in skin.modelo.GetComponentsInChildren<Rigidbody>(true))
            {
                if (r.gameObject == gameObject) continue;
                r.isKinematic = true;
                r.useGravity = false;
            }

            foreach (Collider c in skin.modelo.GetComponentsInChildren<Collider>(true))
                c.enabled = false;

            foreach (Camera cam in skin.modelo.GetComponentsInChildren<Camera>(true))
                cam.enabled = false;

            foreach (AudioListener al in skin.modelo.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;

            foreach (Animator a in skin.modelo.GetComponentsInChildren<Animator>(true))
                a.applyRootMotion = false;
        }
    }

    private void AlCambiarSkin(int anterior, int actual)
    {
        AplicarSkin();
        AlCambiarLista?.Invoke();
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AplicarSkin();
    }

    // Indice real (en el array) de la skin elegida, ignorando el barco
    private int IndiceElegido()
    {
        if (skins == null || skins.Length == 0) return -1;

        int i = Mathf.Clamp(skinIndex.Value, 0, skins.Length - 1);
        if (!skins[i].esBarco) return i;

        for (int k = 0; k < skins.Length; k++)
            if (!skins[k].esBarco) return k;

        return i;
    }

    private int IndiceBarco()
    {
        if (skins == null) return -1;
        for (int k = 0; k < skins.Length; k++)
            if (skins[k].esBarco) return k;
        return -1;
    }

    /// <summary>Solo lo puede llamar el dueño del jugador.</summary>
    public void CambiarSkin(int indice)
    {
        if (!IsOwner || skins == null || skins.Length == 0) return;

        indice = Mathf.Clamp(indice, 0, skins.Length - 1);
        if (skins[indice].esBarco) return; // el barco no se elige a mano

        skinIndex.Value = indice;
    }

    public void SiguienteSkin() => Mover(1);
    public void AnteriorSkin() => Mover(-1);

    private void Mover(int paso)
    {
        if (!IsOwner || skins == null || skins.Length == 0) return;

        int i = IndiceElegido();
        for (int intento = 0; intento < skins.Length; intento++)
        {
            i = (i + paso + skins.Length) % skins.Length;
            if (!skins[i].esBarco)
            {
                skinIndex.Value = i;
                return;
            }
        }
    }

    private void AplicarSkin()
    {
        if (skins == null || skins.Length == 0) return;

        int activa = IndiceElegido();

        // En la escena del barco se muestra el barco, si existe
        int barco = IndiceBarco();
        if (barco >= 0 && SceneManager.GetActiveScene().name == escenaBarco)
            activa = barco;

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].modelo != null)
                skins[i].modelo.SetActive(i == activa);
        }

        Animator anim = skins[activa].modelo != null
            ? skins[activa].modelo.GetComponentInChildren<Animator>()
            : null;

        if (anim == null) return; // el barco puede no tener Animator

        MovePlayer1 mover = GetComponent<MovePlayer1>();
        if (mover != null)
            mover.AsignarAnimator(anim);

        PlayerHit golpe = GetComponent<PlayerHit>();
        if (golpe == null) golpe = GetComponentInChildren<PlayerHit>(true);
        if (golpe != null)
            golpe.AsignarAnimator(anim);
    }
}