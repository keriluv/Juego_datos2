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
