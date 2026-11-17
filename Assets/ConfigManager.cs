using UnityEngine;
using UnityEngine.UI;

public class ConfigManager : MonoBehaviour
{
    [SerializeField] private GameObject pantallaScreen;
    [SerializeField] private GameObject sonidoScreen;
    [SerializeField] private GameObject controlesScreen;
    [SerializeField] private GameObject idiomaScreen;

    [Header("Sliders (se buscan solos si quedan vacíos)")]
    [SerializeField] private Slider sliderBrillo;
    [SerializeField] private Slider sliderSensibilidad;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderAmbiente;

    private bool escuchando;

    void Start()
    {
        pantallaScreen.SetActive(true);
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(false);
        idiomaScreen.SetActive(false);

        BuscarSlidersSiFaltan();
        SincronizarDesdeAjustes();
        ConectarSliders();
    }

    private void OnDestroy()
    {
        DesconectarSliders();
    }

    private void BuscarSlidersSiFaltan()
    {
        if (sliderBrillo == null)
            sliderBrillo = BuscarSliderHijo("brillo");
        if (sliderSensibilidad == null)
            sliderSensibilidad = BuscarSliderHijo("sens");
        if (sliderMusica == null)
            sliderMusica = BuscarSliderHijo("musica");
        if (sliderAmbiente == null)
            sliderAmbiente = BuscarSliderHijo("efectos");
    }

    private Slider BuscarSliderHijo(string nombrePadre)
    {
        Transform[] todos = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < todos.Length; i++)
        {
            if (todos[i].name != nombrePadre) continue;
            Slider slider = todos[i].GetComponentInChildren<Slider>(true);
            if (slider != null) return slider;
        }

        return null;
    }

    private void SincronizarDesdeAjustes()
    {
        AjustesJuego a = AjustesJuego.Instancia;
        if (a == null) return;

        if (sliderBrillo != null) sliderBrillo.SetValueWithoutNotify(a.brillo);
        if (sliderSensibilidad != null) sliderSensibilidad.SetValueWithoutNotify(a.sensibilidad);
        if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(a.volumenMusica);
        if (sliderAmbiente != null) sliderAmbiente.SetValueWithoutNotify(a.volumenAmbiente);
    }

    private void ConectarSliders()
    {
        if (escuchando) return;
        escuchando = true;

        if (sliderBrillo != null) sliderBrillo.onValueChanged.AddListener(AlCambiarBrillo);
        if (sliderSensibilidad != null) sliderSensibilidad.onValueChanged.AddListener(AlCambiarSensibilidad);
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(AlCambiarMusica);
        if (sliderAmbiente != null) sliderAmbiente.onValueChanged.AddListener(AlCambiarAmbiente);
    }

    private void DesconectarSliders()
    {
        if (!escuchando) return;
        escuchando = false;

        if (sliderBrillo != null) sliderBrillo.onValueChanged.RemoveListener(AlCambiarBrillo);
        if (sliderSensibilidad != null) sliderSensibilidad.onValueChanged.RemoveListener(AlCambiarSensibilidad);
        if (sliderMusica != null) sliderMusica.onValueChanged.RemoveListener(AlCambiarMusica);
        if (sliderAmbiente != null) sliderAmbiente.onValueChanged.RemoveListener(AlCambiarAmbiente);
    }

    private void AlCambiarBrillo(float valor)
    {
        if (AjustesJuego.Instancia != null)
            AjustesJuego.Instancia.EstablecerBrillo(valor);
    }

    private void AlCambiarSensibilidad(float valor)
    {
        if (AjustesJuego.Instancia != null)
            AjustesJuego.Instancia.EstablecerSensibilidad(valor);
    }

    private void AlCambiarMusica(float valor)
    {
        if (AjustesJuego.Instancia != null)
            AjustesJuego.Instancia.EstablecerMusica(valor);
    }

    private void AlCambiarAmbiente(float valor)
    {
        if (AjustesJuego.Instancia != null)
            AjustesJuego.Instancia.EstablecerAmbiente(valor);
    }

    public void goToPantalla()
    {
        pantallaScreen.SetActive(true);
        sonidoScreen.SetActive(false);
        controlesScreen.SetActive(false);
        idiomaScreen.SetActive(false);
    }

    public void goToSonido()
    {
        pantallaScreen.SetActive(false);
        sonidoScreen.SetActive(true);
        controlesScreen.SetActive(false);
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
