using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FilaExtensionUI : MonoBehaviour, IPointerClickHandler
{
    public string nombre = "Extensión";
    public string descripcion = "";
    public bool activa = true;

    public InteraccionTutorial manager;
    public Image fondoInterruptor;
    public RectTransform perilla;
    public Image fondoFila;

    private static readonly Color Verde = new Color(0.10f, 0.65f, 0.42f, 1f);
    private static readonly Color Gris = new Color(0.72f, 0.75f, 0.78f, 1f);

    private void Start()
    {
        AplicarVisual();
    }

    public void AplicarVisual()
    {
        if (fondoInterruptor != null)
            fondoInterruptor.color = activa ? Verde : Gris;

        if (perilla != null)
            perilla.anchoredPosition = new Vector2(activa ? 11f : -11f, 0f);

        if (fondoFila != null)
            fondoFila.color = activa
                ? new Color(0.973f, 0.980f, 0.988f, 1f)
                : new Color(0.949f, 0.953f, 0.961f, 1f);
    }

    public void Alternar()
    {
        activa = !activa;
        AplicarVisual();

        if (manager != null)
            manager.AlternarExtension(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Alternar();
    }
}
