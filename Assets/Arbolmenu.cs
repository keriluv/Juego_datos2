using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Arbolmenu : MonoBehaviour
{
    [SerializeField] private GameObject BkArbolView;
    [SerializeField] private Canvas ArbolCanvas;
    private bool isEnabled = false;
    private RawImage rawImage;

    [SerializeField] private TextMeshProUGUI TextoInorden;
    [SerializeField] private GestorPeligros gestorPeligros;
    void Start()
    {
        rawImage = BkArbolView.GetComponent<RawImage>();
        rawImage.enabled = false;
        ArbolCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            this.isEnabled = !isEnabled;
            rawImage.enabled = isEnabled;
            ArbolCanvas.enabled = isEnabled;

            TextoInorden.text = "";

            List<PeligroData> Inorden = gestorPeligros.getArbol().RecorridoInOrder();
            foreach (PeligroData a in Inorden)
            {
                TextoInorden.text += a.nombre + "-";
            }

        }
    }
}
