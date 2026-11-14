using Unity.Netcode;
using UnityEngine;
public class PlayerHit : NetworkBehaviour
{
    [SerializeField] private GameObject hit;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float showHit = 0.2f;

    [SerializeField] private Animator animator;

    private bool hitEnabled = false;
    private float time = 0f;

    private readonly NetworkVariable<bool> networkHit = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private bool ControlaEsteCliente => !IsSpawned || IsOwner;

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hit != null) hit.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        networkHit.OnValueChanged += AlCambiarHitRed;
        AplicarAnimacion(networkHit.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkHit.OnValueChanged -= AlCambiarHitRed;
    }

    public void AsignarAnimator(Animator nuevo)
    {
        animator = nuevo;
    }

    private void AlCambiarHitRed(bool anterior, bool actual)
    {
    
        if (ControlaEsteCliente) return;
        AplicarAnimacion(actual);
    }

    private void AplicarAnimacion(bool golpeando)
    {
        if (animator != null)
            animator.SetBool("isHit", golpeando);
    }

    private void EstablecerGolpe(bool golpeando)
    {
        if (hit != null) hit.SetActive(golpeando);
        AplicarAnimacion(golpeando);

        if (IsSpawned && IsOwner)
            networkHit.Value = golpeando;
    }

    void FixedUpdate()
    {
        if (!ControlaEsteCliente) return;

        if (Input.GetKey(KeyCode.Mouse0) && !hitEnabled)
        {
            hitEnabled = true;
            EstablecerGolpe(true);
            time = 0f;
        }

        if (hitEnabled)
        {
            time += Time.fixedDeltaTime;

            if (time >= showHit && ((hit != null && hit.activeSelf) || networkHit.Value))
            {
                EstablecerGolpe(false);
            }

            if (time >= cooldown)
            {
                time = 0f;
                hitEnabled = false;
            }
        }
    }
}