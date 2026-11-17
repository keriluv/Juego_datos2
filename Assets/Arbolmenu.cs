using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Arbolmenu : MonoBehaviour
{
    [SerializeField] private GameObject BkArbolView;
    private RawImage rawImage;

    [SerializeField] private TextMeshProUGUI TextoInorden;

    void Start()
    {
        rawImage = BkArbolView.GetComponent<RawImage>();
        rawImage.enabled = false;

        // Enlarge inventory/tree panel for readability without scaling the whole pause HUD.
        RectTransform fondo = BkArbolView.GetComponent<RectTransform>();
        if (fondo != null)
        {
            fondo.sizeDelta = new Vector2(
                Mathf.Max(fondo.sizeDelta.x * 1.45f, 520f),
                Mathf.Max(fondo.sizeDelta.y * 1.45f, 420f));
        }
    }

    public void RecorridoInOrder()
    {
        List<PeligroData> Inorden = GestorPeligros.Instance.GetArbol().RecorridoInOrder();
        foreach (PeligroData a in Inorden)
        {
            TextoInorden.text += a.nombre + "-";
        }
    }
}
