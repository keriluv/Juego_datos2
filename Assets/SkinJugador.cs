using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Va en el prefab del jugador (el "Capsule" que carga MultijugadorJuego).
/// Cada skin es un hijo del prefab (Yoshi, Spiderman...). Solo una queda activa.
/// El dueño elige la skin y se replica a todos los clientes con un NetworkVariable.
/// </summary>
public class SkinJugador : NetworkBehaviour
{
    [Serializable]
    public class SkinData
    {
        public string nombre;          // "Yoshi", "Spiderman"
        public GameObject modelo;      // hijo del prefab con el modelo/animator
        public Sprite icono;           // opcional, para la UI
    }

    [SerializeField] private SkinData[] skins;

    private readonly NetworkVariable<int> skinIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // Lista de todos los jugadores con skin (para la UI del lobby)
    public static readonly List<SkinJugador> Todos = new List<SkinJugador>();
    public static SkinJugador Local { get; private set; }

    // Se dispara cuando entra/sale alguien o cambia una skin
    public static event System.Action AlCambiarLista;

    public int IndiceSkin => skinIndex.Value;
    public int CantidadSkins => skins != null ? skins.Length : 0;

    public SkinData[] Skins => skins;

    public string NombreSkin
    {
        get
        {
            if (skins == null || skins.Length == 0) return "?";
            int i = Mathf.Clamp(skinIndex.Value, 0, skins.Length - 1);
            return skins[i].nombre;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!Todos.Contains(this)) Todos.Add(this);
        if (IsOwner) Local = this;

        skinIndex.OnValueChanged += AlCambiarSkin;
        AplicarSkin(skinIndex.Value);
        AlCambiarLista?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        skinIndex.OnValueChanged -= AlCambiarSkin;
        Todos.Remove(this);
        if (Local == this) Local = null;
        AlCambiarLista?.Invoke();
    }

    private void AlCambiarSkin(int anterior, int actual)
    {
        AplicarSkin(actual);
        AlCambiarLista?.Invoke();
    }

    /// <summary>Solo lo puede llamar el dueño del jugador.</summary>
    public void CambiarSkin(int indice)
    {
        if (!IsOwner || skins == null || skins.Length == 0) return;
        skinIndex.Value = Mathf.Clamp(indice, 0, skins.Length - 1);
    }

    public void SiguienteSkin()
    {
        if (skins == null || skins.Length == 0) return;
        CambiarSkin((skinIndex.Value + 1) % skins.Length);
    }

    public void AnteriorSkin()
    {
        if (skins == null || skins.Length == 0) return;
        CambiarSkin((skinIndex.Value - 1 + skins.Length) % skins.Length);
    }

    private void AplicarSkin(int indice)
    {
        if (skins == null || skins.Length == 0) return;
        indice = Mathf.Clamp(indice, 0, skins.Length - 1);

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].modelo != null)
                skins[i].modelo.SetActive(i == indice);
        }

        // MovePlayer1 usa un Animator: hay que apuntarlo al de la skin activa
        Animator anim = skins[indice].modelo != null
            ? skins[indice].modelo.GetComponentInChildren<Animator>()
            : null;

        MovePlayer1 mover = GetComponent<MovePlayer1>();
        if (mover != null && anim != null)
            mover.AsignarAnimator(anim);
    }
}
