using Unity.VisualScripting;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] private GameObject pantallaScreen;
    [SerializeField] private GameObject sonidoScreen;
    [SerializeField] private GameObject controlesScreen;
    [SerializeField] private GameObject idiomaScreen;
    void Start()
    {
        pantallaScreen.SetActive(true);
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(false);
        idiomaScreen.SetActive(false);
    }

    public void goToPantalla()
    {
        pantallaScreen.SetActive(true );
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(false);
        idiomaScreen.SetActive(false);
    }

    public void goToSonido()
    {
        pantallaScreen.SetActive(false);
        sonidoScreen.SetActive(true);
        controlesScreen.SetActive(false) ;
        idiomaScreen.SetActive(false);
    }

    public void goToControl()
    {
        pantallaScreen.SetActive(false);
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(true);
        idiomaScreen.SetActive(false);
    }

    public void goToIdiomas()
    {
        pantallaScreen.SetActive(false);
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(false);
        idiomaScreen.SetActive(true);
    }
}
