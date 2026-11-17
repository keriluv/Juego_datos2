using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BotonSaltarTutorial : MonoBehaviour
{
    public string escena = "LevelSelector";

    private void Awake()
    {
        Button boton = GetComponent<Button>();
        if (boton != null) boton.onClick.AddListener(IrAlMenu);
    }

    private void IrAlMenu()
    {
        if (string.IsNullOrEmpty(escena)) return;

        if (MultijugadorJuego.Instancia != null)
            MultijugadorJuego.Instancia.IrAEscena(escena);
        else
            SceneManager.LoadScene(escena);
    }
}
