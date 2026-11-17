using System.Collections;
using UnityEngine;

public class crackMinecraft : StruckObject
{
    [SerializeField] private GameObject explosion;
    [SerializeField] private ObjectivesManager om;
    [SerializeField] private GameObject crack;

    private bool yaDisparado;

    private void Start()
    {
        if (explosion != null) explosion.SetActive(false);
        if (crack != null) crack.SetActive(false);
    }

    public override void OnHit()
    {
        if (yaDisparado) return;
        yaDisparado = true;

        if (om != null)
            om.UpdateObjective();

        GameProgressionManager prog = GameProgressionManager.Asegurar();
        prog.RegisterObjectiveCompleted("CrackBroken_LevelYoutubev2");
        prog.RegisterLevelCompleted("LevelYoutubev2");

        StartCoroutine(AnimacionYTransicion());
    }

    private IEnumerator AnimacionYTransicion()
    {
        if (explosion != null)
        {
            explosion.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            explosion.SetActive(false);
        }

        if (crack != null)
            crack.SetActive(true);

        bool enRed = Unity.Netcode.NetworkManager.Singleton != null
            && Unity.Netcode.NetworkManager.Singleton.IsListening;

        if (enRed && RelayTransicionMinecraft.Local != null)
            RelayTransicionMinecraft.Local.PedirTransicion();
        else
            TransicionMinecraftVideo.SolicitarDesdeCrack();
    }
}
