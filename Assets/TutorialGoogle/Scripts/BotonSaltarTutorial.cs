using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BotonSaltarTutorial : MonoBehaviour
{
    public string escena = "MainMenu";

    private void Awake()
    {
        Button boton = GetComponent<Button>();
        if (boton != null) boton.onClick.AddListener(IrAlMenu);
    }

    private void IrAlMenu()
    {
        if (!string.IsNullOrEmpty(escena))
            SceneManager.LoadScene(escena);
    }
}
