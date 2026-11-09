using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuscadorGoogle : MonoBehaviour
{
    public TMP_InputField campo;
    public InteraccionTutorial manager;

    private void Awake()
    {
        if (campo == null) campo = GetComponent<TMP_InputField>();
        if (campo != null) campo.onSubmit.AddListener(Enviar);
    }

    private void Enviar(string texto)
    {
        if (manager != null) manager.Buscar(texto);
    }

    public static bool Escribiendo()
    {
        EventSystem sistema = EventSystem.current;
        if (sistema == null || sistema.currentSelectedGameObject == null) return false;

        return sistema.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;
    }

    
}
