using UnityEngine;
using UnityEngine.EventSystems;

public class OnHover : MonoBehaviour
{
    [SerializeField] private HoverMainMenu HoverMainMenu;
    [SerializeField] private int valor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("mouse entró");

        HoverMainMenu.ShowHover(valor);
    }
}
